using Wrist_Remote_for_Windows.Input;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Radios;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace Wrist_Remote_for_Windows.Bluetooth;

public sealed class BluetoothMouseServer : IDisposable
{
    private static readonly Guid ServiceUuid =
        Guid.Parse("3786dafe-0f28-4143-ad53-dd109f43f00c");

    private static readonly Guid CharacteristicUuid =
        Guid.Parse("f1c90f8a-af67-4a8e-b982-522b6a575722");

    private GattServiceProvider? _serviceProvider;
    private GattLocalCharacteristic? _characteristic;
    private GattSession? _session;

    public bool IsRunning { get; private set; }

    public bool IsClientConnected { get; private set; }

    public event EventHandler? Started;
    public event EventHandler? Stopped;

    public event EventHandler? ClientConnected;
    public event EventHandler? ClientDisconnected;

    public event EventHandler<InputCommand>? CommandReceived;
    public event EventHandler? BluetoothStateChanged;

    private Radio? _bluetoothRadio;


    // ============================================================
    // Bluetooth check
    // ============================================================

    public async Task<BluetoothCheckResult>
    CheckBluetoothAsync()
    {
        // ------------------------------------------------------------
        // Bluetooth adapter
        // ------------------------------------------------------------

        var adapter =
            await BluetoothAdapter.GetDefaultAsync();

        if (adapter is null)
            return BluetoothCheckResult.AdapterNotFound;


        // ------------------------------------------------------------
        // Bluetooth radio
        // ------------------------------------------------------------

        if (_bluetoothRadio == null)
        {
            var radioFound =
                await InitializeBluetoothRadioAsync();

            if (!radioFound)
                return BluetoothCheckResult.AdapterNotFound;
        }


        // ------------------------------------------------------------
        // Bluetooth enabled
        // ------------------------------------------------------------

        if (_bluetoothRadio!.State !=
            RadioState.On)
        {
            return BluetoothCheckResult.BluetoothDisabled;
        }


        // ------------------------------------------------------------
        // BLE
        // ------------------------------------------------------------

        if (!adapter.IsLowEnergySupported)
            return BluetoothCheckResult.LowEnergyNotSupported;


        // ------------------------------------------------------------
        // Peripheral
        // ------------------------------------------------------------

        if (!adapter.IsPeripheralRoleSupported)
            return BluetoothCheckResult.PeripheralRoleNotSupported;


        return BluetoothCheckResult.Ok;
    }


    // ============================================================
    // Start
    // ============================================================

    public async Task StartAsync()
    {
        if (IsRunning)
            return;

        var bluetoothResult =
            await CheckBluetoothAsync();

        if (bluetoothResult !=
            BluetoothCheckResult.Ok)
        {
            throw new InvalidOperationException(
                BluetoothCheckResultHelper.GetMessage(
                    bluetoothResult));
        }

        var serviceResult =
            await GattServiceProvider.CreateAsync(
                ServiceUuid);

        if (serviceResult.Error !=
            BluetoothError.Success)
        {
            throw new InvalidOperationException(
                $"Unable to create GATT service: " +
                $"{serviceResult.Error}");
        }

        _serviceProvider =
            serviceResult.ServiceProvider;

        var parameters =
            new GattLocalCharacteristicParameters
            {
                CharacteristicProperties =
                    GattCharacteristicProperties.Write |
                    GattCharacteristicProperties.WriteWithoutResponse,

                WriteProtectionLevel =
                    GattProtectionLevel.Plain
            };

        var characteristicResult =
            await _serviceProvider.Service
                .CreateCharacteristicAsync(
                    CharacteristicUuid,
                    parameters);

        if (characteristicResult.Error !=
            BluetoothError.Success)
        {
            throw new InvalidOperationException(
                $"Unable to create GATT characteristic: " +
                $"{characteristicResult.Error}");
        }

        _characteristic =
            characteristicResult.Characteristic;

        _characteristic.WriteRequested +=
            Characteristic_WriteRequested;

        var advertisingParameters =
            new GattServiceProviderAdvertisingParameters
            {
                IsDiscoverable = true,
                IsConnectable = true
            };

        _serviceProvider.StartAdvertising(
            advertisingParameters);

        Debug.WriteLine(
            "BLE GATT server advertising started");

        IsRunning = true;

        Started?.Invoke(
            this,
            EventArgs.Empty);
    }

    private async Task<bool> InitializeBluetoothRadioAsync()
    {
        var radios =
            await Radio.GetRadiosAsync();

        foreach (var radio in radios)
        {
            if (radio.Kind != RadioKind.Bluetooth)
                continue;

            _bluetoothRadio = radio;

            _bluetoothRadio.StateChanged +=
                BluetoothRadio_StateChanged;

            return true;
        }

        return false;
    }

    private void BluetoothRadio_StateChanged(
    Radio sender,
    object args)
    {
        Debug.WriteLine(
            $"Bluetooth radio state changed: " +
            $"{sender.State}");

        BluetoothStateChanged?.Invoke(
            this,
            EventArgs.Empty);
    }


    // ============================================================
    // Write request
    // ============================================================

    private async void Characteristic_WriteRequested(
        GattLocalCharacteristic sender,
        GattWriteRequestedEventArgs args)
    {
        var deferral =
            args.GetDeferral();

        try
        {
            Debug.WriteLine(
                "BLE WRITE REQUEST RECEIVED");

            // ----------------------------------------------------
            // Получаем GATT session подключенного клиента
            // ----------------------------------------------------

            SetSession(args.Session);

            var request =
                await args.GetRequestAsync();

            if (request == null)
            {
                Debug.WriteLine(
                    "BLE request is null");

                return;
            }

            var reader =
                DataReader.FromBuffer(
                    request.Value);

            uint length =
                reader.UnconsumedBufferLength;

            if (length < 1)
            {
                Debug.WriteLine(
                    "Invalid empty BLE packet");

                return;
            }

            byte typeRaw =
                reader.ReadByte();

            var type =
                (InputCommandType)typeRaw;

            InputCommand? command = null;

            switch (type)
            {
                case InputCommandType.Mouse:

                    command =
                        ReadMouseCommand(
                            reader,
                            length);

                    break;


                case InputCommandType.Media:

                    command =
                        ReadMediaCommand(
                            reader,
                            length);

                    break;


                case InputCommandType.Presentation:

                    command =
                        ReadPresentationCommand(
                            reader,
                            length);

                    break;


                case InputCommandType.System:

                    command =
                        ReadSystemCommand(
                            reader,
                            length);

                    break;


                default:

                    Debug.WriteLine(
                        $"Unknown command type: " +
                        $"{typeRaw:X2}");

                    break;
            }

            if (command != null)
            {
                Debug.WriteLine(
                    $"Command received: " +
                    $"{command.Type}");

                CommandReceived?.Invoke(
                    this,
                    command);
            }

            if (request.Option ==
                GattWriteOption.WriteWithResponse)
            {
                request.Respond();

                Debug.WriteLine(
                    "BLE write response sent");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(
                $"BLE WRITE ERROR: {ex}");
        }
        finally
        {
            deferral.Complete();
        }
    }


    // ============================================================
    // GATT session
    // ============================================================

    private void SetSession(
        GattSession session)
    {
        // Если это уже наша текущая сессия,
        // ничего делать не нужно.
        if (ReferenceEquals(
            _session,
            session))
        {
            return;
        }

        // На всякий случай удаляем старую подписку.
        if (_session != null)
        {
            _session.SessionStatusChanged -=
                Session_SessionStatusChanged;

            _session.Dispose();
        }

        _session = session;

        _session.SessionStatusChanged +=
            Session_SessionStatusChanged;

        Debug.WriteLine(
            $"GATT session received. " +
            $"Status: {_session.SessionStatus}");

        if (_session.SessionStatus ==
            GattSessionStatus.Active)
        {
            SetClientConnected();
        }
    }


    private void Session_SessionStatusChanged(
        GattSession sender,
        GattSessionStatusChangedEventArgs args)
    {
        Debug.WriteLine(
            $"GATT session status changed: " +
            $"{args.Status}");

        if (args.Status ==
            GattSessionStatus.Active)
        {
            SetClientConnected();
        }
        else if (args.Status ==
                 GattSessionStatus.Closed)
        {
            SetClientDisconnected();
        }
    }


    private void SetClientConnected()
    {
        if (IsClientConnected)
            return;

        IsClientConnected = true;

        Debug.WriteLine(
            "BLE CLIENT CONNECTED");

        ClientConnected?.Invoke(
            this,
            EventArgs.Empty);
    }


    private void SetClientDisconnected()
    {
        if (!IsClientConnected)
            return;

        IsClientConnected = false;

        Debug.WriteLine(
            "BLE CLIENT DISCONNECTED");

        ClientDisconnected?.Invoke(
            this,
            EventArgs.Empty);
    }


    // ============================================================
    // Mouse
    //
    // [Type]
    // [Buttons]
    // [Action]
    // [X/Wheel - Int16]
    // [Y       - Int16]
    //
    // Total = 7 bytes
    // ============================================================

    private static MouseInputCommand?
       ReadMouseCommand(
           DataReader reader,
           uint totalLength)
    {
        // Mouse packet:
        //
        // [Type][Buttons][Action][X][Y]
        //
        // 1 + 1 + 1 + 2 + 2 = 7 bytes

        if (totalLength < 7)
        {
            Debug.WriteLine(
                $"Invalid mouse packet length: {totalLength}");

            return null;
        }


        byte buttonsRaw =
            reader.ReadByte();

        byte actionRaw =
            reader.ReadByte();


        reader.ByteOrder =
            ByteOrder.LittleEndian;


        short data1 =
            reader.ReadInt16();

        short data2 =
            reader.ReadInt16();


        var buttons =
            (MouseButtons)buttonsRaw;

        var action =
            (MouseAction)actionRaw;


        short x = 0;
        short y = 0;
        short wheel = 0;


        switch (action)
        {
            case MouseAction.Move:

                x = data1;
                y = data2;

                break;


            case MouseAction.Wheel:

                wheel = data1;

                break;


            default:

                Debug.WriteLine(
                    $"Unknown mouse action: " +
                    $"{actionRaw:X2}");

                return null;
        }


        return new MouseInputCommand(
            buttons,
            action,
            x,
            y,
            wheel);
    }


    // ============================================================
    // Media
    // ============================================================

    private static MediaInputCommand?
        ReadMediaCommand(
            DataReader reader,
            uint totalLength)
    {
        if (totalLength < 4)
            return null;

        byte commandRaw =
            reader.ReadByte();

        byte valueRaw =
            reader.ReadByte();

        byte flags =
            reader.ReadByte();

        var command =
            (MediaCommand)commandRaw;

        var value =
            unchecked((sbyte)valueRaw);

        return new MediaInputCommand(
            command,
            value,
            flags);
    }


    // ============================================================
    // Presentation
    // ============================================================

    private static PresentationInputCommand?
        ReadPresentationCommand(
            DataReader reader,
            uint totalLength)
    {
        if (totalLength < 4)
            return null;

        byte commandRaw =
            reader.ReadByte();

        byte valueRaw =
            reader.ReadByte();

        byte flags =
            reader.ReadByte();

        var command =
            (PresentationCommand)commandRaw;

        var value =
            unchecked((sbyte)valueRaw);

        return new PresentationInputCommand(
            command,
            value,
            flags);
    }


    // ============================================================
    // System
    // ============================================================

    private static SystemInputCommand?
        ReadSystemCommand(
            DataReader reader,
            uint totalLength)
    {
        if (totalLength < 4)
            return null;

        byte commandRaw =
            reader.ReadByte();

        byte valueRaw =
            reader.ReadByte();

        byte flags =
            reader.ReadByte();

        var command =
            (SystemCommand)commandRaw;

        var value =
            unchecked((sbyte)valueRaw);

        return new SystemInputCommand(
            command,
            value,
            flags);
    }


    // ============================================================
    // Stop
    // ============================================================

    public void Stop()
    {
        if (!IsRunning)
            return;

        Debug.WriteLine(
            "Stopping BLE GATT server");

        if (_bluetoothRadio != null)
        {
            _bluetoothRadio.StateChanged -=
                BluetoothRadio_StateChanged;

            _bluetoothRadio = null;
        }

        // --------------------------------------------------------
        // GATT session
        // --------------------------------------------------------

        if (_session != null)
        {
            _session.SessionStatusChanged -=
                Session_SessionStatusChanged;

            _session.Dispose();

            _session = null;
        }

        SetClientDisconnected();

        // --------------------------------------------------------
        // Characteristic
        // --------------------------------------------------------

        if (_characteristic != null)
        {
            _characteristic.WriteRequested -=
                Characteristic_WriteRequested;
        }

        // --------------------------------------------------------
        // Advertising
        // --------------------------------------------------------

        _serviceProvider?.StopAdvertising();

        _characteristic = null;
        _serviceProvider = null;

        IsRunning = false;

        Stopped?.Invoke(
            this,
            EventArgs.Empty);
    }


    // ============================================================
    // Dispose
    // ============================================================

    public void Dispose()
    {
        Stop();
    }
}
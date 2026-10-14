namespace Wrist_Remote_for_Windows.Bluetooth;

public enum BluetoothCheckResult
{
    Ok,
    AdapterNotFound,
    BluetoothDisabled,
    LowEnergyNotSupported,
    PeripheralRoleNotSupported
}
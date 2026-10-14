namespace Wrist_Remote_for_Windows.Bluetooth;

public static class BluetoothCheckResultHelper
{
    public static string GetMessage(
        BluetoothCheckResult result)
    {
        switch (result)
        {
            case BluetoothCheckResult.AdapterNotFound:
                return "Bluetooth adapter not found.";

            case BluetoothCheckResult.BluetoothDisabled:
                return "Bluetooth is turned off.";

            case BluetoothCheckResult.LowEnergyNotSupported:
                return "Bluetooth Low Energy is not supported.";

            case BluetoothCheckResult.PeripheralRoleNotSupported:
                return "Bluetooth adapter does not support BLE Peripheral mode.";

            default:
                return "Bluetooth is ready.";
        }
    }
}
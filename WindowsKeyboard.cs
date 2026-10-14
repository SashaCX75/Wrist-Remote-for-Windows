using System;
using System.Runtime.InteropServices;

namespace Wrist_Remote_for_Windows;

public static class WindowsKeyboard
{
    private const uint INPUT_KEYBOARD = 1;

    private const uint KEYEVENTF_KEYUP = 0x0002;

    // Virtual-Key codes
    private const ushort VK_LWIN = 0x5B;
    private const ushort VK_L = 0x4C;
    private const ushort VK_D = 0x44;

    private const ushort VK_HOME = 0x24;
    private const ushort VK_END = 0x23;

    private const ushort VK_LEFT = 0x25;
    private const ushort VK_UP = 0x26;
    private const ushort VK_RIGHT = 0x27;
    private const ushort VK_DOWN = 0x28;

    private const ushort VK_RETURN = 0x0D;


    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public uint type;

        public InputUnion input;
    }


    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)]
        public MOUSEINPUT mi;

        [FieldOffset(0)]
        public KEYBDINPUT ki;

        [FieldOffset(0)]
        public HARDWAREINPUT hi;
    }


    [StructLayout(LayoutKind.Sequential)]
    private struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public nint dwExtraInfo;
    }


    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public nint dwExtraInfo;
    }


    [StructLayout(LayoutKind.Sequential)]
    private struct HARDWAREINPUT
    {
        public uint uMsg;
        public ushort wParamL;
        public ushort wParamH;
    }


    [DllImport(
        "user32.dll",
        SetLastError = true)]
    private static extern uint SendInput(
        uint nInputs,
        INPUT[] pInputs,
        int cbSize);


    // ---------------------------------------------------------
    // Windows + D
    // ---------------------------------------------------------

    public static void ShowDesktop()
    {
        HotKey(VK_LWIN, VK_D);
    }


    // ---------------------------------------------------------
    // Windows + L
    // ---------------------------------------------------------

    public static void LockWindows()
    {
        HotKey(VK_LWIN, VK_L);
    }


    // ---------------------------------------------------------
    // Одиночные клавиши
    // ---------------------------------------------------------

    public static void Home()
    {
        KeyPress(VK_HOME);
    }

    public static void End()
    {
        KeyPress(VK_END);
    }

    public static void Left()
    {
        KeyPress(VK_LEFT);
    }

    public static void Right()
    {
        KeyPress(VK_RIGHT);
    }

    public static void Up()
    {
        KeyPress(VK_UP);
    }

    public static void Down()
    {
        KeyPress(VK_DOWN);
    }

    public static void Enter()
    {
        KeyPress(VK_RETURN);
    }


    // ---------------------------------------------------------
    // Нажатие комбинации клавиш
    // ---------------------------------------------------------

    private static void HotKey(
        ushort modifier,
        ushort key)
    {
        SendKey(modifier, false);

        SendKey(key, false);
        SendKey(key, true);

        SendKey(modifier, true);
    }


    // ---------------------------------------------------------
    // Обычное нажатие клавиши:
    //
    // KEY DOWN
    // KEY UP
    // ---------------------------------------------------------

    private static void KeyPress(ushort key)
    {
        SendKey(key, false);
        SendKey(key, true);
    }


    // ---------------------------------------------------------
    // Отправка одного события клавиатуры
    // ---------------------------------------------------------

    private static void SendKey(
        ushort key,
        bool keyUp)
    {
        var input = new INPUT
        {
            type = INPUT_KEYBOARD,

            input = new InputUnion
            {
                ki = new KEYBDINPUT
                {
                    // Virtual-Key code
                    wVk = key,

                    // Scan code не используем.
                    wScan = 0,

                    // KEYEVENTF_KEYUP только для отпускания.
                    dwFlags = keyUp
                        ? KEYEVENTF_KEYUP
                        : 0,

                    time = 0,

                    dwExtraInfo = 0
                }
            }
        };


        uint result = SendInput(
            1,
            new[] { input },
            Marshal.SizeOf<INPUT>());


        if (result != 1)
        {
            int error = Marshal.GetLastWin32Error();

            System.Diagnostics.Debug.WriteLine(
                $"SendInput failed. Error={error}");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine(
                $"Keyboard event sent: VK=0x{key:X2}, " +
                $"KeyUp={keyUp}");
        }
    }
}
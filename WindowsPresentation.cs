using System;
using System.Runtime.InteropServices;

namespace Wrist_Remote_for_Windows;

public static class WindowsPresentation
{
    private const uint INPUT_KEYBOARD = 1;
    private const uint KEYEVENTF_KEYUP = 0x0002;

    private const ushort VK_LEFT = 0x25;
    private const ushort VK_RIGHT = 0x27;

    private const ushort VK_F5 = 0x74;
    private const ushort VK_ESCAPE = 0x1B;

    private const ushort VK_B = 0x42;
    private const ushort VK_W = 0x57;


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


    public static void NextSlide()
    {
        KeyPress(VK_RIGHT);
    }


    public static void PreviousSlide()
    {
        KeyPress(VK_LEFT);
    }


    public static void StartPresentation()
    {
        KeyPress(VK_F5);
    }


    public static void EndPresentation()
    {
        KeyPress(VK_ESCAPE);
    }


    public static void BlackScreen()
    {
        KeyPress(VK_B);
    }


    public static void WhiteScreen()
    {
        KeyPress(VK_W);
    }


    private static void KeyPress(ushort key)
    {
        SendKey(key, false);
        SendKey(key, true);
    }


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
                    wVk = key,
                    wScan = 0,

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
                $"Presentation SendInput failed. Error={error}");
        }
    }
}
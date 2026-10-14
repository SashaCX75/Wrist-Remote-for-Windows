using System;
using System.Runtime.InteropServices;

namespace Wrist_Remote_for_Windows;

public static class WindowsMedia
{
    private const uint INPUT_KEYBOARD = 1;
    private const uint KEYEVENTF_KEYUP = 0x0002;

    // Multimedia Virtual-Key codes

    // Play / Pause
    private const ushort VK_MEDIA_PLAY_PAUSE = 0xB3;

    // Следующий трек
    private const ushort VK_MEDIA_NEXT_TRACK = 0xB0;

    // Предыдущий трек
    private const ushort VK_MEDIA_PREV_TRACK = 0xB1;

    // Увеличение громкости
    private const ushort VK_VOLUME_UP = 0xAF;

    // Уменьшение громкости
    private const ushort VK_VOLUME_DOWN = 0xAE;

    // Выключение/включение звука
    private const ushort VK_VOLUME_MUTE = 0xAD;


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


    public static void PlayPause()
    {
        KeyPress(VK_MEDIA_PLAY_PAUSE);
    }


    public static void Next()
    {
        KeyPress(VK_MEDIA_NEXT_TRACK);
    }


    public static void Previous()
    {
        KeyPress(VK_MEDIA_PREV_TRACK);
    }


    public static void VolumeUp()
    {
        KeyPress(VK_VOLUME_UP);
    }


    public static void VolumeDown()
    {
        KeyPress(VK_VOLUME_DOWN);
    }


    public static void Mute()
    {
        KeyPress(VK_VOLUME_MUTE);
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
                $"Media SendInput failed. Error={error}");
        }
    }
}
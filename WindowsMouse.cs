using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Wrist_Remote_for_Windows;

public static class WindowsMouse
{
    private const uint INPUT_MOUSE = 0;

    private const uint MOUSEEVENTF_MOVE = 0x0001;

    private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    private const uint MOUSEEVENTF_LEFTUP = 0x0004;

    private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
    private const uint MOUSEEVENTF_RIGHTUP = 0x0010;

    private const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
    private const uint MOUSEEVENTF_MIDDLEUP = 0x0040;

    private const uint MOUSEEVENTF_WHEEL = 0x0800;

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public uint type;
        public MOUSEINPUT mi;
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

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(
        uint nInputs,
        INPUT[] pInputs,
        int cbSize);

    public static void Move(int x, int y)
    {
        if (x == 0 && y == 0)
            return;

        SendMouse(
            x,
            y,
            0,
            MOUSEEVENTF_MOVE);
    }

    public static void LeftDown()
    {
        SendMouse(
            0,
            0,
            0,
            MOUSEEVENTF_LEFTDOWN);
    }

    public static void LeftUp()
    {
        SendMouse(
            0,
            0,
            0,
            MOUSEEVENTF_LEFTUP);
    }

    public static void RightDown()
    {
        SendMouse(
            0,
            0,
            0,
            MOUSEEVENTF_RIGHTDOWN);
    }

    public static void RightUp()
    {
        SendMouse(
            0,
            0,
            0,
            MOUSEEVENTF_RIGHTUP);
    }

    public static void MiddleDown()
    {
        SendMouse(
            0,
            0,
            0,
            MOUSEEVENTF_MIDDLEDOWN);
    }

    public static void MiddleUp()
    {
        SendMouse(
            0,
            0,
            0,
            MOUSEEVENTF_MIDDLEUP);
    }

    public static async Task LeftClick(int delayMs = 50)
    {
        LeftDown();
        await Task.Delay(delayMs);
        LeftUp();
    }


    public static async Task RightClick(int delayMs = 50)
    {
        RightDown();
        await Task.Delay(delayMs);
        RightUp();
    }

    public static void Wheel(int delta)
    {
        if (delta == 0)
            return;

        SendMouse(
            0,
            0,
            unchecked((uint)delta),
            MOUSEEVENTF_WHEEL);
    }

    private static void SendMouse(
        int dx,
        int dy,
        uint mouseData,
        uint flags)
    {
        var input = new INPUT
        {
            type = INPUT_MOUSE,

            mi = new MOUSEINPUT
            {
                dx = dx,
                dy = dy,
                mouseData = mouseData,
                dwFlags = flags,
                time = 0,
                dwExtraInfo = 0
            }
        };

        SendInput(
            1,
            new[] { input },
            Marshal.SizeOf<INPUT>());
    }
}
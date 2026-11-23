//using System;
//using System.Diagnostics;
//using System.Runtime.InteropServices;

//namespace Wrist_Remote_for_Windows;

//public static class WindowsKeyboard
//{
//    private const uint INPUT_KEYBOARD = 1;

//    private const uint KEYEVENTF_KEYUP = 0x0002;

//    // Virtual-Key codes
//    private const ushort VK_LWIN = 0x5B;
//    private const ushort VK_L = 0x4C;
//    private const ushort VK_D = 0x44;

//    private const ushort VK_HOME = 0x24;
//    private const ushort VK_END = 0x23;

//    private const ushort VK_LEFT = 0x25;
//    private const ushort VK_UP = 0x26;
//    private const ushort VK_RIGHT = 0x27;
//    private const ushort VK_DOWN = 0x28;

//    private const ushort VK_RETURN = 0x0D;

//    private const ushort VK_MENU = 0x12;    // Alt
//    private const ushort VK_CONTROL = 0x11; // Ctrl
//    private const ushort VK_TAB = 0x09;     // Tab


//    [StructLayout(LayoutKind.Sequential)]
//    private struct INPUT
//    {
//        public uint type;

//        public InputUnion input;
//    }


//    [StructLayout(LayoutKind.Explicit)]
//    private struct InputUnion
//    {
//        [FieldOffset(0)]
//        public MOUSEINPUT mi;

//        [FieldOffset(0)]
//        public KEYBDINPUT ki;

//        [FieldOffset(0)]
//        public HARDWAREINPUT hi;
//    }


//    [StructLayout(LayoutKind.Sequential)]
//    private struct MOUSEINPUT
//    {
//        public int dx;
//        public int dy;
//        public uint mouseData;
//        public uint dwFlags;
//        public uint time;
//        public nint dwExtraInfo;
//    }


//    [StructLayout(LayoutKind.Sequential)]
//    private struct KEYBDINPUT
//    {
//        public ushort wVk;
//        public ushort wScan;
//        public uint dwFlags;
//        public uint time;
//        public nint dwExtraInfo;
//    }


//    [StructLayout(LayoutKind.Sequential)]
//    private struct HARDWAREINPUT
//    {
//        public uint uMsg;
//        public ushort wParamL;
//        public ushort wParamH;
//    }


//    [DllImport(
//        "user32.dll",
//        SetLastError = true)]
//    private static extern uint SendInput(
//        uint nInputs,
//        INPUT[] pInputs,
//        int cbSize);


//    // ---------------------------------------------------------
//    // Windows + D
//    // ---------------------------------------------------------

//    public static void ShowDesktop()
//    {
//        HotKey(VK_LWIN, VK_D);
//    }


//    // ---------------------------------------------------------
//    // Windows + L
//    // ---------------------------------------------------------

//    public static void LockWindows()
//    {
//        HotKey(VK_LWIN, VK_L);
//    }


//    // ---------------------------------------------------------
//    // Одиночные клавиши
//    // ---------------------------------------------------------

//    public static void Home()
//    {
//        KeyPress(VK_HOME);
//    }

//    public static void End()
//    {
//        KeyPress(VK_END);
//    }

//    public static void Left()
//    {
//        KeyPress(VK_LEFT);
//    }

//    public static void Right()
//    {
//        KeyPress(VK_RIGHT);
//    }

//    public static void Up()
//    {
//        KeyPress(VK_UP);
//    }

//    public static void Down()
//    {
//        KeyPress(VK_DOWN);
//    }

//    public static void Enter()
//    {
//        KeyPress(VK_RETURN);
//    }

//    // Alt + Home — домашняя страница браузера
//    public static void HomeWeb()
//    {
//        HotKey(VK_MENU, VK_HOME);
//    }

//    // Alt + Left — назад
//    public static void Back()
//    {
//        HotKey(VK_MENU, VK_LEFT);
//    }

//    // Alt + Right — вперёд
//    public static void Forward()
//    {
//        HotKey(VK_MENU, VK_RIGHT);
//    }

//    // Win + Tab — представление задач
//    public static void TaskView()
//    {
//        HotKey(VK_LWIN, VK_TAB);
//    }

//    // Открытие экранной клавиатуры Windows
//    public static void OpenVirtualKeyboard()
//    {
//        try
//        {
//            Process.Start(new ProcessStartInfo
//            {
//                FileName = "osk.exe",
//                UseShellExecute = true
//            });
//        }
//        catch (Exception ex)
//        {
//            System.Diagnostics.Debug.WriteLine(
//                $"Unable to open virtual keyboard: {ex.Message}");
//        }
//    }

//    // Нажатие и отпускание Ctrl для Zoom
//    public static void ControlDown()
//    {
//        SendKey(VK_CONTROL, false);
//    }

//    public static void ControlUp()
//    {
//        SendKey(VK_CONTROL, true);
//    }


//    // ---------------------------------------------------------
//    // Нажатие комбинации клавиш
//    // ---------------------------------------------------------

//    private static void HotKey(
//        ushort modifier,
//        ushort key)
//    {
//        SendKey(modifier, false);

//        SendKey(key, false);
//        SendKey(key, true);

//        SendKey(modifier, true);
//    }


//    // ---------------------------------------------------------
//    // Обычное нажатие клавиши:
//    //
//    // KEY DOWN
//    // KEY UP
//    // ---------------------------------------------------------

//    private static void KeyPress(ushort key)
//    {
//        SendKey(key, false);
//        SendKey(key, true);
//    }


//    // ---------------------------------------------------------
//    // Отправка одного события клавиатуры
//    // ---------------------------------------------------------

//    private static void SendKey(
//        ushort key,
//        bool keyUp)
//    {
//        var input = new INPUT
//        {
//            type = INPUT_KEYBOARD,

//            input = new InputUnion
//            {
//                ki = new KEYBDINPUT
//                {
//                    // Virtual-Key code
//                    wVk = key,

//                    // Scan code не используем.
//                    wScan = 0,

//                    // KEYEVENTF_KEYUP только для отпускания.
//                    dwFlags = keyUp
//                        ? KEYEVENTF_KEYUP
//                        : 0,

//                    time = 0,

//                    dwExtraInfo = 0
//                }
//            }
//        };


//        uint result = SendInput(
//            1,
//            new[] { input },
//            Marshal.SizeOf<INPUT>());


//        if (result != 1)
//        {
//            int error = Marshal.GetLastWin32Error();

//            System.Diagnostics.Debug.WriteLine(
//                $"SendInput failed. Error={error}");
//        }
//        else
//        {
//            System.Diagnostics.Debug.WriteLine(
//                $"Keyboard event sent: VK=0x{key:X2}, " +
//                $"KeyUp={keyUp}");
//        }
//    }
//}


using System;
using System.Diagnostics;
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

    private const ushort VK_MENU = 0x12;    // Alt
    private const ushort VK_CONTROL = 0x11; // Ctrl
    private const ushort VK_TAB = 0x09;     // Tab

    private const ushort VK_SHIFT = 0x10;
    private const ushort VK_F10 = 0x79;


    // ---------------------------------------------------------
    // INPUT structures
    // ---------------------------------------------------------

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


    // ---------------------------------------------------------
    // Windows API
    // ---------------------------------------------------------

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(
        uint nInputs,
        INPUT[] pInputs,
        int cbSize);

    // Нативная блокировка Windows
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool LockWorkStation();


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
        if (!LockWorkStation())
        {
            int error = Marshal.GetLastWin32Error();

            Debug.WriteLine(
                $"LockWorkStation failed. Error={error}");
        }
    }

    // Shift + F10 — контекстное меню
    public static void Menu()
    {
        HotKey(VK_SHIFT, VK_F10);
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
    // Сочетания клавиш
    // ---------------------------------------------------------

    // Alt + Home — домашняя страница браузера
    public static void HomeWeb()
    {
        HotKey(VK_MENU, VK_HOME);
    }

    // Alt + Left — назад
    public static void Back()
    {
        HotKey(VK_MENU, VK_LEFT);
    }

    // Alt + Right — вперёд
    public static void Forward()
    {
        HotKey(VK_MENU, VK_RIGHT);
    }

    // Win + Tab — представление задач
    public static void TaskView()
    {
        HotKey(VK_LWIN, VK_TAB);
    }


    // ---------------------------------------------------------
    // Экранная клавиатура Windows
    // ---------------------------------------------------------

    public static void OpenVirtualKeyboard()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "osk.exe",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine(
                $"Unable to open virtual keyboard: {ex.Message}");
        }
    }


    // ---------------------------------------------------------
    // Ctrl для Zoom
    // ---------------------------------------------------------

    public static void ControlDown()
    {
        SendKey(VK_CONTROL, false);
    }

    public static void ControlUp()
    {
        SendKey(VK_CONTROL, true);
    }


    // ---------------------------------------------------------
    // Нажатие комбинации клавиш
    //
    // Все события отправляются одним вызовом SendInput:
    //
    // Modifier DOWN
    // Key DOWN
    // Key UP
    // Modifier UP
    // ---------------------------------------------------------

    private static void HotKey(
        ushort modifier,
        ushort key)
    {
        INPUT[] inputs =
        {
            CreateKeyInput(modifier, false),
            CreateKeyInput(key, false),
            CreateKeyInput(key, true),
            CreateKeyInput(modifier, true)
        };

        SendKeyboardInputs(inputs);
    }


    // ---------------------------------------------------------
    // Обычное нажатие клавиши
    // ---------------------------------------------------------

    private static void KeyPress(ushort key)
    {
        INPUT[] inputs =
        {
            CreateKeyInput(key, false),
            CreateKeyInput(key, true)
        };

        SendKeyboardInputs(inputs);
    }


    // ---------------------------------------------------------
    // Создание события клавиатуры
    // ---------------------------------------------------------

    private static INPUT CreateKeyInput(
        ushort key,
        bool keyUp)
    {
        return new INPUT
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
    }


    // ---------------------------------------------------------
    // Отправка массива событий клавиатуры
    // ---------------------------------------------------------

    private static void SendKeyboardInputs(INPUT[] inputs)
    {
        uint result = SendInput(
            (uint)inputs.Length,
            inputs,
            Marshal.SizeOf<INPUT>());

        if (result != inputs.Length)
        {
            int error = Marshal.GetLastWin32Error();

            Debug.WriteLine(
                $"SendInput failed. " +
                $"Sent={result}/{inputs.Length}, Error={error}");
        }
        else
        {
            Debug.WriteLine(
                $"Keyboard events sent: {result}");
        }
    }


    // ---------------------------------------------------------
    // Отправка одного события клавиатуры
    // ---------------------------------------------------------

    private static void SendKey(
        ushort key,
        bool keyUp)
    {
        INPUT[] inputs =
        {
            CreateKeyInput(key, keyUp)
        };

        SendKeyboardInputs(inputs);
    }
}

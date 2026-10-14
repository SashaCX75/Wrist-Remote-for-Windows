using Wrist_Remote_for_Windows.Bluetooth;
using Wrist_Remote_for_Windows.Input;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Wrist_Remote_for_Windows
{
    public partial class MainWindow : Window
    {
        private readonly BluetoothMouseServer _server;
        private MouseButtons _previousMouseButtons;
        private bool _isDarkTheme = true;

        public MainWindow()
        {
            InitializeComponent();

            _isDarkTheme = Properties.Settings.Default.IsDarkTheme;
            ApplyTheme();

            _server = new BluetoothMouseServer();

            _server.Started += Server_Started;
            _server.Stopped += Server_Stopped;

            _server.ClientConnected += Server_ClientConnected;
            _server.ClientDisconnected += Server_ClientDisconnected;

            _server.CommandReceived += Server_CommandReceived;

            _server.BluetoothStateChanged += Server_BluetoothStateChanged;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RestoreWindowPosition();
            try
            {
                StatusText.Text = "Checking Bluetooth...";

                var result = await _server.CheckBluetoothAsync();

                if (result != BluetoothCheckResult.Ok)
                {
                    StatusText.Text = BluetoothCheckResultHelper.GetMessage(result);

                    StartButton.IsEnabled = false;
                    StopButton.IsEnabled = true;

                    return;
                }

                StatusText.Text = "Bluetooth ready";
                StartButton.IsEnabled = true;
                StopButton.IsEnabled = false;
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Bluetooth error: {ex.Message}";
            }
        }

        private void RestoreWindowPosition()
        {
            var settings = Properties.Settings.Default;

            var workArea = SystemParameters.WorkArea;

            // Восстанавливаем размер и положение окна.
            //Width = settings.WindowWidth;
            //Height = settings.WindowHeight;
            Left = settings.WindowLeft;
            Top = settings.WindowTop;

            // Не позволяем окну полностью оказаться за пределами экрана.

            if (Left + Width < workArea.Left + 50 ||
                Left > workArea.Right - 50)
            {
                Left = workArea.Left + 100;
            }

            if (Top + Height < workArea.Top + 50 ||
                Top > workArea.Bottom - 50)
            {
                Top = workArea.Top + 100;
            }
        }

        private void SaveWindowPosition()
        {
            // Не сохраняем свёрнутое или развёрнутое окно.
            if (WindowState != WindowState.Normal)
                return;

            var settings = Properties.Settings.Default;

            settings.WindowLeft = Left;
            settings.WindowTop = Top;
            //settings.WindowWidth = Width;
            //settings.WindowHeight = Height;

            settings.Save();
        }

        private void ThemeButton_Click(object sender, RoutedEventArgs e)
        {
            _isDarkTheme = !_isDarkTheme;
            ApplyTheme();

            Properties.Settings.Default.IsDarkTheme = _isDarkTheme;
            Properties.Settings.Default.Save();
        }



        private void ApplyTheme()
        {
            var themeUri = new Uri(
                _isDarkTheme
                    ? "Themes/DarkTheme.xaml"
                    : "Themes/LightTheme.xaml",
                UriKind.Relative);

            var theme = new ResourceDictionary
            {
                Source = themeUri
            };

            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(theme);

            ThemeSunIcon.Visibility = _isDarkTheme
                ? Visibility.Visible
                : Visibility.Collapsed;

            ThemeMoonIcon.Visibility = _isDarkTheme
                ? Visibility.Collapsed
                : Visibility.Visible;

            ThemeButton.ToolTip = _isDarkTheme
                ? "Switch to light theme"
                : "Switch to dark theme";
        }


        private void Server_ClientConnected(
        object? sender,
        EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                StatusText.Text = "Watch connected";
            });
        }


        private void Server_ClientDisconnected(
            object? sender,
            EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (_previousMouseButtons.HasFlag(
                    MouseButtons.Left))
                {
                    WindowsMouse.LeftUp();
                }

                if (_previousMouseButtons.HasFlag(
                    MouseButtons.Right))
                {
                    WindowsMouse.RightUp();
                }

                if (_previousMouseButtons.HasFlag(
                    MouseButtons.Middle))
                {
                    WindowsMouse.MiddleUp();
                }

                _previousMouseButtons =
                    MouseButtons.None;

                StatusText.Text = "Watch disconnected";
            });
        }


        private async void StartButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                Debug.WriteLine(
                    "StartButton_Click");


                StatusText.Text =
                    "Starting BLE server...";


                await _server.StartAsync();
            }
            catch (Exception ex)
            {
                StatusText.Text =
                    $"Error: {ex.Message}";
            }
        }


        private void StopButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            Debug.WriteLine(
                "StopButton_Click");


            _server.Stop();
        }


        private void Server_Started(
            object? sender,
            EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                StatusText.Text = "BLE server running";
                StartButton.IsEnabled = false;
                StopButton.IsEnabled = true;
            });
        }


        private void Server_Stopped(
            object? sender,
            EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                StatusText.Text = "BLE server stopped";
                StartButton.IsEnabled = true;
                StopButton.IsEnabled = false;
            });
        }


        private async void Server_CommandReceived(
            object? sender,
            InputCommand command)
        {
            await Dispatcher.InvokeAsync(async () =>
            {
                switch (command)
                {
                    case MouseInputCommand mouse:
                        HandleMouseCommand(mouse);
                        break;


                    case MediaInputCommand media:
                        HandleMediaCommand(media);
                        break;


                    case PresentationInputCommand presentation:
                        HandlePresentationCommand(presentation);
                        break;


                    case SystemInputCommand system:
                        await HandleSystemCommand(system);
                        break;
                }
            });
        }

        private async void Server_BluetoothStateChanged(
        object? sender,
        EventArgs e)
        {
            try
            {
                var result =
                    await _server.CheckBluetoothAsync();

                Dispatcher.Invoke(() =>
                {
                    if (result != BluetoothCheckResult.Ok)
                    {
                        StatusText.Text =
                            BluetoothCheckResultHelper.GetMessage(
                                result);

                        StartButton.IsEnabled = false;
                        StopButton.IsEnabled = true;

                        return;
                    }

                    StatusText.Text = "Bluetooth ready";
                    StartButton.IsEnabled = true;
                    StopButton.IsEnabled = false;
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    StatusText.Text =
                        $"Bluetooth error: {ex.Message}";
                });
            }
        }

        private void HandleMouseCommand(
            MouseInputCommand command)
        {
            // Движение
            if (command.Action == MouseAction.Move)
            {
                if (command.X != 0 || command.Y != 0)
                {
                    WindowsMouse.Move(
                        command.X,
                        command.Y);
                }
            }


            // Колесо
            if (command.Action == MouseAction.Wheel)
            {
                if (command.Wheel != 0)
                {
                    WindowsMouse.Wheel(
                        command.Wheel * 120);
                }
            }


            // Кнопки мыши

            MouseButtons currentButtons =
                command.Buttons;

            MouseButtons previousButtons =
                _previousMouseButtons;


            // Левая кнопка
            bool leftNow =
                currentButtons.HasFlag(
                    MouseButtons.Left);

            bool leftBefore =
                previousButtons.HasFlag(
                    MouseButtons.Left);


            if (leftNow && !leftBefore)
                WindowsMouse.LeftDown();

            if (!leftNow && leftBefore)
                WindowsMouse.LeftUp();


            // Правая кнопка
            bool rightNow =
                currentButtons.HasFlag(
                    MouseButtons.Right);

            bool rightBefore =
                previousButtons.HasFlag(
                    MouseButtons.Right);


            if (rightNow && !rightBefore)
                WindowsMouse.RightDown();

            if (!rightNow && rightBefore)
                WindowsMouse.RightUp();


            // Средняя кнопка
            bool middleNow =
                currentButtons.HasFlag(
                    MouseButtons.Middle);

            bool middleBefore =
                previousButtons.HasFlag(
                    MouseButtons.Middle);


            if (middleNow && !middleBefore)
                WindowsMouse.MiddleDown();

            if (!middleNow && middleBefore)
                WindowsMouse.MiddleUp();


            _previousMouseButtons =
                currentButtons;
        }


        private void HandleMediaCommand(
            MediaInputCommand command)
        {
            switch (command.Command)
            {
                case MediaCommand.PlayPause:
                    Debug.WriteLine("Media: Play/Pause");
                    WindowsMedia.PlayPause();
                    break;


                case MediaCommand.Next:
                    Debug.WriteLine("Media: Next");
                    WindowsMedia.Next();
                    break;


                case MediaCommand.Previous:
                    Debug.WriteLine("Media: Previous");
                    WindowsMedia.Previous();
                    break;


                case MediaCommand.VolumeUp:
                    Debug.WriteLine("Media: Volume Up");
                    WindowsMedia.VolumeUp();
                    break;


                case MediaCommand.VolumeDown:
                    Debug.WriteLine("Media: Volume Down");
                    WindowsMedia.VolumeDown();
                    break;


                case MediaCommand.Mute:
                    Debug.WriteLine("Media: Mute");
                    WindowsMedia.Mute();
                    break;
            }
        }


        private void HandlePresentationCommand(
            PresentationInputCommand command)
        {
            switch (command.Command)
            {
                case PresentationCommand.NextSlide:
                    Debug.WriteLine("Presentation: Next Slide");
                    WindowsPresentation.NextSlide();
                    break;


                case PresentationCommand.PreviousSlide:
                    Debug.WriteLine("Presentation: Previous Slide");
                    WindowsPresentation.PreviousSlide();
                    break;


                case PresentationCommand.StartPresentation:
                    Debug.WriteLine("Presentation: Start");
                    WindowsPresentation.StartPresentation();
                    break;


                case PresentationCommand.EndPresentation:
                    Debug.WriteLine("Presentation: End");
                    WindowsPresentation.EndPresentation();
                    break;


                case PresentationCommand.BlackScreen:
                    Debug.WriteLine("Presentation: Black Screen");
                    WindowsPresentation.BlackScreen();
                    break;


                case PresentationCommand.WhiteScreen:
                    Debug.WriteLine("Presentation: White Screen");
                    WindowsPresentation.WhiteScreen();
                    break;
            }
        }


        private async Task HandleSystemCommand(SystemInputCommand command)
        {
            switch (command.Command)
            {
                case SystemCommand.ShowDesktop:
                    Debug.WriteLine("System: Show Desktop");
                    WindowsKeyboard.ShowDesktop();
                    break;

                case SystemCommand.LockWindows:
                    Debug.WriteLine("System: Lock Windows");
                    WindowsKeyboard.LockWindows();
                    break;

                case SystemCommand.CursorHome:
                    Debug.WriteLine("System: Cursor Home");
                    WindowsKeyboard.Home();
                    break;


                case SystemCommand.CursorEnd:
                    Debug.WriteLine("System: Cursor End");
                    WindowsKeyboard.End();
                    break;

                case SystemCommand.CursorLeft:
                    Debug.WriteLine("System: Cursor Left");
                    WindowsKeyboard.Left();
                    break;


                case SystemCommand.CursorRight:
                    Debug.WriteLine("System: Cursor Right");
                    WindowsKeyboard.Right();
                    break;


                case SystemCommand.CursorUp:
                    Debug.WriteLine("System: Cursor Up");
                    WindowsKeyboard.Up();
                    break;


                case SystemCommand.CursorDown:
                    Debug.WriteLine("System: Cursor Down");
                    WindowsKeyboard.Down();
                    break;

                case SystemCommand.Enter:
                    Debug.WriteLine("System: Enter");
                    WindowsKeyboard.Enter();
                    break;


                case SystemCommand.LeftClick:
                    Debug.WriteLine("System: Mouse Left Click");
                    await WindowsMouse.LeftClick();
                    break;


                case SystemCommand.RightClick:
                    Debug.WriteLine("System: Mouse Right Click");
                    await WindowsMouse.RightClick();
                    break;
            }
        }


        protected override void OnClosed(EventArgs e)
        {
            SaveWindowPosition();
            _server.Dispose();
            base.OnClosed(e);
        }
    }
}
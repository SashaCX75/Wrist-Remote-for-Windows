# Wrist Remote for Windows

[English](#english) | [Русский](#русский)

## English

A Windows app that turns an Amazfit smartwatch (Zepp OS) into a wireless remote for your PC. The watch connects to the PC over Bluetooth Low Energy, and the app turns its commands into regular mouse and keyboard input.

It works together with the Wrist Remote app on the watch; this program is the Windows side.

### What it does

- **Mouse.** Move the cursor from the watch, use the left, right and middle buttons, drag, and scroll with the wheel.
- **Media.** Play/pause, next and previous track, volume, mute.
- **Presentations.** Next and previous slide, start the slideshow (F5) and end it (Esc), black and white screen (B / W). Works in PowerPoint and other apps that use the same keys.
- **System commands.** Show the desktop, lock the PC, arrow keys, Home, End, Enter, mouse clicks.
- **Quick actions.** Browser back, forward and home page, on-screen keyboard, zoom (Ctrl + wheel), double click, Task View (Win + Tab), context menu.

### Bluetooth requirements

The PC acts as the BLE device that the watch connects to. So the adapter must be able not only to connect to other devices but also to accept connections itself. You need:

1. **A Bluetooth adapter** in the PC, built-in or USB.
2. **Bluetooth turned on** in Windows.
3. **Bluetooth Low Energy support** in the adapter (Bluetooth 4.0 or later).
4. **BLE Peripheral role support** in the adapter. This is the key requirement: many adapters support BLE but not this role. Support depends on both the adapter and its driver, so updating the driver sometimes helps.

You don't need to pair the watch with the PC in Windows settings: the watch connects to the app directly.

The app checks all four conditions at startup and shows the result in its window:

| Message | What it means |
|---|---|
| `Bluetooth ready` | Everything is fine, you can start the server |
| `Bluetooth adapter not found.` | No adapter was found, or Windows doesn't detect it |
| `Bluetooth is turned off.` | Bluetooth is turned off in Windows |
| `Bluetooth Low Energy is not supported.` | The adapter doesn't support BLE |
| `Bluetooth adapter does not support BLE Peripheral mode.` | The adapter doesn't support the Peripheral role, so the app can't work with it |

### System requirements

- Windows 10 version 2004 (build 19041) or later, or Windows 11.
- [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0).
- A Bluetooth adapter with BLE Peripheral support (see above).

### How to use

1. Start the app and wait for the `Bluetooth ready` status.
2. Click **Start Bluetooth Server**: the PC becomes visible to the watch.
3. Open the app on the watch. Once it connects, the status changes to `Watch connected`.
4. To stop receiving commands, click **Stop Bluetooth Server**.

The app remembers its window position and theme (dark or light).

### Limitations

Commands are sent as regular mouse and keyboard input. Windows doesn't deliver such input to windows running as administrator unless the app itself is also running as administrator.

### Building from source

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```powershell
dotnet build "Wrist Remote for Windows.sln"
```

```powershell
dotnet run --project "Wrist Remote for Windows.csproj"
```

### License

[MIT](LICENSE.txt)

## Русский

Программа для Windows, которая превращает умные часы Amazfit (Zepp OS) в беспроводной пульт для компьютера. Часы подключаются к ПК по Bluetooth Low Energy, а программа превращает их команды в обычные нажатия мыши и клавиатуры.

Для работы нужно приложение Wrist Remote на часах. Эта программа — серверная часть для Windows.

### Для чего она нужна

- **Мышь.** Управление курсором с часов, левая, правая и средняя кнопки, перетаскивание, прокрутка колесом.
- **Медиа.** Воспроизведение и пауза, следующий и предыдущий трек, громкость, выключение звука.
- **Презентации.** Следующий и предыдущий слайд, запуск показа (F5) и выход из него (Esc), чёрный и белый экран (B / W). Работает в PowerPoint и других программах с такими же клавишами.
- **Системные команды.** Свернуть все окна, заблокировать компьютер, стрелки, Home, End, Enter, клики мышью.
- **Быстрые действия.** Назад и вперёд в браузере, домашняя страница, экранная клавиатура, масштаб (Ctrl + колесо), двойной клик, представление задач (Win + Tab), контекстное меню.

### Требования к Bluetooth

ПК работает как BLE-устройство, к которому подключаются часы. Поэтому адаптер должен уметь не только подключаться к другим устройствам, но и сам принимать подключения. Нужно, чтобы:

1. **В компьютере был Bluetooth-адаптер:** встроенный или USB.
2. **Bluetooth был включён** в Windows.
3. **Адаптер поддерживал Bluetooth Low Energy** (Bluetooth 4.0 и новее).
4. **Адаптер поддерживал режим BLE Peripheral** (роль периферийного устройства). Это главное требование: многие адаптеры поддерживают BLE, но не этот режим. Поддержка зависит и от самого адаптера, и от его драйвера, поэтому иногда помогает обновление драйвера.

Сопряжение часов с компьютером в настройках Windows не нужно: часы подключаются к программе напрямую.

Программа проверяет все четыре условия при запуске и показывает результат в окне:

| Сообщение | Что это значит |
|---|---|
| `Bluetooth ready` | Всё в порядке, можно запускать сервер |
| `Bluetooth adapter not found.` | Адаптер не найден или не определился в системе |
| `Bluetooth is turned off.` | Bluetooth выключен в Windows |
| `Bluetooth Low Energy is not supported.` | Адаптер не поддерживает BLE |
| `Bluetooth adapter does not support BLE Peripheral mode.` | Адаптер не поддерживает режим Peripheral, с ним программа работать не будет |

### Системные требования

- Windows 10 версии 2004 (сборка 19041) или новее, либо Windows 11.
- [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0).
- Bluetooth-адаптер с поддержкой BLE Peripheral (см. выше).

### Как пользоваться

1. Запустите программу и дождитесь статуса `Bluetooth ready`.
2. Нажмите **Start Bluetooth Server**: компьютер станет виден для часов.
3. Откройте приложение на часах. После подключения статус сменится на `Watch connected`.
4. Чтобы прекратить приём команд, нажмите **Stop Bluetooth Server**.

Программа запоминает положение окна и тему оформления (тёмная или светлая).

### Ограничения

Команды передаются как обычный ввод с мыши и клавиатуры. Windows не пропускает такой ввод в окна, запущенные от имени администратора, если сама программа запущена без этих прав.

### Сборка из исходников

Нужен [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```powershell
dotnet build "Wrist Remote for Windows.sln"
```

```powershell
dotnet run --project "Wrist Remote for Windows.csproj"
```

### Лицензия

[MIT](LICENSE.txt)

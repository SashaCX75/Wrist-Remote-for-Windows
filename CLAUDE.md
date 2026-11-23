# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

A WPF desktop app (window title "Amazfit Mouse") that turns a smartwatch into a remote control for Windows. The PC acts as a **BLE GATT peripheral**; the watch app (not in this repo) connects and writes small binary packets, which are translated into injected mouse/keyboard input via Win32 `SendInput`.

## Commands

```powershell
dotnet build "Wrist Remote for Windows.sln"
```

```powershell
dotnet run --project "Wrist Remote for Windows.csproj"
```

- Target is `net8.0-windows10.0.19041.0` — the Windows 10 SDK TFM is required for the WinRT Bluetooth APIs (`Windows.Devices.Bluetooth.*`, `Windows.Devices.Radios`); don't downgrade it to plain `net8.0-windows`.
- There are no tests and no linter configured.
- Running the app needs a Bluetooth adapter that supports the BLE **peripheral role**; otherwise Start stays disabled and the status shows the reason (`BluetoothCheckResult`).
- Received commands inject real input into the dev machine's desktop. Diagnostics go only to `Debug.WriteLine` (VS Output window); there is no logging framework.

## Architecture

Data flow: **watch → `BluetoothMouseServer` → `InputCommand` → `MainWindow` → static `Windows*` helpers → `SendInput`**.

- `BluetoothMouseServer.cs` — owns the GATT service provider. Advertises one service with one write-only characteristic (`Write | WriteWithoutResponse`, plain protection). The service/characteristic UUIDs are hard-coded and must match the watch app; changing them breaks the client. `Characteristic_WriteRequested` reads byte 0 as `InputCommandType`, dispatches to a `Read*Command` parser, and raises `CommandReceived`. Client connect/disconnect is inferred from the `GattSession` attached to write requests (there is no explicit connect event), and radio on/off changes raise `BluetoothStateChanged`.
- `InputCommand.cs` — the wire protocol: command-type enums and one `InputCommand` subclass per type. Packet layouts are documented in comments there.
- `MainWindow.xaml.cs` — wires server events to the UI and pattern-matches `InputCommand` subclasses to the `Handle*Command` methods. Server events arrive off the UI thread, so all handling goes through `Dispatcher`.
- `WindowsMouse.cs`, `WindowsKeyboard.cs`, `WindowsMedia.cs`, `WindowsPresentation.cs` — static P/Invoke wrappers. Each declares its own private copy of the `INPUT` structs (`WindowsMouse` uses a mouse-only `INPUT` with no union). `WindowsKeyboard` sends hotkeys as one batched `SendInput` call; `LockWindows` uses `LockWorkStation()` rather than Win+L. The top of `WindowsKeyboard.cs` is an old, fully commented-out version of the class — the live implementation is below it.

### Wire protocol

- Byte 0 is the type: Mouse `0x01`, Media `0x02`, Presentation `0x03`, System `0x04`, Shortcut `0x05`.
- **Mouse** (7 bytes): `[type][buttons bitmask][action][int16 LE][int16 LE]`. Action `Move` uses the two int16s as relative X/Y; `Wheel` uses the first as notches (multiplied by 120 in `MainWindow`). Buttons are *held state*, not clicks: `MainWindow` diffs against `_previousMouseButtons` to emit down/up events, and releases any held buttons on client disconnect.
- **All other types** (4 bytes): `[type][command][value: sbyte][flags]`. `Value` and `Flags` are parsed but currently unused by the handlers.
- Only the Shortcut parser rejects undefined command values (`Enum.IsDefined`); the others pass unknown values through and the handler `switch` ignores them.

### Adding a command

A new command touches four places: the enum (and, for a new type, a class) in `InputCommand.cs`; the parser `switch` / `Read*Command` in `BluetoothMouseServer.cs`; the `Handle*Command` switch in `MainWindow.xaml.cs`; and the implementation in the matching `Windows*` helper. The watch app must also be updated to send it.

## UI, themes, settings

- Themes: `Themes/DarkTheme.xaml` and `Themes/LightTheme.xaml` are swapped into `MainWindow.Resources.MergedDictionaries` at runtime by `ApplyTheme()`. XAML references brushes via `DynamicResource`, so any new brush key must be added to **both** theme files.
- User settings (`IsDarkTheme`, `WindowLeft`, `WindowTop`) live in `Properties/Settings.settings`, with generated `Properties/Settings.Designer.cs` and defaults mirrored in `App.config`. When adding a setting by hand, keep all three in sync.

## Conventions

- Code comments are in Russian; user-facing UI strings are in English.
- `Nullable` and `ImplicitUsings` are enabled. `Bluetooth` and `Input` code uses file-scoped namespaces (`Wrist_Remote_for_Windows.Bluetooth`, `Wrist_Remote_for_Windows.Input`); the `Windows*` helpers are in the root `Wrist_Remote_for_Windows` namespace.

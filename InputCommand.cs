using System;

namespace Wrist_Remote_for_Windows.Input;

public enum InputCommandType : byte
{
    Mouse = 0x01,
    Media = 0x02,
    Presentation = 0x03,
    System = 0x04
}


//
// Mouse
//
public enum MouseAction : byte
{
    Move = 0x01,
    Wheel = 0x02
}


[Flags]
public enum MouseButtons : byte
{
    None = 0x00,
    Left = 0x01,
    Right = 0x02,
    Middle = 0x04
}


//
// Media
//
public enum MediaCommand : byte
{
    PlayPause = 0x01,
    Next = 0x02,
    Previous = 0x03,

    VolumeUp = 0x04,
    VolumeDown = 0x05,

    Mute = 0x06
}


//
// PowerPoint
//
public enum PresentationCommand : byte
{
    NextSlide = 0x01,
    PreviousSlide = 0x02,

    StartPresentation = 0x03,
    EndPresentation = 0x04,

    BlackScreen = 0x05,
    WhiteScreen = 0x06
}


//
// System + Cursor
//
public enum SystemCommand : byte
{
    ShowDesktop = 0x01,
    LockWindows = 0x02,

    CursorHome = 0x03,
    CursorEnd = 0x04,

    CursorLeft = 0x05,
    CursorRight = 0x06,
    CursorUp = 0x07,
    CursorDown = 0x08,

    LeftClick = 0x09,
    RightClick = 0x0A,

    Enter = 0x0B
}


//
// Base command
//
public abstract class InputCommand
{
    public InputCommandType Type { get; }

    protected InputCommand(InputCommandType type)
    {
        Type = type;
    }
}


//
// Mouse command
//
// Packet:
//
// [Type]
// [Buttons]
// [Action]
// [X low]
// [X high]
// [Y low]
// [Y high]
//
// Total: 7 bytes
//
public sealed class MouseInputCommand : InputCommand
{
    public MouseButtons Buttons { get; }

    public MouseAction Action { get; }

    public short X { get; }

    public short Y { get; }

    public short Wheel { get; }


    public MouseInputCommand(
        MouseButtons buttons,
        MouseAction action,
        short x,
        short y,
        short wheel)
        : base(InputCommandType.Mouse)
    {
        Buttons = buttons;
        Action = action;
        X = x;
        Y = y;
        Wheel = wheel;
    }
}


//
// Media command
//
// Packet:
//
// [Type][Command][Value][Flags]
//
public sealed class MediaInputCommand : InputCommand
{
    public MediaCommand Command { get; }

    public sbyte Value { get; }

    public byte Flags { get; }


    public MediaInputCommand(
        MediaCommand command,
        sbyte value,
        byte flags)
        : base(InputCommandType.Media)
    {
        Command = command;
        Value = value;
        Flags = flags;
    }
}


//
// PowerPoint presentation command
//
// Packet:
//
// [Type][Command][Value][Flags]
//
public sealed class PresentationInputCommand : InputCommand
{
    public PresentationCommand Command { get; }

    public sbyte Value { get; }

    public byte Flags { get; }


    public PresentationInputCommand(
        PresentationCommand command,
        sbyte value,
        byte flags)
        : base(InputCommandType.Presentation)
    {
        Command = command;
        Value = value;
        Flags = flags;
    }
}


//
// System + Cursor command
//
// Packet:
//
// [Type][Command][Value][Flags]
//
public sealed class SystemInputCommand : InputCommand
{
    public SystemCommand Command { get; }

    public sbyte Value { get; }

    public byte Flags { get; }


    public SystemInputCommand(
        SystemCommand command,
        sbyte value,
        byte flags)
        : base(InputCommandType.System)
    {
        Command = command;
        Value = value;
        Flags = flags;
    }
}
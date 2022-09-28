using Godot;
using System;

public class Events : Node
{
    [Signal]
    public delegate void MapToggled();
    [Signal]
    public delegate void OpenMap();

    [Signal]
    public delegate void OnJumpPressedSignal();
    [Signal]
    public delegate void OnBuildPressedSignal();
    [Signal]
    public delegate void OnBuildClosePressedSignal();
}

using Godot;
using System;

//TODO Fix EVENTS when repaired in godot
public partial class Events : Node
{
	[Signal]
	public delegate void MapToggledEventHandler(bool show, float duration);

	[Signal]
	public delegate void OpenMapEventHandler();

	[Signal]
	public delegate void OnJumpPressedSignalEventHandler();

	[Signal]
	public delegate void OnBuildPressedSignalEventHandler();

	[Signal]
	public delegate void OnBuildClosePressedSignalEventHandler();

	[Signal]
	public delegate void OnPlatformPlacedEventHandler();
}

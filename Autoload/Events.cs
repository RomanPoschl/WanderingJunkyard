using Godot;
using System;

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

	[Signal]
	public delegate void OnEnemyTargetedEventHandler(Node2D enemy);
}

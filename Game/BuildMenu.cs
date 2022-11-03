using Godot;
using System;
using System.Collections.Generic;

public partial class BuildMenu : PopupPanel
{

	HBoxContainer _buildItemsContainer;
	Events _events;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_events = GetNode<Events>(Constants.Events);
		
		_events.Connect(nameof(Events.OnBuildPressedSignal),new Callable(this,nameof(OnBuildPressed)));
		
		_events.Connect(nameof(Events.OnPlatformPlaced),new Callable(this,nameof(PlatformPlaced)));

		_buildItemsContainer = GetNode<HBoxContainer>(Constants.UIConstants.BuildItemsContainer);
	}

	void OnBuildPressed()
	{
		GD.Print("BUILD");

		FillBuildMenu();

		Popup();
	}

	void FillBuildMenu()
	{
		var childs = _buildItemsContainer.GetChildren();
		foreach (var item in childs)
		{
			((Node)item).QueueFree();
		}

		AddBuildItem("res://Game/Ships/AttackShips/MachineGun.tscn");
	}

	void AddBuildItem(string path)
	{
		var platformPreLoad = ResourceLoader.Load<PackedScene>(path);

		//Platform for button
		var platform = platformPreLoad.Instantiate<ShipBoidBase>();
		platform.WeaponState = ShipBoidBase.SnapableComponentState.idle;
		platform.Position = new Vector2(34, 34);
		platform.GetNode<Node2D>("Node2D").Scale = new Vector2(0.2f, 0.2f); // Scale down for display in build menu

		//Button
		var btn = ResourceLoader.Load<PackedScene>("res://Game/Button.tscn").Instantiate<Button>();
		btn.AddChild(platform);
		var arr = new Godot.Collections.Array();
		arr.Add(platformPreLoad);
		arr.Add(btn);

		//btn.Connect("pressed",new Callable(this, nameof(OnItemPressed)));
		btn.Pressed += () => OnItemPressed(platformPreLoad, btn);

		btn.Size = btn.CustomMinimumSize = new Vector2i(68, 68);
		btn.OffsetRight = 68;
		btn.OffsetBottom = 68;
		btn.ClipContents = true;

		//Place button
		_buildItemsContainer.AddChild(btn);
	}

	void OnItemPressed(PackedScene platform, Button btn)
	{
		GD.Print($"PRESSED {platform.ResourceName}");

		var node = GetNode<Node>("/root/Game");
		var player = node.GetNode<Node2D>("Player");
		var platformInstance = platform.Instantiate<ShipBoidBase>();
		platformInstance.WeaponState = ShipBoidBase.SnapableComponentState.grabbing;
		platformInstance.GlobalPosition = player.GetGlobalMousePosition();
		
		node.AddChild(platformInstance);

		Hide();
		
		
	}

	void PlatformPlaced()
	{
		OnBuildPressed();
	}

	void OnBuildClosePressed()
	{
		GD.Print("HIDE BUILD");
		Hide();

		var childs = _buildItemsContainer.GetChildren();
		foreach (var item in childs)
		{
			((Node)item).QueueFree();
		}
		
		_events.EmitSignal(nameof(Events.OnBuildClosePressedSignal));
	}
}

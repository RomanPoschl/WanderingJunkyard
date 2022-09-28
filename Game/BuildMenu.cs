using Godot;
using System;
using System.Collections.Generic;

public class BuildMenu : PopupPanel
{

    HBoxContainer _buildItemsContainer;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        var events = GetNode<Events>(Constants.Events);
        events.Connect(nameof(Events.OnBuildPressedSignal), this, nameof(OnBuildPressed));

        _buildItemsContainer = GetNode<HBoxContainer>(Constants.UIConstants.BuildItemsContainer);
    }

    void OnBuildPressed()
    {
        GD.Print("BUILD");

        FillBuildMenu();

        Popup_();
    }

    void FillBuildMenu()
    {
        var gun1 = ResourceLoader.Load<PackedScene>("res://Game/Platforms/Weapon/MachineGun.tscn").Instance<SnapableComponent>();
        gun1.WeaponState = SnapableComponent.SnapableComponentState.idle;
        gun1.Position = new Vector2(34, 34);
        gun1.GetNode<Node2D>("Node2D").Scale = new Vector2(0.2f, 0.2f);

        var btn1 = ResourceLoader.Load<PackedScene>("res://Game/Button.tscn").Instance<Button>();
        btn1.AddChild(gun1);
        btn1.Connect("pressed", this, nameof(OnItemPressed), new Godot.Collections.Array(new List<string>() { "1" }));
        btn1.RectSize = btn1.RectMinSize = new Vector2(68, 68);
        btn1.MarginRight = 68;
        btn1.MarginBottom = 68;
        btn1.RectClipContent = true;

        _buildItemsContainer.AddChild(btn1);

        var gun = ResourceLoader.Load<PackedScene>("res://Game/Platforms/Weapon/MachineGun.tscn").Instance<SnapableComponent>();
        gun.WeaponState = SnapableComponent.SnapableComponentState.idle;
        gun.Position = new Vector2(34, 34);
        gun.GetNode<Node2D>("Node2D").Scale = new Vector2(0.2f, 0.2f);

        var btn = ResourceLoader.Load<PackedScene>("res://Game/Button.tscn").Instance<Button>();
        btn.AddChild(gun);
        btn.Connect("pressed", this, nameof(OnItemPressed), new Godot.Collections.Array(new List<string>() { "2" }));
        btn.RectSize = btn.RectMinSize = new Vector2(68, 68);
        btn.MarginRight = 68;
        btn.MarginBottom = 68;
        btn.RectClipContent = true;

        _buildItemsContainer.AddChild(btn);
    }

    void OnItemPressed(string number)
    {
        Console.WriteLine($"PRESSED {number}");
        GD.Print($"PRESSED {number}");
    }

    void OnBuildClosePressed()
    {
        GD.Print("HIDE BUILD");
        Hide();
        EmitSignal(nameof(Events.OnBuildClosePressedSignal));
    }
}

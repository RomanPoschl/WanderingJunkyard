using Godot;
using System;
using System.Collections.Generic;

public class UniverseMap : Node2D
{
    Universe _universe;
    PackedScene _preloadedCell;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _preloadedCell = (PackedScene)ResourceLoader.Load("res://Game/Universe/MapCell.tscn");  
    }

    public void Display(List<MapCell> map)
    { 
        foreach(var cell in map)
        {
            var scene = _preloadedCell.Instance<MapCell>();
            scene.HexPosition = cell.HexPosition;
            scene.NoiseSeed = cell.NoiseSeed;

            AddChild(scene);

            scene.InitShape();
        }
    }

    int _ongoingDrag = -1;

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        if(@event is InputEventScreenDrag drag)
        {
            GD.Print("drag");
        }       
        else if (@event is InputEventScreenTouch touch && @event.IsPressed())
        {
            GD.Print("touch");
        }

        if(@event is InputEventScreenTouch touchStop && !@event.IsPressed() && touchStop.Index == _ongoingDrag)
        {
            _ongoingDrag = -1;
        }
    }
}

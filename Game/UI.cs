using Godot;
using System;

public class UI : CanvasLayer
{
    
    MapDisplay _map;
    Events _events;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _map = GetNode<MapDisplay>("MapDisplay");
        _events = GetNode<Events>("/root/Events");

        _events.Connect("OpenMap", this, nameof(ToggleMap));
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);

        if(@event.IsActionPressed("OpenMap") && !_map.IsAnimating)
        {
            _map.Toggle();
        }
    }

    void ToggleMap()
    {
        _map.Toggle();
    }
}

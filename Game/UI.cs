using Godot;
using System;

public partial class UI : CanvasLayer
{
    
    MapDisplay _map;
    Events _events;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _map = GetNode<MapDisplay>(Constants.UIConstants.MapDisplay);
        _events = GetNode<Events>(Constants.Events);

        _events.Connect(nameof(Events.OpenMap),new Callable(this,nameof(ToggleMap)));
        //_events.OpenMapEvent += ToggleMap;
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

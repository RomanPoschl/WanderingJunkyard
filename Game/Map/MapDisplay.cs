using Godot;
using System;

public partial class MapDisplay : TextureRect
{
    AnimationPlayer _ap;
    Events _events;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _ap = GetNode<AnimationPlayer>("AnimationPlayer");
        _events = GetNode<Events>("/root/Events");
    }

    public void Toggle()
    {
        if (Visible)
        {
            _ap.Play("disappear");
        }
        else
        {
            _ap.Play("appear");
        }

        _events.EmitSignal("MapToggled", Visible, _ap.CurrentAnimationLength);
    }

    public bool IsAnimating => _ap.IsPlaying();
}

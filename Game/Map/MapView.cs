using Godot;
using System;

public class MapView : ViewportContainer
{
    Viewport _viewport;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _viewport = GetNode<Viewport>("Viewport");
    }

    public void RegisterCamera(PlayerCamera2D camera)
    {
        _viewport.AddChild(camera);
    }
}

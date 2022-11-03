using Godot;
using System;

public partial class MapView : SubViewportContainer
{
    SubViewport _viewport;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _viewport = GetNode<SubViewport>("SubViewport");
    }

    public void RegisterCamera(PlayerCamera2D camera)
    {
        _viewport.AddChild(camera);
    }
}

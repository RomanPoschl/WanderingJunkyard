using Godot;
using System;

public partial class Mob : RigidBody2D
{
    public override void _Ready()
    {
        var animSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        animSprite.Playing = true;
        string[] mobTypes = animSprite.Frames.GetAnimationNames();
        animSprite.Animation = mobTypes[GD.Randi() % mobTypes.Length];
    }
    
    private void _on_VisibilityNotifier2D_screen_exited()
    {
        QueueFree();
    }
}

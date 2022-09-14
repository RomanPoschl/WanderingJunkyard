using Godot;
using System;

public class Engine : Sprite
{
    CPUParticles2D _particles;
    public override void _Ready()
    {
        _particles = GetNode<CPUParticles2D>("Particles2D");
    }

    public void OnPlayerEnginesOn(bool on)
    {
        _particles.Emitting = on;
    }
}

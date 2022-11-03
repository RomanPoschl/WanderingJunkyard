using Godot;
using System;

public partial class Engine : Sprite2D
{
    CPUParticles2D _particles;
    public override void _Ready()
    {
        _particles = GetNode<CPUParticles2D>("GPUParticles2D");
    }

    public void OnPlayerEnginesOn(bool on)
    {
        _particles.Emitting = on;
    }
}

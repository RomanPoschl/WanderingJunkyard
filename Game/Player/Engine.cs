using Godot;
using System;

public partial class Engine : Sprite3D
{
    CPUParticles3D _particles;
    public override void _Ready()
    {
        _particles = GetNode<CPUParticles3D>("GPUParticles2D");
    }

    public void OnPlayerEnginesOn(bool on)
    {
        _particles.Emitting = on;
    }
}

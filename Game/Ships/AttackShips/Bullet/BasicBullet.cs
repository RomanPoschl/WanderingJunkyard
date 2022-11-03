using Godot;
using System;

public partial class BasicBullet : BulletBase
{
    public override string ScenePath => "res://Game/Platforms/Weapon/Bullet/BasicBullet.tscn";

    public override void _Ready()
    {
        base._Ready();
    }
}

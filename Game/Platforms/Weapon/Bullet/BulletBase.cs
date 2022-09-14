using Godot;
using System;

public abstract class BulletBase : KinematicBody2D
{
    public abstract string ScenePath { get; }
    public virtual int Speed => 750;

    Vector2 _velocity = new Vector2();

    public void Start(Vector2 pos, float dir)
    {
        Rotation = dir;
        Position = pos;
        _velocity = new Vector2(0, -Speed).Rotated(Rotation);
    }

    public override void _PhysicsProcess(float delta)
    {
        var collision = MoveAndCollide(_velocity * delta);
        if (collision != null)
        {
            _velocity = _velocity.Bounce(collision.Normal);
            if (collision.Collider.HasMethod("Hit"))
            {
                collision.Collider.Call("Hit");
            }
        }
    }
}

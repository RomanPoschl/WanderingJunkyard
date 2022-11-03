using System;
using Godot;

public abstract partial class BulletBase : CharacterBody2D
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

	public override void _PhysicsProcess(double delta)
	{
		var collision = MoveAndCollide(_velocity * (float)delta);
		if (collision != null)
		{
			_velocity = _velocity.Bounce(collision.GetNormal());
			if (collision.GetCollider().HasMethod("Hit"))
			{
				collision.GetCollider().Call("Hit");
			}
		}
	}
}

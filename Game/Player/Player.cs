using Godot;
using System;

public partial class Player : SnapablePlayer
{
	float speed_max = 675.0f;
	float acceleration = 1500.0f;
	float angular_speed_max => Deg2rad(150);
	float angular_acceleration => Deg2rad(1200);

	float drag_factor = 0.05f;
	float angular_drag_factor = 0.1f;

	Vector2 velocity = Vector2.Zero;
	float angular_velocity = 0.0f;

    [Signal]
	public delegate void EnginesOnEventHandler(bool on);
    EnginesOnEventHandler EnginesOnEvent;

	public delegate void HitEventHandler();

	public override void _Ready()
	{
		Hide();
		
		//EmitSignal(nameof(EnginesOnEventHandler), false);
		EnginesOnEvent?.Invoke(false);

		base._Ready();
	}

	public override void Flying()
	{
		var animationVelocity = Vector2.Zero;

		velocity = velocity.LimitLength(speed_max);
		ConstantForce = velocity;
		Rotation = angular_velocity; // * delta;

		var movement = animationVelocity = GetMovement();

        //TODO Fix when posible
        //EmitSignal(nameof(EnginesOnEventHandler), movement.Length() > 0);
        EnginesOnEvent?.Invoke(movement.Length() > 0);

		if (movement.IsEqualApprox(Vector2.Zero))
		{
			velocity = (velocity.Lerp(Vector2.Zero, drag_factor));
		}

		var direction = Vector2.Up.Rotated(Rotation);

		velocity += movement * acceleration;

		if (movement.IsEqualApprox(Vector2.Zero))
		{
			angular_velocity = Rotation;
		}
		else
		{
			angular_velocity = movement.Angle() + Deg2rad(90); // * delta;
		}
	}

	Vector2 GetMovement()
	{
		return new Vector2(Input.GetActionStrength(Constants.MovementConstants.MoveRight) - Input.GetActionStrength(Constants.MovementConstants.MoveLeft), Input.GetActionStrength(Constants.MovementConstants.MoveDown) - Input.GetActionStrength(Constants.MovementConstants.MoveUp));
	}
	
	private void _on_Player_body_entered(object body)
	{
		Hide();
		
		//EmitSignal(nameof(HitEventHandler));

		GetNode<CollisionShape2D>(Constants.PlayerConstants.CollisionShape2D).SetDeferred("disabled", true);
	}

	public void Start(Vector2 pos)
	{
		Position = pos;
		Show();
		GetNode<CollisionShape2D>(Constants.PlayerConstants.CollisionShape2D).Disabled = false;
	}

	public float Rad2deg(float rad)
	{
		return (float)((180 / Mathf.Pi) * rad);
	}

	public float Deg2rad(float deg)
	{
		return (float)((Mathf.Pi * deg) / 180);
	}

    public override void _IntegrateForces(PhysicsDirectBodyState2D state)
    {
        base._IntegrateForces(state);
    }
}

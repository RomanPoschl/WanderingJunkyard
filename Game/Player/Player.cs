using Godot;
using System;

public partial class Player : SnapablePlayer
{
	float speed_max = 25.0f;
	float acceleration = 1500.0f;
	float angular_speed_max => Deg2rad(150);
	float angular_acceleration => Deg2rad(1200);

	float drag_factor = 0.05f;
	float angular_drag_factor = 0.1f;

	Vector3 velocity = Vector3.Zero;
	float angular_velocity = 0f;

    [Signal]
	public delegate void EnginesOnEventHandler(bool on);
    EnginesOnEventHandler EnginesOnEvent;


	Camera3D _camera;

	public delegate void HitEventHandler();

	public override void _Ready()
	{
		//EmitSignal(nameof(EnginesOnEventHandler), false);
		EnginesOnEvent?.Invoke(false);

		_camera = GetNode<Camera3D>("/root/Game/PlayerCamera3D");

		base._Ready();
	}

	public override void _Process(double delta)
	{
		_camera.GlobalPosition = new Vector3(this.GlobalPosition.x, this.GlobalPosition.y, _camera.GlobalPosition.z);
	}

	public override void Flying()
	{
		var animationVelocity = Vector3.Zero;

		velocity = velocity.LimitLength(speed_max);
		ConstantForce = velocity;

		if(angular_velocity != 0)
			RotateZ(angular_velocity);

		var movement = animationVelocity = GetMovement();

        //TODO Fix when posible
        //EmitSignal(nameof(EnginesOnEventHandler), movement.Length() > 0);
        EnginesOnEvent?.Invoke(movement.Length() > 0);

		if (movement.IsEqualApprox(Vector3.Zero))
		{
			velocity = (velocity.Lerp(Vector3.Zero, drag_factor));
			return;
		}

		var direction = Vector3.Up.Rotated(Vector3.Up, Rotation.z);

		velocity += movement * acceleration;

		if (movement.IsEqualApprox(Vector3.Zero))
		{
			angular_velocity = Rotation.z;
		}
		else
		{
			angular_velocity = new Vector2(movement.x, movement.y).Angle() + Deg2rad(90);
		}
	}

	Vector3 GetMovement()
	{
		return new Vector3(
			Input.GetActionStrength(Constants.MovementConstants.MoveRight) - Input.GetActionStrength(Constants.MovementConstants.MoveLeft),
			Input.GetActionStrength(Constants.MovementConstants.MoveDown) - Input.GetActionStrength(Constants.MovementConstants.MoveUp),
			0);
	}
	
	private void _on_Player_body_entered(object body)
	{
		Hide();
		
		//EmitSignal(nameof(HitEventHandler));

		GetNode<CollisionShape3D>(Constants.PlayerConstants.CollisionShape3D).SetDeferred("disabled", true);
	}

	public void Start(Vector3 pos)
	{
		Position = pos;
		GetNode<CollisionShape3D>(Constants.PlayerConstants.CollisionShape3D).Disabled = false;
	}

	public float Rad2deg(float rad)
	{
		return (float)((180 / Mathf.Pi) * rad);
	}

	public float Deg2rad(float deg)
	{
		return (float)((Mathf.Pi * deg) / 180);
	}

    public override void _IntegrateForces(PhysicsDirectBodyState3D state)
    {
        base._IntegrateForces(state);
    }
}

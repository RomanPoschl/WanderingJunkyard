using Godot;
using System;

public class Player : KinematicBody2D
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
    public delegate void EnginesOn(bool on);

    [Signal]
    public delegate void Hit();

    public override void _Ready()
    {
        Hide();
        EmitSignal(nameof(EnginesOn), false);
    }

    public override void _PhysicsProcess(float delta)
    {
        var animationVelocity = Vector2.Zero;


        velocity = velocity.LimitLength(speed_max);

        velocity = MoveAndSlide(velocity);
        Rotation = angular_velocity; // * delta;

        var movement = animationVelocity = GetMovement();
        EmitSignal(nameof(EnginesOn), movement.Length() > 0);

        if (movement.IsEqualApprox(Vector2.Zero))
        {
		    velocity = (velocity.LinearInterpolate(Vector2.Zero, drag_factor));
        }

        var direction = Vector2.Up.Rotated(Rotation);

        velocity += movement * acceleration * delta;

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
        return new Vector2(Input.GetActionStrength("move_right") - Input.GetActionStrength("move_left"), Input.GetActionStrength("move_down") - Input.GetActionStrength("move_up"));
    }
    
    private void _on_Player_body_entered(object body)
    {
        Hide();
        EmitSignal(nameof(Hit));
        GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred("disabled", true);
    }

    public void Start(Vector2 pos)
    {
        Position = pos;
        Show();
        GetNode<CollisionShape2D>("CollisionShape2D").Disabled = false;
    }

    public float Rad2deg(float rad)
    {
        return (float)((180 / Mathf.Pi) * rad);
    }

    public float Deg2rad(float deg)
    {
        return (float)((Mathf.Pi * deg) / 180);
    }
}
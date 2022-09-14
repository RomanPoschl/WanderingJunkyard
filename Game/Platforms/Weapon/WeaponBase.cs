using Godot;
using System;

public abstract class WeaponBase : Node2D
{
    public enum WeaponBaseState
    {
        idle,
        grabbing,
        dead
    }

    WeaponBaseState _weaponState;

    public abstract BulletBase Bullet { get; }
    public abstract float RateOfFire { get; }
    public PackedScene _bulletScene;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _weaponState = WeaponBaseState.idle;

        _bulletScene = GD.Load<PackedScene>(Bullet.ScenePath);
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        switch (_weaponState)
        {
            case WeaponBaseState.idle:
                break;
            case WeaponBaseState.grabbing:
                break;
            case WeaponBaseState.dead:
                break;
            default:
                return;
        }
    }

    public override void _Input(InputEvent @event)
    {
        if(@event is InputEventScreenDrag drag)
        {
            Position = drag.Position;
        }
        if(@event is InputEventScreenTouch touch && @event.IsPressed())
        {
            _weaponState = WeaponBaseState.grabbing;
            Position = touch.Position;
        }
        else if(@event is InputEventScreenTouch noTouch && !@event.IsPressed())
        {
            _weaponState = WeaponBaseState.idle;
        }
    }

    public abstract bool CanShoot();
    public abstract void Shoot();
}

using Godot;
using System;
using System.Collections.Generic;

public abstract partial class ShipBoidBase : RigidBody2D
{
	public enum SnapableComponentState
	{
		idle,
		grabbing,
		inFlock,
		dead
	}

	float _maxSpeed = 2000f;
	Vector2 _maxSpeedVector;


	Events _events;
	SnapableComponentState _weaponState;
	public SnapableComponentState WeaponState { get => _weaponState; set => _weaponState = value; }
	public PackedScene _bulletScene;

	Node2D _components;

	bool _connectedToBase = false;
	bool _nonBaseConnected = false;

	Area2D _vision;
	bool placingPosible = false;

	Marker2D _engine;

	NavigationAgent2D _navigationAgent;
	Player _player;
	float _distanceToPlayer;
	float _angleToPlayer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_weaponState = SnapableComponentState.idle;
		_components = GetNode<Node2D>("Node2D/Components");
		_events = GetNode<Events>(Constants.Events);
		_vision = GetNode<Area2D>("Node2D/Vision");
		_engine = GetNode<Marker2D>("Node2D/Engine");
		_navigationAgent = GetNode<NavigationAgent2D>("Node2D/NavigationAgent2d");
		_navigationAgent.MaxSpeed = _maxSpeed;

		_player = GetNode<Player>("/root/Game/Player");

		_maxSpeedVector = new Vector2(_maxSpeed, _maxSpeed);

		GetNode<Timer>("Node2D/NavigationTimer").Start();
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		switch (_weaponState)
		{
			case SnapableComponentState.idle:
				break;
			case SnapableComponentState.grabbing:
				Position = GetGlobalMousePosition();

				var visibleBodies = _vision.GetOverlappingBodies();
				placingPosible = visibleBodies.Count > 0;

				break;
			case SnapableComponentState.dead:
				break;
			default:
				return;
		}
	}

	public bool IsDistanceToPlayerEqual()
	{
		var dist = _player.Position.DistanceTo(this.Position);
		return Mathf.IsEqualApprox(dist, _distanceToPlayer);
	}

	public bool IsAngleToPlayerEqual()
	{
		var angle = this.GetAngleTo(_player.Position);
		return Mathf.IsEqualApprox(angle, _angleToPlayer);
	}

	public void SetTarget(Vector2 targetPosition)
	{
		_navigationAgent.SetTargetLocation(targetPosition);
	}

	public override void _PhysicsProcess(double delta)
	{
		if(_weaponState != SnapableComponentState.inFlock)
			return;

		if(IsDistanceToPlayerEqual() || IsAngleToPlayerEqual())
			return;

		var nextPathPosition = _navigationAgent.GetNextLocation();
		var currentPosition = GlobalPosition;
		var direction = (nextPathPosition - currentPosition);
		var directionNormalized = direction.Normalized();
		var newVelocity = directionNormalized * _maxSpeed * (float)delta;
		_navigationAgent.SetVelocity(newVelocity);
	}

	public void OnArea2DInputEvent(Node viewPort, InputEvent @event, int shapeIdx)
	{
		if(@event is InputEventScreenDrag drag)
		{
			//Position = drag.Position;
		}

		if(@event is InputEventMouseButton touch && @event.IsPressed())
		{
			OnTouch();
		}
		else if(@event is InputEventMouseButton noTouch && !@event.IsPressed())
		{
			NoTouch();
		}

		// if(@event is InputEventScreenTouch screenTouch && @event.IsPressed())
		// {
		// 	OnTouch();
		// }
		// else if(@event is InputEventScreenTouch noTouch && !@event.IsPressed())
		// {
		// 	NoTouch();
		// }
	}

	private void PlaceBoidToFlock()
	{
		_angleToPlayer = this.GetAngleTo(_player.Position);
		_distanceToPlayer = this.Position.DistanceTo(_player.Position);
		_events.EmitSignal(nameof(Events.OnPlatformPlaced));
	}

	private void RemoveBoidFromFlock()
	{
	}

	private void NoTouch()
	{
		if(_weaponState == SnapableComponentState.grabbing && placingPosible)
		{
			PlaceBoidToFlock();
			_weaponState = SnapableComponentState.inFlock;
		}
		else
		{
			GD.Print($"WeaponBase._Input() noTouch");
			_weaponState = SnapableComponentState.idle;
		}
	}

	private void OnTouch()
	{
		if(_weaponState == SnapableComponentState.inFlock)
		{
			RemoveBoidFromFlock();
		}

		_weaponState = SnapableComponentState.grabbing;
	}

	Vector2 _velocity;

	public void OnNavigationAgent2DVelocityComputed(Vector3 safeVelocity)
	{
		LinearVelocity = (new Vector2(safeVelocity.x, safeVelocity.y));
	}

	public void OnNavigationTimerTimeout()
	{
		//var targetPosition = _player.Position + new Vector2(_distanceToPlayer, 0).Rotated(_angleToPlayer);
		var targetPosition = new Vector2(0,0);
		SetTarget(targetPosition);
	}
}

public class OtherBoids
{
	public Node2D Boid { get; set; }
	public float Angle { get; set; }
	public float Distance { get; set; }
}


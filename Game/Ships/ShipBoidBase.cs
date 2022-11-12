using Godot;
using System;
using System.Collections.Generic;

public abstract partial class ShipBase : RigidBody3D
{
	public enum DragAndDropShipState
	{
		idle,
		grabbing,
		inFlock,
		attacking,
		dead
	}

	float _maxSpeed = 20f;
	Events _events;
	DragAndDropShipState _shipState;
	public DragAndDropShipState ShipState { get => _shipState; set => _shipState = value; }
	Area2D _vision;
	Marker2D _engine;
	Player _player;
	Node3D? _enemy;

	float _distanceToPlayer;
	float _angleToPlayer;
	Vector2 _velocity;
	Vector3 _target;

	public override void _Ready()
	{
		_shipState = DragAndDropShipState.idle;

		_events = GetNode<Events>(Constants.Events);
		_events.Connect(nameof(Events.OnEnemyTargetedEventHandler), new Callable(this, nameof(OnEnemyTargeted)));

		_vision = GetNode<Area2D>("Node2D/Vision");
		_engine = GetNode<Marker2D>("Node2D/Engine");

		_player = GetNode<Player>("/root/Game/Player");
		

		GetNode<Timer>("Node2D/NavigationTimer").Start();
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		if(_shipState == DragAndDropShipState.grabbing)
		{
			var mouse = GetViewport().GetMousePosition();
			Position = new Vector3(mouse.x, mouse.y, 0);
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if(_shipState != DragAndDropShipState.inFlock)
			return;

		if(IsDistanceToPlayerEqual() && IsAngleToPlayerEqual())
			return;

		var newVelocity = (_target - Transform.origin).Normalized() * _maxSpeed;
		ApplyCentralForce(newVelocity);
		Rotation = _player.Rotation;
	}

	public void OnNavigationTimerTimeout()
	{
		if(_shipState == DragAndDropShipState.inFlock)
		{
			var test = Vector3.Right.Rotated(Vector3.Up, _player.Rotation.z + _angleToPlayer);
			var targetPosition = _player.Position + (test * -_distanceToPlayer);
			SetTarget(targetPosition);
		}
		else if(_shipState == DragAndDropShipState.attacking && _enemy != null)
		{
			var x = Vector3.Right * 100;
			var targetPosition = _enemy.Position + x;
			SetTarget(targetPosition);
		}
	}

	void SetTarget(Vector3 targetPosition)
	{
		_target = targetPosition;
	}

	public bool IsDistanceToPlayerEqual()
	{
		var dist = _player.Position.DistanceTo(this.Position);
		return Mathf.IsEqualApprox(dist, _distanceToPlayer);
	}

	public bool IsAngleToPlayerEqual()
	{
		var angle = this.Position.DirectionTo(_player.Position);
		return Mathf.IsEqualApprox(angle.z, _angleToPlayer);
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

	public void OnEnemyTargeted(Node3D enemy)
	{
		_shipState = DragAndDropShipState.attacking;
		_enemy = enemy;
	}

	private void PlaceBoidToFlock()
	{
		_angleToPlayer = this.Position.DirectionTo(_player.Position).z;
		_distanceToPlayer = this.Position.DistanceTo(_player.Position);
		_events.EmitSignal(nameof(Events.OnPlatformPlaced));
	}

	private void NoTouch()
	{
		if(_shipState == DragAndDropShipState.grabbing)
		{
			PlaceBoidToFlock();
			_shipState = DragAndDropShipState.inFlock;
		}
		else
		{
			GD.Print($"WeaponBase._Input() noTouch");
			_shipState = DragAndDropShipState.idle;
		}
	}

	private void OnTouch()
	{
		_shipState = DragAndDropShipState.grabbing;
	}
}
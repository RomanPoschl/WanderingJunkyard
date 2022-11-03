using Godot;
using System;

public abstract partial class SnapablePlayer : RigidBody2D
{
	Events _events;

	public enum MainState
	{
		Building,
		Flying,
		Dead,
	}

	public override void _Ready()
	{
		_currentState = MainState.Flying;

		_events = GetNode<Events>(Constants.Events);
		//Connect(nameof(Events.OnBuildPressedSignalEventHandler),new Callable(this,nameof(EnterBuildState)));
		//Connect(nameof(Events.OnBuildClosePressedSignalEventHandler),new Callable(this,nameof(EnterFlyingState)));
		_events.OnBuildPressedSignal += EnterBuildState;
		_events.OnBuildClosePressedSignal += EnterFlyingState;
		
		FreezeMode = RigidBody2D.FreezeModeEnum.Kinematic;
		Freeze = false;

		base._Ready();
	}

	MainState _currentState;

	public abstract void Flying();

	public override void _IntegrateForces(PhysicsDirectBodyState2D state)
	{
		base._IntegrateForces(state);

		switch (_currentState)
		{
			case MainState.Building:
				break;
			case MainState.Flying:
				Flying();
				break;
			case MainState.Dead:
				break;
			default:
				break;
		}
	}

	Vector2 _positionStorage = new Vector2();

	void EnterBuildState()
	{
		_positionStorage = Position;
		Freeze = true;
		ChangeState(MainState.Building);
	}

	void EnterFlyingState()
	{
		Position = _positionStorage;
		Position = _positionStorage;
		Freeze = false;
		ChangeState(MainState.Flying);
	}
	
	void ChangeState(MainState state)
	{
		_currentState = state;
	}
}

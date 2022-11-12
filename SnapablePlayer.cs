using Godot;
using System;

public abstract partial class SnapablePlayer : RigidBody3D
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
		_events.OnBuildPressedSignal += EnterBuildState;
		_events.OnBuildClosePressedSignal += EnterFlyingState;
		
		FreezeMode = RigidBody3D.FreezeModeEnum.Kinematic;
		Freeze = false;

		base._Ready();
	}

	MainState _currentState;

	public abstract void Flying();

	public override void _IntegrateForces(PhysicsDirectBodyState3D state)
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

	Vector3 _positionStorage = new ();

	void EnterBuildState()
	{
		_positionStorage = Position;
		Freeze = true;
		ChangeState(MainState.Building);
	}

	void EnterFlyingState()
	{
		Position = _positionStorage;
		Freeze = false;
		ChangeState(MainState.Flying);
	}
	
	void ChangeState(MainState state)
	{
		_currentState = state;
	}
}

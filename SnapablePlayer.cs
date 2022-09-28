using Godot;
using System;

public abstract class SnapablePlayer : SnapableComponent
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
        _events.Connect(nameof(Events.OnBuildPressedSignal), this, nameof(EnterBuildState));

        base._Ready();
    }

    MainState _currentState;

    public abstract void Flying();

    public override void _IntegrateForces(Physics2DDirectBodyState state)
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

    void EnterBuildState()
    {
        Mode = RigidBody2D.ModeEnum.Kinematic;
        ChangeState(MainState.Building);
    }

    void EnterFlyingState()
    {
        Mode = RigidBody2D.ModeEnum.Rigid;
        ChangeState(MainState.Flying);
    }
    
    void ChangeState(MainState state)
    {
        _currentState = state;
    }

    public override void AddComponent(Node2D component, Vector2 position)
    {
        base.AddComponent(component, position);

        if(component is SnapableComponent com)
        {
            Mass += com.ComponentAffect.Weight;
        }
    }

    public override void RemoveComponent(SnapableComponent component)
    {
        base.RemoveComponent(component);

        if(component is SnapableComponent com)
        {
            Mass -= com.ComponentAffect.Weight;
        }
    }
}

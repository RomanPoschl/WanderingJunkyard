using Godot;
using System;
using System.Collections.Generic;

public abstract class SnapableComponent : RigidBody2D
{
    public virtual ComponentAffect ComponentAffect { get; set; }= new ComponentAffect();
    public enum SnapableComponentState
    {
        idle,
        grabbing,
        dead
    }

    SnapableComponentState _weaponState;
    public SnapableComponentState WeaponState { get => _weaponState; set => _weaponState = value; }
    public PackedScene _bulletScene;

    Node2D _components;

    SnapableComponent _parent;
    bool _connectedToBase = false;
    bool _nonBaseConnected = false;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _weaponState = SnapableComponentState.idle;
        _components = GetNode<Node2D>("Node2D/Components");
        _rayCasts = new List<SnapPointRayCast2D>();
        GetAllRaycasts(this);
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        switch (_weaponState)
        {
            case SnapableComponentState.idle:
                break;
            case SnapableComponentState.grabbing:
                Position = GetGlobalMousePosition();
                foreach(var ray in _rayCasts)
                {
                    if(ray.IsColliding())
                    {   
                        var collider = ray.GetCollider();
                        GD.Print($"Collider {collider.GetType().FullName}");
                        var collisionBody = collider as StaticBody2D;
                        if(collisionBody == null)
                            return;

                        var snapPoint = (Position2D)collisionBody.GetParent();
                        WeaponState = SnapableComponentState.idle;
                        TriggerRaycasts(false);
                        _nonBaseConnected = true;

                        var rayPosition = ray.GetParent<Node2D>().GetParent<Node2D>().Position;

                        ((SnapableComponent)snapPoint.Owner).AddComponent(this, snapPoint.GlobalPosition + snapPoint.Position);
                        _parent = (SnapableComponent)snapPoint.Owner;
                        ray.Component = collisionBody.Owner;
                        foreach(var c in collisionBody.GetChildren())
                        {
                            if(c is SnapPointRayCast2D rc2d)
                            {
                                rc2d.Component = this;
                            }
                        }
                    }
                }
                break;
            case SnapableComponentState.dead:
                break;
            default:
                return;
        }
    }
    
    public virtual void AddComponent(Node2D component, Vector2 position)
    {
        var parent = component.GetParent();
        parent?.RemoveChild(component);
        _components.AddChild(component);
        component.GlobalPosition = position;
    }
    
    public virtual void RemoveComponent(SnapableComponent component)
    {
        component.RemoveComponentData(this);
        RemoveComponentData(component);
        _components.RemoveChild(component);
        GetTree().Root.AddChild(component);
    }

    void RemoveComponentData(Node2D component)
    {
        foreach(var c in _rayCasts)
        {
            if(c.Component == component)
            {
                c.Component = null;
            }
        }
    }

    public void OnArea2DInputEvent(Node viewPort, InputEvent @event, int shapeIdx)
    {
        if(@event is InputEventScreenDrag drag)
        {
            //Position = drag.Position;
        }

        if(@event is InputEventMouse touch && @event.IsPressed())
        {
            OnTouch();
        }
        else if(@event is InputEventScreenTouch noTouch && !@event.IsPressed())
        {
            NoTouch();
        }
    }

    private void NoTouch()
    {
        GD.Print($"WeaponBase._Input() noTouch");
        _weaponState = SnapableComponentState.idle;

        TriggerRaycasts(false);
    }

    private void OnTouch()
    {
        _weaponState = SnapableComponentState.grabbing;
        //Position = touch.Position;

        TriggerRaycasts(true);

        if (_nonBaseConnected)
        {
            _parent?.RemoveComponent(this);
            _nonBaseConnected = false;
        }
    }

    List<SnapPointRayCast2D> _rayCasts;

    async void TriggerRaycasts(bool enabled)
    {
        GD.Print($"TriggerRaycasts {enabled}");

        if(enabled)
        {
            var t = new Timer();
            t.WaitTime = .2f;
            AddChild(t);
            t.Start();
            await ToSignal(t, "timeout");
            t.QueueFree();
        }

        foreach(var ray in _rayCasts)
        {
            ray.Enabled = enabled;
            GD.Print($"Ray {ray.Name} enabled: {enabled}");
        }
    }

    void GetAllRaycasts(Node node)
    {
        foreach(var n in node.GetChildren())
        {
            if(n is SnapPointRayCast2D rc2d)
            {
                GD.Print($"Adding RayCast2D {rc2d.Name}");
                _rayCasts.Add(rc2d);
            }
            
            if(n is Node nod)
                GetAllRaycasts(nod);
        }
    }
}

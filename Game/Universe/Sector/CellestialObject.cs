using System.Runtime.InteropServices;
using System.Collections.Generic;
using Godot;

public class CellestialObject : RigidBody2D
{
    float _gravitionalConstant = 0.0001f;
    Vector2 _lastPosition;
    Vector2 _startPosition;
    CellestialObject _parentObject;
    List<CellestialObject> _childObjects = new List<CellestialObject>();
    public float Mass { get; set; }
    float _radius;
    Vector2 _initialVelocity;
    Vector2 _currentVelocity;

    bool IsMain => ParentObject == null;

    public CellestialObject ParentObject { get => _parentObject; set => _parentObject = value; }
    public List<CellestialObject> ChildObjects { get => _childObjects; set => _childObjects = value; }
    public Vector2 CurrentVelocity { get => _currentVelocity; set => _currentVelocity = value; }

    //CollisionShape2D _collisionShape;

    public override void _Ready()
    {
        base._Ready();
        //_collisionShape = GetNode<CollisionShape2D>("CollisionShape2D-TEST");
    }

    public void UpdateVelocity(CellestialObject[] allBodies, float timeStep)
    {
        foreach(var otherBody in allBodies)
        {
            if(otherBody == this) 
                continue;

            if(IsMain)
                continue;

            float squareDistance = this.Position.DistanceSquaredTo(otherBody.Position);
            Vector2 forceDir = (otherBody.Position - this.Position).Normalized();
            Vector2 force = forceDir * _gravitionalConstant * Mass * otherBody.Mass / squareDistance;
            Vector2 acceleration = force / Mass;
            _currentVelocity += acceleration * timeStep;
        }
    }

    public void UpdatePosition(float timeStep)
    {
        Position += _currentVelocity * timeStep;
        //MoveAndCollide(_currentVelocity * timeStep);
    }
}
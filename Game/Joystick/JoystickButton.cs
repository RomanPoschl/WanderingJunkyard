using Godot;
using System;
using System.IO;
using System.Runtime.Serialization;

public class JoystickButton : TouchScreenButton
{
    Vector2 _radius = new Vector2(50, 50);
    float _maxDistance = 100f;
    int _ongoingDrag = -1;
    float _returnAccel = 20f;
    float _threshold = 10f;

    public override void _Ready()
    {
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        if (_ongoingDrag == -1)
        {
            var posDiference = (Vector2.Zero - _radius) - Position;
            Position += posDiference * _returnAccel * delta;
        }

        var pos = GetButtonPosition();

        if(pos.Length() > _threshold)
        {
            var posNorm = pos.Normalized();
            if(pos.x > 0)
            {
                Input.ActionPress(Constants.MovementConstants.MoveRight, posNorm.x);
            }
            else
                Input.ActionPress(Constants.MovementConstants.MoveRight, 0);


            if(pos.x < 0)
                Input.ActionPress(Constants.MovementConstants.MoveLeft, posNorm.x * -1);
            else
                Input.ActionPress(Constants.MovementConstants.MoveLeft, 0);


            if(pos.y > 0)
                Input.ActionPress(Constants.MovementConstants.MoveDown, posNorm.y);
            else
                Input.ActionPress(Constants.MovementConstants.MoveDown, 0);


            if(pos.y < 0)
                Input.ActionPress(Constants.MovementConstants.MoveUp, posNorm.y * -1);
            else
                Input.ActionPress(Constants.MovementConstants.MoveUp, 0);
        }
        else
        {
            Input.ActionPress(Constants.MovementConstants.MoveRight, 0);
            Input.ActionPress(Constants.MovementConstants.MoveLeft, 0);
            Input.ActionPress(Constants.MovementConstants.MoveUp, 0);
            Input.ActionPress(Constants.MovementConstants.MoveDown, 0);
        }
    }

    public Vector2 GetButtonPosition()
    {
        return Position + _radius;
    }

    public Vector2 CalculateButtonPosition(Vector2 position)
    {
        return position - _radius;
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);

        if(@event is InputEventScreenDrag drag)
        {
            MoveButton(drag.Position, drag.Index);
        }       
        else if (@event is InputEventScreenTouch touch && @event.IsPressed())
        {
            MoveButton(touch.Position, touch.Index);
        }

        if(@event is InputEventScreenTouch touchStop && !@event.IsPressed() && touchStop.Index == _ongoingDrag)
        {
            _ongoingDrag = -1;
        }
    }


    void MoveButton(Vector2 position, int index)
    {
        var eventDistFromCenter = (position - GetParent<Sprite>().GlobalPosition).Length();

        if (eventDistFromCenter <= _maxDistance * GlobalScale.x || index == _ongoingDrag)
        {
            GlobalPosition = position - _radius * GlobalScale;

            if(GetButtonPosition().Length() > _maxDistance)
            {
                Position = GetButtonPosition().Normalized() * _maxDistance - _radius;
            }

            _ongoingDrag = index;
        }
    }

    public Vector2 GetValue()
    {
        var pos = GetButtonPosition();

        if(pos.Length() > _threshold)
        {
            return pos.Normalized();
        }
            
        return Vector2.Zero;
    }
}

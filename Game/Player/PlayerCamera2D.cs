using Godot;
using System;
using System.Collections.Generic;

public partial class PlayerCamera3D : Camera3D
{
    Events _events;
    Tween _tween;
    RemoteTransform2D _remoteMap;

    float _lastDragInstance;
    float _zoomSensitivity = 10f;
    float _zoomSpeed = 0.05f;
    float _defaultZoom = 2f;
    float _minPinchZoom = 0.5f;
    float _maxPinchZoom = 5f;
    float _fromGameToMapThreshold = 2f;
    float _defaultMapZoom = 5.0f;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _events = GetNode<Events>("/root/Events");
        _remoteMap = GetNode<RemoteTransform2D>("RemoteMap");
        
        _tween = CreateTween(); //GetNode<Tween>("Tween");

        _events.Connect(nameof(Events.MapToggled), new Callable(this,nameof(ToggleMap)));
        //_events.MapToggled += ToggleMap;

        Fov = 75f * _defaultZoom;
    }

    List<InputEventScreenDrag> _inputEvents = new List<InputEventScreenDrag>();

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);

        if(@event is InputEventScreenTouch touch && !touch.IsPressed())
        {
            _inputEvents.Clear();
            Fov = 75f * _defaultZoom;
        }

        if(@event is InputEventScreenDrag drag)
        {
            _inputEvents.Insert(drag.Index, drag);

            if(_inputEvents.Count == 2)
            {
                var dragDistance = _inputEvents[0].Position.DistanceTo(_inputEvents[1].Position);

                if(Mathf.Abs(dragDistance - _lastDragInstance) > _zoomSensitivity)
                {
                    var newZoom = (1 + _zoomSpeed);
                    if(dragDistance < _lastDragInstance)
                        newZoom = (1 - _zoomSpeed);

                    if(newZoom < _fromGameToMapThreshold)
                    {
                        newZoom = Mathf.Clamp(Fov * newZoom, _minPinchZoom, _maxPinchZoom);

                        Fov = 75f * newZoom;
                        _lastDragInstance = dragDistance;
                    }
                    else
                    {
                        _events.EmitSignal("OpenMap");
                    }
                }
            }
        }
    }

    public void SetCameraMap(MapView map)
    {
        var cameraMap = (PlayerCamera3D)this.Duplicate();
        map.RegisterCamera(cameraMap);
        _remoteMap.RemotePath = cameraMap.GetPath();
    }

    void ToggleMap(bool show, float duration)
    {
        if(show)
        {
            _tween.TweenProperty(this, "zoom", 75f * _maxPinchZoom, duration)
                .From(Fov)
                .SetTrans(Tween.TransitionType.Linear)
                .SetEase(Tween.EaseType.OutIn);
        }
        else
        {
            _tween.TweenProperty(this, "zoom", 75f * _defaultMapZoom, duration)
                .From(Fov)
                .SetTrans(Tween.TransitionType.Linear)
                .SetEase(Tween.EaseType.OutIn);
        }

        _tween.Play();
    }
}

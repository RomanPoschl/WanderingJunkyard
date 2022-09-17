using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class UniverseMap : Node2D
{
    [Signal]
    public delegate void JumpToSector(MapCell mapCell);

    Layout _layout = new Layout(Layout.flat, new Point(50, 50), new Point(0, 0));
    Universe _universe;
    PackedScene _preloadedCell;
    float _threshold = 10f;
    bool _isDragging;
    Vector2 _dragPosition;
    Vector2 _oldPosition;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _universe = GetNode<Universe>("/root/Universe");
        _preloadedCell = (PackedScene)ResourceLoader.Load("res://Game/Universe/MapCell.tscn");
    }

    public void Display(List<MapCell> map)
    { 
        foreach(var cell in map)
        {
            var scene = _preloadedCell.Instance<MapCell>();
            scene.HexPosition = cell.HexPosition;
            scene.Name = $"{cell.HexPosition.Q}_{cell.HexPosition.R}_{cell.HexPosition.S}";
            scene.NoiseSeed = cell.NoiseSeed;

            AddChild(scene);

            scene.InitShape();
        }
    }

    int _ongoingDrag = -1;

    float mapHalfWidth;
    float mapHalfHeight;

    MapCell _currentlySelected;

    void OnPanelGuiInput(InputEvent @event)
    {
        base._Input(@event);

        if(@event is InputEventScreenDrag drag)
        {
            GD.Print("drag");
            if(drag.Position.Length() > _threshold)
            {
                _ongoingDrag = drag.Index;
                _isDragging = true;
                _dragPosition = drag.Position;
            }
        }       
        
        if (@event is InputEventScreenTouch touch && @event.IsPressed())
        {
            _oldPosition = Position - touch.Position;
            if(_ongoingDrag == -1)
            {
                var touchCalculatedPosition = Position - touch.Position;
                var fhex = _layout.PixelToHex(Point.FromVector2(touchCalculatedPosition));
                var hex = fhex.HexRound();
                var map = _universe.GetMap();
                var poly = map.Where(x => x.HexPosition.Q == hex.Q && x.HexPosition.R == hex.R && x.HexPosition.S == hex.S).FirstOrDefault();

                GD.Print("touch", $"{hex.Q}_{hex.R}_{hex.S}");
                var polyObject = GetNode<MapCell>($"{hex.Q}_{hex.R}_{hex.S}");

                if(polyObject == null)
                {
                    GD.Print("HEX not found");
                }
                else
                {
                    _currentlySelected?.ResetColor();
                    polyObject.SetColor(Colors.Orange);
                    _currentlySelected = polyObject;
                    ShowSectorInfo(touch.Position);
                }
            }
        }

        if(@event is InputEventScreenTouch touchStop && !@event.IsPressed() && touchStop.Index == _ongoingDrag)
        {
            _ongoingDrag = -1;
            _isDragging = false;
        }
    }

    void ShowSectorInfo(Vector2 mousePosition)
    {
        if(_currentlySelected == null)
            return;
        
        var sectorPopupPanel = GetNode<PopupPanel>("/root/Game/UI/PopupPanel/Panel/SectorPopupPanel");
        var titleLabel = GetNode<Label>("/root/Game/UI/PopupPanel/Panel/SectorPopupPanel/VBoxContainer/SectorName");
        var sectorTypeLabel = GetNode<Label>("/root/Game/UI/PopupPanel/Panel/SectorPopupPanel/VBoxContainer/HBoxContainer/Type");

        titleLabel.Text = $"{_currentlySelected.HexPosition.Q}_{_currentlySelected.HexPosition.R}_{_currentlySelected.HexPosition.S}";
        sectorTypeLabel.Text = Sector.GetSectorType(_currentlySelected.NoiseSeed).ToString();
        sectorPopupPanel.RectPosition = mousePosition;
        sectorPopupPanel.Popup_();
    }

    void OnJumpButtonPressed()
    {
        GD.Print("Jump NOW");
        var sectorPopupPanel = GetNode<PopupPanel>("/root/Game/UI/PopupPanel/Panel/SectorPopupPanel");
        var mapPopup = GetNode<Popup>("/root/Game/UI/PopupPanel");
        sectorPopupPanel.Hide();
        mapPopup.Hide();
        EmitSignal(nameof(JumpToSector), _currentlySelected);
    }

    void OnSectorPopupCloseButtonPressed()
    {
        var sectorPopupPanel = GetNode<PopupPanel>("/root/Game/UI/PopupPanel/Panel/SectorPopupPanel");
        sectorPopupPanel.Hide();
    }

    public override void _Process(float delta)
    {
        if(_isDragging && _dragPosition != null)
        {
            Position = _dragPosition + _oldPosition;
        }
    }
}

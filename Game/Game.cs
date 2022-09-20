using Godot;
using System;

public class Game : Node
{
    MapView _mapView;
    Universe _universe;

    Popup _mapPopup;
    Button _mapCloseButton;
    UniverseMap _universeMap;

    public override void _Ready()
    {
        var events = GetNode<Events>("/root/Events");
        events.Connect(nameof(Events.OnJumpPressedSignal), this, nameof(OnJumpPressed));
        _universe = GetNode<Universe>("/root/Universe");

        _mapView = GetNode<MapView>("MapView");

        _mapPopup = GetNode<Popup>("UI/PopupPanel");
        _mapCloseButton = GetNode<Button>("UI/PopupPanel/CloseButton");
        _universeMap = GetNode<UniverseMap>("UI/PopupPanel/Panel/UniverseMap");

        GD.Randomize();
        NewGame();
    }
    
    private void game_over()
    {
        GetNode<HUD>("HUD").ShowGameOver();
    }
    
    public void NewGame()
    {
        var player = GetNode<Player>("Player");
        var startPosition = GetNode<Position2D>("StartPosition");
        player.Start(startPosition.Position);

        _universe.NewGame();
        var currentSector = _universe.GetCurrentMapCell();

        GD.Print($"Current sector {currentSector.HexPosition.ToVector3()} {currentSector.OffsetCoordFromHex.ToVector2()} {currentSector.NoiseSeed}");

        var hud = GetNode<HUD>("UI/HUD");
        hud.ShowMessage("Get Ready!");

        _universeMap.Display(_universe.MapCellGrid);
    }

    private void OnJumpPressed()
    {
        //TODO Show universe map
        GD.Print("JUMP");
        _mapPopup.Popup_();
    }

    void OnCloseButtonPressed()
    {
        _mapPopup.Hide();
    }

}

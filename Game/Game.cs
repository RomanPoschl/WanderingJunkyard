using Godot;
using System;
using System.Collections.Generic;

public partial class Game : Node
{
	MapView _mapView;
	Universe _universe;

	Popup _mapPopup;
	Button _mapCloseButton;
	UniverseMap _universeMap;

	SectorGenerator _sector;
	Marker3D _startPosition;

	public override void _Ready()
	{
		_startPosition = GetNode<Marker3D>("StartPosition");
		SetNavigationRegion();

		var events = GetNode<Events>(Constants.Events);
		events.OnJumpPressedSignal += OnJumpPressed;

		_universe = GetNode<Universe>(Constants.Universe);

		_mapView = GetNode<MapView>(Constants.MapView);

		_mapPopup = GetNode<Popup>(Constants.UIConstants.UniverseMapPopUp);
		_mapCloseButton = GetNode<Button>(Constants.UIConstants.UniveseMapClose);
		_universeMap = GetNode<UniverseMap>(Constants.UIConstants.UniveseMap);

		_sector = GetNode<SectorGenerator>("Sector");

		GD.Randomize();
		NewGame();
	}
	
	void SetNavigationRegion()
	{
		try
		{
			var storage = GetNode<Storage>(Constants.Storage);

			var regionRId = NavigationServer2D.RegionCreate();
			var defaultMapRid = _startPosition.GetWorld3d().NavigationMap;
			storage.SetDefaultMap(defaultMapRid);
			
			NavigationServer2D.RegionSetMap(regionRId, defaultMapRid);

			var navPoly = new NavigationPolygon();

			var shape = new Vector2[6];
			var r = 1000000;

			for(var a = 0; a < 6; a++)
			{
				shape[a] = new Vector2(
					//_startPosition.Position.x + r * (Mathf.Cos(a * 60 * Mathf.Pi / 180f)),
					0 + r * (Mathf.Cos(a * 60 * Mathf.Pi / 180f)),
					//_startPosition.Position.y + r * (Mathf.Cos(a * 60 * Mathf.Pi / 180f)));
					0 + r * (Mathf.Cos(a * 60 * Mathf.Pi / 180f)));
			}

			navPoly.AddOutline(shape);
			navPoly.MakePolygonsFromOutlines();

			NavigationServer2D.RegionSetNavpoly(regionRId, navPoly);
		}
		catch (System.Exception e)
		{
			GD.Print(e.Message);
		}
	}

	private void game_over()
	{
		GetNode<HUD>("HUD").ShowGameOver();
	}
	
	public void NewGame()
	{
		var player = GetNode<Player>("PlayerBody");
		player.Start(_startPosition.Position);

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
		_mapPopup.Popup();
	}

	void OnCloseButtonPressed()
	{
		_mapPopup.Hide();
	}
}

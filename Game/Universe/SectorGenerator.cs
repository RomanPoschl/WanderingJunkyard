using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;

public partial class TravelLane
{
	public Vector2 Position { get; set; }
	public Vector2 Destination { get; set; }
}

public partial class SectorGenerator : Node2D
{
	const int PLANET_BASE_SIZE = 96;
	const int MOON_BASE_SIZE  = 32;
	const int ASTEROID_BASE_SIZE  = 16;

	float _planetGenerationAreaThreshold = 6000f;
	float _moonGenerationChance = 1.1f / 3.0f;
	int _maxMoonCount = 5;
	float _asteroidGenerationChance = 3.0f / 4.0f;
	int _maxAsteroidCount = 10;

	Vector2[] NEIGHBORS => new Vector2[] 
	{
		new Vector2(1, 0),
		new Vector2(1, 1),
		new Vector2(0, 1),
		new Vector2(-1, 1),
		new Vector2(-1, 0),
		new Vector2(-1, -1),
		new Vector2(0, -1),
		new Vector2(1, -1)
	};

	string[] LAYERS = new string[]
	{
		"Seeds",
		"Planet",
		"Moons",
		"Asteroids",
		"TravelLanes",
	};

	[Export]
	float sectorSize = 1000.0f;
	[Export]
	int sectorAxisCount = 10;
	[Export]
	string startSeed = "world_generation";

	Dictionary<Vector2, Sector> _sectors = new Dictionary<Vector2, Sector>();
	Vector2 _currentSector = Vector2.Zero;
	RandomNumberGenerator _rng = new RandomNumberGenerator(); 

	float _halfSectorSize, _sectorSizeSquared;
	int _halfSectorCount;
	float sectorMarginProportion = .1f;
	float SectorMargin => sectorSize * sectorMarginProportion;


	Universe _universe;
	private Player _player;

	float _sectorSeed;

	List<CellestialObject> _objects = new List<CellestialObject>();
	bool _generated = false;
	float _physicsTimeStep = .1f;

	PackedScene _star;
	PackedScene _planet;
	PackedScene _moon;
	PackedScene _asteroid;

	void OnUniverseMapJumpToSector(MapCell mapCell)
	{
		_generated = false;
		_sectorSeed = mapCell.NoiseSeed;
		_sectors.Clear();
		Generate();
	}

	public override void _Ready()
	{
		base._Ready();

		_halfSectorSize = sectorSize / 2.0f;
		_sectorSizeSquared = sectorSize * sectorSize;
		_halfSectorCount = (int)(sectorAxisCount / 2.0);

		_player = GetNode<Player>("/root/Game/Player");
		_universe = GetNode<Universe>("/root/Universe");

		_star = ResourceLoader.Load<PackedScene>("res://Game/Universe/Sector/Star.tscn");
		_planet = ResourceLoader.Load<PackedScene>("res://Game/Universe/Sector/Planet.tscn");
		_moon = ResourceLoader.Load<PackedScene>("res://Game/Universe/Sector/Moon.tscn");
		_asteroid = ResourceLoader.Load<PackedScene>("res://Game/Universe/Sector/Asteroid.tscn");

		Generate();
	}

	public void Generate()
	{
		var sectorType = Sector.GetSectorType(_sectorSeed);

		var mainStar = _star.Instantiate<CellestialObject>();
		mainStar.Position = new Vector3(0,0, 0);
		mainStar.Scale *= 10;
		mainStar.Mass = 100000000;

		for(var a = 0; a < 10; a++)
		{
			var planet = _planet.Instantiate<CellestialObject>();
			planet.ParentObject = mainStar;
			planet.Scale *= 2f;
			planet.Mass = 10000;

			var distanceFromParent = _rng.RandfRange(100 * a, 1000);
			planet.Position = planet.ParentObject.Position + new Vector3(0, distanceFromParent, 0);
			planet.CurrentVelocity = new Vector3(distanceFromParent / 200, 0, 0);

			var moonsNumber = _rng.RandiRange(0, 2);

			for(var x = 0; x < moonsNumber; x++)
			{
				var moon = _moon.Instantiate<CellestialObject>();
				moon.ParentObject = planet;
				moon.Scale *= 1f;
				moon.Mass = 100f;

				var distanceFromParent2 = _rng.RandfRange(10 * x, 100);
				moon.Position = moon.ParentObject.Position + new Vector3(distanceFromParent2, 0, 0);
				moon.CurrentVelocity = new Vector3(0, distanceFromParent2 / 500, 0);

				planet.ChildObjects.Add(moon);
				_objects.Add(moon);
			}

			mainStar.ChildObjects.Add(planet);
			_objects.Add(planet);
		}
		_objects.Add(mainStar);
		
		foreach(var ob in _objects)
		{
			AddChild(ob);
		}

		_generated = true;

		// var index = -1;
		// for (int i = 0; i < LAYERS.Length; i++)
		// {
		//     index += 1;

		//     for (int x = (int)(_currentSector.x - _halfSectorCount + index); x < _currentSector.x + _halfSectorCount - index; x++)
		//     {
		//         for (int y = (int)(_currentSector.y - _halfSectorCount + index); y < _currentSector.y + _halfSectorCount - index; y++)
		//         {
		//             var sector = new Vector2(x, y);

		//             switch(LAYERS[i])
		//             {
		//                 case "Seeds":
		//                     if(!_sectors.ContainsKey(sector))
		//                         _sectors.Add(sector, new Sector());
								
		//                     GenerateSeedsAt(sector);
		//                     break;
		//                 case "Planet":
		//                     GeneratePlanetsAt(sector);
		//                     break;
		//                 case "Moons":
		//                     GenerateMoonsAt(sector);
		//                     break;
		//                 case "TravelLines":
		//                     GenerateTravelLanesAt(sector);
		//                     break;
		//                 case "Asteroids":
		//                     GenerateAsteroidsAt(sector);
		//                     break;

		//             }
		//         }
		//     }
		// }
		
		// Update();
	}

	private void AddStar()
	{
		_objects.Add(new Star());
	}

	public Vector2 MoveToPlayer(Vector2 v)
	{
		return v - new Vector2(-196, -66);
	}

	public override void _Draw()
	{
		base._Draw();

		// for (int i = 0; i < _sectors.Count; i++)
		// {
		//     var data = _sectors.ElementAt(i);

		//     if(data.Value.Seeds != null)
		//     {
		//         for (int x = 0; x < data.Value.Seeds.Count; x++)
		//         {
		//             var point = data.Value.Seeds[x];
		//             DrawCircle(point, 12, new Color(0.5f, 0.5f, 0.5f, 0.5f));
		//         }
		//     }

		//     if(data.Value.Planet != null)
		//     {
		//         DrawCircle(data.Value.Planet.Position, PLANET_BASE_SIZE * data.Value.Planet.Scale, Color.ColorN("bisque"));
		//     }

		//     if(data.Value.Moons != null)
		//     {
		//         for (int x = 0; x < data.Value.Moons.Count; x++)
		//         {
		//             var moon = data.Value.Moons[x];
		//             DrawCircle(moon.Position, MOON_BASE_SIZE * moon.Scale, Color.ColorN("aquamarine"));
		//         }
		//     }

		//     if(data.Value.TravelLines != null)
		//     {
		//         for (int x = 0; x < data.Value.TravelLines.Count; x++)
		//         {
		//             var travelLine = data.Value.TravelLines[x];
		//             var start = travelLine.Position;
		//             var end = travelLine.Destination;

		//             DrawLine(start, end, Color.ColorN("cornflower"), 6f);
		//         }
		//     }

		//     if(data.Value.Asteroids != null)
		//     {
		//         for (int x = 0; x < data.Value.Asteroids.Count; x++)
		//         {
		//             var asteroid = data.Value.Asteroids[x];
		//             DrawCircle(asteroid.Position, ASTEROID_BASE_SIZE * asteroid.Scale, Color.ColorN("orangered"));
		//         }
		//     }
		// }
	}

	uint MakeSeedFor(float _x_id,float _y_id,string custom_data = "")
	{
		var newSeed = $"{_sectorSeed}_{startSeed}_{_x_id}_{_y_id}"; //%s_%s_%s" % [startSeed, _x_id, _y_id];
		if (!string.IsNullOrEmpty(custom_data)){
			newSeed = $"{newSeed}_{custom_data}";//"%s_%s" % [newSeed, custom_data];
		}

		return newSeed.Hash();
	}

	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

		if(_generated)
		{
			foreach(var ob in _objects)
			{
				ob.UpdateVelocity(_objects.ToArray(), _physicsTimeStep);
			}

			foreach(var ob in _objects)
			{
				ob.UpdatePosition(_physicsTimeStep);
			}
		}

		// var sector_offset = Vector2.Zero;

		// var sector_location = _currentSector * sectorSize;

		// if (_player != null && _player.GlobalPosition.DistanceSquaredTo(sector_location) > _sectorSizeSquared)
		// {

		//     sector_offset = (_player.GlobalPosition - sector_location) / sectorSize;
		//     sector_offset.x = (int)(sector_offset.x);
		//     sector_offset.y = (int)(sector_offset.y);

		//     UpdateSectors(sector_offset);
		// }
	}

	void UpdateSectors(Vector2 difference)
	{
		UpdateAlongAxis(AxisB.AXIS_X, difference.x);
		UpdateAlongAxis(AxisB.AXIS_Y, difference.y);
		Generate();
	}

	void UpdateAlongAxis(AxisB axis,float difference)
	{

		if (difference == 0 || (axis != AxisB.AXIS_X && axis != AxisB.AXIS_Y))
			return;

		var axisModifier = (difference > 0)? 1 : 0;
		var sectorAxisCoordinate = (axis == AxisB.AXIS_X)? _currentSector.x : _currentSector.y;

		var newSectorLineIndex = (int)(sectorAxisCoordinate + (_halfSectorCount - axisModifier) * difference + difference);

		var otherAxisPosition = (axis == AxisB.AXIS_X)? _currentSector.y : _currentSector.x;
		var otherAxisMin = otherAxisPosition - _halfSectorCount;
		var otherAxisMax = otherAxisPosition + _halfSectorCount;

		for (var otherAxisCoordinate = otherAxisMin; otherAxisCoordinate < otherAxisMax; otherAxisCoordinate++)
		{
			var x = (axis == AxisB.AXIS_X)? newSectorLineIndex : otherAxisCoordinate;
			var y = (axis == AxisB.AXIS_X)? otherAxisCoordinate : newSectorLineIndex;
		}

		var obsoleteSectorLineIndex = (int)(newSectorLineIndex + sectorAxisCount * -difference);

		for(var otherAxisCoordinate = otherAxisMin; otherAxisCoordinate < otherAxisMax; otherAxisCoordinate++)
		{
			var key = new Vector2(
				(axis == AxisB.AXIS_X)? obsoleteSectorLineIndex : otherAxisCoordinate,
				(axis == AxisB.AXIS_X)? otherAxisCoordinate : obsoleteSectorLineIndex
			);

			if(_sectors.ContainsKey(key))
			{
				var sectorData = _sectors[key];

				_sectors.Remove(key);
			}
		}

		if (axis == AxisB.AXIS_X)
			_currentSector.x += difference;
		else
			_currentSector.y += difference;
	}

	void GenerateSeedsAt(Vector2 sector)
	{
		if (_sectors[sector].Seeds != null && _sectors[sector].Seeds.Count != 0)
			return;

		_rng.Seed = MakeSeedFor(sector.x, sector.y, "seeds");

		var half_size = new Vector2(_halfSectorSize, _halfSectorSize);
		var margin = new Vector2(SectorMargin, SectorMargin);
		var top_left = sector * sectorSize - half_size + margin;
		var bottom_right = sector * sectorSize + half_size - margin;

		List<Vector2> seeds = new List<Vector2>();

		for (int i = 0; i < 3; i++)
		{
			var seedPosition = new Vector2(_rng.RandfRange(top_left.x, bottom_right.x), _rng.RandfRange(top_left.y, bottom_right.y));
			seeds.Add(seedPosition);
		}

		_sectors[sector].Seeds = seeds;
	}

	void GeneratePlanetsAt(Vector2 sector)
	{
		// if (_sectors[sector].Planet.Scale != 0)
		//     return;

		// var vertices = _sectors[sector].Seeds;
		// var area = CalculateTriangleArea(vertices[0], vertices[1], vertices[2]);

		// if (area < _planetGenerationAreaThreshold)
		//     _sectors[sector].Planet = new Planet()
		//     {
		//         Position = CalculateTriangleEpicenter(vertices[0], vertices[1], vertices[2]),
		//         Scale = .5f + area / (_planetGenerationAreaThreshold / 2f)
		//     };
	}

	void GenerateMoonsAt(Vector2 sector)
	{
		// if (_sectors[sector].Moons.Count != 0)
		//     return;

		// var planet = _sectors[sector].Planet;

		// if(planet.Scale == 0)
		//     return;

		// _rng.Seed = MakeSeedFor(sector.x, sector.y, "Moons");

		// var moonCount = 0;

		// while(_rng.Randf() < _moonGenerationChance || moonCount == _maxMoonCount)
		// {
		//     var randomOffset = Vector2.Up.Rotated(_rng.RandfRange(-Mathf.Pi, Mathf.Pi)) * planet.Scale * PLANET_BASE_SIZE * 3.0f;

		//     moonCount += 1;

		//     _sectors[sector].Moons.Add(new Moon() {
		//         Position = planet.Position + randomOffset,
		//         Scale = planet.Scale / 3f
		//     });
		// }
	}

	void GenerateTravelLanesAt(Vector2 sector)
	{
		// if (_sectors[sector].TravelLines.Count != 0)
		//     return;

		// var planet = _sectors[sector].Planet;

		// if(planet.Scale == 0)
		//     return;

		// for (int i = 0; i < NEIGHBORS.Length; i++)
		// {
		//     var neighbor = NEIGHBORS[i];
		//     var neighborSector = sector + neighbor;

		//     if (!_sectors.ContainsKey(neighborSector))
		//         continue;

		//     var neighborPosition = _sectors[neighborSector].Planet.Position;
		//     _sectors[sector].TravelLines.Add(new TravelLane()
		//     {
		//         Position = planet.Position,
		//         Destination = neighborPosition
		//     });
		// }
	}

	void GenerateAsteroidsAt(Vector2 sector)
	{
		// if (_sectors[sector].Asteroids.Count != 0)
		//     return;

		// var planet = _sectors[sector].Planet;

		// if(planet.Scale == 0 || _sectors[sector].Moons != null || _sectors[sector].TravelLines != null)
		//     return;

		// _rng.Seed = MakeSeedFor(sector.x, sector.y, "asteroids");

		// var count = 0;

		// while (_rng.Randf() < _asteroidGenerationChance && count < _maxAsteroidCount)
		// {
		//     count += 1;

		//     var randomOffset = 
		//         Vector2.Up.Rotated(_rng.RandfRange(-Mathf.Pi, Mathf.Pi))
		//         * planet.Scale
		//         * PLANET_BASE_SIZE
		//         * _rng.RandfRange(3.0f, 4.0f);
			
		//     _sectors[sector].Asteroids.Add(new Asteroid()
		//     {
		//         Position = planet.Position + randomOffset,
		//         Scale = planet.Scale / 5.0f,
		//     });
		// }
			
	}

	float CalculateTriangleArea(Vector2 a, Vector2 b, Vector2 c)
	{
		return Mathf.Abs(a.x * (b.y - c.y) + b.x * (c.y - a.y) + c.x * (a.y - b.y)) / 2.0f;
	}

	Vector2 CalculateTriangleEpicenter(Vector2 a, Vector2 b, Vector2 c)
	{
		return (a + b + c) / 3.0f;
	}
}

public enum AxisB
{
	AXIS_X,
	AXIS_Y,
}

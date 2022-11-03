using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Vector3 = Godot.Vector3;
using Vector2 = Godot.Vector2;

[Tool]
public partial class ShipMeshInstance : MeshInstance3D
{
	RandomNumberGenerator _rng;

	ulong _seed = 1000;
	[Export]
	public ulong Seed 
	{
		get { return _seed; }
		set 
		{ 
			_seed = value;
			_propertyChanged = true;
		}
	}

	public override void _Ready()
	{
		_rng = new RandomNumberGenerator();
	}

	bool _generating, _propertyChanged;

	public override void _Process(double delta)
	{
		base._Process(delta);

		if((!_generating && Time.GetTicksMsec() > 5000) || _propertyChanged)
		{
			_rng.Seed = Seed;
			_generating = true;
			_propertyChanged = false;

			var mesh = ShipMeshFactory.CreateCube();

			var sg = new ShipGenerator3D(_rng);

			Mesh = mesh.ToGodotMesh();

			foreach(var m in sg.Generate(mesh))
			{
				Mesh = m.ToGodotMesh();
			}

			Mesh = mesh.ToGodotMesh();
		}
	}

}

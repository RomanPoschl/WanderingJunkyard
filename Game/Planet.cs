using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[Tool]
public partial class Planet : RigidBody3D
{
	Resource _planetConfiguration;
	[Export]
	public Resource PlanetConfiguration 
	{ 
		get => _planetConfiguration;
		set => _planetConfiguration = SetProperty(value, Changed); 
	}

	T SetProperty<T>(
            T value,
			Action? onChanged = null)
		{
			onChanged?.Invoke();
			return value;
		}

	void Changed()
	{
		RenderPlanet();

		if(PlanetConfiguration != null && !PlanetConfiguration.IsConnected("changed", new Callable(this, "RenderPlanet")))
		{
			PlanetConfiguration.Connect("changed", new Callable(this, "RenderPlanet"));
		}
	}

	public override void _Ready()
	{
		RenderPlanet();
	}

	void RenderPlanet()
	{
		for(var child = 0; child < this.GetChildCount(); child++)
		{
			var childNode = this.GetChild<PlanetMeshFace>(child);
			childNode.RegenerateMesh(PlanetConfiguration);
		}
	}
}
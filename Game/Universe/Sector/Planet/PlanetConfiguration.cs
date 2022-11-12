using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Godot;

[Tool]
public partial class PlanetConfiguration : Resource 
{
    int _resolution = 10;
	[Export]
    public int Resolution { get => _resolution; set => SetProperty(value, onChanged: OnChanged); }

    float _radius = 100;
	[Export]
    public float Radius { get => _radius; set => SetProperty(value, onChanged: OnChanged); }

    Godot.Collections.Array<Resource> _planetNoises;
    [Export]
    public Godot.Collections.Array<Resource> PlanetNoises
    { 
        get => _planetNoises;
        set {
            _planetNoises = SetProperty(value, onChanged: OnChanged);

            if(value == null)
                return;

            foreach(var noise in value)
            {
                if(noise != null && !noise.IsConnected("changed", new Callable(this, "OnChanged")))
                {
                    noise.Connect("changed", new Callable(this, "OnChanged"));
                }
            }
        } 
    }

    void OnChanged()
    {
        EmitSignal("changed");
    }

    T SetProperty<T>(
            T value,
			Action? onChanged = null)
		{
			onChanged?.Invoke();
			return value;
		}
}
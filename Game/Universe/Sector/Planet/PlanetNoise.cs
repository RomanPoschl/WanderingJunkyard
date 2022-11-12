using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[Tool]
public partial class PlanetNoise : Resource
{
    FastNoiseLite _noiseMap = new FastNoiseLite();
	[Export]
    public FastNoiseLite NoiseMap 
    {
        get => _noiseMap;
        set {
            _noiseMap = SetProperty(value, onChanged: OnChanged);

            if(NoiseMap != null && !NoiseMap.IsConnected("changed", new Callable(this, "OnChanged")))
            {
                NoiseMap.Connect("changed", new Callable(this, "OnChanged"));
            }
        }
    }

    float _amplitude = 0;
	[Export]
    public float Amplitude { get => _amplitude; set => _amplitude = SetProperty(value, onChanged: OnChanged); }

    float _minHeight = 0f;
	[Export]
    public float MinHeight { get => _minHeight; set => _minHeight = SetProperty(value, onChanged: OnChanged); }

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
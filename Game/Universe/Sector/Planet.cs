using Godot;

public partial class PlanetTest : CellestialObject
{

    int resolution = 1000;
    Vector2 _center;

    public override void _Ready()
    {
        base._Ready();
        Name = "Planet_";
    }

    public enum PlanetType
    {
        ROCK_NO_ATMO,
        ROCK_ATMO,
        GAS
    }
}
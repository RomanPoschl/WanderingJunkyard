public partial class Planet : CellestialObject
{

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
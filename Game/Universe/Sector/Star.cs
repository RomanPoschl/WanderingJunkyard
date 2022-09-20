public class Star : CellestialObject
{
    float _temperature;

    public override void _Ready()
    {
        base._Ready();
        Name = "Star_";
    }
}
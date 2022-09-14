using Godot;

public class ShipMeshVertex
{
    internal int Index {get; set;}

    public Vector3 Coordinates { get; set; }

    public ShipMeshVertex(Vector3 coordinates)
    {
        this.Coordinates = coordinates;
    }

    internal ShipMeshVertex Clone()
    {
        return new ShipMeshVertex(this.Coordinates);
    }
}

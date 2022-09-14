using System.Collections.Generic;
using System.Linq;
using Godot;

public class ShipMeshTriangleFace : ShipMeshFace
{
    public ShipMeshTriangleFace(ShipMeshVertex a, ShipMeshVertex b, ShipMeshVertex c) : base(new ShipMeshVertex[] { a, b, c })
    {
    }

    public override Vector3 Normal
    {
        get
        {
            throw new System.NotImplementedException();
        }
    }

    public override Vector2 Size { get { throw new System.NotImplementedException(); } }

    public override float AspectRatio { get { throw new System.NotImplementedException(); } }

    public override float Width { get { throw new System.NotImplementedException(); } }

    public override float Height { get { throw new System.NotImplementedException(); } }

    public override ShipMeshVertex LeftTop { get { throw new System.NotImplementedException(); } }

    public override ShipMeshVertex LeftBottom { get { throw new System.NotImplementedException(); } }

    public override ShipMeshVertex RightBottom { get { throw new System.NotImplementedException(); } }

    public override ShipMeshVertex RightTop { get { throw new System.NotImplementedException(); } }

    public override float Area()
    {
        throw new System.NotImplementedException();
    }

    public override ShipMeshFace Clone()
    {
        return new ShipMeshTriangleFace(this.Vertices[0].Clone(), this.Vertices[1].Clone(), this.Vertices[2].Clone());
    }

    public override ShipMeshFace[] Extrude()
    {
        throw new System.NotImplementedException();
    }

    public override int[] GetTriangles()
    {
        return new int[]
        {
            this.Vertices[0].Index,
            this.Vertices[1].Index,
            this.Vertices[2].Index
        };
    }

    public override ShipMeshFace[] Subdivide(int numberOfCuts)
    {
        var faces = new List<ShipMeshFace>() { this };

        for (var i = 0; i < numberOfCuts; i++)
        {
            var tmpFaces = faces.ToList();
            faces.Clear();

            foreach (var face in tmpFaces.ToList())
            {
                var a = GetMidpoint(face.Vertices[1], face.Vertices[0]);
                var b = GetMidpoint(face.Vertices[2], face.Vertices[1]);
                var c = GetMidpoint(face.Vertices[0], face.Vertices[2]);


                faces.Add(new ShipMeshTriangleFace(face.Vertices[0], new ShipMeshVertex(a), new ShipMeshVertex(c)));
                faces.Add(new ShipMeshTriangleFace(face.Vertices[1], new ShipMeshVertex(b), new ShipMeshVertex(a)));
                faces.Add(new ShipMeshTriangleFace(face.Vertices[2], new ShipMeshVertex(c), new ShipMeshVertex(b)));
                faces.Add(new ShipMeshTriangleFace(new ShipMeshVertex(a), new ShipMeshVertex(b), new ShipMeshVertex(c)));
            }
        }

        return faces.ToArray();
    }

    private static Vector3 GetMidpoint(ShipMeshVertex a, ShipMeshVertex b)
    {
        var p = (a.Coordinates + b.Coordinates) / 2;

        var length = Mathf.Sqrt(p.x * p.x + p.y * p.y + p.z * p.z);

        return new Vector3(p.x / length, p.y / length, p.z / length);
    }
}
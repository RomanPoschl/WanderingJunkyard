using System;
using System.Collections.Generic;
using Godot;

public partial class ShipMeshSquareFace : ShipMeshFace
{
    public override Vector3 Normal
    {
        get
        {
            var a = RightBottom.Coordinates - LeftBottom.Coordinates;
            var b = LeftTop.Coordinates - LeftBottom.Coordinates;

            return a.Cross(-b).Normalized();
        }
    }

    public override Vector2 Size
    {
        get
        {
            var width = Mathf.Abs((LeftTop.Coordinates - RightTop.Coordinates).Length());
            var height = Mathf.Abs(LeftTop.Coordinates.y - LeftBottom.Coordinates.y);

            return new Vector2(width, height);
        }
    }

    public override float AspectRatio
    {
        get
        {
            var faceAspectRatio = Mathf.Max(
                0.01f,
                (LeftTop.Coordinates - RightTop.Coordinates).Length() / (LeftTop.Coordinates - LeftBottom.Coordinates).Length());

            if (faceAspectRatio < 1.0f)
            {
                faceAspectRatio = 1.0f / faceAspectRatio;
            }

            return faceAspectRatio;
        }
    }

    public override ShipMeshVertex LeftTop { get { return Vertices[0]; } }
    public override ShipMeshVertex LeftBottom { get { return Vertices[1]; } }
    public override ShipMeshVertex RightBottom { get { return Vertices[2]; } }
    public override ShipMeshVertex RightTop { get { return Vertices[3]; } }

    public override float Width { get { return (RightTop.Coordinates - LeftTop.Coordinates).Length(); } }
    public override float Height { get { return (LeftBottom.Coordinates - LeftTop.Coordinates).Length(); } }

    public ShipMeshSquareFace() : base(new ShipMeshVertex[4])
    {
    }

    public ShipMeshSquareFace(ShipMeshVertex leftTop, ShipMeshVertex leftBottom, ShipMeshVertex rightBottom, ShipMeshVertex rightTop) : this()
    {
        this.Vertices[0] = leftTop;
        this.Vertices[1] = leftBottom;
        this.Vertices[2] = rightBottom;
        this.Vertices[3] = rightTop;
    }

    public override ShipMeshFace Clone()
    {
        return new ShipMeshSquareFace(
            this.LeftTop.Clone(),
            this.LeftBottom.Clone(),
            this.RightBottom.Clone(),
            this.RightTop.Clone()
        );
    }

    public override int[] GetTriangles()
    {
        return new int[]
        {
            LeftTop.Index,
            RightBottom.Index,
            LeftBottom.Index,

            LeftTop.Index,
            RightTop.Index,
            RightBottom.Index,
        };
    }

    public override ShipMeshFace[] Extrude()
    {
        var frontFace = this.Clone() as ShipMeshSquareFace;
        var leftFace = new ShipMeshSquareFace(this.LeftTop, this.LeftBottom, frontFace.LeftBottom, frontFace.LeftTop);
        var topFace = new ShipMeshSquareFace(this.LeftTop, frontFace.LeftTop, frontFace.RightTop, this.RightTop);
        var rightFace = new ShipMeshSquareFace(frontFace.RightTop, frontFace.RightBottom, this.RightBottom, this.RightTop);
        var bottomFace = new ShipMeshSquareFace(frontFace.LeftBottom, this.LeftBottom, this.RightBottom, frontFace.RightBottom);

        return new ShipMeshFace[]
        {
            frontFace,
            leftFace,
            topFace,
            rightFace,
            bottomFace
        };
    }

    public override ShipMeshFace[] Subdivide(int numberOfCuts)
    {
        var steps = numberOfCuts + 2;

        var topSize = (this.RightTop.Coordinates - this.LeftTop.Coordinates).Length();
        var bottomSize = (this.RightBottom.Coordinates - this.LeftBottom.Coordinates).Length();
        var leftSize = (this.LeftTop.Coordinates - this.LeftBottom.Coordinates).Length();
        var rightSize = (this.RightTop.Coordinates - this.RightBottom.Coordinates).Length();

        var result = new List<ShipMeshFace>();

        var topPoints = new List<Vector3>();
        var bottomPoints = new List<Vector3>();
        var leftPoints = new List<Vector3>();
        var rightPoints = new List<Vector3>();


        for (var i = 0; i < steps; i++)
        {
            // horizontal
            var hfrom = this.LeftTop.Coordinates.Lerp(this.RightTop.Coordinates, ((float)i) / (steps - 1));
            var hto = this.LeftBottom.Coordinates.Lerp(this.RightBottom.Coordinates, ((float)i) / (steps - 1));

            // vertical
            var vfrom = this.LeftTop.Coordinates.Lerp(this.LeftBottom.Coordinates, ((float)i) / (steps - 1));
            var vto = this.RightTop.Coordinates.Lerp(this.RightBottom.Coordinates, ((float)i) / (steps - 1));

            topPoints.Add(hfrom);
            bottomPoints.Add(hto);
            leftPoints.Add(vfrom);
            rightPoints.Add(vto);
        }

        var points = new Vector3[steps, steps];

        for (var x = 0; x < steps; x++)
        {
            for (var y = 0; y < steps; y++)
            {
                var top = topPoints[x];
                var bottom = bottomPoints[x];
                var left = leftPoints[y];
                var right = rightPoints[y];

                Vector3 intersection;
                if (!ShipMesh.LineLineIntersection(out intersection, top, (bottom - top), left, (right - left)))
                {
                    throw new InvalidOperationException("Points here should always intersect");
                }

                points[x, y] = intersection;
            }
        }

        for (var x = 0; x < steps - 1; x++)
        {
            for (var y = 0; y < steps - 1; y++)
            {
                result.Add(new ShipMeshSquareFace(
                    new ShipMeshVertex(points[x, y]),
                    new ShipMeshVertex(points[x, y + 1]),
                    new ShipMeshVertex(points[x + 1, y + 1]),
                    new ShipMeshVertex(points[x + 1, y])));
            }
        }

        return result.ToArray();
    }

    public override float Area3D()
    {
        return this.Width * this.Height;
    }
}
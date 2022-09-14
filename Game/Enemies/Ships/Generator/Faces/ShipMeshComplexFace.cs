using System;
using System.Linq;
using Godot;

public class ShipMeshComplexFace : ShipMeshFace
{
    private int[] triangles;

        public ShipMeshComplexFace(ShipMeshVertex[] vertices, int[] triangles) : base(vertices)
        {
            this.triangles = triangles;
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
            return new ShipMeshComplexFace(Vertices, triangles);
        }

        public override ShipMeshFace[] Extrude()
        {
            throw new System.NotImplementedException();
        }

        public override int[] GetTriangles()
        {
            return this.triangles.Select(e => Vertices.ElementAt(e).Index).ToArray();
        }

        public override ShipMeshFace[] Subdivide(int numberOfCuts)
        {
            throw new System.NotImplementedException();
        }
}

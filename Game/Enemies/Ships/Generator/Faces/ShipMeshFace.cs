using Godot;

public abstract class ShipMeshFace
{
    public abstract Vector3 Normal { get; }

        public abstract Vector2 Size { get; }

        public ShipMeshVertex[] Vertices { get; private set; }
        public abstract float AspectRatio { get; }

        public int MaterialIndex { get; internal set; }
        public abstract float Width { get; }
        public abstract float Height { get; }

        public abstract ShipMeshVertex LeftTop { get; }
        public abstract ShipMeshVertex LeftBottom { get; }
        public abstract ShipMeshVertex RightBottom { get; }
        public abstract ShipMeshVertex RightTop { get; }

        public ShipMeshFace(ShipMeshVertex[] vertices)
        {
            this.Vertices = vertices;
        }

        public Vector3 CalculateCenterBounds()
        {
            var sum = Vector3.Zero;

            foreach (var vertex in this.Vertices)
            {
                sum += vertex.Coordinates;
            }

            return sum / this.Vertices.Length;
        }

        public abstract ShipMeshFace Clone();

        public abstract int[] GetTriangles();

        public abstract ShipMeshFace[] Extrude();

        public abstract ShipMeshFace[] Subdivide(int numberOfCuts);

        public abstract float Area();
}

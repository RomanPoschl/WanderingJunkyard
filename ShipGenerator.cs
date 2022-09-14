using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

[Tool]
public class ShipGenerator : Node2D
{
    ulong _seed = 8;
    [Export]
    public ulong Seed 
    { 
        get => _seed;
        set 
        { 
            _seed = value;
            Update();
        } 
    }

    RandomNumberGenerator _rng;

    public override void _Ready()
    {
        _rng = new RandomNumberGenerator();

        base._Ready();
    }

    public override void _Draw()
    {
        base._Draw();
        _rng.Seed = Seed;
        var tri = new Shape2D(
            new Edge[] 
            {
                new Edge(
                    new ShipVertex(-50, -50),
                    new ShipVertex(50, -50)
                ),
                new Edge(
                    new ShipVertex(50, -50),
                    new ShipVertex(50, 50)
                ),
                new Edge(
                    new ShipVertex(50, 50),
                    new ShipVertex(-50, 50)
                ),
                new Edge(
                    new ShipVertex(-50, 50),
                    new ShipVertex(-50, -50)
                )
            }
        );

        foreach(var m in Generate(tri))
        {
            //DrawColoredPolygon(m.GetVertices().Select(x => x.Coord).ToArray(), Colors.Orange);
        }

        DrawPolyline(tri.GetVertices().Select(x => x.Coord).ToArray(), Colors.Orange, width: 2);
        DrawColoredPolygon(tri.GetVertices().Select(x => x.Coord).ToArray(), Colors.Orange);
    }

    public IEnumerable<Object2D> Generate(Object2D object2D)
    {
        // Make hull
        var edges = object2D.Edges;

        for (int i = 0; i < edges.Length; i++)
        {
            var edge = edges[i];

            // Look for forward edge
            if(edge.A.Coord.DirectionTo(edge.B.Coord) == new Vector2(-1, 0))
            {
                var segmentLength = _rng.RandfRange(0.3f, 1.0f) * 100;
                var numberOfSegments = _rng.RandiRange(1, 6);

                for (int s = 0; s < numberOfSegments; s++)
                {
                    if(_rng.Randf() > .1f)
                    {
                        //Normal extrude
                        var newEdge = ExtrudeEdge(object2D, edge, new Vector2(0, 1), segmentLength);

                        if (_rng.Randf() > 0.75f)
                        {
                            newEdge = ExtrudeEdge(object2D, edge, new Vector2(0, 1), segmentLength * 0.25f);
                        }

                        edge = newEdge;
                    }
                }
            }
        }
        
        GD.Print(object2D.ToString());
        yield return object2D;
    }

    public Edge ExtrudeEdge(Object2D object2D, Edge edge, Vector2 dir, float length)
    {
        var newEdges = object2D.ExtrudeEdge(edge, dir, length);
        return newEdges[0];
    }

    public abstract class Object2D
    {
        public Edge[] Edges => _edges.ToArray();
        private List<Edge> _edges;

        protected Object2D(Edge[] edges)
        {
            this._edges = new List<Edge>(edges);
        }

        public Edge[] ExtrudeEdge(Edge edge, Vector2 dir, float length)
        {
            var newEdges = new List<Edge>();

            var edgeIndex = _edges.IndexOf(edge);
            _edges.Remove(edge);
            
            var clonedOriginal = edge.Clone();
            clonedOriginal.Translate(dir, length);

            newEdges.Add(edge);
            newEdges.Add(new Edge(edge.A, clonedOriginal.A));
            newEdges.Add(clonedOriginal);
            newEdges.Add(new Edge(clonedOriginal.B, edge.B));

            _edges.InsertRange(edgeIndex, newEdges);

            return newEdges.ToArray();
        }
    
        public List<ShipVertex> GetVertices()
        {
            var result = new List<ShipVertex>();

            foreach(var edge in _edges)
            {
                result.Add(edge.A);
                result.Add(edge.B);
            }

            GD.Print($"Vertices count {result.Count}");
            return result;
        }
    
        public override string ToString()
        {
            var result = $"Object2D Vertices: {System.Environment.NewLine}";

            foreach(var edge in _edges)
            {
                result += $"Edge: {System.Environment.NewLine}{edge}{System.Environment.NewLine}";
            }

            return result;
        }
    }
    public class Triangle : Object2D
    {
        public Triangle(ShipVertex a, ShipVertex b, ShipVertex c) : base(new Edge[] {new Edge(a, b), new Edge(b,c), new Edge(c, a)})
        {
        }
    }
    public class Shape2D : Object2D
    {
        public Shape2D(Edge[] edges) : base(edges)
        {
        }
    }
    public class Edge
    {
        public ShipVertex A { get; internal set; }
        public ShipVertex B { get; internal set; }

        public Edge(ShipVertex a, ShipVertex b)
        {
            A = a;
            B = b;
        }

        public Edge(Vector2 a, Vector2 b)
        {
            A = new ShipVertex(a);
            B = new ShipVertex(b);
        }

        internal Edge Clone()
        {
            return new Edge(A.Clone(), B.Clone());
        }

        internal void Translate(Vector2 dir, float length)
        {
            this.A.Coord += (dir.Normalized() * length);
            this.B.Coord += (dir.Normalized() * length);
        }

        public override string ToString()
        {
            return $"A: {A.ToString()}{System.Environment.NewLine}B: {B.ToString()}";
        }
    }
    public class ShipVertex
    {
        public Vector2 Coord { get; set; }

        public ShipVertex(Vector2 coord)
        {
            Coord = coord;
        }

        public ShipVertex(float x, float y)
        {
            Coord = new Vector2(x, y);
        }

        public ShipVertex Clone()
        {
            return new ShipVertex(Coord);
        }

        public override string ToString()
        {
            return $"ShipVertex.x {Coord.x} .y {Coord.y}";
        }
    }
}

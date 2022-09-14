using Godot;
using System;
using System.Linq;

public class MapCell : Area2D
{
    Polygon2D _poly;
    CollisionPolygon2D _collisionPoly;
    Vector2[] _shape;

    Layout _layout = new Layout(Layout.flat, new Point(50, 50), new Point(0, 0));

    public Hex HexPosition { get; set; }
    public OffsetCoord OffsetCoordFromHex => HexPosition != null ? OffsetCoord.QoffsetFromCube(OffsetCoord.EVEN, HexPosition) : null;
    public float NoiseSeed { get; set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _poly = GetNode<Polygon2D>("Polygon2D");
        _collisionPoly = GetNode<CollisionPolygon2D>("CollisionPolygon2D");

        Position = MoveToPlayer(OffsetCoordFromHex.ToVector2());
    }

    public Vector2 MoveToPlayer(Vector2 v)
    {
        return v - new Vector2(-196, -66);
    }

    public override bool Equals(object obj)
    {
        if(obj is MapCell mc)
            return HexPosition.Q == mc.HexPosition.Q && HexPosition.R == mc.HexPosition.R && HexPosition.S == mc.HexPosition.S;

        return base.Equals(obj);
    }

    internal void InitShape()
    {
        var cs = _layout.PolygonCorners(HexPosition);
        SetShape(cs.Select(x => new Vector2((float)x.x, (float)x.y)).ToArray());
    }

    public void SetShape(Vector2[] shape)
    {
        if(_poly == null || _collisionPoly == null)
            return;
        
        _poly.Polygon = shape;
        var color = Colors.AliceBlue;
        color.a = .2f;
        _poly.Color = color;

        _collisionPoly.Polygon = shape;

        _shape = shape;
    }
}

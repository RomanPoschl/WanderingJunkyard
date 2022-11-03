using Godot;
using System;
using System.Linq;

public partial class MapCell : Area2D
{
    Polygon2D _poly;
    CollisionPolygon2D _collisionPoly;
    Vector2[] _shape;
    readonly Layout _layout = new(Layout.flat, new Point(50, 50), new Point(0, 0));

    public Hex HexPosition { get; set; }
    public OffsetCoord OffsetCoordFromHex => HexPosition != null ? OffsetCoord.QoffsetFromCube(OffsetCoord.EVEN, HexPosition) : null;
    public float NoiseSeed { get; set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _poly = GetNode<Polygon2D>("Polygon2D");
        _collisionPoly = GetNode<CollisionPolygon2D>("CollisionPolygon2D");

        Position = OffsetCoordFromHex.ToVector2();
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
        SetShape(cs.Select(x => new Vector2(-(float)x.x, -(float)x.y)).ToArray());
    }

    public void SetShape(Vector2[] shape)
    {
        if(_poly == null || _collisionPoly == null)
            return;
        
        _poly.Polygon = shape;
        ResetColor();

        _collisionPoly.Polygon = shape;

        _shape = shape;
    }

    internal void SetColor(Color color)
    {
        color.a = .2f;
        _poly.Color = color;
    }

    internal void ResetColor()
    {
        var color = Colors.AliceBlue;
        color.a = .2f;
        _poly.Color = color;
    }

    void OnMapCellInputEvent(Node viewPort, InputEvent @event, int shapeId)
    {

    }

    public override int GetHashCode()
    {
        HashCode hash = new HashCode();
        hash.Add(NativeInstance);
        hash.Add(_ImportPath);
        hash.Add(Name);
        hash.Add(UniqueNameInOwner);
        hash.Add(SceneFilePath);
        hash.Add(Owner);
        hash.Add(Multiplayer);
        hash.Add(ProcessMode);
        hash.Add(ProcessPriority);
        hash.Add(EditorDescription);
        hash.Add(Visible);
        hash.Add(Modulate);
        hash.Add(SelfModulate);
        hash.Add(ShowBehindParent);
        hash.Add(TopLevel);
        hash.Add(ClipChildren);
        hash.Add(LightMask);
        hash.Add(TextureFilter);
        hash.Add(TextureRepeat);
        hash.Add(Material);
        hash.Add(UseParentMaterial);
        hash.Add(Position);
        hash.Add(Rotation);
        hash.Add(Scale);
        hash.Add(Skew);
        hash.Add(Transform);
        hash.Add(GlobalPosition);
        hash.Add(GlobalRotation);
        hash.Add(GlobalScale);
        hash.Add(GlobalSkew);
        hash.Add(GlobalTransform);
        hash.Add(ZIndex);
        hash.Add(ZAsRelative);
        hash.Add(YSortEnabled);
        hash.Add(DisableMode);
        hash.Add(CollisionLayer);
        hash.Add(CollisionMask);
        hash.Add(CollisionPriority);
        hash.Add(InputPickable);
        hash.Add(Monitoring);
        hash.Add(Monitorable);
        hash.Add(Priority);
        hash.Add(GravitySpaceOverride);
        hash.Add(GravityPoint);
        hash.Add(GravityPointDistanceScale);
        hash.Add(GravityPointCenter);
        hash.Add(GravityDirection);
        hash.Add(Gravity);
        hash.Add(LinearDampSpaceOverride);
        hash.Add(LinearDamp);
        hash.Add(AngularDampSpaceOverride);
        hash.Add(AngularDamp);
        hash.Add(AudioBusOverride);
        hash.Add(AudioBusName);
        hash.Add(_poly);
        hash.Add(_collisionPoly);
        hash.Add(_shape);
        hash.Add(_layout);
        hash.Add(HexPosition);
        hash.Add(OffsetCoordFromHex);
        hash.Add(NoiseSeed);
        return hash.ToHashCode();
    }
}

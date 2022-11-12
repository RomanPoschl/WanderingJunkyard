using Godot;

public partial class Storage : Node
{
    RID _defaultMapRID;

    public void SetDefaultMap(RID defaultMapRID) => _defaultMapRID = defaultMapRID;
    public RID GetDefaultMap() => _defaultMapRID;
}
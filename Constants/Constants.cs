using System.Data;

public partial class Constants
{
    public const string Events = "/root/Events";
    public const string Storage = "/root/Storage";
    public const string Universe = "/root/Universe";
    public const string MapView = "MapView";

    public partial class PlayerConstants
    {
        public static string CollisionShape3D = "CollisionShape3D";
    }

    public partial class MovementConstants
    {
        public static string MoveRight = "move_right";
        public static string MoveLeft = "move_left";
        public static string MoveUp = "move_up";
        public static string MoveDown = "move_down";
    }

    public partial class UIConstants
    {
        public const string MapDisplay = "MapDisplay";

        public const string UniverseMapPopUp = "UI/PopupPanel";
        public const string UniveseMapClose = "UI/PopupPanel/CloseButton";
        public const string UniveseMap = "UI/PopupPanel/Panel/UniverseMap";

        public const string BuildMenuPopup = "UI/BuildMenu";
        public const string BuildItemsContainer = "VBoxContainer/ScrollContainer/HBoxContainer";
    }

    public partial class HUDConstants
    {

    }
}

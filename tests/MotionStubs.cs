namespace StardewValley
{
    public class GameLocation { }
    public static class Game1
    {
        public static bool eventUp;
        public static object? activeClickableMenu;
        public static Farmer player = new();
        public static GameLocation currentLocation = new();
    }
    public class Farmer
    {
        public StardewValley.Tools.MeleeWeapon? CurrentTool;
        public bool UsingTool;
        public bool CanMove = true;
        public int FacingDirection;
        public FarmerSprite FarmerSprite = new();
    }
    public class FarmerSprite
    {
        public bool PauseForSingleAnimation;
        public int Calls;
        public sealed class AnimationFrame
        {
            public AnimationFrame(int frame, int ms, int yOffset, bool arm, bool flip) { }
        }
        public void animateOnce(AnimationFrame[] frames)
        {
            Calls++;
            // Like the game's method, animateOnce acquires single-animation ownership.
            PauseForSingleAnimation = true;
        }
    }
}
namespace StardewValley.Tools { public class MeleeWeapon { } }
namespace StardewModdingAPI { public static class Context { public static bool IsWorldReady = true; } }

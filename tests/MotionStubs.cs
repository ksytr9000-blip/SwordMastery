namespace StardewValley
{
    public class GameLocation
    {
        public int Kills;
        public Farmer? LastKiller;
        public StardewValley.Monsters.Monster? LastMonster;
        public Microsoft.Xna.Framework.Rectangle LastBox;
        public bool LastBomb;
        private void onMonsterKilled(Farmer farmer, StardewValley.Monsters.Monster monster, Microsoft.Xna.Framework.Rectangle box, bool bomb)
        {
            Kills++; LastKiller = farmer; LastMonster = monster; LastBox = box; LastBomb = bomb;
        }
    }
    public static class Game1
    {
        public static int ToolPresses;
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        public static bool pressUseToolButton() { ToolPresses++; return true; }
        public static bool eventUp;
        public static object? activeClickableMenu;
        public static Farmer player = new();
        public static GameLocation currentLocation = new();
    }
    public class Farmer
    {
        public StardewValley.Tools.MeleeWeapon? CurrentTool;
        public int FireCalls;
        public int WeaponCalls;
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        public void FireTool() { FireCalls++; }
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
namespace StardewValley.Tools
{
    public class MeleeWeapon
    {
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        public void leftClick(StardewValley.Farmer who) { who.WeaponCalls++; }
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        public void animateSpecialMove(StardewValley.Farmer who) { who.WeaponCalls++; }
    }
}
namespace StardewModdingAPI { public static class Context { public static bool IsWorldReady = true; } }

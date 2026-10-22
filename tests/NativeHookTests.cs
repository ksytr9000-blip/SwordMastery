using StardewValley;
using StardewValley.Tools;
using StardewModdingAPI;
using SwordMastery;

internal static class NativeHookTests
{
    public static void Run(Action<bool, string> check)
    {
        var instance = ModEntry.Create(); var weapon = new MeleeWeapon();
        var farmer = new Farmer { UsingTool = true, CurrentTool = weapon };
        farmer.FarmerSprite.PauseForSingleAnimation = true;
        Game1.player = farmer; Game1.eventUp = false; Game1.activeClickableMenu = null;
        for (int i = 0; i < 100; i++) ModEntry.Invoke(weapon, farmer, false);
        check(instance.Waves == 0, "100 cancelled original attacks emit no waves");
        for (int i = 0; i < 100; i++) ModEntry.Invoke(weapon, farmer, true);
        check(instance.Waves == 100, "100 actual native attack starts emit exactly 100 wave requests");
        farmer.UsingTool = false; ModEntry.Invoke(weapon, farmer, true);
        check(instance.Waves == 100, "no wave when native tool action did not start");
        farmer.UsingTool = true; farmer.FarmerSprite.PauseForSingleAnimation = false;
        ModEntry.Invoke(weapon, farmer, true);
        check(instance.Waves == 100, "no wave without native animation");
        farmer.FarmerSprite.PauseForSingleAnimation = true; farmer.CurrentTool = new MeleeWeapon();
        ModEntry.Invoke(weapon, farmer, true);
        check(instance.Waves == 100, "swapped weapon is rejected");
        farmer.CurrentTool = weapon; Game1.eventUp = true;
        ModEntry.Invoke(weapon, farmer, true);
        check(instance.Waves == 100, "event cannot emit a wave");
        Game1.eventUp = false; Game1.activeClickableMenu = new object();
        ModEntry.Invoke(weapon, farmer, true);
        check(instance.Waves == 100, "menu cannot emit a wave");
        Game1.activeClickableMenu = null;
        ModEntry.Invoke(weapon, new Farmer { UsingTool = true, CurrentTool = weapon }, true);
        check(instance.Waves == 100, "remote player cannot emit local waves");
        Context.IsWorldReady = false; ModEntry.Invoke(weapon, farmer, true);
        check(instance.Waves == 100, "title screen cannot emit a wave");
        Context.IsWorldReady = true;
    }
}

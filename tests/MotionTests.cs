using StardewValley;
using SwordMastery.Services;

internal static class MotionTests
{
    public static void Run(Action<bool, string> check)
    {
        var farmer = new Farmer();
        Game1.player = farmer;
        var service = new SkillMotionService(() => true);
        // Repeated skill inputs must not replace an in-progress native attack's animation.
        farmer.UsingTool = true; farmer.CanMove = false; farmer.FarmerSprite.PauseForSingleAnimation = true;
        foreach (SkillMotion motion in Enum.GetValues<SkillMotion>())
            for (int i = 0; i < 100; i++) service.Play(farmer, motion, 10);
        check(farmer.FarmerSprite.Calls == 0, "600 overlapping skill poses preserve native animation");
        check(farmer.UsingTool && !farmer.CanMove && farmer.FarmerSprite.PauseForSingleAnimation, "native lock ownership untouched");
        farmer.UsingTool = false;
        service.Play(farmer, SkillMotion.Slash);
        check(farmer.FarmerSprite.Calls == 0, "movement lock without UsingTool is respected");
        farmer.CanMove = true;
        service.Play(farmer, SkillMotion.Slash);
        check(farmer.FarmerSprite.Calls == 0, "paused native animation without UsingTool is respected");
        farmer.FarmerSprite.PauseForSingleAnimation = false;
        service.Play(farmer, SkillMotion.Slash);
        check(farmer.FarmerSprite.Calls == 1, "idle player still gets skill animation");
        service.Reset();
        check(farmer.FarmerSprite.PauseForSingleAnimation, "Reset never clears a native sprite lock");
        farmer.FarmerSprite.PauseForSingleAnimation = false;
        Game1.eventUp = true;
        service.Play(farmer, SkillMotion.Slash);
        check(farmer.FarmerSprite.Calls == 1, "event keeps animation ownership");
        Game1.eventUp = false; Game1.activeClickableMenu = new object();
        service.Play(farmer, SkillMotion.Slash);
        check(farmer.FarmerSprite.Calls == 1, "menu keeps animation ownership");
        Game1.activeClickableMenu = null;
        service.Play(new Farmer(), SkillMotion.Slash);
        check(farmer.FarmerSprite.Calls == 1, "remote farmer never modified");
    }
}

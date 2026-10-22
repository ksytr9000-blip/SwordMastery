using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;
using SwordMastery.Integrations;

namespace PeacefulValleyCowardlyGunfighter.Services
{
    public static class RollSkill
    {
        public static Farmer? ActiveFarmer;
        internal static bool IsRolling(Farmer farmer) => ReferenceEquals(ActiveFarmer, farmer);
    }
}
internal static class RollGuardTests
{
    private sealed class Helper : IModHelper, IModRegistry, IModInfo, IManifest
    {
        public bool Installed;
        public IModRegistry ModRegistry => this;
        public IManifest Manifest => this;
        public string Version => "0.12.11";
        public IModInfo? Get(string id) => Installed && id == "ValleyCoward.CowardlyGunfighter" ? this : null;
    }
    private sealed class Monitor : IMonitor
    {
        public List<string> Messages = new();
        public void Log(string message, LogLevel level) => Messages.Add(message);
    }
    public static void Run(Action<bool, string> check)
    {
        var helper = new Helper(); var monitor = new Monitor();
        Context.IsWorldReady = true;
        var local = new Farmer(); var remote = new Farmer(); var weapon = new MeleeWeapon();
        Game1.player = local;
        PeacefulValleyCowardlyGunfighter.Services.RollSkill.ActiveFarmer = local;
        RollAttackCompatibility.Install(helper, monitor, "SwordMastery.Test.NoRoll");
        check(!RollAttackCompatibility.IsRolling(local), "no installed roll mod does not block attacks");
        helper.Installed = true;
        RollAttackCompatibility.Install(helper, monitor, "SwordMastery.Test.RollGuard");
        check(monitor.Messages.Any(s => s.Contains("4/4 targets installed")), "all native roll guards patched");
        Game1.ToolPresses = 0;
        for (int i = 0; i < 100; i++)
        {
            local.CanMove = false; local.UsingTool = false;
            PeacefulValleyCowardlyGunfighter.Services.RollSkill.ActiveFarmer = local;
            check(!Game1.pressUseToolButton(), "tool input rejected during roll");
            local.FireTool(); weapon.leftClick(local); weapon.animateSpecialMove(local);
            check(local.FireCalls == i && local.WeaponCalls == i * 2 && Game1.ToolPresses == i,
                "no queued tool event, combo or special body executes during roll");
            check(!local.CanMove && !local.UsingTool, "attack guard does not alter roll-owned state");
            check(!RollAttackCompatibility.IsRolling(remote), "remote farmer is not blocked by local roll");
            PeacefulValleyCowardlyGunfighter.Services.RollSkill.ActiveFarmer = null;
            local.CanMove = true;
            check(Game1.pressUseToolButton(), "first input after roll immediately accepted");
            local.FireTool(); weapon.leftClick(local); weapon.animateSpecialMove(local);
            check(local.FireCalls == i + 1 && local.WeaponCalls == (i + 1) * 2 && Game1.ToolPresses == i + 1,
                "normal attack entries restored without delay");
        }
        PeacefulValleyCowardlyGunfighter.Services.RollSkill.ActiveFarmer = local;
        int count = remote.WeaponCalls;
        weapon.leftClick(remote); weapon.animateSpecialMove(remote);
        check(remote.WeaponCalls == count + 2, "remote attack bodies still run during local roll");
        PeacefulValleyCowardlyGunfighter.Services.RollSkill.ActiveFarmer = null;
    }
}

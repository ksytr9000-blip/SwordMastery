using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;
using SwordMastery.Integrations;

internal static class WolCompatibilityTests
{
    private sealed class Helper : IModHelper, IModRegistry, IModInfo, IManifest
    {
        public IModRegistry ModRegistry => this;
        public IManifest Manifest => this;
        public string Version { get; set; } = "2.2.6";
        public bool Installed = true;
        public IModInfo? Get(string id) => Installed && id == "DaLion.Core" ? this : null;
    }
    private sealed class Monitor : IMonitor
    {
        public List<string> Messages = new();
        public void Log(string message, LogLevel level) => Messages.Add(message);
    }
    private sealed class ThreeArgumentLocation
    {
        public int Calls;
        private void onMonsterKilled(Farmer farmer, Monster monster, Rectangle box) => Calls++;
    }
    private sealed class InvalidLocation
    {
        private static void onMonsterKilled(Farmer farmer, Monster monster, Rectangle box, bool bomb) { }
    }
    public static void Run(Action<bool, string> check)
    {
        var helper = new Helper(); var monitor = new Monitor();
        helper.Installed = false;
        WalkOfLifeCompatibility.Install(helper, monitor, "SwordMastery.Test.Absent");
        check(monitor.Messages.Count == 0, "absent core installs nothing");
        helper.Installed = true; helper.Version = "2.2.7";
        WalkOfLifeCompatibility.Install(helper, monitor, "SwordMastery.Test.Newer");
        check(monitor.Messages.Any(s => s.Contains("left unchanged")), "future core version left unchanged");
        helper.Version = "2.2.6";
        WalkOfLifeCompatibility.Install(helper, monitor, "SwordMastery.Test.Wol");
        check(monitor.Messages.Any(s => s.Contains("2/2 targets installed")), "both real production transpiler targets installed");
        check(!monitor.Messages.Any(s => s.Contains("patch failed")), "transpiler installation succeeds");

        var owningLocation = new GameLocation(); var irrelevantLocation = new GameLocation();
        Game1.currentLocation = irrelevantLocation;
        var farmer = new Farmer(); var monster = new Monster { currentLocation = owningLocation };
        var box = new Rectangle(7, 9, 32, 48);
        for (int i = 0; i < 100; i++)
            check(!DaLion.Core.Framework.Patchers.MonsterUpdatePatcher.MonsterUpdatePrefix(monster, farmer, box, false), "DoT prefix return preserved");
        check(owningLocation.Kills == 100 && irrelevantLocation.Kills == 0, "100 DoT callbacks use monster owning location exactly once");
        for (int i = 0; i < 100; i++)
            check(!DaLion.Core.Framework.Debuffs.Monster_Poisoned.Increment_Poisoned(monster, farmer, box, true), "poison return preserved");
        check(owningLocation.Kills == 200 && irrelevantLocation.Kills == 0, "100 poison callbacks use native instance exactly once");
        check(ReferenceEquals(owningLocation.LastKiller, farmer) && ReferenceEquals(owningLocation.LastMonster, monster)
              && owningLocation.LastBox == box && owningLocation.LastBomb, "killer, monster, hitbox and bomb flag preserved");
        bool untouched = false;
        try { DaLion.Shared.Reflection.Reflector.GetStaticMethodDelegate<Action>(typeof(GameLocation), "unrelated"); }
        catch (InvalidOperationException) { untouched = true; }
        check(untouched, "shared reflector remains untouched");
        var three = new ThreeArgumentLocation();
        MonsterKillDelegateFactory.Create<ThreeArgumentLocation, Farmer, Monster, Rectangle>()(three, farmer, monster, box, true);
        check(three.Calls == 1, "legacy three-argument instance supported");
        bool rejected = false;
        try { MonsterKillDelegateFactory.Create<InvalidLocation, Farmer, Monster, Rectangle>(); }
        catch (MissingMethodException) { rejected = true; }
        check(rejected, "unknown static callback shape rejected");
    }
}

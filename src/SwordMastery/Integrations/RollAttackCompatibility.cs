using System.Reflection;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;

namespace SwordMastery.Integrations;

internal static class RollAttackCompatibility
{
    private static Func<Farmer, bool>? QueryRolling;

    public static bool IsRolling(Farmer farmer)
        => Context.IsWorldReady && ReferenceEquals(farmer, Game1.player)
            && QueryRolling?.Invoke(farmer) == true;

    public static void Install(IModHelper helper, IMonitor monitor, string owner)
    {
        if (helper.ModRegistry.Get("ValleyCoward.CowardlyGunfighter") is null)
            return;
        try
        {
            Type? rollType = AccessTools.TypeByName("PeacefulValleyCowardlyGunfighter.Services.RollSkill");
            MethodInfo? query = rollType?.GetMethod("IsRolling", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null, new[] { typeof(Farmer) }, null);
            if (query is null || query.ReturnType != typeof(bool))
                throw new MissingMethodException("Cowardly Gunfighter RollSkill.IsRolling(Farmer) was not found.");
            QueryRolling = query.CreateDelegate<Func<Farmer, bool>>();
        }
        catch (Exception ex)
        {
            monitor.Log($"Roll attack guard unavailable: {ex}", LogLevel.Error);
            return;
        }

        var harmony = new Harmony(owner + ".RollAttackGuard");
        int patched = 0;
        foreach (var (type, name, prefix) in new[]
        {
            (typeof(Game1), "pressUseToolButton", nameof(AllowToolPress)),
            (typeof(Farmer), "FireTool", nameof(AllowFireTool)),
            (typeof(MeleeWeapon), "leftClick", nameof(AllowWeaponAction)),
            (typeof(MeleeWeapon), "animateSpecialMove", nameof(AllowWeaponAction))
        })
        {
            try
            {
                MethodInfo? target = AccessTools.Method(type, name);
                if (target is null)
                    throw new MissingMethodException(type.FullName, name);
                harmony.Patch(target, prefix: new HarmonyMethod(typeof(RollAttackCompatibility), prefix) { priority = Priority.First });
                patched++;
            }
            catch (Exception ex)
            {
                monitor.Log($"Roll attack guard failed for {type.Name}.{name}: {ex}", LogLevel.Error);
            }
        }
        monitor.Log($"PATCH44 FIX04 roll attack guard: {patched}/4 targets installed. Attacks are blocked only while Cowardly Gunfighter is rolling.",
            patched == 4 ? LogLevel.Info : LogLevel.Error);
    }

    private static bool AllowToolPress(ref bool __result)
    {
        if (!IsRolling(Game1.player))
            return true;
        __result = false;
        return false;
    }

    private static bool AllowFireTool(Farmer __instance) => !IsRolling(__instance);
    private static bool AllowWeaponAction(Farmer who) => !IsRolling(who);
}

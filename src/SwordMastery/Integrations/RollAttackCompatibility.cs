using System.Reflection;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;

namespace SwordMastery.Integrations;

internal static class RollAttackCompatibility
{
    private const string GunId = "ValleyCoward.CowardlyGunfighter";
    private const string ProtocolKey = "ValleyCoward.ModInterop.NativeRoll/v1";
    private static Func<Farmer, bool>? QueryRolling;
    private static bool installed;

    public static bool IsRolling(Farmer farmer)
    {
        if (!Context.IsWorldReady || farmer is null || !ReferenceEquals(farmer, Game1.player))
            return false;
        try { return QueryRolling?.Invoke(farmer) == true; }
        catch { return false; } // Never leave players permanently attack-locked.
    }

    public static void Install(IModHelper helper, IMonitor monitor, string owner)
    {
        if (helper.ModRegistry.Get(GunId) is null) return;
        if (installed) return;
        installed = true;
        // Legacy state is available immediately; refresh with preferred API at GameLaunched.
        ConfigureProvider(helper, monitor);
        helper.Events.GameLoop.GameLaunched += (_, _) => ConfigureProvider(helper, monitor);

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

    private static void ConfigureProvider(IModHelper helper, IMonitor monitor)
    {
        QueryRolling = null;
        if (helper.ModRegistry.Get(GunId) is null) return;
        try
        {
            INativeRollCompatibilityApi? api = helper.ModRegistry.GetApi<INativeRollCompatibilityApi>(GunId);
            if (api is not null && string.Equals(api.GetNativeRollCompatibilityKey(), ProtocolKey, StringComparison.Ordinal))
            {
                QueryRolling = api.IsDodgeRolling;
                monitor.Log("NativeRoll/v1 attack guard linked to shared API.", LogLevel.Info);
                return;
            }
        }
        catch (Exception ex)
        {
            monitor.Log("NativeRoll/v1 unavailable; trying legacy roll-state query: " + ex.Message, LogLevel.Trace);
        }
        // Backward compatibility for existing stable gun builds without NativeRoll/v1.
        try
        {
            Type? rollType = AccessTools.TypeByName("PeacefulValleyCowardlyGunfighter.Services.RollSkill");
            MethodInfo? method = rollType?.GetMethod("IsRolling", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null, new[] { typeof(Farmer) }, null);
            if (method?.ReturnType == typeof(bool))
                QueryRolling = method.CreateDelegate<Func<Farmer, bool>>();
        }
        catch (Exception ex)
        {
            monitor.Log("Legacy roll guard unavailable: " + ex.Message, LogLevel.Warn);
        }
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

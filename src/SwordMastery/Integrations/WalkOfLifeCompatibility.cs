using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;

namespace SwordMastery.Integrations;

internal static class WalkOfLifeCompatibility
{
    private static Action<GameLocation, Farmer, Monster, Rectangle, bool>? NativeKill;
    private static readonly Action<Farmer, Monster, Rectangle, bool> KillOnMonsterLocation = DispatchKill;

    public static void Install(IModHelper helper, IMonitor monitor, string owner)
    {
        var core = helper.ModRegistry.Get("DaLion.Core");
        if (core is null)
            return;
        // Upstream may change these internals in later versions. Do not apply a
        // speculative patch to those versions or alter the shared reflection cache.
        if (core.Manifest.Version.ToString() != "2.2.6")
        {
            monitor.Log("Walk Of Life callback compatibility targets DaLionheart 2.2.6; other core versions are left unchanged.", LogLevel.Info);
            return;
        }

        try
        {
            NativeKill = MonsterKillDelegateFactory.Create<GameLocation, Farmer, Monster, Rectangle>();
        }
        catch (Exception ex)
        {
            monitor.Log($"WOL compatibility unavailable: native kill method signature was not recognized: {ex}", LogLevel.Error);
            return;
        }

        var harmony = new Harmony(owner + ".WalkOfLifeKillCallback");
        int patched = 0;
        foreach (var (typeName, methodName) in new[]
        {
            ("DaLion.Core.Framework.Patchers.MonsterUpdatePatcher", "MonsterUpdatePrefix"),
            ("DaLion.Core.Framework.Debuffs.Monster_Poisoned", "Increment_Poisoned")
        })
        {
            try
            {
                Type? type = AccessTools.TypeByName(typeName);
                MethodInfo? target = type is null ? null : AccessTools.Method(type, methodName);
                if (target is null)
                    throw new MissingMethodException(typeName, methodName);
                harmony.Patch(target, transpiler: new HarmonyMethod(typeof(WalkOfLifeCompatibility), nameof(ReplaceFactory)));
                patched++;
            }
            catch (Exception ex)
            {
                // Installation errors remain visible. No gameplay exception is
                // swallowed, no DoT logic is skipped, and no kill event is blocked.
                monitor.Log($"WOL callback patch failed for {typeName}.{methodName}: {ex}", LogLevel.Error);
            }
        }
        monitor.Log($"PATCH44 FIX03 WOL kill callback compatibility: {patched}/2 targets installed (DaLionheart 2.2.6).", patched == 2 ? LogLevel.Info : LogLevel.Error);
    }

    private static IEnumerable<CodeInstruction> ReplaceFactory(IEnumerable<CodeInstruction> instructions)
    {
        var code = instructions.ToList();
        int replacements = 0;
        MethodInfo replacement = AccessTools.Method(typeof(WalkOfLifeCompatibility), nameof(GetKillDelegate));
        for (int i = 1; i < code.Count; i++)
        {
            if (code[i].opcode != OpCodes.Call || code[i].operand is not MethodInfo method
                || method.DeclaringType?.FullName != "DaLion.Shared.Reflection.Reflector"
                || method.Name != "GetStaticMethodDelegate" || !method.IsStatic || !method.IsGenericMethod
                || method.GetGenericArguments().Length != 1
                || method.GetGenericArguments()[0] != typeof(Action<Farmer, Monster, Rectangle, bool>)
                || method.GetParameters().Length != 2 || method.GetParameters()[0].ParameterType != typeof(Type)
                || method.GetParameters()[1].ParameterType != typeof(string)
                || code[i - 1].opcode != OpCodes.Ldstr || code[i - 1].operand as string != "onMonsterKilled")
                continue;

            // Modify only the operand: preserve all labels and exception blocks.
            code[i].operand = replacement;
            replacements++;
        }
        if (replacements != 1)
            throw new InvalidOperationException($"Expected one exact DaLion onMonsterKilled factory call; found {replacements}. No compatible replacement was emitted.");
        return code;
    }

    private static Action<Farmer, Monster, Rectangle, bool> GetKillDelegate(Type type, string name)
    {
        if (type != typeof(GameLocation) || name != "onMonsterKilled")
            throw new InvalidOperationException("Unexpected kill callback request.");
        return KillOnMonsterLocation;
    }

    private static void DispatchKill(Farmer killer, Monster monster, Rectangle box, bool isBomb)
    {
        GameLocation location = monster.currentLocation
            ?? throw new InvalidOperationException("Cannot finish a monster without its owning location.");
        (NativeKill ?? throw new InvalidOperationException("Native kill delegate was not initialized."))(
            location, killer, monster, box, isBomb);
    }
}

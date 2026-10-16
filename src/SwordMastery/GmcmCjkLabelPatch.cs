using System;
using HarmonyLib;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace SwordMastery;

/// <summary>Fix own GMCM labels without changing game-wide fonts or other mods.</summary>
internal static class GmcmCjkLabelPatch
{
    private static IMonitor? monitor;
    private static bool installed;
    private static bool hasWarned;

    public static void Install(IMonitor logger)
    {
        if (installed) return;
        monitor = logger;
        Type? type = AccessTools.TypeByName("SpaceShared.UI.Label");
        if (type is null)
        {
            logger.Log("GMCM Chinese font: SpaceShared.UI.Label not found; SpriteText fallback remains active.", LogLevel.Warn);
            return;
        }
        var harmony = new Harmony("Prototype.SwordMastery.PATCH38.GmcmLabel");
        int patched = 0;
        foreach (var (methodName, prefix) in new[] { ("Draw", nameof(DrawPrefix)), ("Measure", nameof(MeasurePrefix)) })
        {
            MethodInfo? method = methodName == "Draw"
                ? AccessTools.Method(type, methodName, new[] { typeof(SpriteBatch) })
                : AccessTools.Method(type, methodName, Type.EmptyTypes);
            if (method is null) continue;
            try
            {
                harmony.Patch(method, prefix: new HarmonyMethod(typeof(GmcmCjkLabelPatch), prefix));
                patched++;
            }
            catch (Exception ex)
            {
                logger.Log($"GMCM Chinese font: failed to patch {methodName}: {ex.Message}", LogLevel.Warn);
            }
        }
        installed = patched == 2;
        logger.Log($"Sword Mastery GMCM Chinese/Japanese label hooks: {patched}/2 installed.",
            patched == 2 ? LogLevel.Info : LogLevel.Warn);
    }

    private static bool IsOurLabel(object label, out string text, out SpriteFont font)
    {
        text = "";
        font = null!;
        if (!I18n.IsOwnGmcmPageActive()) return false;
        Type type = label.GetType();
        text = AccessTools.Property(type, "String")?.GetValue(label) as string ?? "";
        if (!I18n.IsOwnedText(text) || !ContainsCjk(text)) return false;
        bool bold = AccessTools.Property(type, "Bold")?.GetValue(label) is true;
        font = bold ? I18n.HeadingFont : I18n.SmallFont;
        return font is not null;
    }

    private static bool ContainsCjk(string str)
    {
        foreach (char c in str)
            if (c is >= '\u2E80' and <= '\u9FFF'
                or >= '\uAC00' and <= '\uD7AF'
                or >= '\uF900' and <= '\uFAFF'
                or >= '\uFF01' and <= '\uFFEF')
                return true;
        return false;
    }

    private static bool MeasurePrefix(object __instance, ref Vector2 __result)
    {
        try
        {
            if (!IsOurLabel(__instance, out string text, out SpriteFont font)) return true;
            float scale = (float?)AccessTools.Property(__instance.GetType(), "Scale")?.GetValue(__instance) ?? 1f;
            __result = font.MeasureString(text) * scale;
            return false;
        }
        catch (Exception ex)
        {
            WarnOnce(ex);
            return true;
        }
    }

    private static bool DrawPrefix(object __instance, SpriteBatch b)
    {
        try
        {
            if (!IsOurLabel(__instance, out string text, out SpriteFont font)) return true;
            Type type = __instance.GetType();
            if (AccessTools.Method(type, "IsHidden", Type.EmptyTypes)?.Invoke(__instance, null) is true)
                return false;
            if (AccessTools.Property(type, "Position")?.GetValue(__instance) is not Vector2 position)
                return true;
            float scale = (float?)AccessTools.Property(type, "Scale")?.GetValue(__instance) ?? 1f;
            bool bold = AccessTools.Property(type, "Bold")?.GetValue(__instance) is true;
            bool hover = AccessTools.Property(type, "Hover")?.GetValue(__instance) is true;
            bool clickable = AccessTools.Property(type, "Callback")?.GetValue(__instance) is not null;
            bool highlight = hover && clickable;
            Color color = bold ? (highlight ? Color.Gray : Color.Black)
                : highlight
                    ? ((Color?)AccessTools.Property(type, "HoverTextColor")?.GetValue(__instance) ?? Game1.unselectedOptionColor)
                    : ((Color?)AccessTools.Property(type, "IdleTextColor")?.GetValue(__instance) ?? Game1.textColor);
            if (color.A > 0)
                b.DrawString(font, text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 1f);
            return false;
        }
        catch (Exception ex)
        {
            WarnOnce(ex);
            return true;
        }
    }

    private static void WarnOnce(Exception ex)
    {
        if (hasWarned) return;
        hasWarned = true;
        monitor?.Log("GMCM Chinese label fallback activated: " + ex, LogLevel.Warn);
    }
}

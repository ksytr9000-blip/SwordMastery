using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace SwordMastery;

/// <summary>
/// Allows Sword Mastery text to stay in the selected language while the main
/// game runs in a different locale. No font binaries are distributed: we use
/// glyph atlases supplied with the mod. No global language switch.
/// </summary>
internal static class CjkTextService
{
    private static IMonitor? monitor;
    private static bool installed;
    private static readonly HashSet<string> warned = new();

    public static void Install(IMonitor log)
    {
        if (installed)
            return;
        installed = true;
        monitor = log;
        var harmony = new Harmony("Prototype.SwordMastery.PATCH34.CjkText");

        // Covers every SpriteBatch.DrawString overload that takes a string or
        // StringBuilder, including native tooltip, quest, GMCM, and HUD draws.
        foreach (MethodInfo method in AccessTools.GetDeclaredMethods(typeof(SpriteBatch)))
        {
            if (method.Name != "DrawString")
                continue;
            ParameterInfo[] p = method.GetParameters();
            if (p.Length < 2 || p[0].ParameterType != typeof(SpriteFont))
                continue;
            string? prefixName = p[1].ParameterType == typeof(string)
                ? nameof(DrawStringPrefix)
                : p[1].ParameterType == typeof(StringBuilder)
                    ? nameof(DrawBuilderPrefix)
                    : null;
            if (prefixName is not null)
                PatchSafely(harmony, method, prefixName);
        }

        foreach (MethodInfo method in AccessTools.GetDeclaredMethods(typeof(SpriteFont)))
        {
            if (method.Name != "MeasureString" || method.ReturnType != typeof(Vector2))
                continue;
            ParameterInfo[] p = method.GetParameters();
            if (p.Length != 1)
                continue;
            string? prefixName = p[0].ParameterType == typeof(string)
                ? nameof(MeasureStringPrefix)
                : p[0].ParameterType == typeof(StringBuilder)
                    ? nameof(MeasureBuilderPrefix)
                    : null;
            if (prefixName is not null)
                PatchSafely(harmony, method, prefixName);
        }

        // Stardew's SpriteText renderer is independent of SpriteBatch.DrawString
        // and prints stars for CJK strings under an English game language.
        // For strings containing CJK, route the glyphs through the selected
        // game's SpriteFont, without changing non-CJK native drawing.
        Type? spriteText = AccessTools.TypeByName("StardewValley.BellsAndWhistles.SpriteText")
            ?? AccessTools.TypeByName("StardewValley.SpriteText");
        if (spriteText is not null)
        {
            foreach (MethodInfo method in AccessTools.GetDeclaredMethods(spriteText))
            {
                ParameterInfo[] p = method.GetParameters();
                if (method.Name == "drawString"
                    && p.Length >= 4 && p[0].ParameterType == typeof(SpriteBatch)
                    && p[1].ParameterType == typeof(string)
                    && p[2].ParameterType == typeof(int)
                    && p[3].ParameterType == typeof(int))
                    PatchSafely(harmony, method, nameof(SpriteTextDrawPrefix));
                else if (method.Name == "getWidthOfString"
                    && method.ReturnType == typeof(int) && p.Length > 0
                    && p[0].ParameterType == typeof(string))
                    PatchSafely(harmony, method, nameof(SpriteTextWidthPrefix));
                else if (method.Name == "getHeightOfString"
                    && method.ReturnType == typeof(int) && p.Length > 0
                    && p[0].ParameterType == typeof(string))
                    PatchSafely(harmony, method, nameof(SpriteTextHeightPrefix));
            }
        }
        else
        {
            WarnOnce("spritetext", "SpriteText type not found; native journal fonts may still use the game's locale.");
        }
    }

    private static void PatchSafely(Harmony harmony, MethodInfo target, string prefix)
    {
        try
        {
            harmony.Patch(target, prefix: new HarmonyMethod(typeof(CjkTextService), prefix));
        }
        catch (Exception ex)
        {
            WarnOnce("patch:" + target, $"Could not patch {target}: {ex.Message}");
        }
    }

    private static bool Active(out SpriteFont font)
    {
        font = null!;
        if (!I18n.NeedsIndependentCjkFont)
            return false;

        // Stardew already understands its native language text in native menus.
        if ((I18n.SelectedLanguage == "Japanese"
                && LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ja)
            || (I18n.SelectedLanguage == "Chinese"
                && LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh))
            return false;

        // PATCH34 ships original *glyph images*, not a redistributed font file.
        // The atlas font contains every character used by our localized strings.
        // Use that font when drawing in the game's otherwise Latin-only menus.
        font = I18n.SmallFont;
        return font is not null && !ReferenceEquals(font, Game1.smallFont);
    }

    private static bool ContainsCjk(string? text)
    {
        if (string.IsNullOrEmpty(text)) return false;
        foreach (char c in text)
            if (c is >= '\u2E80' and <= '\u9FFF'
                or >= '\uAC00' and <= '\uD7AF'
                or >= '\uF900' and <= '\uFAFF'
                or >= '\uFF01' and <= '\uFFEF')
                return true;
        return false;
    }

    private static bool NeedsFont(string? text, out SpriteFont replacement)
    {
        replacement = null!;
        // Never modify arbitrary mod names, GMCM's shared Save/Reset/Close
        // controls, or other mods' text even when they contain CJK glyphs.
        return text is not null && I18n.IsOwnedText(text)
            && ContainsCjk(text) && Active(out replacement);
    }

    private static void DrawStringPrefix(ref SpriteFont spriteFont, string text)
    {
        if (NeedsFont(text, out SpriteFont alt))
            spriteFont = alt;
    }

    private static void DrawBuilderPrefix(ref SpriteFont spriteFont, StringBuilder text)
    {
        if (NeedsFont(text?.ToString(), out SpriteFont alt))
            spriteFont = alt;
    }

    private static bool MeasureStringPrefix(SpriteFont __instance, string text, ref Vector2 __result)
    {
        if (NeedsFont(text, out SpriteFont alt) && !ReferenceEquals(__instance, alt))
        {
            __result = alt.MeasureString(text);
            return false;
        }
        return true;
    }

    private static bool MeasureBuilderPrefix(SpriteFont __instance, StringBuilder text, ref Vector2 __result)
    {
        if (NeedsFont(text?.ToString(), out SpriteFont alt) && !ReferenceEquals(__instance, alt))
        {
            __result = alt.MeasureString(text);
            return false;
        }
        return true;
    }

    private static bool SpriteTextWidthPrefix(object[] __args, ref int __result)
    {
        if (__args.Length > 0 && __args[0] is string str && NeedsFont(str, out SpriteFont font))
        {
            __result = (int)Math.Ceiling(font.MeasureString(str).X);
            return false;
        }
        return true;
    }

    private static bool SpriteTextHeightPrefix(object[] __args, ref int __result)
    {
        if (__args.Length > 0 && __args[0] is string str && NeedsFont(str, out SpriteFont font))
        {
            __result = (int)Math.Ceiling(font.MeasureString(str).Y);
            return false;
        }
        return true;
    }

    private static bool SpriteTextDrawPrefix(object[] __args, MethodBase __originalMethod)
    {
        if (__args.Length < 4 || __args[1] is not string text
            || __args[0] is not SpriteBatch batch
            || !NeedsFont(text, out SpriteFont font))
            return true;

        try
        {
            int x = Convert.ToInt32(__args[2]);
            int y = Convert.ToInt32(__args[3]);
            var parameters = __originalMethod.GetParameters();
            float alpha = 1f;
            float layerDepth = 0.88f;
            float scale = 1f;
            int maxWidth = -1;
            Color color = Color.Black;
            for (int i = 4; i < Math.Min(parameters.Length, __args.Length); i++)
            {
                string name = parameters[i].Name ?? "";
                object? value = __args[i];
                if (value is null) continue;
                switch (name.ToLowerInvariant())
                {
                    case "alpha": alpha = Convert.ToSingle(value); break;
                    case "layerdepth": layerDepth = Convert.ToSingle(value); break;
                    case "scale":
                        float configuredScale = Convert.ToSingle(value);
                        if (configuredScale > 0f) scale = configuredScale;
                        break;
                    case "width":
                    case "maxwidth":
                        int candidate = Convert.ToInt32(value);
                        if (candidate > 0) maxWidth = candidate;
                        break;
                    case "color":
                        if (value is Color c) color = c;
                        break;
                }
            }

            // SpriteText's width argument is usually a wrapping boundary.
            // Preserve the requested layout so journal/tooltip panels still fit.
            if (maxWidth > 0 && font.MeasureString(text).X * scale > maxWidth)
                text = WrapText(text, font, maxWidth / scale);

            batch.DrawString(font, text, new Vector2(x,y), color * alpha, 0f,
                Vector2.Zero, scale, SpriteEffects.None, layerDepth);
            return false;
        }
        catch (Exception ex)
        {
            WarnOnce("spritetext-draw", "CJK SpriteText drawing failed: " + ex.Message);
            return true;
        }
    }

    private static string WrapText(string input, SpriteFont font, float width)
    {
        var output = new StringBuilder(input.Length + input.Length / 20);
        float x = 0;
        foreach (char c in input)
        {
            if (c == '\n') { output.Append(c); x = 0; continue; }
            float cw = font.MeasureString(c.ToString()).X;
            if (x > 0 && x + cw > width) { output.Append('\n'); x = 0; }
            output.Append(c);
            x += cw;
        }
        return output.ToString();
    }

    private static void WarnOnce(string key, string message)
    {
        if (warned.Add(key))
            monitor?.Log("CJK Font: " + message, LogLevel.Warn);
    }
}

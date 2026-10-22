using System.Reflection;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace SwordMastery;

internal static class I18n
{
    private static readonly Dictionary<string, Dictionary<string, string>> Localizations = new();
    private static Func<string> CurrentLanguage = () => "English";
    private static LocalizedFontRenderer? fontRenderer;

    // Identity-based ownership marker: two mods may produce identical Chinese
    // strings, but only values returned by SwordMastery.I18n may use our font.
    // ConditionalWeakTable avoids holding dynamic HUD/tooltip strings forever.
    private sealed class ModTextMarker { }
    private static readonly ConditionalWeakTable<string, ModTextMarker> OwnedText = new();
    private static readonly HashSet<string> NativeDataText = new(StringComparer.Ordinal);
    private static readonly HashSet<string> AllModCjkText = new(StringComparer.Ordinal);
    private static Func<bool>? IsOwnGmcmPage;

    public static void SetGmcmPageScope(Func<bool> check) => IsOwnGmcmPage = check;

    private static bool InGmcmMenu
    {
        get
        {
            var menu = Game1.activeClickableMenu;
            return menu is not null &&
                (menu.GetType().FullName?.Contains("GenericModConfigMenu", StringComparison.OrdinalIgnoreCase) ?? false);
        }
    }



    public static void Init(IModHelper helper, Func<string> currentLanguage)
    {
        CurrentLanguage=currentLanguage;
        Localizations["English"] = LoadDictionary(Path.Combine(helper.DirectoryPath,"i18n","default.json"));
        Localizations["Korean"] = LoadDictionary(Path.Combine(helper.DirectoryPath,"i18n","ko.json"));
        Localizations["Japanese"] = LoadDictionary(Path.Combine(helper.DirectoryPath,"i18n","ja.json"));
        Localizations["Chinese"] = LoadDictionary(Path.Combine(helper.DirectoryPath,"i18n","zh.json"));
        NativeDataText.Clear();
        AllModCjkText.Clear();
        foreach (string language in new[] { "Chinese", "Japanese" })
        {
            foreach (var pair in Localizations[language])
            {
                AllModCjkText.Add(pair.Value);
                // Native GameData can clone item/quest/notification text; allow
                // exact-value matching for these *specific mod fields only*.
                if (pair.Key.StartsWith("item.", StringComparison.Ordinal)
                    || pair.Key.StartsWith("quest.", StringComparison.Ordinal)
                    || pair.Key.StartsWith("notify.", StringComparison.Ordinal)
                    || pair.Key.StartsWith("unlock.", StringComparison.Ordinal))
                    NativeDataText.Add(pair.Value);
                // Other UI text is handled by reference identity, not by
                // arbitrary Chinese/Japanese glyph matching.
            }
        }
    }

    public static void SetFontRenderer(LocalizedFontRenderer renderer) => fontRenderer=renderer;
    public static bool NeedsIndependentCjkFont => CurrentLanguage() is "Japanese" or "Chinese";
    public static SpriteFont SmallFont => NeedsIndependentCjkFont
        ? fontRenderer?.Small ?? Game1.smallFont : Game1.smallFont;
    public static SpriteFont HeadingFont => NeedsIndependentCjkFont
        ? fontRenderer?.Heading ?? Game1.dialogueFont : Game1.dialogueFont;

    private static Dictionary<string, string> LoadDictionary(string path)
    {
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(path))
                ?? new Dictionary<string, string>();
        }
        catch { return new Dictionary<string, string>(); }
    }

    // Translate every Sword Mastery field, even when the game language differs.
    // CjkTextService handles native rendering; no English substitution.
    public static string Get(string key, object? tokens = null)
        => Translate(CurrentLanguage(), key, tokens);

    public static string SelectedLanguage => CurrentLanguage();

    public static string GetUi(string key, object? tokens = null)
        => Translate(CurrentLanguage(), key, tokens);

    /// <summary>Use GMCM's own current menu: title-screen child menus may not
    /// be Game1.activeClickableMenu.</summary>
    public static bool IsOwnGmcmPageActive()
    {
        try { return IsOwnGmcmPage?.Invoke() == true; }
        catch { return false; }
    }

    public static bool IsOwnedText(string? text)
    {
        if (string.IsNullOrEmpty(text) || !NeedsIndependentCjkFont)
            return false;
        if (IsOwnGmcmPageActive())
            return OwnedText.TryGetValue(text, out _)
                || AllModCjkText.Contains(text)
                || IsDerivedGmcmText(text);
        if (InGmcmMenu)
            return false;
        return OwnedText.TryGetValue(text, out _) || NativeDataText.Contains(text);
    }

    // GMCM may copy/wrap/split text, losing its original string identity.
    // Only compare fragments on Sword Mastery's own configuration page.
    private static bool IsDerivedGmcmText(string text)
    {
        string normalized = text.Replace("\r", "", StringComparison.Ordinal)
            .Replace("\n", "", StringComparison.Ordinal).Trim();
        if (normalized.Length < 2)
            return false;
        int cjkCount = 0;
        foreach (char c in normalized)
            if (c is >= '\u2E80' and <= '\u9FFF'
                or >= '\uAC00' and <= '\uD7AF'
                or >= '\uF900' and <= '\uFAFF'
                or >= '\uFF01' and <= '\uFFEF')
                cjkCount++;
        if (cjkCount < 2) return false;
        foreach (string candidate in AllModCjkText)
        {
            if (candidate.Contains(normalized, StringComparison.Ordinal))
                return true;
            if (normalized.Length > 2 &&
                candidate.Contains(normalized.Replace(" ", "", StringComparison.Ordinal),
                    StringComparison.Ordinal))
                return true;
        }
        return false;
    }

    private static string TrackModText(string text)
    {
        if (NeedsIndependentCjkFont)
            OwnedText.GetValue(text, _ => new ModTextMarker());
        return text;
    }

    private static string Translate(string language, string key, object? tokens)
    {
        if (!Localizations.TryGetValue(language, out var selected))
            selected = Localizations["English"];
        if (!selected.TryGetValue(key, out string? value)
            && !Localizations["English"].TryGetValue(key, out value))
            return key;
        if (tokens is null)
            return TrackModText(value);
        foreach (PropertyInfo property in tokens.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            string token = "{{" + property.Name + "}}";
            value = value.Replace(token, property.GetValue(tokens)?.ToString() ?? "", StringComparison.Ordinal);
        }
        return TrackModText(value);
    }
}

using System.Text.Json;
using SwordMastery.Models;

namespace SwordMastery.Services;

// Transient input only. GMCM's save/reset setter replay must never stage edits.
internal sealed class DebugProgressionDraft
{
    public int? Level { get; private set; }
    public int? Points { get; private set; }
    public bool? Ohgi { get; private set; }
    public bool? Ultimate { get; private set; }
    private string? Baseline;

    public bool Stage(string field, object value, SaveData data, int maxLevel)
    {
        if (field is not ("DebugSwordLevel" or "DebugSkillPoints" or "DebugOhgiAccess" or "DebugUltimateAccess"))
            return false;
        Baseline ??= JsonSerializer.Serialize(data);
        switch (field)
        {
            case "DebugSwordLevel" when value is int level: Level = Math.Clamp(level, 0, maxLevel); break;
            case "DebugSkillPoints" when value is int points: Points = Math.Clamp(points, 0, 200); break;
            case "DebugOhgiAccess" when value is bool ohgi: Ohgi = ohgi; break;
            case "DebugUltimateAccess" when value is bool ultimate: Ultimate = ultimate; break;
            default: return false;
        }
        return true;
    }

    public bool Validate(SaveData data, out string reason)
    {
        if (Baseline is null)
        {
            reason = "No debug fields were edited. Edit the values before applying.";
            return false;
        }
        if (data.SwordLevel < 0 || data.SwordExperience < 0 || data.UnspentSkillPoints < 0
            || data.Skills is null || data.Skills.Values.Any(s => s is null || s.Stage1 < 0 || s.Stage2 < 0 || s.Stage3 < 0))
        {
            reason = "Invalid progression state; no debug changes applied.";
            return false;
        }
        if (Baseline != JsonSerializer.Serialize(data))
        {
            reason = "Progression changed after editing began. Reopen the debug page and enter the values again.";
            return false;
        }
        reason = "";
        return true;
    }

    public bool TryApply(SaveData data, out string reason)
    {
        if (!Validate(data, out reason))
        {
            Clear();
            return false;
        }
        // Each field is independent: changing level never resets XP or recalculates SP.
        if (Level.HasValue) data.SwordLevel = Level.Value;
        if (Points.HasValue) data.UnspentSkillPoints = Points.Value;
        if (Ohgi.HasValue && Ohgi.Value != data.OhgiAccessGranted)
        {
            data.OhgiAccessGranted = Ohgi.Value;
            data.OhgiQuestAvailable = false;
            if (!Ohgi.Value) data.SelectedOhgi = null;
        }
        if (Ultimate.HasValue && Ultimate.Value != data.UltimateAccessGranted)
        {
            data.UltimateAccessGranted = Ultimate.Value;
            data.UltimateQuestAvailable = false;
            if (!Ultimate.Value) data.SelectedUltimate = null;
        }
        Clear();
        return true;
    }

    public void Clear()
    {
        Level = null;
        Points = null;
        Ohgi = null;
        Ultimate = null;
        Baseline = null;
    }
}

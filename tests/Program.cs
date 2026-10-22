using System.Text.Json;
using SwordMastery.Models;
using SwordMastery.Services;

int checks = 0;
void Check(bool condition, string name) { if (!condition) throw new Exception(name); checks++; }
string Snapshot(SaveData data) => JsonSerializer.Serialize(data);
SaveData Create() {
    var d = new SaveData { SwordLevel = 60, SwordExperience = 137, UnspentSkillPoints = 7,
        OhgiAccessGranted = true, UltimateAccessGranted = true,
        SelectedOhgi = "B", SelectedUltimate = "A", OhgiQuestCompleted = true, UltimateQuestCompleted = true };
    d.Skills["BasicB"].Stage1 = 5; d.Skills["BasicB"].Stage2 = 5; d.Skills["BasicB"].Stage3 = 5;
    d.Skills["OhgiB"].Stage1 = 3;
    return d;
}
var data = Create(); var draft = new DebugProgressionDraft(); string original = Snapshot(data);
for (int i = 0; i < 100; i++) {
    Check(!draft.Stage("ShowHud", false, data, 75), "ordinary field ignored");
    Check(Snapshot(data) == original, "100 ordinary config operations preserve progression");
}
Check(!draft.TryApply(data, out _), "unedited Apply is a no-op");
Check(Snapshot(data) == original, "unedited Apply preserves full state");
draft.Stage("DebugSwordLevel", 70, data, 75);
draft.Stage("DebugSkillPoints", 120, data, 75);
Check(Snapshot(data) == original, "edits only stage inputs");
Check(draft.TryApply(data, out _), "explicit Apply succeeds");
Check(data.SwordLevel == 70 && data.UnspentSkillPoints == 120 && data.SwordExperience == 137, "exact level and SP; preserve XP");
Check(data.OhgiAccessGranted && data.UltimateAccessGranted && data.SelectedOhgi == "B" && data.SelectedUltimate == "A", "unchanged unlocks and selections preserved");
Check(data.OhgiQuestCompleted && data.UltimateQuestCompleted && data.Skills["BasicB"].IsMastered && data.Skills["OhgiB"].Stage1 == 3, "quest and investments preserved");
original = Snapshot(data);
Check(!draft.TryApply(data, out _), "Apply cannot replay");
Check(Snapshot(data) == original, "replay does not change data");
draft.Stage("DebugSwordLevel", 65, data, 75); draft.Clear();
Check(!draft.TryApply(data, out _), "cancel/reset clears draft");
Check(Snapshot(data) == original, "cancel/reset preserves data");
draft.Stage("DebugSkillPoints", 80, data, 75); data.SwordExperience++;
original = Snapshot(data);
Check(!draft.TryApply(data, out _), "concurrent growth invalidates stale draft");
Check(Snapshot(data) == original, "stale draft cannot overwrite new growth");
draft.Stage("DebugSwordLevel", 50, data, 75);
Check(draft.TryApply(data, out _), "level-only change succeeds");
Check(data.SwordExperience == 138 && data.UnspentSkillPoints == 120, "level-only change preserves XP and SP");
draft.Stage("DebugOhgiAccess", false, data, 75);
Check(draft.TryApply(data, out _), "explicit unlock OFF succeeds");
Check(!data.OhgiAccessGranted && data.SelectedOhgi == null && data.UltimateAccessGranted && data.SelectedUltimate == "A", "OFF only affects requested branch");
var roundtrip = JsonSerializer.Deserialize<SaveData>(Snapshot(data))!;
Check(Snapshot(roundtrip) == Snapshot(data), "save serialization roundtrip preserves state");
Console.WriteLine($"{checks} progression regression checks passed. These are not in-game tests.");

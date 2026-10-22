#!/usr/bin/env python3
"""PATCH44: stop GMCM option setters from mutating Sword Mastery save data.

The debug page now edits local drafts only. The explicit APPLY checkbox/button is
the sole point that applies those drafts to live save data. Normal GMCM saves,
language changes, page navigation, and resetting config cannot reset progress.
"""
from pathlib import Path
import json

root = Path("SwordMastery")
target = root / "ModEntry.cs"
s = target.read_text(encoding="utf-8-sig")

def replace_once(old, new, label):
    global s
    count = s.count(old)
    if count != 1:
        raise RuntimeError(f"{label}: expected one target, saw {count}")
    s = s.replace(old, new, 1)
    print("OK:", label)

# Add volatile staging fields (not written to config nor SaveData).
replace_once(
    "    private const string SaveKey = \"SwordMastery.SaveData\";",
    '''    private const string SaveKey = "SwordMastery.SaveData";

    // GMCM may invoke setters during ordinary Save/Reset. Never bind those setters
    // directly to permanent progression fields: stage them until explicit Apply.
    private int? DebugDraftLevel;
    private int? DebugDraftSkillPoints;
    private bool? DebugDraftOhgiAccess;
    private bool? DebugDraftUltimateAccess;''',
    "draft fields")

replace_once(
    '''getValue: () => Context.IsWorldReady ? Data.SwordLevel : 0,
            setValue: value => ApplyDebugSwordLevel(value),''',
    '''getValue: () => DebugDraftLevel ?? (Context.IsWorldReady ? Data.SwordLevel : 0),
            setValue: value => DebugDraftLevel = Math.Clamp(value, 0, Config.MaxSwordLevel),''',
    "level setter is staging only")

replace_once(
    '''getValue: () => Context.IsWorldReady ? Data.UnspentSkillPoints : 0,
            setValue: value =>
            {
                if (Context.IsWorldReady)
                    Data.UnspentSkillPoints = Math.Max(0, value);
            },''',
    '''getValue: () => DebugDraftSkillPoints ?? (Context.IsWorldReady ? Data.UnspentSkillPoints : 0),
            setValue: value => DebugDraftSkillPoints = Math.Max(0, value),''',
    "SP setter is staging only")

replace_once(
    '''getValue: () => Context.IsWorldReady && Data.OhgiAccessGranted,
            setValue: value =>
            {
                if (!Context.IsWorldReady)
                    return;

                Data.OhgiAccessGranted = value;
                Data.OhgiQuestAvailable = false;

                if (!value)
                    Data.SelectedOhgi = null;
            },''',
    '''getValue: () => DebugDraftOhgiAccess ?? (Context.IsWorldReady && Data.OhgiAccessGranted),
            setValue: value => DebugDraftOhgiAccess = value,''',
    "Ohgi setter is staging only")

replace_once(
    '''getValue: () => Context.IsWorldReady && Data.UltimateAccessGranted,
            setValue: value =>
            {
                if (!Context.IsWorldReady)
                    return;

                Data.UltimateAccessGranted = value;
                Data.UltimateQuestAvailable = false;

                if (!value)
                    Data.SelectedUltimate = null;
            },''',
    '''getValue: () => DebugDraftUltimateAccess ?? (Context.IsWorldReady && Data.UltimateAccessGranted),
            setValue: value => DebugDraftUltimateAccess = value,''',
    "Ultimate setter is staging only")

# Place explicit Apply as a distinct one-shot action in the debug page before
# old developer tools (grant items, activate skill nodes, quest actions).
replace_once(
    '''        gmcm.AddSectionTitle(
            mod: ModManifest,
            text: () => I18n.GetUi("gmcm.debug.section.actions")
        );''',
    '''        gmcm.AddParagraph(
            mod: ModManifest,
            text: () => I18n.GetUi("gmcm.debug.apply.explanation")
        );

        gmcm.AddBoolOption(
            mod: ModManifest,
            getValue: () => false,
            setValue: value =>
            {
                // Only the explicit Apply action touches live progression.
                if (value)
                    ApplyStagedDebugChanges();
            },
            name: () => I18n.GetUi("gmcm.debug.apply.name"),
            tooltip: () => I18n.GetUi("gmcm.debug.apply.tip"),
            fieldId: "DebugApplyChangesExplicitly"
        );

        gmcm.AddSectionTitle(
            mod: ModManifest,
            text: () => I18n.GetUi("gmcm.debug.section.actions")
        );''',
    "explicit apply action")

# Existing debug leveling method remains but is never a GMCM setter.
# Ensure EXP is reset only on a genuinely different level and only via Apply.
replace_once(
    '''        Data.SwordLevel = level;
        Data.SwordExperience = 0;''',
    '''        if (Data.SwordLevel == level)
            return;  // Preserves the existing EXP when level was not changed.

        Data.SwordLevel = level;
        Data.SwordExperience = 0;''',
    "preserve EXP if level unchanged")

insert_at = '''    private void ApplyDebugSwordLevel(int value)
    {'''
if s.count(insert_at) != 1:
    raise RuntimeError("could not locate debug level method")
new_method = '''    private void ApplyStagedDebugChanges()
    {
        if (!Context.IsWorldReady)
            return;

        // Check draft values against live data so GMCM re-applying unchanged
        // values during Save cannot accidentally modify progression.
        int? level = DebugDraftLevel;
        int? points = DebugDraftSkillPoints;
        bool? ohgi = DebugDraftOhgiAccess;
        bool? ultimate = DebugDraftUltimateAccess;

        // Clear all transient drafts. Next page load reflects real save data.
        DebugDraftLevel = null;
        DebugDraftSkillPoints = null;
        DebugDraftOhgiAccess = null;
        DebugDraftUltimateAccess = null;

        if (level.HasValue && level.Value != Data.SwordLevel)
            ApplyDebugSwordLevel(level.Value);

        if (points.HasValue && points.Value != Data.UnspentSkillPoints)
            Data.UnspentSkillPoints = Math.Max(0, points.Value);

        if (ohgi.HasValue && ohgi.Value != Data.OhgiAccessGranted)
        {
            Data.OhgiAccessGranted = ohgi.Value;
            Data.OhgiQuestAvailable = false;
            if (!ohgi.Value)
                Data.SelectedOhgi = null;
        }

        if (ultimate.HasValue && ultimate.Value != Data.UltimateAccessGranted)
        {
            Data.UltimateAccessGranted = ultimate.Value;
            Data.UltimateQuestAvailable = false;
            if (!ultimate.Value)
                Data.SelectedUltimate = null;
        }

        // Save once only after the user has activated the dedicated debug action.
        Helper.Data.WriteSaveData(SaveKey, Data);
        Monitor.Log("Explicit GMCM debug action applied; normal GMCM saves never change progression.", LogLevel.Info);
        Game1.addHUDMessage(new HUDMessage(I18n.GetUi("gmcm.debug.apply.done")));
    }

'''
s = s.replace(insert_at,new_method+insert_at,1)

# When the player loads or leaves a save, discard any stale drafts from the
# previous character/session. Otherwise old debug values could be applied
# to a different save later by an explicit debug action.
needle = '''        Data = Helper.Data.ReadSaveData<SaveData>(SaveKey) ?? new SaveData();'''
assert needle in s, "save loaded code not found"
s=s.replace(needle,'''        DebugDraftLevel = null;
        DebugDraftSkillPoints = null;
        DebugDraftOhgiAccess = null;
        DebugDraftUltimateAccess = null;
'''+needle,1)

# Strict checks: no old dangerous GMCM direct setters remain.
assert 'setValue: value => ApplyDebugSwordLevel(value),' not in s
assert 'setValue: value => DebugDraftLevel = ' in s
assert 'setValue: value => DebugDraftSkillPoints = ' in s
assert 'setValue: value => DebugDraftOhgiAccess = value,' in s
assert 'setValue: value => DebugDraftUltimateAccess = value,' in s
assert 'fieldId: "DebugApplyChangesExplicitly"' in s
target.write_text(s,encoding="utf-8")

locales = {
    "default": {
       "gmcm.debug.apply.explanation": "Debug values are drafts. Saving GMCM settings does NOT change your progression. Explicitly enable Apply Debug Changes below to commit the draft values.",
       "gmcm.debug.apply.name": "Apply Debug Changes (explicit)",
       "gmcm.debug.apply.tip": "WARNING: applies the drafted level, SP and unlocks to your current character ONLY if you explicitly enable this option. Normal GMCM Save/Reset never changes skills.",
       "gmcm.debug.apply.done": "Sword Mastery debug changes applied.",
       "gmcm.debug.level.tip": "Stage a Sword Level value. Does not change your save until Apply Debug Changes is enabled.",
       "gmcm.debug.sp.tip": "Stage an unspent Skill Point value. Does not change your save until Apply Debug Changes is enabled.",
       "gmcm.debug.ohgi.tip": "Stage Secret Arts access. Does not change your save until Apply Debug Changes is enabled.",
       "gmcm.debug.ultimate.tip": "Stage Ultimate access. Does not change your save until Apply Debug Changes is enabled."
    },
    "ko": {
       "gmcm.debug.apply.explanation": "디버그 수치는 임시 입력값입니다. GMCM 설정 저장만으로 성장 데이터가 변경되지 않습니다. 적용하려면 아래 '디버그 변경사항 적용'을 직접 활성화하세요.",
       "gmcm.debug.apply.name": "디버그 변경사항 적용 (수동)",
       "gmcm.debug.apply.tip": "주의: 직접 활성화한 경우에만 임시 입력한 레벨, SP, 해금 상태를 실제 캐릭터에 반영합니다. 일반 GMCM 저장/초기화는 성장 데이터를 바꾸지 않습니다.",
       "gmcm.debug.apply.done": "검술 디버그 변경사항이 적용되었습니다.",
       "gmcm.debug.level.tip": "검술 레벨 임시 설정. 직접 '디버그 변경사항 적용'을 활성화해야 반영됩니다.",
       "gmcm.debug.sp.tip": "미사용 스킬 포인트 임시 설정. 수동 적용 전에는 저장 데이터가 바뀌지 않습니다.",
       "gmcm.debug.ohgi.tip": "오의 해금 상태 임시 설정. 수동 적용 전에는 변경되지 않습니다.",
       "gmcm.debug.ultimate.tip": "극의 해금 상태 임시 설정. 수동 적용 전에는 변경되지 않습니다."
    },
    "ja": {
       "gmcm.debug.apply.explanation": "デバッグ値は一時的な設定です。通常のGMCM保存では成長データは変更されません。変更する場合のみ、下の「デバッグ変更を適用」を明示的に有効化してください。",
       "gmcm.debug.apply.name": "デバッグ変更を適用（手動）",
       "gmcm.debug.apply.tip": "レベル、SP、解放状態の変更はこの項目を明示的に有効化した場合に限ります。通常の保存では変わりません。",
       "gmcm.debug.apply.done": "剣術のデバッグ変更が適用されました。",
       "gmcm.debug.level.tip": "剣術レベルの一時入力。明示的に適用するまで保存データは変更されません。",
       "gmcm.debug.sp.tip": "未使用SPの一時入力。明示的に適用するまで変更されません。",
       "gmcm.debug.ohgi.tip": "奥義解放状態の一時入力。明示的に適用するまで変更されません。",
       "gmcm.debug.ultimate.tip": "極意解放状態の一時入力。明示的に適用するまで変更されません。"
    },
    "zh": {
       "gmcm.debug.apply.explanation": "调试数值仅暂存。正常保存GMCM设置不会改变成长数据。只有主动启用下方的“应用调试更改”才会修改。",
       "gmcm.debug.apply.name": "应用调试更改（手动）",
       "gmcm.debug.apply.tip": "仅在主动启用此选项时修改剑术等级、技能点和解锁状态。普通保存不会修改。",
       "gmcm.debug.apply.done": "已应用剑术调试更改。",
       "gmcm.debug.level.tip": "暂存剑术等级。手动应用前不会更改存档。",
       "gmcm.debug.sp.tip": "暂存剩余技能点。手动应用前不会更改存档。",
       "gmcm.debug.ohgi.tip": "暂存秘技解锁状态。手动应用前不会更改存档。",
       "gmcm.debug.ultimate.tip": "暂存终极技能解锁状态。手动应用前不会更改存档。"
    }
}
for lang,values in locales.items():
    path=root/"i18n"/(lang+".json")
    data=json.loads(path.read_text(encoding="utf-8-sig"))
    data.update(values)
    path.write_text(json.dumps(data,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
    print("Updated",lang)
print("PATCH44 GMCM protection successfully applied.")

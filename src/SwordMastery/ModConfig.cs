using StardewModdingAPI;

namespace SwordMastery;

internal sealed class ModConfig
{
    // Mod display language. This is intentionally independent from the
    // Stardew Valley game language.
    public string Language { get; set; } = "English";
    // Full-body HD combat poses shown only on skill activation.
    // Does not alter combat damage, cooldowns or normal movement.
    public bool EnableSkillPoseAnimation { get; set; } = true;
    public bool EnableSkillPoseOverlay { get; set; } = true;

    // Passive skills are independently controllable without changing skill allocations.
    public bool SwordWaveEnabled { get; set; } = true;
    // Stage III wave: separately switchable, 1% current max Energy per cast by default.
    public bool SwordWaveStage3Enabled { get; set; } = true;
    public int SwordWaveStage3EnergyHundredthsPercent { get; set; } = 100;
    public SButton SwordWaveStage3ToggleKey { get; set; } = SButton.None;
    public bool SwordsmanshipApexEnabled { get; set; } = true;
    // Off by default: the passive Sword Wave should never ruin mine stairs or hay grass.
    public bool SwordWaveBreaksRocks { get; set; } = false;
    public bool SwordWaveCutsGrass { get; set; } = false;
    // 100 means 1.00% of the CURRENT maximum stamina per successful activation cycle.
    // The GMCM option displays the percentage, not this internal integer.
    public int SwordsmanshipApexEnergyHundredthsPercent { get; set; } = 100;
    public int UltimateFootworkEnergyHundredthsPercent { get; set; } = 100;
    public bool UltimateFootworkEnabled { get; set; } = true;
    public SButton SwordWaveToggleKey { get; set; } = SButton.F11;
    public SButton SwordsmanshipApexToggleKey { get; set; } = SButton.F12;

    public bool ShowHud { get; set; } = true;
    public int HudX { get; set; } = 24;
    public int HudY { get; set; } = 24;

    public bool ShowCooldownHud { get; set; } = true;
    public int CooldownHudX { get; set; } = 24;
    public int CooldownHudY { get; set; } = 88;
    public SButton OpenMenuKey { get; set; } = SButton.K;

    // Combat skill hotkeys (PC test defaults; configurable through GMCM).
    public SButton BasicSkillAKey { get; set; } = SButton.F6;
    public SButton BasicSkillBKey { get; set; } = SButton.F7;
    public SButton BasicSkillCKey { get; set; } = SButton.F8;

    // Selected branch activation keys.
    // Ohgi B and Ultimate A are passive, so these keys are used by the active branches.
    public SButton OhgiSkillKey { get; set; } = SButton.F9;
    public SButton UltimateSkillKey { get; set; } = SButton.F10;

    public int MaxSwordLevel { get; set; } = 75;
    public int ExpPerMonsterKill { get; set; } = 10;

    // Required XP for the next level:
    // BaseXpRequirement + (currentLevel * XpGrowthPerLevel)
    public int BaseXpRequirement { get; set; } = 40;
    public int XpGrowthPerLevel { get; set; } = 4;

    public int ResetCost { get; set; } = 1_000_000;
}

Sword Mastery 0.1.8 — PATCH44 FIX04 cumulative update
====================================================

INSTALL / UPDATE
1. Close Stardew Valley and back up your save.
2. Move the old SwordMastery folder outside Mods and keep it as a backup.
3. Extract this ZIP's SwordMastery folder into Stardew Valley/Mods.
4. If using Vortex, replace/disable the old package so it cannot restore an old DLL.
5. Launch through SMAPI. The mod list should show Sword Mastery 0.1.8.
Do not install the SOURCE archive as a mod. No DaLion or Cowardly Gunfighter DLL is replaced.

REQUIREMENTS
SMAPI 4.3.0 or later. Generic Mod Config Menu is optional.
Walk Of Life, DaLionheart and Cowardly Gunfighter are optional integrations, not new requirements.
Game validation environment: Stardew Valley 1.6.15, SMAPI 4.5.2.

LANGUAGES
English (default), Korean, Japanese and Simplified Chinese. Select the language in GMCM.

DEFAULT KEYS
K: Sword Mastery menu
F6 / F7 / F8: basic A / B / C
F9: selected active Secret Art
F10: selected active Ultimate
F11: sword-wave toggle
F12: swordsmanship apex toggle
Bindings can be changed in GMCM.

PROGRESSION / DEBUG
Ordinary GMCM Save/Reset does not apply debug growth edits.
Edit debug level/SP/access, then click the explicit Apply option OFF to ON.
Level changes preserve EXP; SP is edited separately. For another application, click OFF to ON again.
Original growth is backed up under data/debug-backups/<save>/ before an explicit apply.
Backups are not automatically restored and lost historical EXP is never guessed.
The sewer statue's explicitly requested skill reset remains separate from GMCM saving.

COMPATIBILITY
DaLionheart 2.2.6: startup should report WOL kill callback compatibility: 2/2 targets installed.
Cowardly Gunfighter with RollSkill.IsRolling: startup should report roll attack guard: 4/4 targets installed.
New attacks/skills are ignored during a local roll. Release a held attack key and press again after rolling.
Sword waves follow actual native swings rather than click rate.

DIAGNOSTICS
SMAPI console: sm_status / sm_combat_status
For roll issues with Cowardly Gunfighter: cowboy_rollstatus
Send the full SMAPI log and circumstances if errors occur. See VALIDATION_STATUS.txt for test limits.

Nexus: https://www.nexusmods.com/stardewvalley/mods/53641

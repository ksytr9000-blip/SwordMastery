# Sword Mastery PATCH44 — Astra handoff (2026-10-22)

## Network failure workaround
The Cloud Environment could not clone/download the original repo due to a network proxy error. **Do not retry the cloud clone as a prerequisite.** GitHub's connected tool can read/edit text files directly in this branch; GitHub Actions can compile and package them inside GitHub.

Repository: https://github.com/ksytr9000-blip/SwordMastery
Working branch: `build/patch44-gmcm-safety`
Editable cumulative source: `src/SwordMastery/` (including `src/SwordMastery/ModEntry.cs`, `Services/CombatService.cs`, `ModConfig.cs`, i18n).
GitHub Actions workflow: `.github/workflows/patch44.yml` (builds **directly from** this source, no unpack scripts run on every build).
Successful full build: https://github.com/ksytr9000-blip/SwordMastery/actions/runs/39755099013
Artifacts: `SwordMastery_PATCH44_GMCM_SAFETY_TEST_INSTALL` and `SwordMastery_PATCH44_EDITABLE_SOURCE`.

## Implemented and compile-verified
1. PATCH43 FIX05 cumulative source reconstructed in GitHub Actions, with existing sewer-statue skill reset intact.
2. GMCM debug Level / SP / Secret Arts unlock / Ultimate unlock setter callbacks no longer directly mutate permanent progress. They stage local draft values.
3. Explicit `DebugApplyChangesExplicitly` GMCM option applies drafts to save only when user intentionally triggers it.
4. GMCM ordinary Save/Reset, HUD settings and localization changes do not execute those progress mutations. Switching save file clears transient drafts.
5. EN / KO / JA / ZH instructions updated, debug level applied only when changed.
6. SMAPI .NET6 compilation and packaging passed.

## Still open
- Runtime in-game verification: repeated GMCM saves/restarts, debug changes, sewer statue test, check previous regressions. Automated build is **not** an in-game test.
- Walk Of Life - Rebirth compatibility: reported exception `InvalidOperationException: Method onMonsterKilled must be static` during ranged projectile hit; actual full SMAPI stack trace and exact WOL/Sword Mastery version needed before modifying damageMonster/other mod hooks.
- Recovering a player's old damaged level cannot be inferred reliably from allocated skill points; do not guess or overwrite lost XP.

## To resume work
1. Use the connected GitHub integration to fetch text files from this branch; the source is now unpacked, not buried in a ZIP. Do not assume `main` is current.
2. Modify source files on a separate new working branch based on this one.
3. Use GitHub Actions to compile/package; collect build logs and artifact URLs.
4. Preserve existing localization, stat scaling, sword-only restrictions and other cumulative features.

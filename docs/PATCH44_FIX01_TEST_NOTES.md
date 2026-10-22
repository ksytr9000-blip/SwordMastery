# PATCH44 FIX01 — explicit debug apply

Based on PATCH44 / PATCH43 FIX05 cumulative source. The sewer statue, skills, quests, sword restrictions, combat, stamina settings and four languages are retained.

## Changes for Nexus

- Debug level, SP and access fields are staged from actual GMCM field-edit notifications. Ordinary Save/Reset callbacks cannot execute them.
- Clicking the Apply option OFF→ON applies edited fields immediately. It is not a persistent setting. For another application, click OFF→ON again.
- Debug level changes preserve current XP and SP. Set SP separately when desired.
- An original growth snapshot is saved to `data/debug-backups/<save-folder>/` before an explicit apply. Backups are never auto-restored. Stale or invalid drafts are rejected.
- Version `0.1.7-patch44.1` and the `PATCH44 FIX01 loaded` startup message distinguish the installer from older DLLs.

## Validation

Automated checks run the production draft service and check that GMCM setter callbacks are read-only. Build/ZIP verification is separate from in-game verification.

In-game pending: edit level/SP then click Apply OFF→ON; check `sm_status` immediately; save ordinary HUD/language/key/stamina settings repeatedly; close/reopen without applying; sleep/restart; verify learned/selected skills and quest state; test sewer statue only on a disposable save copy.

Reported user verification before this fix: the genuine PATCH44 installer preserved level 10, XP 20 and SP 10 after HUD save. Debug level/SP application was reported ineffective; this revision changes the event path but still requires a user in-game retest.

Walk Of Life 1.5.0-Beta5 compatibility remains unverified and unmodified. The supplied log did not contain the reported `onMonsterKilled must be static` exception. No kill hooks are blocked and no combat exceptions are swallowed.

Install only the INSTALL artifact. Remove the old mod folder from Mods before installing. Keep the original folder/save backup elsewhere; Vortex deployments can restore files. Do not install the editable source ZIP as the mod.

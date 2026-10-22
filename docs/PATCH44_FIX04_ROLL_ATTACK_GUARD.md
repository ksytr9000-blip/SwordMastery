# Sword Mastery PATCH44 FIX04 — block attacks during rolls

Requested behavior: while Cowardly Gunfighter's Space dodge roll is active, ignore new attacks; accept attacks normally as soon as the roll ends.

## Changes for Nexus

- Read Cowardly Gunfighter's live `RollSkill.IsRolling(Farmer)` state through an optional reflection delegate. No guessed duration/cooldown or permanent dependency is introduced.
- During local-player rolls, suppress attack buttons and configured Sword Mastery skill keys. Reject native tool presses, queued FireTool events, melee combo inputs and special attacks before their method bodies run.
- Sword Mastery active skills and passive wave creation independently reject activation while rolling, before stamina/skill cooldown changes. Projectiles already in flight retain their behavior.
- After the actual roll ends, guards stop blocking immediately; no timer, queued auto-attack or extra recovery delay is added. A held/suppressed input may need to be released and pressed again.
- The guard does not force movement, reset tool/sprite flags, change remote players, alter roll distance/cooldown, or disable the roll. Startup reports `PATCH44 FIX04 roll attack guard: 4/4 targets installed`.
- Includes all FIX01 GMCM/debug, FIX02 native-swing wave synchronization and FIX03 DaLionheart kill callback compatibility changes. Installer version: `0.1.7-patch44.4`.

## Validation

Automated tests install the production Harmony guard on minimal game stand-ins and exercise 100 roll/attack/end-roll cycles. They verify attack entry bodies never run while rolling, no local lock state is rewritten, first post-roll inputs are accepted, and remote attacks still run. Existing GMCM, animation, native-wave and WOL callback tests are retained. These are not actual game reproduction tests.

In-game pending: confirm the 4/4 startup line; perform Space→left-click repeatedly (including holding/releasing), Space→right-click and Space→Sword Mastery skill hotkeys. During the roll there should be no new swing, wave or skill stamina/cooldown charge. After the roll, a fresh attack should work and movement should remain normal. Retest slow/fast swords, ordinary attacks, sword-wave OFF/ON, sleep/restart and WOL Poacher (P) combat.

If the freeze persists, capture `sm_combat_status` and `cowboy_rollstatus` before right-clicking, plus the full SMAPI log. Existing DoT/poison kill callback compatibility still needs an in-game retest; this change does not claim that validation is complete.

Install only this cumulative INSTALL artifact after backing up/removing the old SwordMastery folder. No Cowardly Gunfighter or DaLion DLL is replaced.

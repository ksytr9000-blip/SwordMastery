# PATCH44 FIX03 — Walk Of Life / DaLionheart kill callback

## Confirmed cause

The user reproduced `InvalidOperationException: Method onMonsterKilled must be static` with Walk Of Life 1.5.0-Beta5, Poacher (P), DaLionheart 2.2.6 and Stardew Valley 1.6.15. The stack reaches `DaLion.Core.Framework.Patchers.MonsterUpdatePatcher.MonsterUpdatePrefix`, line 135, through `GetStaticMethodDelegate` / `CompileStaticDelegate`.

Matching upstream 2.2.6 source incorrectly requests a static delegate for `GameLocation.onMonsterKilled` in two places:

- [MonsterUpdatePatcher](https://github.com/daleao/sdv/blob/main/Core/Framework/Patchers/MonsterUpdatePatcher.cs): death processing after bleed, burn and poison updates.
- [Monster_Poisoned](https://github.com/daleao/sdv/blob/main/Core/Framework/Debuffs/Monster_Poisoned.cs): maximum poison stack instant kill.

The exception is raised in DaLionheart's delegate creation, not in Sword Mastery's projectile damage method. A direct upstream correction belongs in DaLionheart. The stack alone does not identify which DoT type produced this individual death.

## Compatibility change / Nexus changelog

- Added a narrowly scoped optional compatibility bridge for **DaLionheart 2.2.6**. Only the exact erroneous static factory calls in the two named methods are replaced. Other core versions and shared reflection caches are unchanged.
- Invoke the game's instance `onMonsterKilled` using the **monster's owning location**, preserving killer, monster, hitbox and the fourth flag. Resolve the native four-argument shape, with a three-argument compatibility shape for older game APIs.
- Poison/bleed/burn updates, Poacher procs, profession effects and the original game/mod kill hooks remain in place. No gameplay exceptions are swallowed; no kill callbacks are blocked. Native method handles drops, XP and quest/profession callbacks. No additional damage/loot/XP loop is added.
- Startup reports `PATCH44 FIX03 WOL kill callback compatibility: 2/2 targets installed`. Unknown native method signatures and patch mismatches are logged as errors, not treated as success.
- Includes FIX01 GMCM/debug safeguards and FIX02 actual-swing sword-wave synchronization. Installer version: `0.1.7-patch44.3`.

## Validation and limits

CI executes the production compatibility transpiler against minimal stand-in DaLion methods containing the same failing factory calls. It invokes 100 DoT and 100 poison callbacks, checking exact single native instance dispatch, correct owning location and preserved arguments/return values. It also verifies unrelated reflection behavior remains unchanged, newer/missing core is skipped, and three/four-argument native signatures are handled. These tests use stubs and are **not actual in-game kills** or proof of live mod load-order compatibility.

Actual game retest pending: back up the save, install this cumulative SwordMastery ZIP, verify the 2/2 startup line, keep Poacher (P), and repeat the combat sequence that just produced the exception. Check poison/DoT kills, second poison application, normal attacks, sword waves, simultaneous kills, drops and combat XP. Include 100+ actual attack/kill situations across the full test; stop and send the full SMAPI log on an exception. Check sleep/restart and GMCM state preservation too.

Cowardly Gunfighter Space-roll followed by attack can still require separate investigation; this patch does not claim to fix its movement lock. Capture `sm_combat_status` and `cowboy_rollstatus` before using right-click to escape.

Install only the INSTALL artifact, replacing the old mod folder after backing it up. Do not replace any DaLion DLL; this bridge lives in Sword Mastery. No save is automatically repaired or overwritten from a guessed recovery value.

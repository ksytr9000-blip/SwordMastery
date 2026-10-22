# Sword Mastery PATCH44 FIX02

## Changes for Nexus

- Passive sword waves now launch from an actual native sword swing, instead of from every raw left-click/C/controller-X input. Attack speed and the game's combo timing determine accepted swings. Each accepted swing requests one wave; rejected clicks request none. Native/automatic combo swings use the same hook.
- Wave stages, damage, distance/pierce scaling, stamina settings, toggles, skill requirements and vanilla monster damage/loot/experience hooks are unchanged. No extra sword attack is invoked by the new hook.
- Cosmetic skill poses no longer replace an active tool/single animation or an existing movement lock. Skill damage and cooldown behavior remain in place.
- Added read-only `sm_combat_status` and a diagnostic warning after a sword animation remains on the same frame/index with movement locked for 180 update ticks. The diagnostic never forces movement, resets player/tool state, blocks kill callbacks, or hides exceptions.
- Includes PATCH44 FIX01's explicit GMCM debug application, XP/SP preservation, snapshots, four languages and all cumulative features. Installer version: `0.1.7-patch44.2`.

## Evidence and limitations

The raw-click emission path was confirmed in source. The new hook runs after `MeleeWeapon.setFarmerAnimating` and rejects skipped originals, remote actors, swapped weapons, menus/events, and missing native tool/sprite actions. This keeps the player's real weapon swing as the source of truth.

The user initially reported a movement lock with an extreme +777-speed frying pan, even with waves OFF, and normal behavior with other swords. The user then identified rolling followed by attacking as the trigger. The earlier weapon-only hypothesis is not confirmed. This revision adds animation ownership guards and diagnostics, but does not claim to resolve the roll transition or establish which mod owns the lock. No arbitrary stat cap is applied. If using Cowardly Gunfighter roll, capture `cowboy_rollstatus` together with `sm_combat_status` before right-clicking.

Walk Of Life 1.5.0-Beta5 is loaded in the user's log. The supplied combat log contains no `onMonsterKilled must be static` exception; that issue remains unverified and unmodified.

## Tests

Automated: production progression draft tests, production skill-motion guard tests with minimal game stubs, production native hook extracted verbatim for guard/count tests with game stubs, source checks and full SMAPI compilation. These are not 100 in-game combat kills and do not prove third-party runtime compatibility.

In-game pending: use normal slow and fast swords; spam click and hold attack; confirm one wave per actual swing and native attack-speed dependence; confirm wave OFF, stages I/II/III, stamina, drops/XP and special moves; repeat with active skills; sleep/restart and GMCM regression checks. If movement locks, run `sm_combat_status` before right-clicking and send the output. Check that startup shows PATCH44 FIX02.

Install the INSTALL ZIP into Mods after preserving/removing the old folder. Do not install the EDITABLE_SOURCE artifact as the mod. Retain save backups. A new native-hook startup message confirms hook installation; startup failure is logged explicitly, with no raw-click fallback.

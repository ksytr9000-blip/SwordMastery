PATCH39 - mining and motion restoration (based on PATCH38 FIX01 + PATCH36 cumulative source)

- Sword mining stone/ore now invokes GameLocation.OnStoneDestroyed once per stone, mirroring Pickaxe.DoFunction rewards and MineShaft ladder/shaft chance. No direct stone deletion without rewards.
- Removed the unconditional silent-delete fallback for non-stone mining objects.
- Restored vanilla six-frame sword swings for Swordsman Step (Basic A), Calico Style (Basic C) and Issen (Ohgi A).
- Sword Wave (Ohgi B) already triggers from the player's regular vanilla sword attack; left unchanged.
- Swordsmanship Apex (Ultimate A) is passive and never initiated a custom pose; left untouched.
- Slash (Basic B), Pinnacle of Swordsmanship (Ohgi C), Ultimate Footwork (Ultimate B) retain their current custom motion.
- PATCH38 GMCM scoped Chinese/Japanese label correction retained, no global font changes.
- Preserve existing movement/damage/cooldown and localization assets.
- Build/test in actual game still needed.

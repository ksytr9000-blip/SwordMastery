PATCH 40 - PASSIVE CONTROL / ENERGY BALANCE

New features
- Sword Wave defaults to monster-only; mine rocks and hay grass are untouched.
- Sword Wave rock breaking and grass clearing are separate GMCM opt-ins (both default OFF).
- Sword Wave toggle via GMCM or F11 (configurable).
- Swordsmanship Apex toggle via GMCM or F12 (configurable; Steam screenshot hotkey warning).
- Apex spends 0.15% of CURRENT maximum stamina only when an attack cycle finds an enemy.
- Apex automatically skips that attack cycle if stamina is insufficient. Zero cost is supported.
- Percentage adjustable from 0.00% to 5.00% in 0.05% steps in GMCM.
- Both toggles persist in config.json and have new strings in all four languages.

Unchanged: skill damage, stages, CD, existing save IDs, melee hit behavior, other skill mining behavior, and earlier CJK font scope.

Install: Replace older SwordMastery folder; do not load two copies sharing Prototype.SwordMastery.

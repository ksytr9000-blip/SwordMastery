Sword Mastery PATCH34 – Native farmer animation + real dash + CJK glyphs

- Uses real FarmerSprite/Vanilla FarmerRenderer only: clothes, hats, pants, hair, boots and mods keep their original textures.
- No Farmer.draw Harmony hook and no overlaid cutout character sprite.
- Restores the PATCH30 immediate collision-checked dash for Basic A, Ohgi A, Ultimate B; removes reversion-to-start that broke actual motion.
- CJK texture glyph atlases rendered from open-source Noto CJK; original font files are NOT shipped.
- Skill tree and SwordMastery HUD use atlas SpriteFont; GMCM temporarily substitutes atlases only during GMCM menu rendering and immediately restores original fonts.
- Translation strings from JA/ZH JSON are used for GMCM, independently of game locale.

Actual in-game tests still required. English default.

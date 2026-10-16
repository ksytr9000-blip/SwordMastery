PATCH 32 - CUSTOM SWORD POSES (SOURCE ONLY)
================================================
Base: Original SwordMastery PATCH30 + cumulative PATCH31 (Japanese, Chinese).

Added:
- SkillMotionService builds distinct real farmer attack-pose sequences using
  the valid vanilla directional sword animation frames (up/right/down/left).
- Per-skill choreography for Step, Slash, Calico multi-slash,
  Issen, Swordsmanship Pinnacle, and Ultimate Footwork.
- Two original 32px-cell pixel animation sheets: pose_arm.png, pose_blade.png.
- Overlay syncs to current skill while it is active; never replaces player's
  pants, shirt or hair sprite. Shows the extra sword only when a melee weapon
  is currently equipped.
- GMCM toggles for pose animations and overlay (the latter can be disabled
  when the user has a custom farmer graphics mod).
- On warp/menu/weapon switch/returned-to-title, pose overlays are cleared.
- Passive Sword Wave and passive Swordsmanship Apex have no independent
  player-cast animation, matching the existing passive gameplay.

IMPORTANT
- This is a cumulative SOURCE patch; DLL has not been built.
- No live game test. Verify animation layering, directions, interaction with
  custom outfits, 4-direction view, switching tools, and multi-hit timings.
- Sprite poses are original overlays plus re-choreographed vanilla character
  frames, not an entirely replaced FarmerRenderer or full-body spritesheet.
- Older save data and skill mechanics have not been altered.

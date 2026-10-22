SwordMastery PATCH36: Scoped JA/ZH UI font rendering

- Removed PATCH35's global Game1.smallFont/dialogueFont swap inside the entire GMCM menu.
- Font hooks now affect only strings produced by this mod's I18n.Get/GetUi
  (tracked by weak reference identity) and exact original game-data strings
  for this mod's quest/item/notification content.
- GMCM shared UI, system buttons, other mods and their Korean labels remain
  in the currently selected game language and font.
- GMCM TryGetCurrentMenu scopes the glyph hook to this mod's own settings
  page. Other mod settings, GMCM common footer/header and mod list are untouched.
- Keeps PATCH35 Japanese/Chinese translations, native farmer motion, dash recovery.
- Game display and integration still require real in-game validation.

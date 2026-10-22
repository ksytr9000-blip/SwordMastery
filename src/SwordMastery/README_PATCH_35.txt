Sword Mastery PATCH35 -- fully localized native menus (JA/ZH)

BASE: PATCH34 native FarmerSprite + dash restoration + CJK glyph atlas. All previous features are preserved.

- Removed the forced English fallback in I18n.Get for Japanese and Simplified Chinese.
- Selected translation now flows into items, descriptions, quest journal, quest notifications, HUD, GMCM and skill tree.
- Added CjkTextService using PATCH34's localized glyph image atlas (no TTF/OTF/XNB files bundled).
- CjkTextService adapts native SpriteBatch.DrawString, SpriteFont.MeasureString and SpriteText drawing/measurement where necessary, while leaving other game text and locale unmodified.
- No external required mod.
- Complete key+placeholder consistency can be verified for all four languages.
- Actual in-game rendering must still be verified by user; successful compilation alone does not guarantee that every vanilla/third-party UI route displays every string correctly.

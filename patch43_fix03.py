from pathlib import Path
p=Path("SwordMastery/ModEntry.cs")
s=p.read_text(encoding="utf-8-sig")
old='I18n.Get("reset.statue.confirm", new { cost = mod.Config.ResetCost })'
new='I18n.Get("reset.statue.confirm").Replace("{cost}", mod.Config.ResetCost.ToString("N0")).Replace("[cost]", mod.Config.ResetCost.ToString("N0"))'
assert s.count(old)==1
s=s.replace(old,new)
# SMAPI question dialogue callback has game-specific response keys. Use unique keys without underscore to avoid dispatch splitting.
for a,b in [
 ('"SwordMastery_Vanilla"','"SMVanilla"'),
 ('"SwordMastery_Respec"','"SMRespec"'),
 ('"SwordMastery_Cancel"','"SMCancel"'),
 ('"SM_ConfirmYes"','"SMConfirmYes"'),
 ('"SM_ConfirmNo"','"SMConfirmNo"')]:
 s=s.replace(a,b)
# Callback is a delegate handler and strings must match; diagnostic on interaction.
s=s.replace('if (answer == "SMConfirmYes")\n                                mod.ScheduleStatueReset();','if (answer == "SMConfirmYes")\n                                mod.ScheduleStatueReset();')
# Avoid reliance on I18n interpolator: confirm string uses a manual replacement; completion HUD also direct.
s=s.replace('I18n.Get("reset.scheduled", new { cost = Config.ResetCost })','I18n.Get("reset.scheduled").Replace("{cost}", Config.ResetCost.ToString("N0"))')
p.write_text(s,encoding="utf-8")
print("FIX03 dialogue keys and cost text updated")

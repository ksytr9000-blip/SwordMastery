from pathlib import Path
p=Path("SwordMastery/ModEntry.cs")
s=p.read_text(encoding="utf-8-sig")
old='OriginalStatueAction = AccessTools.Method(typeof(GameLocation), "performAction");'
new='OriginalStatueAction = AccessTools.Method(typeof(GameLocation), "performAction", new[] { typeof(string[]), typeof(Farmer), typeof(xTile.Dimensions.Location) });'
assert s.count(old)==1
p.write_text(s.replace(old,new),encoding="utf-8")
print("Disambiguated performAction overload")

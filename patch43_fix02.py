from pathlib import Path
p=Path("SwordMastery/ModEntry.cs")
s=p.read_text(encoding="utf-8-sig")
old='__args.Length == 0 || __args[0] is not string action ||\n            !action.StartsWith("DogStatue", StringComparison.OrdinalIgnoreCase))'
new='__args.Length == 0 || __args[0] is not string[] actionParts || actionParts.Length == 0 ||\n            !actionParts[0].Equals("DogStatue", StringComparison.OrdinalIgnoreCase))'
assert s.count(old)==1,f"old condition count {s.count(old)}"
s=s.replace(old,new)
p.write_text(s,encoding="utf-8")
print("FIX02: use string[] actionParts, not string, to match performAction signature")

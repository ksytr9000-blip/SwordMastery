from pathlib import Path
p=Path("SwordMastery/ModEntry.cs")
s=p.read_text(encoding="utf-8-sig")
def replace(a,b):
 global s
 assert s.count(a)==1, f"expected one instance ({s.count(a)}): {a[:100]}"
 s=s.replace(a,b)
replace('    private bool PendingStatueConfirmation;','    private bool PendingStatueConfirmation;\n    private const int StatueResetCost = 10000;')
replace('I18n.Get("reset.statue.confirm", new { cost = Config.ResetCost.ToString("N0") })','I18n.Get("reset.statue.confirm", new { cost = StatueResetCost.ToString("N0") })')
start=s.index('    private void ScheduleStatueReset()')
end=s.index('    private void CommandReset(',start)
part=s[start:end].replace('Config.ResetCost','StatueResetCost')
s=s[:start]+part+s[end:]
p.write_text(s,encoding="utf-8")
print("Statue-specific fee fixed to 10,000g, including dialogue and charge")

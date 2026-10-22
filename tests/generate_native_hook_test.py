from pathlib import Path
source=Path('src/SwordMastery/ModEntry.cs').read_text()
start=source.index('    private static void SwordAttackPostfix(')
brace=source.index('{',start)
depth=1; end=brace+1
while depth:
 if source[end]=='{': depth+=1
 elif source[end]=='}': depth-=1
 end+=1
method=source[start:end]
Path('tests/NativeHookUnderTest.g.cs').write_text('''// Generated from the production hook; do not edit this copy.
using StardewValley;
using StardewValley.Tools;
using StardewModdingAPI;
using SwordMastery.Models;
namespace SwordMastery;
internal sealed class ModEntry
{
    private static ModEntry? StatueInstance;
    private readonly SaveData Data = new();
    private readonly FakeCombat Combat = new();
    internal static ModEntry Create() { var instance = new ModEntry(); StatueInstance = instance; return instance; }
    internal int Waves => Combat.Waves;
    internal static void Invoke(MeleeWeapon weapon, Farmer farmer, bool runOriginal) => SwordAttackPostfix(weapon, farmer, runOriginal);
    private sealed class FakeCombat { public int Waves; public void OnVanillaSwordAttack(SaveData _) => Waves++; }
'''+method+'\n}\n')

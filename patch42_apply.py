from pathlib import Path
import json

root=Path("SwordMastery")
def patch(path, old, new):
    p=root/path
    s=p.read_text(encoding="utf-8-sig")
    if s.count(old)!=1:
        raise RuntimeError(f"{path}: expected 1 occurrence, found {s.count(old)}: {old[:80]}")
    p.write_text(s.replace(old,new),encoding="utf-8")

patch("ModConfig.cs",
    "public int SwordsmanshipApexEnergyHundredthsPercent { get; set; } = 100;",
    "public int SwordsmanshipApexEnergyHundredthsPercent { get; set; } = 100;\n    public int UltimateFootworkEnergyHundredthsPercent { get; set; } = 100;")

patch("Services/CombatService.cs",
    '        // 보법의 극은 컨셉대로 쿨다운 없음.\n        UseUltimateFootwork(progress, stage, data);\n        message = "";\n        return true;',
    '''        // Footwork Ultimate is usable even without a sword. Charge exactly once.
        if (!TrySpendMaxStaminaPercent(Game1.player,
            currentConfig().UltimateFootworkEnergyHundredthsPercent))
        {
            message = I18n.Get("combat.footwork-insufficient-energy");
            return false;
        }

        UseUltimateFootwork(progress, stage, data);
        message = "";
        return true;''')

patch("Services/CombatService.cs",
    "int weaponDamage = Math.Max(InfinityBladeReferenceDamage, GetWeaponReferenceDamage(data));",
    "int weaponDamage = TryGetEquippedSword(out _)\n            ? Math.Max(InfinityBladeReferenceDamage, GetWeaponReferenceDamage(data))\n            : InfinityBladeReferenceDamage;")

patch("Services/CombatService.cs",
    "int weaponDamage = GetWeaponReferenceDamage(data);\n\n\n        Vector2 start = player.Position;",
    "int weaponDamage = TryGetEquippedSword(out _)\n            ? GetWeaponReferenceDamage(data)\n            : 1;\n\n        Vector2 start = player.Position;")

patch("Services/CombatService.cs",
    "float percent = Math.Clamp(hundredthsPercent, 0, 500);",
    "float percent = Math.Clamp(hundredthsPercent, 0, 2000);")

patch("ModEntry.cs",
    '        gmcm.AddKeybind(\n            mod: ModManifest,\n            getValue: () => Config.SwordsmanshipApexToggleKey,',
    '''        gmcm.AddNumberOption(
            mod: ModManifest,
            getValue: () => Config.UltimateFootworkEnergyHundredthsPercent,
            setValue: value => Config.UltimateFootworkEnergyHundredthsPercent = Math.Clamp(value, 0, 2000),
            name: () => I18n.GetUi("gmcm.footwork-cost.name"),
            tooltip: () => I18n.GetUi("gmcm.footwork-cost.tip"),
            min: 0,
            max: 2000,
            interval: 25,
            formatValue: value => $"{value / 100f:0.00}%",
            fieldId: "UltimateFootworkEnergyHundredthsPercent"
        );
        gmcm.AddKeybind(
            mod: ModManifest,
            getValue: () => Config.SwordsmanshipApexToggleKey,''')

translations={
    "default":("Ultimate Footwork Energy cost","Percent of maximum Energy spent per Ultimate Footwork cast (0–20%).","Not enough Energy to activate Ultimate Footwork."),
    "ko":("보법의 극 기력 소모","보법의 극 1회 사용 시 최대 기력 대비 소모 비율(0~20%).","보법의 극을 사용하기 위한 기력이 부족합니다."),
    "ja":("歩法の極・エナジー消費","歩法の極の使用時に最大エナジーの何％を消費するか（0～20％）。","歩法の極を使うエナジーが足りません。"),
    "zh":("极意步法体力消耗","每次施放极意步法消耗最大体力的百分比（0–20%）。","体力不足，无法施放极意步法。")
}
for lang, (name,tip,insufficient) in translations.items():
    p=root/"i18n"/(lang+".json")
    d=json.loads(p.read_text(encoding="utf-8-sig"))
    d["gmcm.footwork-cost.name"]=name
    d["gmcm.footwork-cost.tip"]=tip
    d["combat.footwork-insufficient-energy"]=insufficient
    p.write_text(json.dumps(d,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
print("PATCH42 source updates applied")

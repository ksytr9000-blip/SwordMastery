from pathlib import Path
import json
root=Path("SwordMastery")
def replace(path, old, new):
 p=root/path
 s=p.read_text(encoding="utf-8-sig")
 if s.count(old)!=1: raise Exception(str(path)+": expected one: "+str(s.count(old)))
 p.write_text(s.replace(old,new),encoding="utf-8")

replace("ModConfig.cs",
 "public int UltimateFootworkEnergyHundredthsPercent { get; set; } = 100;",
 "public int UltimateFootworkEnergyHundredthsPercent { get; set; } = 100;\n    public bool UltimateFootworkEnabled { get; set; } = true;")
replace("Services/CombatService.cs",
 '        // Footwork Ultimate is usable even without a sword. Charge exactly once.',
 '''        if (!currentConfig().UltimateFootworkEnabled)
        {
            message = I18n.Get("combat.footwork-disabled");
            return false;
        }

        // Footwork Ultimate is usable even without a sword. Charge exactly once.''')
replace("ModEntry.cs",
 '        gmcm.AddNumberOption(\n            mod: ModManifest,\n            getValue: () => Config.UltimateFootworkEnergyHundredthsPercent,',
 '''        gmcm.AddBoolOption(
            mod: ModManifest,
            getValue: () => Config.UltimateFootworkEnabled,
            setValue: value => Config.UltimateFootworkEnabled = value,
            name: () => I18n.GetUi("gmcm.footwork-enabled.name"),
            tooltip: () => I18n.GetUi("gmcm.footwork-enabled.tip"),
            fieldId: "UltimateFootworkEnabled"
        );
        gmcm.AddNumberOption(
            mod: ModManifest,
            getValue: () => Config.UltimateFootworkEnergyHundredthsPercent,''')
strings={
"default":("Enable Ultimate Footwork","Enable or disable Ultimate Footwork independently of Swordsmanship Apex.","Ultimate Footwork is disabled in GMCM."),
"ko":("보법의 극 사용","검술의 극과 별개로 보법의 극 사용 여부를 설정합니다.","GMCM에서 보법의 극이 비활성화되어 있습니다."),
"ja":("歩法の極を有効化","剣術の極とは別に歩法の極を有効または無効にします。","GMCMで歩法の極が無効になっています。"),
"zh":("启用极意步法","独立于剑术之极，启用或禁用极意步法。","极意步法已在GMCM中禁用。")}
for lang,(name,tip,warning) in strings.items():
 p=root/"i18n"/(lang+".json")
 d=json.loads(p.read_text(encoding="utf-8-sig"))
 d["gmcm.footwork-enabled.name"]=name
 d["gmcm.footwork-enabled.tip"]=tip
 d["combat.footwork-disabled"]=warning
 p.write_text(json.dumps(d,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
print("PATCH42 FIX01 controls: Footwork enabled + Energy cost")

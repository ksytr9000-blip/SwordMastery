from pathlib import Path
p=Path("SwordMastery/ModEntry.cs")
s=p.read_text(encoding="utf-8-sig")
assert "private const string SaveKey" in s
s=s.replace("using System.Reflection;","using System.Reflection;\nusing HarmonyLib;",1)
s=s.replace('    private const string SaveKey = "SwordMastery.SaveData";','''    private const string SaveKey = "SwordMastery.SaveData";
    private static ModEntry? StatueInstance;
    private static bool PassThroughVanillaStatue;
    private static MethodInfo? OriginalStatueAction;
''',1)
s=s.replace("        LoadCooldownIcons();",'''        LoadCooldownIcons();
        StatueInstance = this;
        OriginalStatueAction = AccessTools.Method(typeof(GameLocation), "performAction");
        if (OriginalStatueAction != null)
        {
            new Harmony(ModManifest.UniqueID + ".StatueReset").Patch(
                OriginalStatueAction,
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(StatueActionPrefix))
            );
        }
        else
            Monitor.Log("Could not connect to the Statue of Uncertainty interaction.", LogLevel.Warn);
''',1)
marker="    private void CommandReset(string command, string[] args)"
assert marker in s
addition='''    // Inject a separate Sword Mastery option BEFORE the vanilla statue dialogue.
    // The vanilla path replays the original action, leaving profession selection unchanged.
    private static bool StatueActionPrefix(GameLocation __instance, object[] __args, ref bool __result)
    {
        if (PassThroughVanillaStatue || StatueInstance == null ||
            !Context.IsWorldReady || !__instance.Name.Equals("Sewer", StringComparison.OrdinalIgnoreCase) ||
            __args.Length == 0 || __args[0] is not string action ||
            !action.StartsWith("DogStatue", StringComparison.OrdinalIgnoreCase))
            return true;

        var mod = StatueInstance;
        object[] original = (object[])__args.Clone();
        __instance.createQuestionDialogue(
            I18n.Get("reset.statue.prompt"),
            new[]
            {
                new Response("SwordMastery_Vanilla", I18n.Get("reset.statue.vanilla")),
                new Response("SwordMastery_Respec", I18n.Get("reset.statue.sword")),
                new Response("SwordMastery_Cancel", I18n.Get("reset.statue.cancel"))
            },
            (Farmer farmer, string choice) =>
            {
                if (choice == "SwordMastery_Vanilla")
                {
                    try
                    {
                        PassThroughVanillaStatue = true;
                        OriginalStatueAction?.Invoke(__instance, original);
                    }
                    finally { PassThroughVanillaStatue = false; }
                }
                else if (choice == "SwordMastery_Respec")
                {
                    __instance.createQuestionDialogue(
                        I18n.Get("reset.statue.confirm", new { cost = mod.Config.ResetCost }),
                        new[]
                        {
                            new Response("SM_ConfirmYes", I18n.Get("reset.statue.yes")),
                            new Response("SM_ConfirmNo", I18n.Get("reset.statue.cancel"))
                        },
                        (Farmer currentFarmer, string answer) =>
                        {
                            if (answer == "SM_ConfirmYes")
                                mod.ScheduleStatueReset();
                        }
                    );
                }
            }
        );
        __result = true;
        return false;
    }

    private void ScheduleStatueReset()
    {
        if (Data.PendingNightReset)
        {
            Game1.addHUDMessage(new HUDMessage(I18n.Get("reset.statue.pending"), HUDMessage.error_type));
            return;
        }
        if (Game1.player.Money < Config.ResetCost)
        {
            Game1.addHUDMessage(new HUDMessage(I18n.Get("reset.statue.no-money"), HUDMessage.error_type));
            return;
        }
        Game1.player.Money -= Config.ResetCost;
        Data.PendingNightReset = true;
        Game1.addHUDMessage(new HUDMessage(
            I18n.Get("reset.scheduled", new { cost = Config.ResetCost }), HUDMessage.newQuest_type));
    }

'''
s=s.replace(marker,addition+marker,1)
p.write_text(s,encoding="utf-8")
import json
translations={
"default":("What would you like to reset?","Vanilla professions","Sword Mastery skills","Cancel","Spend {cost}g to reset your Sword Mastery skills overnight?","Yes","Sword Mastery reset is already scheduled.","Not enough gold."),
"ko":("어떤 능력을 초기화하시겠습니까?","일반 직업 초기화","검술 스킬 초기화","취소","{cost}골드를 지불하고 오늘 밤 검술 스킬을 초기화하시겠습니까?","예","이미 검술 초기화가 예약되어 있습니다.","골드가 부족합니다."),
"ja":("どれをリセットしますか？","通常の職業","剣術スキル","キャンセル","{cost}Gを払い、今夜剣術スキルをリセットしますか？","はい","剣術のリセットは予約済みです。","お金が足りません。"),
"zh":("选择要重置的技能：","原版职业","剑术技能","取消","支付{cost}金币，今晚重置剑术技能吗？","是","剑术重置已经预约。","金币不足。")}
keys=["reset.statue.prompt","reset.statue.vanilla","reset.statue.sword","reset.statue.cancel","reset.statue.confirm","reset.statue.yes","reset.statue.pending","reset.statue.no-money"]
for lang,values in translations.items():
 p=Path("SwordMastery/i18n")/(lang+".json")
 d=json.loads(p.read_text(encoding="utf-8-sig"))
 d.update(dict(zip(keys,values)))
 p.write_text(json.dumps(d,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
print("PATCH43 statue respec dialog installed")

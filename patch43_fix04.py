from pathlib import Path
import json
p=Path("SwordMastery/ModEntry.cs")
s=p.read_text(encoding="utf-8-sig")
def replace(a,b):
 global s
 assert s.count(a)==1, f"match {s.count(a)}: {a[:80]}"
 s=s.replace(a,b)
replace('    private static MethodInfo? OriginalStatueAction;','    private static MethodInfo? OriginalStatueAction;\n    private bool PendingStatueConfirmation;')
replace('''                    __instance.createQuestionDialogue(
                        I18n.Get("reset.statue.confirm").Replace("{cost}", mod.Config.ResetCost.ToString("N0")).Replace("[cost]", mod.Config.ResetCost.ToString("N0")),''','''                    // Defer nested dialogue: vanilla clears the original callback after returning.
                    mod.PendingStatueConfirmation = true;
                }
                else if (choice == "SMCancel")
                {
                    // No changes.
                }
            }
        );
        __result = true;
        return false;
    }

    private void ShowStatueConfirmation()
    {
        GameLocation location = Game1.currentLocation;
        location.createQuestionDialogue(
            I18n.Get("reset.statue.confirm", new { cost = Config.ResetCost.ToString("N0") }),''')
# replace entire remainder of former nested block up to ScheduleStatueReset prefix
start=s.index('''                        new[]
                        {
                            new Response("SMConfirmYes"''')
end=s.index('''    private void ScheduleStatueReset()''',start)
s=s[:start]+'''            new[]
            {
                new Response("SMConfirmYes", I18n.Get("reset.statue.yes")),
                new Response("SMConfirmNo", I18n.Get("reset.statue.cancel"))
            },
            (Farmer farmer, string answer) =>
            {
                Monitor.Log($"Statue confirmation response: {answer}", LogLevel.Debug);
                if (answer == "SMConfirmYes")
                    ScheduleStatueReset();
            }
        );
    }

'''+s[end:]
# defer display until next tick so game isn't wiping callback
replace('''    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        if (!Context.IsWorldReady)
            return;

        Combat.Update(Data);''','''    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        if (!Context.IsWorldReady)
            return;
        if (PendingStatueConfirmation)
        {
            PendingStatueConfirmation = false;
            ShowStatueConfirmation();
        }

        Combat.Update(Data);''')
replace('''        Data.PendingNightReset = true;
        Game1.addHUDMessage(new HUDMessage(
            I18n.Get("reset.scheduled").Replace("{cost}", Config.ResetCost.ToString("N0")), HUDMessage.newQuest_type));''','''        Data.PendingNightReset = true;
        Helper.Data.WriteSaveData(SaveKey, Data);
        Monitor.Log($"Statue: paid {Config.ResetCost}g; sword reset scheduled.", LogLevel.Info);
        Game1.addHUDMessage(new HUDMessage(
            I18n.Get("reset.scheduled", new { cost = Config.ResetCost.ToString("N0") }),
            HUDMessage.newQuest_type));''')
replace('''        Skills.ExecuteRespec(Data);
        Game1.addHUDMessage(new HUDMessage(''','''        Skills.ExecuteRespec(Data);
        Monitor.Log("Statue: Sword Mastery skill reset completed after sleep.", LogLevel.Info);
        Game1.addHUDMessage(new HUDMessage(''')
# correct tokens across translations
for lang in ["default","ko","ja","zh"]:
 t=Path("SwordMastery/i18n")/(lang+".json")
 d=json.loads(t.read_text(encoding="utf-8-sig"))
 d["reset.statue.confirm"]=d["reset.statue.confirm"].replace("{cost}","{{cost}}")
 t.write_text(json.dumps(d,ensure_ascii=False,indent=2)+"\n",encoding="utf-8")
p.write_text(s,encoding="utf-8")
print("FIX04: deferred confirmation + token + persistent reset")

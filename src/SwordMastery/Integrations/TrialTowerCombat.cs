using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Tools;

namespace SwordMastery.Integrations;

public interface ITrialTowerCombatApi
{
    int GetApiVersion();
    bool SupportsNativeHitCallbacks();
    bool RegisterDirectAttackProvider(string id,Func<Farmer,Item?> equippedWeapon,Func<Item,bool> matchesWeapon);
    bool BeginDirectAttack(string id,Farmer who,Item weapon,string attackId);
    Dictionary<string,double> GetDirectAttackSnapshot(string id,Farmer who,Item weapon,string attackId);
    bool ExecuteDirectHit(string id,Farmer who,Item weapon,string attackId,string hitId,Monster target,
        int rawDamage,double basicDamage,double criticalRoll,double criticalChance,double criticalPower,Action<int,bool> applyNativeDamage);
}

internal sealed class TrialTowerCombat
{
    private const string Owner="SwordMastery/SkillHits";
    private ITrialTowerCombatApi? api;private Item? starting;
    private readonly IMonitor monitor;
    internal TrialTowerCombat(IModHelper helper,IMonitor monitor,Func<Item,bool> sword)
    {
        this.monitor=monitor;
        helper.Events.GameLoop.GameLaunched+=(_,_)=>
        {
            if(!helper.ModRegistry.IsLoaded("ValleyCoward.TrialTower"))return;
            try
            {
                var candidate=helper.ModRegistry.GetApi<ITrialTowerCombatApi>("ValleyCoward.TrialTower");
                if(candidate is null||candidate.GetApiVersion()<4||!candidate.RegisterDirectAttackProvider(Owner,
                    f=>Context.IsWorldReady&&ReferenceEquals(f,Game1.player)?starting:null,sword))
                    throw new InvalidOperationException("Tower API4/unique skill ownership required.");
                api=candidate;
            }
            catch(Exception ex){monitor.Log("Tower skill integration unavailable: "+ex.Message,LogLevel.Warn);}
        };
    }
    internal sealed class Attack
    {
        internal readonly Item Weapon;internal readonly Farmer Farmer;internal readonly string Id=Guid.NewGuid().ToString("N");
        internal bool Ready;internal double Basic,Scale=1,Chance,Power;
        internal Attack(Item weapon,Farmer farmer){Weapon=weapon;Farmer=farmer;}
    }
    internal Attack? Begin(MeleeWeapon weapon)
    {
        if(!Game1.currentLocation.NameOrUniqueName.StartsWith("ValleyCoward.TrialTower_",StringComparison.Ordinal))return null;
        var attack=new Attack(weapon,Game1.player);
        try
        {
            if(api?.SupportsNativeHitCallbacks()!=true)return attack;
            starting=weapon;
            if(!api.BeginDirectAttack(Owner,attack.Farmer,weapon,attack.Id))return attack;
            var stats=api.GetDirectAttackSnapshot(Owner,attack.Farmer,weapon,attack.Id);
            if(stats.Count==0||stats.Values.Any(v=>!double.IsFinite(v)))return attack;
            attack.Basic=((double)weapon.minDamage.Value+weapon.maxDamage.Value)/2*Math.Max(0,1+stats.GetValueOrDefault("AttackDamage")/100);
            attack.Scale=stats.GetValueOrDefault("DirectDamageMultiplier",1);
            // Ordinary skills used chance0/power1; preserve that baseline and the external additive modifiers.
            attack.Chance=stats.GetValueOrDefault("ExternalCriticalChance")/100;
            attack.Power=stats.GetValueOrDefault("ExternalCriticalDamage")/100;
            attack.Ready=attack.Basic>0&&attack.Basic<=int.MaxValue&&double.IsFinite(attack.Basic)&&attack.Scale>0;
        }
        catch(Exception ex){monitor.LogOnce("Tower skill snapshot rejected: "+ex.Message,LogLevel.Warn);}
        finally{starting=null;}
        return attack;
    }
    internal bool? Hit(Attack? attack,Monster target,int damage,bool guaranteed,float power,Action<int> nativeDamage)
    {
        if(attack is null)return null; // Unarmed skills and ordinary attacks retain their existing native path.
        if(!attack.Ready||api is null)return false;
        int before=target.Health;
        try
        {
            int raw=(int)Math.Clamp(Math.Round(damage*attack.Scale,MidpointRounding.AwayFromZero),1,int.MaxValue/4d);
            // Native EW attack buffs still apply in damageMonster; never apply the same attack percentage here.
            bool accepted=api.ExecuteDirectHit(Owner,attack.Farmer,attack.Weapon,attack.Id,Guid.NewGuid().ToString("N"),target,raw,attack.Basic,
                Game1.random.NextDouble(),guaranteed?1:Math.Clamp(attack.Chance,0,1),Math.Clamp(power+attack.Power,1,1000),(value,_)=>nativeDamage(value));
            return accepted&&target.Health<before;
        }
        catch(Exception ex){monitor.LogOnce("Tower skill hit failed; no native retry: "+ex.Message,LogLevel.Warn);return false;}
    }
}

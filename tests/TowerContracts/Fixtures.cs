using SwordMastery.Integrations;
using StardewValley;
using StardewValley.Monsters;
namespace StardewValley
{
    public class Item {public string QualifiedItemId="gun";}
    public class Farmer {public Item? CurrentItem;public Item? CurrentTool=>CurrentItem;}
    public class GameLocation {public string NameOrUniqueName="ValleyCoward.TrialTower_Test";}
    public static class Game1 {public static Farmer player=new();public static GameLocation currentLocation=new();public static Random random=new(5);}
}
namespace StardewValley.Monsters {public class Monster {public int Health=1000;}}
namespace StardewModdingAPI
{
    public static class Context {public static bool IsWorldReady=true;}
    public enum LogLevel {Warn}
    public interface IMonitor {void Log(string message,LogLevel level);void LogOnce(string message,LogLevel level);}
    public interface IModHelper {Registry ModRegistry{get;} Events Events{get;}}
    public class Registry {public object? Api;public bool IsLoaded(string id)=>Api is not null;public T? GetApi<T>(string id)where T:class=>Api as T;}
    public class Events {public Loop GameLoop=new();}
    public class Loop {public event EventHandler? GameLaunched;public void Raise()=>GameLaunched?.Invoke(this,EventArgs.Empty);}
    public class Helper:IModHelper {public Registry ModRegistry{get;}=new();public Events Events{get;}=new();}
    public class Monitor:IMonitor {public int Warnings;public void Log(string message,LogLevel level){Warnings++;}public void LogOnce(string message,LogLevel level){Warnings++;}}
}
internal sealed class TowerApi:ITrialTowerCombatApi
{
    internal bool Ready=true,Accept=true,Throw;
    internal Func<Farmer,Item?>? Equipped;
    private Func<Item,bool>? matches;
    internal readonly Dictionary<string,double> Stats=new(){{"AttackDamage",40},{"DirectDamageMultiplier",1.1},{"ExternalCriticalChance",3},{"ExternalCriticalDamage",20}};
    internal int Begins,Hits,Raw;internal double Basic,Chance,Power;internal Item? Source;
    public int GetApiVersion()=>4;
    public bool SupportsNativeHitCallbacks()=>Ready;
    public bool RegisterDirectAttackProvider(string id,Func<Farmer,Item?> equippedWeapon,Func<Item,bool> matchesWeapon){Equipped=equippedWeapon;matches=matchesWeapon;return true;}
    public bool BeginDirectAttack(string id,Farmer who,Item weapon,string attackId){Begins++;return Accept&&ReferenceEquals(Equipped!(who),weapon)&&matches!(weapon);}
    public Dictionary<string,double> GetDirectAttackSnapshot(string id,Farmer who,Item weapon,string attackId)=>new(Stats);
    public bool ExecuteDirectHit(string id,Farmer who,Item weapon,string attackId,string hitId,Monster target,int rawDamage,double basicDamage,double criticalRoll,double criticalChance,double criticalPower,Action<int,bool> applyNativeDamage)
    {Hits++;Raw=rawDamage;Basic=basicDamage;Chance=criticalChance;Power=criticalPower;Source=weapon;applyNativeDamage(rawDamage,criticalRoll<criticalChance);if(Throw)throw new InvalidOperationException();return true;}
}

namespace StardewValley.Tools { public class NetInt {public int Value=100;} public class MeleeWeapon:Item {public NetInt minDamage=new(),maxDamage=new();} }

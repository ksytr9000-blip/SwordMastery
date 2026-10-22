using System.Linq.Expressions;
using System.Reflection;

namespace SwordMastery.Integrations;

// An open INSTANCE delegate: the location is supplied at invocation, not captured
// from Game1.currentLocation (which can differ from the monster's location).
internal static class MonsterKillDelegateFactory
{
    public static Action<TLocation, TFarmer, TMonster, TBox, bool> Create<TLocation, TFarmer, TMonster, TBox>()
    {
        Type[] arguments = { typeof(TFarmer), typeof(TMonster), typeof(TBox), typeof(bool) };
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        MethodInfo? method = typeof(TLocation).GetMethod("onMonsterKilled", flags, null, arguments, null);
        method ??= typeof(TLocation).GetMethod("onMonsterKilled", flags, null, arguments[..3], null);
        if (method is null || method.IsStatic || method.ReturnType != typeof(void))
            throw new MissingMethodException(typeof(TLocation).FullName, "instance onMonsterKilled(Farmer, Monster, Rectangle[, bool])");

        var location = Expression.Parameter(typeof(TLocation), "location");
        var farmer = Expression.Parameter(typeof(TFarmer), "farmer");
        var monster = Expression.Parameter(typeof(TMonster), "monster");
        var box = Expression.Parameter(typeof(TBox), "box");
        var isBomb = Expression.Parameter(typeof(bool), "isBomb");
        Expression[] callArguments = method.GetParameters().Length == 4
            ? new Expression[] { farmer, monster, box, isBomb }
            : new Expression[] { farmer, monster, box };
        return Expression.Lambda<Action<TLocation, TFarmer, TMonster, TBox, bool>>(
            Expression.Call(location, method, callArguments), location, farmer, monster, box, isBomb).Compile();
    }
}

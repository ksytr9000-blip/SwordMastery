using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Monsters;

namespace Microsoft.Xna.Framework
{
    public readonly record struct Rectangle(int X, int Y, int Width, int Height);
}
namespace StardewValley.Monsters
{
    public sealed class Monster { public GameLocation? currentLocation; }
}
namespace StardewModdingAPI
{
    public enum LogLevel { Info, Warn, Error, Trace }
    public interface IMonitor { void Log(string message, LogLevel level); }
    public interface IGameLoopEvents { event EventHandler<EventArgs>? GameLaunched; }
    public sealed class TestGameLoopEvents : IGameLoopEvents
    {
        public event EventHandler<EventArgs>? GameLaunched;
        public void Launch() => GameLaunched?.Invoke(this, EventArgs.Empty);
    }
    public interface IModEvents { IGameLoopEvents GameLoop { get; } }
    public sealed class TestModEvents : IModEvents
    {
        public IGameLoopEvents GameLoop { get; } = new TestGameLoopEvents();
    }
    public interface IModHelper
    {
        IModRegistry ModRegistry { get; }
        IModEvents Events => new TestModEvents();
    }
    public interface IModRegistry
    {
        IModInfo? Get(string id);
        T? GetApi<T>(string id) where T : class => null;
    }
    public interface IModInfo { IManifest Manifest { get; } }
    public interface IManifest { string Version { get; } }
}
namespace DaLion.Shared.Reflection
{
    public static class Reflector
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static T GetStaticMethodDelegate<T>(Type type, string name) where T : Delegate
            => throw new InvalidOperationException($"Method {name} must be static.");
    }
}
namespace DaLion.Core.Framework.Patchers
{
    public static class MonsterUpdatePatcher
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool MonsterUpdatePrefix(Monster monster, Farmer who, Rectangle box, bool bomb)
        {
            DaLion.Shared.Reflection.Reflector.GetStaticMethodDelegate<Action<Farmer, Monster, Rectangle, bool>>(typeof(GameLocation), "onMonsterKilled")
                .Invoke(who, monster, box, bomb);
            return false;
        }
    }
}
namespace DaLion.Core.Framework.Debuffs
{
    public static class Monster_Poisoned
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool Increment_Poisoned(Monster monster, Farmer who, Rectangle box, bool bomb)
        {
            DaLion.Shared.Reflection.Reflector.GetStaticMethodDelegate<Action<Farmer, Monster, Rectangle, bool>>(typeof(GameLocation), "onMonsterKilled")
                .Invoke(who, monster, box, bomb);
            return false;
        }
    }
}

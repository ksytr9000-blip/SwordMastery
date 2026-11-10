using StardewValley;

namespace SwordMastery.Integrations;

/// <summary>Optional, backward-compatible Cowardly Gunfighter NativeRoll/v1 consumer.</summary>
public interface INativeRollCompatibilityApi
{
    string GetNativeRollCompatibilityKey();
    bool IsDodgeRolling(Farmer who);
}

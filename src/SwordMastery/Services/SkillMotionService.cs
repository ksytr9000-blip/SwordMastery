using System;
using System.Collections.Generic;
using StardewValley;

namespace SwordMastery.Services;

internal enum SkillMotion { Step, Slash, Flurry, Issen, Pinnacle, Footwork }

// Only use VANILLA FarmerSprite frames. The game draws this same Farmer object
// through FarmerRenderer so every equipped hairstyle, hat, shirt, pants and boots
// stays visible. No replacement farmer texture or Farmer.draw Harmony patch.
internal sealed class SkillMotionService
{
    private readonly Func<bool> enabled;
    private Farmer? activeFarmer;
    private GameLocation? activeLocation;
    private int remainingTicks;

    public SkillMotionService(Func<bool> enabled) => this.enabled = enabled;

    public void Play(Farmer farmer, SkillMotion motion, int hitCount = 1)
    {
        if (!enabled() || Game1.eventUp || Game1.activeClickableMenu is not null
            || farmer.UsingTool || !farmer.CanMove || farmer.FarmerSprite.PauseForSingleAnimation
            || !ReferenceEquals(farmer, Game1.player))
            return;

        // These are native farmer sprite sheet frames, not animation indices.
        // For each direction, the regular standing/walking/swiping frames are
        // preserved and the built-in renderer supplies actual clothes and hair.
        int idle = farmer.FacingDirection switch { 0 => 12, 1 or 3 => 6, _ => 0 };
        int lean = farmer.FacingDirection switch { 0 => 22, 1 or 3 => 20, _ => 18 };
        int strong = farmer.FacingDirection switch { 0 => 23, 1 or 3 => 21, _ => 19 };
        int stride = farmer.FacingDirection switch { 0 => 13, 1 or 3 => 7, _ => 1 };
        int recoil = farmer.FacingDirection switch { 0 => 14, 1 or 3 => 8, _ => 2 };
        bool flip = farmer.FacingDirection == 3;
        var frames = new List<FarmerSprite.AnimationFrame>();

        void Add(int pose, int ms, int yOffset = 0, bool arm = false)
            => frames.Add(new FarmerSprite.AnimationFrame(pose, ms, yOffset, arm, flip));

        switch (motion)
        {
            case SkillMotion.Step:
                Add(idle, 65); Add(lean, 45, 2); Add(stride, 45, 1, true);
                Add(strong, 50, 1, true); Add(recoil, 65); break;
            case SkillMotion.Slash:
                Add(idle, 100); Add(lean, 70, 1); Add(stride, 60, 1, true);
                Add(strong, 70, -1, true); Add(recoil, 90); break;
            case SkillMotion.Flurry:
                Add(idle, 60);
                for (int i = 0; i < Math.Clamp(hitCount, 1, 6); i++)
                { Add(i % 2 == 0 ? lean : stride, 55, 1, true);
                  Add(i % 2 == 0 ? strong : recoil, 55, -1, true); }
                Add(idle, 80); break;
            case SkillMotion.Issen:
                Add(idle, 100); Add(lean, 90, 2); Add(lean, 65, 2);
                Add(strong, 40, -1, true); Add(recoil, 75); break;
            case SkillMotion.Pinnacle:
                Add(lean, 75, 1);
                for (int i = 0; i < Math.Clamp(hitCount, 1, 10); i++)
                { Add(i % 2 == 0 ? stride : recoil, 46, 1, true);
                  Add(i % 2 == 0 ? strong : lean, 46, -1, true); }
                Add(idle, 80); break;
            case SkillMotion.Footwork:
                Add(idle, 35); Add(lean, 35, 1);
                Add(stride, 42, 1, true); Add(strong, 45, -1, true); break;
        }
        // Critically: do not touch farmer.Position, CanMove, or any renderer.
        farmer.FarmerSprite.PauseForSingleAnimation = false;
        farmer.FarmerSprite.animateOnce(frames.ToArray());
        activeFarmer = farmer;
        activeLocation = Game1.currentLocation;
        remainingTicks = Math.Max(1, (int)Math.Ceiling(frames.Count * 7.5));
    }

    public void Update()
    {
        if (remainingTicks == 0) return;
        if (!enabled() || !ReferenceEquals(activeFarmer, Game1.player)
            || activeLocation != Game1.currentLocation || Game1.eventUp
            || Game1.activeClickableMenu is not null)
        { Reset(); return; }
        --remainingTicks;
        if (remainingTicks == 0) { activeFarmer = null; activeLocation = null; }
    }

    public void Reset()
    {
        remainingTicks = 0; activeFarmer = null; activeLocation = null;
    }
}

from pathlib import Path
s=Path('src/SwordMastery/ModEntry.cs').read_text()
button=s[s.index('private void OnButtonPressed'):s.index('private void OnSaveLoaded')]
assert 'Combat.OnVanillaSwordAttack(Data)' not in button
assert 'nameof(SwordAttackPostfix)' in s
hook=s[s.index('private static void SwordAttackPostfix'):s.index('private void OnAssetRequested')]
for guard in ['!__runOriginal','ReferenceEquals(who, Game1.player)','ReferenceEquals(who.CurrentTool, __instance)','!who.UsingTool','!who.FarmerSprite.PauseForSingleAnimation']:
 assert guard in hook, guard
combat=Path('src/SwordMastery/Services/CombatService.cs').read_text()
visual=combat[combat.index('private void PlayVanillaSwordSwing'):combat.index('private void AnimateSwordSkill')]
assert visual.index('player.UsingTool') < visual.index('StopAnimation()')
assert visual.index('!player.CanMove') < visual.index('StopAnimation()')
assert visual.index('player.FarmerSprite.PauseForSingleAnimation') < visual.index('StopAnimation()')
assert 'forceCanMove' not in combat
print('Native swing hook, raw-click removal and animation ownership guards verified. Not an in-game test.')

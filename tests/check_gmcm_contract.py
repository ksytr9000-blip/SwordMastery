from pathlib import Path
import re
s=Path('src/SwordMastery/ModEntry.cs').read_text()
for field in ['DebugSwordLevel','DebugSkillPoints','DebugOhgiAccess','DebugUltimateAccess','DebugApplyChangesExplicitly']:
 end=s.index('fieldId: "'+field+'"')
 start=max(s.rfind('gmcm.AddNumberOption(',0,end),s.rfind('gmcm.AddBoolOption(',0,end))
 option=s[start:end]
 assert 'setValue: _ => { }' in option, field
assert 'gmcm.OnFieldChanged(ModManifest, OnDebugFieldChanged)' in s
assert 'save: () => Helper.WriteConfig(Config)' in s
print('GMCM save/reset callbacks cannot execute progression edits; explicit field-change hook registered.')

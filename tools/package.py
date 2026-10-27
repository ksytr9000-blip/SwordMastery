"""One versioned development installer; preserve older archives and exclude game DLLs."""
import hashlib
import json
from pathlib import Path
from zipfile import ZipFile, ZIP_DEFLATED

root = Path(__file__).resolve().parents[1]
mod = root / 'src/SwordMastery'
version = json.loads((mod / 'manifest.json').read_text())['Version']
out = root / 'artifacts' / f'SwordMastery-{version}-Development.zip'
out.parent.mkdir(exist_ok=True)
files = {'SwordMastery/SwordMastery.dll': mod / 'bin/Release/net6.0/SwordMastery.dll',
         'SwordMastery/manifest.json': mod / 'manifest.json',
         'SwordMastery/TOWER_INTEGRATION_KO.md': root / 'docs/TOWER_INTEGRATION_KO.md',
         'SwordMastery/README_USER.txt': mod / 'README_USER.txt'}
for directory in ('i18n', 'assets'):
    for path in sorted((mod / directory).rglob('*')):
        if path.is_file(): files['SwordMastery/' + path.relative_to(mod).as_posix()] = path
with ZipFile(out, 'w', ZIP_DEFLATED) as z:
    for name, path in files.items(): z.write(path, name)
with ZipFile(out) as z:
    assert z.testzip() is None
    assert set(n for n in z.namelist() if n.endswith('.dll')) == {'SwordMastery/SwordMastery.dll'}
    assert all(z.read(n) == p.read_bytes() for n, p in files.items())
    assert json.loads(z.read('SwordMastery/manifest.json'))['Version'] == version
digest = hashlib.sha256(out.read_bytes()).hexdigest()
out.with_suffix('.zip.sha256').write_text(f'{digest}  {out.name}\n')
print(f'Verified {out.name}: {len(files)} files; SHA256={digest}')

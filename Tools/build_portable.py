#!/usr/bin/env python3
"""Compile the complete Windows player into one self-extracting portable EXE."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import shutil
import subprocess

ROOT = Path(__file__).resolve().parents[1]
p = argparse.ArgumentParser()
p.add_argument('--player-dir', type=Path, default=ROOT/'Builds/Windows')
p.add_argument('--output', type=Path, default=ROOT/'Delivery/DuneDash-Portable.exe')
p.add_argument('--makensis', type=Path)
a = p.parse_args()
player = a.player_dir.resolve()
output = a.output.resolve()
required = ['DuneDash.exe', 'UnityPlayer.dll', 'DuneDash_Data/Managed/Assembly-CSharp.dll', 'MonoBleedingEdge/EmbedRuntime/mono-2.0-bdwgc.dll']
for name in required:
    if not (player/name).is_file():
        p.error('Missing Windows player file: '+str(player/name))
compiler = a.makensis or shutil.which('makensis')
if compiler is None:
    candidate = Path.home()/'.local/share/hm1-nsis/usr/bin/makensis'
    if candidate.is_file():
        compiler = candidate
if compiler is None:
    p.error('Install NSIS (makensis), or pass --makensis /path/to/makensis.')
compiler = Path(compiler).resolve()
env = os.environ.copy()
nsis_dir = compiler.parent.parent/'share/nsis'
if nsis_dir.is_dir():
    env['NSISDIR'] = str(nsis_dir)
output.parent.mkdir(parents=True, exist_ok=True)
script = ROOT/'Tools/Portable/DuneDashPortable.nsi'
icon = ROOT/'Tools/Portable/DuneDash.ico'
cmd = [str(compiler), '-V3', '-DPLAYER_DIR='+str(player), '-DPORTABLE_OUTPUT='+str(output), '-DPORTABLE_ICON='+str(icon), str(script)]
subprocess.run(cmd, env=env, cwd=ROOT, check=True)
def sha256(path):
    with path.open('rb') as f:
        return hashlib.file_digest(f, 'sha256').hexdigest()
files = sorted(f for f in player.rglob('*') if f.is_file())
manifest = {
    'format': 'NSIS portable self-extracting Windows application',
    'game': 'Dune Dash - Salt Coast',
    'requires': '64-bit Windows',
    'executable': output.name,
    'sha256': sha256(output),
    'size_bytes': output.stat().st_size,
    'payload_files': {f.relative_to(player).as_posix(): sha256(f) for f in files},
    'temporary_extraction': True,
    'administrator_required': False,
    'windows_runtime_tested': False,
}
manifest_path = output.with_suffix('.manifest.json')
manifest_path.write_text(json.dumps(manifest, indent=2)+'\n')
print('PORTABLE EXE: '+str(output))
print('MANIFEST: '+str(manifest_path))

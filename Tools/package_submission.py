#!/usr/bin/env python3
"""Package the requested player, a PDF report and repository link; omit Unity caches."""
import argparse
from pathlib import Path
import zipfile

root = Path(__file__).resolve().parents[1]
p = argparse.ArgumentParser()
p.add_argument('--first-name', required=True)
p.add_argument('--last-name', required=True)
p.add_argument('--student-id', required=True)
p.add_argument('--repository-url', required=True)
p.add_argument('--platform', choices=['windows', 'linux'], default='windows')
a = p.parse_args()
if not a.repository_url.startswith(('https://github.com/', 'https://gitlab.com/')):
    p.error('Provide the real GitHub/GitLab repository URL.')
platform = 'Windows' if a.platform == 'windows' else 'Linux'
build = root/'Builds'/platform
player = 'DuneDash.exe' if a.platform == 'windows' else 'DuneDash.x86_64'
if not (build/player).is_file():
    p.error('Build the '+platform+' player before packaging.')
report = root/'Documentation/GD_Hw1_Amirhossein_Ghadrdan_401521489.pdf'
if not report.is_file():
    p.error('Generate the PDF report first.')
folder = root/'Submission'
folder.mkdir(exist_ok=True)
name = f'GD_Hw1_{a.first_name}_{a.last_name}_{a.student_id}.zip'
if Path(name).name != name:
    p.error('Names cannot contain path separators.')
with zipfile.ZipFile(folder/name, 'w', zipfile.ZIP_DEFLATED) as z:
    z.writestr('repository.txt', a.repository_url+'\n')
    z.write(report, 'Report.pdf')
    for f in sorted(build.rglob('*')):
        if f.is_file():
            z.write(f, f.relative_to(root))
    launcher = root/('RunGame.bat' if a.platform == 'windows' else 'RunGame.sh')
    z.write(launcher, launcher.name)
print(folder/name)

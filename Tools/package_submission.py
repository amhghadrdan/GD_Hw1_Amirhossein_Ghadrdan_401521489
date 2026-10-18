#!/usr/bin/env python3
"""Package a real build and project report without Unity caches. No publishing."""
import argparse, zipfile
from pathlib import Path
root=Path(__file__).resolve().parents[1]
p=argparse.ArgumentParser()
p.add_argument('--first-name',required=True);p.add_argument('--last-name',required=True);p.add_argument('--student-id',required=True)
p.add_argument('--repository-url',required=True)
a=p.parse_args()
if not a.repository_url.startswith(('https://github.com/','https://gitlab.com/')):
 p.error('Provide the real GitHub/GitLab repository URL.')
build=root/'Builds/Linux'
if not (build/'DuneDash.x86_64').exists(): p.error('Build the Linux game first using Tools/unity.sh build.')
folder=root/'Submission';folder.mkdir(exist_ok=True)
name=f'GD_Hw1_{a.first_name}_{a.last_name}_{a.student_id}.zip'
if Path(name).name!=name:p.error('Names cannot contain path separators.')
with zipfile.ZipFile(folder/name,'w',zipfile.ZIP_DEFLATED) as z:
 z.writestr('repository.txt',a.repository_url+'\n')
 for area in [build,root/'Documentation']:
  for f in area.rglob('*'):
   if f.is_file(): z.write(f,f.relative_to(root))
print(folder/name)

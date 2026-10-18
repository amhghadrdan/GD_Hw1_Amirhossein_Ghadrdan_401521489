#!/usr/bin/env bash
set -euo pipefail
HM1_ROOT="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)"
HM1_UNITY="${UNITY_EDITOR:-$HOME/Unity/Hub/Editor/2022.3.62f1/Editor/Unity}"
if [[ ! -x "$HM1_UNITY" ]]; then
  printf '%s\n' 'Unity Editor was not found. Install Unity 2022.3 LTS in Unity Hub,' 'or set UNITY_EDITOR to the full path of your Unity executable.' >&2
  exit 1
fi
mkdir -p "$HM1_ROOT/Logs"
case "${1:-open}" in
  open) exec "$HM1_UNITY" -projectPath "$HM1_ROOT" -logFile "$HM1_ROOT/Logs/Editor.log" ;;
  build) exec "$HM1_UNITY" -batchmode -nographics -quit -projectPath "$HM1_ROOT" -executeMethod DuneDash.Editor.ProjectBuilder.BuildFromCommandLine -logFile "$HM1_ROOT/Logs/build.log" ;;
  windows) exec "$HM1_UNITY" -batchmode -nographics -quit -projectPath "$HM1_ROOT" -buildTarget Win64 -windows -executeMethod DuneDash.Editor.ProjectBuilder.BuildFromCommandLine -logFile "$HM1_ROOT/Logs/build-windows.log" ;;
  test) exec "$HM1_ROOT/Builds/Linux/DuneDash.x86_64" -screen-fullscreen 0 -screen-width 1280 -screen-height 720 -smokeTest -logFile "$HM1_ROOT/Logs/smoke-test.log" ;;
  *) printf '%s\n' 'Usage: Tools/unity.sh [open|build|windows|test]'; exit 2 ;;
esac

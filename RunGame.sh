#!/usr/bin/env bash
set -euo pipefail
HM1_ROOT="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
if [[ -x "$HM1_ROOT/Builds/Linux/DuneDash.x86_64" ]]; then
  exec "$HM1_ROOT/Builds/Linux/DuneDash.x86_64" -screen-fullscreen 0 -screen-width 1280 -screen-height 720 "$@"
fi
exec "$HM1_ROOT/Tools/unity.sh" open

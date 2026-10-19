#!/usr/bin/env bash
# Publish CuttingOptimizer.Api for MonsterASP IIS hosting (wwwroot upload).
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
OUT="${ROOT}/publish/api"

cd "$ROOT"

echo "Publishing API → ${OUT}"
rm -rf "${OUT}"
dotnet publish \
  "${ROOT}/src/CuttingOptimizer.Api/CuttingOptimizer.Api.csproj" \
  -c Release \
  -o "${OUT}"

echo ""
echo "Publish OK."
echo "Upload EVERYTHING inside publish/api/ to MonsterASP \\wwwroot (WebFTP)."
echo "Must include web.config:"
ls -la "${OUT}/web.config" 2>/dev/null || {
  echo "ERROR: web.config missing — IIS will not start the app." >&2
  exit 1
}
echo ""
echo "See docs/monsterasp-deploy.md for env vars + restart steps."

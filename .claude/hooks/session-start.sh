#!/bin/bash
# Deja el contenedor listo para correr las tres órdenes de CLAUDE.md («Antes de dar algo por
# terminado»). Sin esto, la sesión remota arranca sin dotnet y no puede ni compilar.
set -euo pipefail

# Solo en las sesiones remotas (Claude Code en la web): en una máquina local el entorno es del
# desarrollador y no le toca a un hook instalarle paquetes.
if [ "${CLAUDE_CODE_REMOTE:-}" != "true" ]; then
  exit 0
fi

# El SDK viene del repositorio de Ubuntu: dot.net / builds.dotnet.microsoft.com no pasan por el
# proxy de la sesión, y el paquete de apt sí.
if ! command -v dotnet >/dev/null 2>&1; then
  apt-get update -qq || true
  DEBIAN_FRONTEND=noninteractive apt-get install -y -qq dotnet-sdk-8.0
fi

# Mismo paso que .github/workflows/ci.yml.
if ! dotnet workload list 2>/dev/null | grep -q wasm-tools; then
  dotnet workload install wasm-tools
fi

#!/usr/bin/env bash
# Sincroniza el monorepo de GitHub hacia Azure DevOps Repos.
# Uso: AZDO_PAT=<pat> ./Scripts/Sync-Repo-To-Azure.sh [--force] [--dry-run]

set -euo pipefail

ORG="VSCAD"
PROJECT="tandem2026"
REPO="tandem2026"
BRANCH="master"
REMOTE="azure"
FORCE=0
DRY_RUN=0

for arg in "$@"; do
  case "$arg" in
    --force) FORCE=1 ;;
    --dry-run) DRY_RUN=1 ;;
    -h|--help)
      echo "Uso: AZDO_PAT=<pat> $0 [--force] [--dry-run]"
      exit 0
      ;;
    *)
      echo "Opcion desconocida: $arg" >&2
      exit 1
      ;;
  esac
done

PAT="${AZDO_PAT:-${AZURE_DEVOPS_PAT:-}}"
if [[ -z "$PAT" ]]; then
  echo "ERROR: Define AZDO_PAT o AZURE_DEVOPS_PAT" >&2
  exit 1
fi

AZURE_URL="https://dev.azure.com/${ORG}/${PROJECT}/_git/${REPO}"

echo "========================================"
echo " SYNC REPO COMPLETO -> AZURE DEVOPS"
echo "========================================"
echo

echo "[1/4] Validando PAT..."
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" -u ":$PAT" \
  "https://dev.azure.com/${ORG}/${PROJECT}/_apis/git/repositories?api-version=7.1")
if [[ "$HTTP_CODE" != "200" ]]; then
  echo "ERROR: PAT invalido o sin permisos (HTTP $HTTP_CODE)" >&2
  exit 1
fi
echo "  OK - PAT valido"

echo "[2/4] Comprobando estructura local..."
for item in Design.sln Desing DAL TamdenZwcadPluging Scripts; do
  if [[ ! -e "$item" ]]; then
    echo "ERROR: Falta $item. Ejecuta desde la raiz del monorepo." >&2
    exit 1
  fi
done
echo "  OK - Monorepo completo detectado"

echo "[3/4] Configurando remote '$REMOTE'..."
if git remote get-url "$REMOTE" >/dev/null 2>&1; then
  git remote set-url "$REMOTE" "$AZURE_URL"
else
  git remote add "$REMOTE" "$AZURE_URL"
fi
echo "  OK - Remote configurado"

echo "[4/4] Publicando rama '$BRANCH'..."
PUSH_URL="https://:${PAT}@dev.azure.com/${ORG}/${PROJECT}/_git/${REPO}"
echo "Destino: $AZURE_URL"
echo "Rama:    $BRANCH"
if [[ "$FORCE" -eq 1 ]]; then
  echo "Modo:    FORCE"
fi
echo

if [[ "$DRY_RUN" -eq 1 ]]; then
  echo "DRY RUN - Comando:"
  if [[ "$FORCE" -eq 1 ]]; then
    echo "  git push --force $AZURE_URL ${BRANCH}:${BRANCH}"
  else
    echo "  git push $AZURE_URL ${BRANCH}:${BRANCH}"
  fi
  exit 0
fi

GIT_ARGS=(-c credential.helper= push)
if [[ "$FORCE" -eq 1 ]]; then
  GIT_ARGS+=(--force)
fi
GIT_ARGS+=("$PUSH_URL" "${BRANCH}:${BRANCH}")

if ! git "${GIT_ARGS[@]}"; then
  echo "ERROR: git push fallo. Prueba con --force." >&2
  exit 1
fi

echo
echo "OK - Repositorio publicado en Azure DevOps"
echo "Files: https://dev.azure.com/${ORG}/${PROJECT}/_git/${REPO}?path=/&version=GB${BRANCH}"

#!/bin/bash

REPO_ROOT="$(cd "$(dirname "$0")" && pwd)"

echo ""
echo "========================================"
echo "  GestionBiblioteca — Build + Run (Mac)"
echo "========================================"
echo ""

PROJECTS=(
  "Microservicio_Usuario/Microservicio_Usuario"
  "Microservicio_LibroEjemplar/Microservicio_LibroEjemplar"
  "Microservicio_Multas/Microservicio_Multas"
  "Microservicio_Autor/Microservicio_Autor"
  "Microservicio_Frontend/Microservicio_Frontend"
)

for PROJ in "${PROJECTS[@]}"; do
  FULL="$REPO_ROOT/$PROJ"
  NAME="$(basename "$PROJ")"

  if [ ! -d "$FULL" ]; then
    echo "[SKIP] $NAME"
    continue
  fi

  echo "[BUILD] $NAME"
  dotnet build "$FULL" -q

  if [ $? -ne 0 ]; then
    echo "Error compilando $NAME"
    exit 1
  fi
done

echo ""
echo "Build OK. Starting services..."
echo ""

osascript -e "tell application \"Terminal\" to do script \"cd '$REPO_ROOT/Microservicio_Usuario/Microservicio_Usuario' && echo Usuario:5292 && dotnet run --no-build\""

osascript -e "tell application \"Terminal\" to do script \"cd '$REPO_ROOT/Microservicio_LibroEjemplar/Microservicio_LibroEjemplar' && echo LibroEjemplar:5101 && dotnet run --no-build\""

osascript -e "tell application \"Terminal\" to do script \"cd '$REPO_ROOT/Microservicio_Multas/Microservicio_Multas' && echo Multas:5293 && dotnet run --no-build\""

osascript -e "tell application \"Terminal\" to do script \"cd '$REPO_ROOT/Microservicio_Autor/Microservicio_Autor' && echo Autor:5045 && dotnet run --no-build\""

osascript -e "tell application \"Terminal\" to do script \"cd '$REPO_ROOT/Microservicio_Frontend/Microservicio_Frontend' && echo Frontend:5125 && dotnet run --no-build\""

echo ""
echo "Done. Close Terminal windows to stop."
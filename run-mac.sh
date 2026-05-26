#!/bin/bash

REPO_ROOT="$(cd "$(dirname "$0")" && pwd)"

echo ""
echo "========================================"
echo "  GestionBiblioteca — Run (Mac)"
echo "========================================"
echo ""

run_service () {
  NAME="$1"
  PATH_SERVICE="$2"

  osascript -e "tell application \"Terminal\" to do script \"cd '$REPO_ROOT/$PATH_SERVICE' && clear && echo '$NAME' && dotnet run; echo ''; echo 'Proceso terminado. Presiona ENTER para cerrar...'; read\""
}

run_service "Usuario:5292" "Microservicio_Usuario/Microservicio_Usuario"
run_service "LibroEjemplar:5101" "Microservicio_LibroEjemplar/Microservicio_LibroEjemplar"
run_service "Multas:5293" "Microservicio_Multas/Microservicio_Multas"
run_service "Autor:5045" "Microservicio_Autor/Microservicio_Autor"
run_service "Frontend:5125" "Microservicio_Frontend/Microservicio_Frontend"

echo ""
echo "Done. Close Terminal windows to stop."
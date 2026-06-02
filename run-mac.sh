#!/bin/bash

# run-mac.sh — Build limpio e inicio de todos los microservicios en Mac

REPO_ROOT="$(cd "$(dirname "$0")" && pwd)"

echo ""
echo "========================================"
echo "  GestionBiblioteca — Build + Run (Mac)"
echo "========================================"
echo ""

proyectos=(
  "Microservicio_Usuario/Microservicio_Usuario"
  "Microservicio_LibroEjemplar/Microservicio_LibroEjemplar"
  "Microservicio_Multas/Microservicio_Multas"
  "Microservicio_Autor/Microservicio_Autor"
  "Frontend/Frontend"
  "Microservicio_Prestamo/Microservicio_Prestamo"
  "Microservicio_Reportes/Microservicio_Reportes"
  "Microservicio_SagaOrquestador/Microservicio_SagaOrquestador"
)

echo "Limpiando y compilando proyectos..."
echo ""

for proj in "${proyectos[@]}"; do
  FULL="$REPO_ROOT/$proj"
  NAME="$(basename "$proj")"

  if [ ! -d "$FULL" ]; then
    echo "  [SKIP] $NAME — carpeta no encontrada"
    continue
  fi

  echo "  [BUILD] $NAME"

  if [ -d "$FULL/obj" ]; then
    echo "    Cleaning: obj"
    rm -rf "$FULL/obj"
  fi

  if [ -d "$FULL/bin" ]; then
    echo "    Cleaning: bin"
    rm -rf "$FULL/bin"
  fi

  dotnet build "$FULL"

  if [ $? -ne 0 ]; then
    echo ""
    echo "ERROR: Falló el build de $NAME"
    exit 1
  fi

  echo ""
done

echo ""
echo "Build OK. Starting services..."
echo ""

# RabbitMQ con Docker
echo "Verificando RabbitMQ..."

if docker ps --format '{{.Names}}' | grep -q "^rabbitmq-gestion$"; then
  echo "  [MQ] RabbitMQ ya está corriendo"
elif docker ps -a --format '{{.Names}}' | grep -q "^rabbitmq-gestion$"; then
  echo "  [MQ] Iniciando contenedor rabbitmq-gestion"
  docker start rabbitmq-gestion
else
  echo "  [MQ] Creando contenedor rabbitmq-gestion"
  docker run -d --name rabbitmq-gestion \
    -p 5672:5672 \
    -p 15672:15672 \
    rabbitmq:3.13.7-management
fi

echo ""
echo "Abriendo microservicios en Terminal..."
echo ""

run_service () {
  NAME="$1"
  PATH_SERVICE="$2"

  FULL_PATH="$REPO_ROOT/$PATH_SERVICE"

  if [ ! -d "$FULL_PATH" ]; then
    echo "  [SKIP] $NAME — carpeta no encontrada"
    return
  fi

  osascript -e "tell application \"Terminal\" to do script \"cd '$FULL_PATH' && clear && echo '$NAME' && dotnet run --no-build; echo ''; echo 'Proceso terminado. Presiona ENTER para cerrar...'; read\""
}

run_service "Usuario:5292" "Microservicio_Usuario/Microservicio_Usuario"
run_service "LibroEjemplar:5101" "Microservicio_LibroEjemplar/Microservicio_LibroEjemplar"
run_service "Multas:5293" "Microservicio_Multas/Microservicio_Multas"
run_service "Autor:5045" "Microservicio_Autor/Microservicio_Autor"
run_service "Frontend:5125" "Frontend/Frontend"
run_service "Prestamo:5103" "Microservicio_Prestamo/Microservicio_Prestamo"
run_service "SagaOrquestador" "Microservicio_SagaOrquestador/Microservicio_SagaOrquestador"
run_service "Reportes:5126" "Microservicio_Reportes/Microservicio_Reportes"

echo ""
echo "Done. Close Terminal windows to stop."
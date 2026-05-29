# run-all.ps1 — Build limpio e inicio de todos los microservicios

$repoRoot = $PSScriptRoot

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  GestionBiblioteca — Build + Run" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$proyectos = @(
    "Microservicio_Usuario\Microservicio_Usuario",
    "Microservicio_LibroEjemplar\Microservicio_LibroEjemplar",
    "Microservicio_Multas\Microservicio_Multas",
    "Microservicio_Autor\Microservicio_Autor",
    "Frontend\Frontend"
)

foreach ($proj in $proyectos) {
    $full = Join-Path $repoRoot $proj
    $name = Split-Path -Leaf $proj

    if (-not (Test-Path $full)) {
        Write-Host "  [SKIP] $name" -ForegroundColor Red
        continue
    }

    Write-Host "  [BUILD] $name" -ForegroundColor DarkGray
    # Ensure stale build artifacts don't block MSBuild (common cause of MSB3492)
    $objDir = Join-Path $full 'obj'
    $binDir = Join-Path $full 'bin'
    if (Test-Path $objDir) {
        Write-Host "    Cleaning: obj" -ForegroundColor DarkGray
        Remove-Item -Recurse -Force -ErrorAction SilentlyContinue $objDir
    }
    if (Test-Path $binDir) {
        Write-Host "    Cleaning: bin" -ForegroundColor DarkGray
        Remove-Item -Recurse -Force -ErrorAction SilentlyContinue $binDir
    }

    dotnet build $full
    if ($LASTEXITCODE -ne 0) { exit 1 }
}

Write-Host ""
Write-Host "Build OK. Starting services..." -ForegroundColor Green
Write-Host ""

Start-Process powershell -ArgumentList '-NoExit','-Command','Write-Host Usuario:5292 -ForegroundColor Green; dotnet run --no-build' -WorkingDirectory "$repoRoot\Microservicio_Usuario\Microservicio_Usuario"
Start-Process powershell -ArgumentList '-NoExit','-Command','Write-Host LibroEjemplar:5101 -ForegroundColor Yellow; dotnet run --no-build' -WorkingDirectory "$repoRoot\Microservicio_LibroEjemplar\Microservicio_LibroEjemplar"
Start-Process powershell -ArgumentList '-NoExit','-Command','Write-Host Multas:5293 -ForegroundColor Magenta; dotnet run --no-build' -WorkingDirectory "$repoRoot\Microservicio_Multas\Microservicio_Multas"
Start-Process powershell -ArgumentList '-NoExit','-Command','Write-Host Autor:5045 -ForegroundColor Blue; dotnet run --no-build' -WorkingDirectory "$repoRoot\Microservicio_Autor\Microservicio_Autor"
Start-Process powershell -ArgumentList '-NoExit','-Command','Write-Host Frontend:5125 -ForegroundColor Cyan; dotnet run --no-build' -WorkingDirectory "$repoRoot\Frontend\Frontend"

Write-Host "Done. Close each window to stop." -ForegroundColor Gray
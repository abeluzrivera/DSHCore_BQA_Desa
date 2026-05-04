# Script para generar scripts SQL de migración para Producción
# Uso: .\Generate-MigrationScript.ps1 -MigrationName "NombreMigracion" [-FromMigration "MigracionInicial"]

param(
    [Parameter(Mandatory=$false)]
    [string]$MigrationName,
    
    [Parameter(Mandatory=$false)]
    [string]$FromMigration,
    
    [Parameter(Mandatory=$false)]
    [switch]$ListMigrations,
    
    [Parameter(Mandatory=$false)]
    [switch]$FullScript
)

# Colores para output
function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    Write-Host $Message -ForegroundColor $Color
}

# Banner
Write-ColorOutput "`n??????????????????????????????????????????????????????????????????" "Cyan"
Write-ColorOutput "?   Generador de Scripts SQL de Migración - Producción         ?" "Cyan"
Write-ColorOutput "?   Contactabilidad Inteligente - Banco Machala                ?" "Cyan"
Write-ColorOutput "??????????????????????????????????????????????????????????????????`n" "Cyan"

# Verificar que estamos en la carpeta correcta
$projectPath = "contactabilidad_inteligente.mcv"
if (-not (Test-Path $projectPath)) {
    Write-ColorOutput "? Error: No se encuentra la carpeta del proyecto '$projectPath'" "Red"
    Write-ColorOutput "   Ejecute este script desde la raíz de la solución.`n" "Yellow"
    exit 1
}

# Cambiar a la carpeta del proyecto
Set-Location $projectPath

# Crear carpeta de scripts si no existe
$scriptsFolder = "Data\Migrations\Scripts"
if (-not (Test-Path $scriptsFolder)) {
    New-Item -ItemType Directory -Path $scriptsFolder -Force | Out-Null
    Write-ColorOutput "? Carpeta de scripts creada: $scriptsFolder`n" "Green"
}

# Listar migraciones
if ($ListMigrations) {
    Write-ColorOutput "?? Migraciones disponibles:" "Yellow"
    Write-ColorOutput "?????????????????????????????????????????????????????????????`n" "Gray"
    dotnet ef migrations list --context ApplicationDbContext
    Write-ColorOutput "`n" "White"
    Set-Location ..
    exit 0
}

# Obtener timestamp
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"

# Generar script completo o incremental
if ($FullScript) {
    Write-ColorOutput "?? Generando script COMPLETO de todas las migraciones..." "Yellow"
    $outputFile = "$scriptsFolder\Prod_FullMigration_$timestamp.sql"
    
    dotnet ef migrations script --context ApplicationDbContext --output $outputFile --idempotent
    
} elseif ($MigrationName) {
    Write-ColorOutput "?? Generando script para la migración: $MigrationName" "Yellow"
    
    if ($FromMigration) {
        Write-ColorOutput "   Desde: $FromMigration" "Gray"
        $outputFile = "$scriptsFolder\Prod_${MigrationName}_$timestamp.sql"
        dotnet ef migrations script $FromMigration $MigrationName --context ApplicationDbContext --output $outputFile --idempotent
    } else {
        $outputFile = "$scriptsFolder\Prod_${MigrationName}_$timestamp.sql"
        dotnet ef migrations script $MigrationName --context ApplicationDbContext --output $outputFile --idempotent
    }
} else {
    # Generar script desde la última migración aplicada
    Write-ColorOutput "?? Generando script de TODAS las migraciones pendientes..." "Yellow"
    $outputFile = "$scriptsFolder\Prod_AllPending_$timestamp.sql"
    
    dotnet ef migrations script --context ApplicationDbContext --output $outputFile --idempotent
}

# Verificar si se generó el archivo
if (Test-Path $outputFile) {
    $fileSize = (Get-Item $outputFile).Length
    Write-ColorOutput "`n? Script generado exitosamente!" "Green"
    Write-ColorOutput "?????????????????????????????????????????????????????????????" "Gray"
    Write-ColorOutput "   ?? Archivo: $outputFile" "White"
    Write-ColorOutput "   ?? Tamaño: $fileSize bytes" "White"
    Write-ColorOutput "?????????????????????????????????????????????????????????????`n" "Gray"
    
    Write-ColorOutput "?? Próximos pasos:" "Cyan"
    Write-ColorOutput "   1. Revisar el archivo SQL generado" "White"
    Write-ColorOutput "   2. Validar los cambios en el script" "White"
    Write-ColorOutput "   3. Crear ticket de cambio en producción" "White"
    Write-ColorOutput "   4. Adjuntar el script al ticket" "White"
    Write-ColorOutput "   5. Coordinar con el equipo de base de datos`n" "White"
    
    # Abrir el archivo en el editor predeterminado
    $openFile = Read-Host "¿Desea abrir el archivo ahora? (S/N)"
    if ($openFile -eq "S" -or $openFile -eq "s") {
        Start-Process $outputFile
    }
} else {
    Write-ColorOutput "`n? Error: No se pudo generar el script" "Red"
    Write-ColorOutput "   Revise los errores anteriores.`n" "Yellow"
}

# Volver a la carpeta raíz
Set-Location ..

Write-ColorOutput "`n? Proceso finalizado.`n" "Green"

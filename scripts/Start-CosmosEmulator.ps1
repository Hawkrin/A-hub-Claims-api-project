param(
    [switch]$NoLaunch,
    [int]$WaitSeconds = 60
)

$ErrorActionPreference = 'Stop'

function Test-EmulatorUp {
    try {
        $resp = Invoke-WebRequest -Uri 'https://localhost:8081/_explorer/emulator.pem' -UseBasicParsing -TimeoutSec 3
        return $resp.StatusCode -eq 200
    } catch {
        return $false
    }
}

if (Test-EmulatorUp) {
    Write-Host 'Cosmos DB Emulator already running.' -ForegroundColor Green
    exit 0
}

if ($NoLaunch) {
    Write-Host 'Cosmos DB Emulator is not running and -NoLaunch was specified.' -ForegroundColor Yellow
    exit 1
}

$emulatorExe = Join-Path $env:ProgramFiles 'Azure Cosmos DB Emulator\CosmosDB.Emulator.exe'

if (-not (Test-Path $emulatorExe)) {
    Write-Host "Cosmos DB Emulator not found at: $emulatorExe" -ForegroundColor Red
    Write-Host 'Install the Azure Cosmos DB Emulator or use InMemory repositories.' -ForegroundColor Red
    exit 2
}

Write-Host 'Starting Cosmos DB Emulator...' -ForegroundColor Cyan
Start-Process -FilePath $emulatorExe -ArgumentList '/NoUI' | Out-Null

$sw = [Diagnostics.Stopwatch]::StartNew()
while ($sw.Elapsed.TotalSeconds -lt $WaitSeconds) {
    if (Test-EmulatorUp) {
        Write-Host 'Cosmos DB Emulator is up.' -ForegroundColor Green
        exit 0
    }
    Start-Sleep -Seconds 2
}

Write-Host "Cosmos DB Emulator did not become ready within $WaitSeconds seconds." -ForegroundColor Red
exit 3

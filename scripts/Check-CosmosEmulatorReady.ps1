# Quick check if Docker is ready for Cosmos Emulator
param(
    [switch]$Fix
)

$ErrorActionPreference = 'Continue'

Write-Host "`n=== Cosmos Emulator + Aspire Readiness Check ===`n" -ForegroundColor Cyan

# 1. Check Docker Desktop
Write-Host "1. Checking Docker Desktop..." -ForegroundColor Yellow
try {
    $dockerVersion = docker --version 2>$null
    if ($dockerVersion) {
        Write-Host "   ? Docker installed: $dockerVersion" -ForegroundColor Green
    } else {
        Write-Host "   ? Docker not found" -ForegroundColor Red
        Write-Host "   ? Install from: https://www.docker.com/products/docker-desktop" -ForegroundColor Yellow
        exit 1
    }
} catch {
    Write-Host "   ? Docker not installed" -ForegroundColor Red
    exit 1
}

# 2. Check Docker is running
Write-Host "`n2. Checking Docker daemon..." -ForegroundColor Yellow
try {
    docker ps >$null 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "   ? Docker daemon is running" -ForegroundColor Green
    } else {
        Write-Host "   ? Docker daemon not running" -ForegroundColor Red
        Write-Host "   ? Start Docker Desktop and try again" -ForegroundColor Yellow
        if ($Fix) {
            Write-Host "   ? Attempting to start Docker Desktop..." -ForegroundColor Cyan
            Start-Process "C:\Program Files\Docker\Docker\Docker Desktop.exe"
            Write-Host "   ? Waiting 30 seconds for Docker to start..." -ForegroundColor Cyan
            Start-Sleep -Seconds 30
        }
        exit 1
    }
} catch {
    Write-Host "   ? Docker daemon not running" -ForegroundColor Red
    exit 1
}

# 3. Check if Cosmos Emulator container already running
Write-Host "`n3. Checking for existing Cosmos Emulator..." -ForegroundColor Yellow
$cosmosContainer = docker ps --filter "ancestor=mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator" --format "{{.ID}}"
if ($cosmosContainer) {
    Write-Host "   ??  Cosmos Emulator already running (Container: $cosmosContainer)" -ForegroundColor Yellow
    Write-Host "   ? Aspire will reuse this container or create a new one" -ForegroundColor Cyan
} else {
    Write-Host "   ??  No existing Cosmos Emulator found" -ForegroundColor Cyan
    Write-Host "   ? Aspire will create one automatically" -ForegroundColor Green
}

# 4. Check port 8081 availability
Write-Host "`n4. Checking port 8081 (Cosmos Emulator)..." -ForegroundColor Yellow
try {
    $connection = Test-NetConnection -ComputerName localhost -Port 8081 -WarningAction SilentlyContinue -InformationLevel Quiet
    if ($connection) {
        Write-Host "   ??  Port 8081 is already in use" -ForegroundColor Yellow
        
        # Check if it's the standalone emulator
        $standaloneProcess = Get-Process -Name "Microsoft.Azure.Cosmos.Emulator" -ErrorAction SilentlyContinue
        if ($standaloneProcess) {
            Write-Host "   ? Standalone Cosmos Emulator is running" -ForegroundColor Yellow
            if ($Fix) {
                Write-Host "   ? Stopping standalone emulator..." -ForegroundColor Cyan
                Stop-Process -Name "Microsoft.Azure.Cosmos.Emulator" -Force
                Start-Sleep -Seconds 5
                Write-Host "   ? Standalone emulator stopped" -ForegroundColor Green
            } else {
                Write-Host "   ? Run with -Fix to stop it" -ForegroundColor Yellow
            }
        } else {
            Write-Host "   ? Another process is using port 8081" -ForegroundColor Yellow
        }
    } else {
        Write-Host "   ? Port 8081 is available" -ForegroundColor Green
    }
} catch {
    Write-Host "   ? Port 8081 is available" -ForegroundColor Green
}

# 5. Check API port 5020
Write-Host "`n5. Checking port 5020 (API)..." -ForegroundColor Yellow
try {
    $connection = Test-NetConnection -ComputerName localhost -Port 5020 -WarningAction SilentlyContinue -InformationLevel Quiet
    if ($connection) {
        Write-Host "   ??  Port 5020 is already in use" -ForegroundColor Yellow
        Write-Host "   ? Stop any running API instances" -ForegroundColor Yellow
    } else {
        Write-Host "   ? Port 5020 is available" -ForegroundColor Green
    }
} catch {
    Write-Host "   ? Port 5020 is available" -ForegroundColor Green
}

# 6. Check configuration
Write-Host "`n6. Checking configuration..." -ForegroundColor Yellow
$appsettings = Get-Content "ASP.Claims.API\appsettings.Development.json" -Raw | ConvertFrom-Json
if ($appsettings.CosmosDb.Account -eq "https://localhost:8081") {
    Write-Host "   ? Cosmos Account URL configured correctly" -ForegroundColor Green
} else {
    Write-Host "   ? Cosmos Account URL not configured" -ForegroundColor Red
    Write-Host "   ? Should be: https://localhost:8081" -ForegroundColor Yellow
    Write-Host "   ? Current: $($appsettings.CosmosDb.Account)" -ForegroundColor Yellow
}

# Summary
Write-Host "`n=== Summary ===" -ForegroundColor Cyan
Write-Host "Docker Desktop:     " -NoNewline
if ($dockerVersion) { Write-Host "? Ready" -ForegroundColor Green } else { Write-Host "? Not Ready" -ForegroundColor Red }
Write-Host "Configuration:      " -NoNewline
if ($appsettings.CosmosDb.Account -eq "https://localhost:8081") { Write-Host "? Ready" -ForegroundColor Green } else { Write-Host "? Not Ready" -ForegroundColor Red }

Write-Host "`n=== Next Steps ===" -ForegroundColor Cyan
if ($dockerVersion -and $appsettings.CosmosDb.Account -eq "https://localhost:8081") {
    Write-Host "? You're ready to run Aspire!" -ForegroundColor Green
    Write-Host "`nRun:" -ForegroundColor Cyan
    Write-Host "   dotnet run --project ASP.Claims.AppHost" -ForegroundColor White
    Write-Host "   OR press F5 in Visual Studio" -ForegroundColor White
} else {
    Write-Host "? Please fix the issues above first" -ForegroundColor Red
}

Write-Host ""

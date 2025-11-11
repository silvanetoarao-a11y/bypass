@echo off
REM Script de build para Visual Studio 2022 (Release)

echo ========================================
echo   Build Bypass BlueStacks - Release
echo ========================================
echo.

REM Verificar se dotnet está instalado
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo [ERRO] .NET SDK nao encontrado!
    echo Instale .NET 6.0 SDK ou superior.
    pause
    exit /b 1
)

echo [1/2] Restaurando pacotes...
dotnet restore

if errorlevel 1 (
    echo [ERRO] Falha ao restaurar pacotes!
    pause
    exit /b 1
)

echo [2/2] Compilando em modo Release...
dotnet build -c Release

if errorlevel 1 (
    echo [ERRO] Falha ao compilar!
    pause
    exit /b 1
)

echo.
echo ========================================
echo   Build concluido com sucesso!
echo ========================================
echo.
echo Executavel criado em: bin\Release\net8.0-windows\BypassBlueStacks.exe
echo.
pause

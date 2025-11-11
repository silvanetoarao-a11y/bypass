@echo off
REM Script para baixar e configurar ADB embutido

echo ========================================
echo   Configurar ADB Embutido
echo ========================================
echo.

REM Criar pasta Resources se não existir
if not exist "Resources" mkdir Resources

echo [1/3] Baixando Android SDK Platform Tools...
echo.

REM Tentar baixar usando PowerShell
powershell -Command "& {[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12; Invoke-WebRequest -Uri 'https://dl.google.com/android/repository/platform-tools-latest-windows.zip' -OutFile 'platform-tools.zip'}"

if not exist "platform-tools.zip" (
    echo [ERRO] Falha ao baixar platform-tools.zip
    echo.
    echo Por favor, baixe manualmente:
    echo https://developer.android.com/studio/releases/platform-tools
    echo.
    echo E extraia os seguintes arquivos para a pasta Resources:
    echo - adb.exe
    echo - AdbWinApi.dll
    echo - AdbWinUsbApi.dll
    pause
    exit /b 1
)

echo [2/3] Extraindo arquivos...
powershell -Command "Expand-Archive -Path 'platform-tools.zip' -DestinationPath 'temp_extract' -Force"

if not exist "temp_extract\platform-tools\adb.exe" (
    echo [ERRO] Arquivo adb.exe nao encontrado no ZIP
    pause
    exit /b 1
)

echo [3/3] Copiando arquivos para Resources...
copy "temp_extract\platform-tools\adb.exe" "Resources\" >nul
copy "temp_extract\platform-tools\AdbWinApi.dll" "Resources\" >nul
copy "temp_extract\platform-tools\AdbWinUsbApi.dll" "Resources\" >nul

REM Limpar arquivos temporários
rmdir /s /q temp_extract
del platform-tools.zip

if exist "Resources\adb.exe" (
    echo.
    echo ========================================
    echo   ADB configurado com sucesso!
    echo ========================================
    echo.
    echo Arquivos copiados para Resources:
    echo - adb.exe
    echo - AdbWinApi.dll
    echo - AdbWinUsbApi.dll
    echo.
    echo Agora voce pode compilar o projeto normalmente.
    echo.
) else (
    echo [ERRO] Falha ao copiar arquivos
    pause
    exit /b 1
)

pause

@echo off
REM Script de build para gerar executável

echo ========================================
echo   Build Bypass BlueStacks - Executavel
echo ========================================
echo.

REM Verificar Python
python --version >nul 2>&1
if errorlevel 1 (
    echo [ERRO] Python nao encontrado!
    echo Instale Python 3.8 ou superior.
    pause
    exit /b 1
)

echo [1/3] Instalando dependencias...
pip install -r requirements.txt

if errorlevel 1 (
    echo [ERRO] Falha ao instalar dependencias!
    pause
    exit /b 1
)

echo [2/3] Criando executavel...
pyinstaller --onefile --windowed --name "BypassBlueStacks" --icon=NONE --clean bypass_gui.py

REM Alternativa usando spec file:
REM pyinstaller bypass_gui.spec

if errorlevel 1 (
    echo [ERRO] Falha ao criar executavel!
    pause
    exit /b 1
)

echo [3/3] Limpando arquivos temporarios...
if exist build rmdir /s /q build
if exist __pycache__ rmdir /s /q __pycache__

echo.
echo ========================================
echo   Build concluido com sucesso!
echo ========================================
echo.
echo Executavel criado em: dist\BypassBlueStacks.exe
echo.
pause

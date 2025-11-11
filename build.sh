#!/bin/bash
# Script de build para Linux (caso necessário)

echo "========================================"
echo "  Build Bypass BlueStacks - Executável"
echo "========================================"
echo ""

# Verificar Python
if ! command -v python3 &> /dev/null; then
    echo "[ERRO] Python 3 não encontrado!"
    exit 1
fi

echo "[1/3] Instalando dependências..."
pip3 install -r requirements.txt

if [ $? -ne 0 ]; then
    echo "[ERRO] Falha ao instalar dependências!"
    exit 1
fi

echo "[2/3] Criando executável..."
pyinstaller --onefile --windowed --name "BypassBlueStacks" bypass_gui.py

if [ $? -ne 0 ]; then
    echo "[ERRO] Falha ao criar executável!"
    exit 1
fi

echo "[3/3] Limpando arquivos temporários..."
rm -rf build __pycache__

echo ""
echo "========================================"
echo "  Build concluído com sucesso!"
echo "========================================"
echo ""
echo "Executável criado em: dist/BypassBlueStacks"
echo ""

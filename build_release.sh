#!/bin/bash
# Script de build para Linux/Mac (caso necessário)

echo "========================================"
echo "  Build Bypass BlueStacks - Release"
echo "========================================"
echo ""

# Verificar dotnet
if ! command -v dotnet &> /dev/null; then
    echo "[ERRO] .NET SDK não encontrado!"
    exit 1
fi

echo "[1/2] Restaurando pacotes..."
dotnet restore

if [ $? -ne 0 ]; then
    echo "[ERRO] Falha ao restaurar pacotes!"
    exit 1
fi

echo "[2/2] Compilando em modo Release..."
dotnet build -c Release

if [ $? -ne 0 ]; then
    echo "[ERRO] Falha ao compilar!"
    exit 1
fi

echo ""
echo "========================================"
echo "  Build concluído com sucesso!"
echo "========================================"
echo ""
echo "Executável criado em: bin/Release/net6.0-windows/BypassBlueStacks.exe"
echo ""

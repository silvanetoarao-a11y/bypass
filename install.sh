#!/bin/bash
# Script de instalação e configuração do bypass BlueStacks

set -e

echo "🔧 Instalador do Bypass BlueStacks"
echo "=================================="

# Verificar se está rodando no BlueStacks/Android
if [ ! -f "/system/build.prop" ]; then
    echo "❌ Este script deve ser executado dentro do BlueStacks/Android"
    echo "   Use ADB para executar: adb shell < script.sh"
    exit 1
fi

# Verificar root
if [ "$(id -u)" != "0" ]; then
    echo "⚠️  Executando com su..."
    if ! su -c "id" > /dev/null 2>&1; then
        echo "❌ Acesso root necessário!"
        exit 1
    fi
    SUDO="su -c"
else
    SUDO=""
fi

# Criar diretório de trabalho
WORK_DIR="/data/local/tmp/bypass_bluestacks"
mkdir -p "$WORK_DIR"

echo "📦 Copiando arquivos..."

# Montar sistema como read-write
$SUDO mount -o remount,rw /system

# Fazer backup do build.prop
if [ ! -f "/system/build.prop.backup" ]; then
    echo "💾 Criando backup..."
    $SUDO cp /system/build.prop /system/build.prop.backup
fi

echo "✅ Instalação concluída!"
echo ""
echo "Próximos passos:"
echo "1. Execute: python3 bypass_main.py [perfil]"
echo "2. Reinicie o BlueStacks"
echo ""
echo "Para restaurar: python3 bypass_main.py restore"

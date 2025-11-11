#!/bin/bash
# Script ADB para aplicar bypass remotamente no BlueStacks

BLUESTACKS_ADB_PORT=5555
DEVICE_PROFILE="${1:-samsung_galaxy_s21}"

echo "🔌 Conectando ao BlueStacks..."
adb connect 127.0.0.1:$BLUESTACKS_ADB_PORT

echo "📱 Verificando conexão..."
if ! adb devices | grep -q "127.0.0.1:$BLUESTACKS_ADB_PORT"; then
    echo "❌ Não foi possível conectar ao BlueStacks!"
    echo "   Certifique-se de que o BlueStacks está rodando e"
    echo "   que a depuração USB está habilitada nas configurações."
    exit 1
fi

echo "📦 Enviando arquivos..."
adb push bypass_main.py /data/local/tmp/
adb push install.sh /data/local/tmp/
adb push hooking_module.py /data/local/tmp/

echo "🔧 Executando instalação..."
adb shell "su -c 'chmod +x /data/local/tmp/install.sh'"
adb shell "su -c '/data/local/tmp/install.sh'"

echo "🚀 Aplicando bypass com perfil: $DEVICE_PROFILE"
adb shell "su -c 'cd /data/local/tmp && python3 bypass_main.py $DEVICE_PROFILE'"

echo ""
echo "✅ Bypass aplicado!"
echo "⚠️  REINICIE o BlueStacks para aplicar as mudanças!"
echo ""
echo "Para verificar: adb shell getprop ro.product.model"
echo "Para restaurar: adb shell 'su -c \"cd /data/local/tmp && python3 bypass_main.py restore\"'"

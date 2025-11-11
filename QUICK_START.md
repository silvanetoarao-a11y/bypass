# Guia Rápido - Bypass BlueStacks

## ⚡ Uso Rápido (3 passos)

### 1. Preparar BlueStacks
- Abra o BlueStacks
- Vá em Configurações > Avançado > Habilitar Root
- Vá em Configurações > Avançado > Habilitar Depuração USB

### 2. Conectar via ADB
```bash
adb connect 127.0.0.1:5555
adb devices  # Verificar conexão
```

### 3. Aplicar Bypass
```bash
./apply_bypass.sh samsung_galaxy_s21
```

### 4. Reiniciar BlueStacks
**IMPORTANTE**: Reinicie completamente o BlueStacks após aplicar!

## ✅ Verificar se Funcionou

```bash
adb shell getprop ro.product.model
# Deve mostrar: SM-G991B (ou outro modelo do perfil escolhido)

adb shell getprop ro.kernel.qemu
# Deve estar vazio ou retornar 0
```

## 🔄 Desfazer Mudanças

```bash
adb shell "su -c 'cd /data/local/tmp && python3 bypass_main.py restore'"
# Depois reinicie o BlueStacks
```

## 📱 Escolher Outro Dispositivo

```bash
# Listar perfis disponíveis
adb shell "su -c 'cd /data/local/tmp && python3 bypass_main.py list'"

# Aplicar perfil específico
./apply_bypass.sh xiaomi_redmi_note_11
./apply_bypass.sh oneplus_9
```

## ⚠️ Problemas Comuns

**ADB não conecta?**
```bash
adb kill-server
adb start-server
adb connect 127.0.0.1:5555
```

**Sem permissão root?**
- Verifique se root está habilitado no BlueStacks
- Teste: `adb shell su -c "id"`

**Mudanças não aplicam?**
- Certifique-se de **reiniciar o BlueStacks** após aplicar
- Verifique se o backup foi criado: `/system/build.prop.backup`

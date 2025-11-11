# Bypass BlueStacks - Simular Dispositivo Mobile Real

Solução completa para fazer o BlueStacks parecer um dispositivo móvel real, evitando detecção de emulador e possível banimento em jogos.

## 🎨 Nova Versão com Interface Gráfica!

**Agora disponível**: Aplicação Windows com interface gráfica moderna que funciona **SEM ROOT**!

👉 **[Ver GUI_README.md](GUI_README.md)** para instruções da versão GUI

---

## 📦 Versões Disponíveis

1. **GUI Executável** (Recomendado) - Interface gráfica, sem root, fácil de usar
2. **Scripts CLI** - Versão avançada com root para modificação permanente

## 🎯 Objetivo

Modificar propriedades do sistema Android no BlueStacks para que aplicativos e jogos detectem o dispositivo como um smartphone real ao invés de um emulador.

## ⚠️ Avisos Importantes

### Versão GUI (Recomendada)
- ✅ **NÃO requer ROOT** - Usa ADB com depuração USB
- ✅ Interface gráfica fácil de usar
- ⚠️ Propriedades temporárias (precisa manter app aberto)

### Versão CLI (Avançada)
- ⚠️ **Requer acesso ROOT** no BlueStacks
- ⚠️ Modificações permanentes no sistema
- ⚠️ Faça **backup** antes de aplicar modificações
- ⚠️ Use por sua **própria conta e risco**

**Nota**: Alguns jogos podem ter detecção avançada que não será contornada.

## 📋 Requisitos

### Para Versão GUI (Sem Root)
1. Windows 10/11
2. BlueStacks instalado e configurado
3. ADB (Android Debug Bridge) instalado no PC
4. Depuração USB habilitada no BlueStacks

### Para Versão CLI (Com Root)
1. BlueStacks instalado e configurado
2. Acesso root habilitado no BlueStacks
3. ADB (Android Debug Bridge) instalado no PC
4. Python 3 instalado no BlueStacks (ou via ADB)

## 🚀 Instalação Rápida

### 🎨 Versão GUI (Recomendada - Sem Root)

Veja **[GUI_README.md](GUI_README.md)** para instruções completas.

**Resumo rápido:**
1. Execute `build.bat` para criar o executável
2. Ou execute diretamente: `python bypass_gui.py`
3. A aplicação detectará o BlueStacks automaticamente
4. Selecione o dispositivo e clique em "Ativar Bypass"

### 💻 Versão CLI (Avançada - Com Root)

### Método 1: Via ADB (Recomendado)

```bash
# 1. Conecte ao BlueStacks via ADB
adb connect 127.0.0.1:5555

# 2. Execute o script de aplicação
chmod +x apply_bypass.sh
./apply_bypass.sh [perfil]

# Perfis disponíveis:
# - samsung_galaxy_s21 (padrão)
# - xiaomi_redmi_note_11
# - oneplus_9
```

### Método 2: Manual (dentro do BlueStacks)

```bash
# 1. Copie os arquivos para o BlueStacks
adb push bypass_main.py /data/local/tmp/
adb push install.sh /data/local/tmp/

# 2. Execute dentro do BlueStacks
adb shell
su
cd /data/local/tmp
chmod +x install.sh
./install.sh
python3 bypass_main.py [perfil]
```

## 🔧 Como Funciona

O bypass modifica as seguintes propriedades do sistema:

1. **build.prop**: Propriedades do dispositivo (modelo, fabricante, fingerprint)
2. **Hooks de Sistema**: Intercepta chamadas que detectam emulador
3. **Propriedades de Hardware**: Modifica informações de CPU, memória, etc.

### Propriedades Modificadas

- `ro.product.model`: Modelo do dispositivo
- `ro.product.brand`: Marca do dispositivo  
- `ro.product.manufacturer`: Fabricante
- `ro.product.device`: Código do dispositivo
- `ro.build.fingerprint`: Fingerprint do build Android
- `ro.kernel.qemu`: Remove flag de emulador
- `ro.hardware`: Altera hardware reportado

## 📱 Perfis de Dispositivos Disponíveis

### Samsung Galaxy S21
```bash
python3 bypass_main.py samsung_galaxy_s21
```

### Xiaomi Redmi Note 11
```bash
python3 bypass_main.py xiaomi_redmi_note_11
```

### OnePlus 9
```bash
python3 bypass_main.py oneplus_9
```

## 🔄 Restaurar Configuração Original

```bash
# Via ADB
adb shell "su -c 'cd /data/local/tmp && python3 bypass_main.py restore'"

# Ou manualmente
python3 bypass_main.py restore
```

## 🛠️ Componentes

- **bypass_main.py**: Script principal que modifica o build.prop
- **hooking_module.py**: Módulo de hooking avançado (requer Xposed)
- **install.sh**: Script de instalação automática
- **apply_bypass.sh**: Script ADB para aplicação remota

## 🔍 Verificação

Após aplicar o bypass, verifique as propriedades:

```bash
adb shell getprop ro.product.model
adb shell getprop ro.product.brand
adb shell getprop ro.product.manufacturer
adb shell getprop ro.kernel.qemu  # Deve retornar vazio ou 0
```

## 📝 Notas Técnicas

### Detecção de Emulador

Jogos e apps detectam emuladores através de:

1. **Propriedades do Sistema**: `ro.kernel.qemu`, `ro.hardware`, etc.
2. **Fingerprint do Build**: Padrões específicos de emulador
3. **Hardware Virtual**: CPU, GPU, sensores
4. **Comportamento**: Padrões de uso, sensores, etc.

### Limitações

- Não modifica hardware virtual (CPU/GPU)
- Alguns jogos usam detecção server-side avançada
- Pode não funcionar com todos os métodos de detecção
- Requer reinicialização do BlueStacks após aplicação

## 🐛 Troubleshooting

### Erro: "Acesso root necessário"
- Habilite root no BlueStacks (configurações avançadas)
- Verifique com: `adb shell su -c "id"`

### Erro: "build.prop não encontrado"
- Certifique-se de estar no BlueStacks/Android
- Verifique caminho: `/system/build.prop`

### Mudanças não aplicadas
- **Reinicie o BlueStacks** após aplicar o bypass
- Verifique se o backup foi criado: `/system/build.prop.backup`

### ADB não conecta
- Habilite "Depuração USB" no BlueStacks
- Verifique porta ADB (padrão: 5555)
- Tente: `adb kill-server && adb start-server`

## 📚 Referências

- [Android System Properties](https://source.android.com/devices/tech/config)
- [BlueStacks Documentation](https://support.bluestacks.com/)
- [ADB Guide](https://developer.android.com/studio/command-line/adb)

## ⚖️ Licença

Uso educacional. Use por sua própria conta e risco.

## 🤝 Contribuições

Sinta-se livre para adicionar novos perfis de dispositivos ou melhorar os métodos de bypass!
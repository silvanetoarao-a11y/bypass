# Bypass BlueStacks - GUI Executável

## 🎯 Aplicação com Interface Gráfica

Aplicação Windows com interface gráfica moderna que detecta automaticamente o BlueStacks e aplica bypass sem necessidade de root.

## ✨ Características

- ✅ **Sem necessidade de ROOT** - Usa ADB com depuração USB
- 🎨 **Interface gráfica moderna** - Painel intuitivo e fácil de usar
- 🔍 **Detecção automática** - Detecta quando BlueStacks está aberto
- 📱 **Múltiplos dispositivos** - 4 perfis de dispositivos pré-configurados
- 🔄 **Daemon automático** - Mantém bypass ativo continuamente
- 📊 **Log em tempo real** - Acompanhe o processo em tempo real

## 📋 Requisitos

1. **Windows 10/11**
2. **Python 3.8+** (para desenvolvimento) ou executável pré-compilado
3. **ADB (Android Debug Bridge)** instalado
4. **BlueStacks** com depuração USB habilitada

## 🚀 Como Usar

### Opção 1: Executável (.exe)

1. Baixe `BypassBlueStacks.exe`
2. Execute o arquivo
3. Aguarde detectar o BlueStacks
4. Clique em "Conectar ADB" (se necessário)
5. Selecione o dispositivo desejado
6. Clique em "Ativar Bypass"

### Opção 2: Código Fonte

```bash
# 1. Instalar dependências
pip install -r requirements.txt

# 2. Executar aplicação
python bypass_gui.py
```

### Opção 3: Build do Executável

```bash
# Windows
build.bat

# O executável será criado em: dist\BypassBlueStacks.exe
```

## 🔧 Configuração Inicial

### 1. Habilitar Depuração USB no BlueStacks

1. Abra BlueStacks
2. Vá em **Configurações** > **Avançado**
3. Ative **Depuração USB**
4. Anote a porta ADB (geralmente 5555)

### 2. Instalar ADB

Baixe o Android SDK Platform Tools:
- https://developer.android.com/studio/releases/platform-tools

Adicione ao PATH do Windows ou coloque `adb.exe` na mesma pasta do executável.

### 3. Conectar BlueStacks

O aplicativo tentará conectar automaticamente. Se não conectar:

```bash
adb connect 127.0.0.1:5555
```

## 📱 Perfis de Dispositivos

1. **Samsung Galaxy S21** (padrão)
2. **Xiaomi Redmi Note 11**
3. **OnePlus 9**
4. **Google Pixel 6**

## 🎮 Como Funciona

1. **Detecção**: Monitora processos do Windows para detectar BlueStacks
2. **Conexão ADB**: Conecta ao BlueStacks via ADB
3. **Aplicação**: Usa `setprop` para modificar propriedades do sistema
4. **Daemon**: Mantém propriedades ativas reaplicando a cada 5 segundos

### Propriedades Modificadas

- `ro.kernel.qemu` → Remove flag de emulador
- `ro.hardware` → Altera hardware reportado
- `ro.product.model` → Modelo do dispositivo
- `ro.product.brand` → Marca do dispositivo
- `ro.product.manufacturer` → Fabricante
- `ro.product.device` → Código do dispositivo
- `ro.build.fingerprint` → Fingerprint do Android

## ⚠️ Limitações

- **Propriedades temporárias**: `setprop` não persiste após reiniciar BlueStacks
- **Daemon necessário**: O aplicativo precisa ficar aberto para manter bypass ativo
- **Sem root**: Algumas propriedades podem não ser modificáveis sem root
- **Detecção avançada**: Alguns jogos podem usar métodos de detecção que não são contornados

## 🐛 Troubleshooting

### BlueStacks não detectado
- Certifique-se de que o BlueStacks está realmente aberto
- Verifique processos: `HD-Player.exe`, `BlueStacks.exe`

### ADB não conecta
- Verifique se depuração USB está habilitada
- Tente: `adb kill-server && adb start-server`
- Verifique porta: `adb connect 127.0.0.1:5555`

### Bypass não funciona
- Mantenha o aplicativo aberto (daemon precisa estar rodando)
- Verifique se ADB está conectado
- Tente reiniciar o BlueStacks após ativar bypass

### Erro "ADB não encontrado"
- Instale Android SDK Platform Tools
- Adicione ao PATH ou coloque `adb.exe` na mesma pasta

## 📝 Notas Técnicas

- Usa `setprop` via ADB (não requer root)
- Propriedades são temporárias e precisam ser reaplicadas
- Daemon roda em thread separada para não travar UI
- Monitoramento de processos usa `psutil`

## 🔒 Segurança

- Não modifica arquivos do sistema permanentemente
- Não requer root ou permissões especiais
- Usa apenas ADB padrão do Android
- Todas as modificações são temporárias

## 📚 Arquivos

- `bypass_gui.py` - Interface gráfica principal
- `bypass_daemon.py` - Daemon standalone (opcional)
- `build.bat` - Script de build para Windows
- `requirements.txt` - Dependências Python

## 🤝 Suporte

Para problemas ou sugestões, verifique:
1. Logs na interface gráfica
2. Conexão ADB: `adb devices`
3. Propriedades: `adb shell getprop ro.product.model`

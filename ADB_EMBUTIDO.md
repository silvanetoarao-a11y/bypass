# Como Incluir ADB no Executável

## 📥 Baixar Arquivos do ADB

1. Baixe o **Android SDK Platform Tools**:
   - https://developer.android.com/studio/releases/platform-tools
   - Ou use: https://dl.google.com/android/repository/platform-tools-latest-windows.zip

2. Extraia o arquivo ZIP

3. Copie os seguintes arquivos para a pasta `Resources` do projeto:
   - `adb.exe`
   - `AdbWinApi.dll`
   - `AdbWinUsbApi.dll`

## 📁 Estrutura de Pastas

Crie a seguinte estrutura:

```
/workspace/
├── BypassBlueStacks.csproj
├── MainForm.cs
├── Program.cs
└── Resources/
    ├── adb.exe
    ├── AdbWinApi.dll
    └── AdbWinUsbApi.dll
```

## 🔧 Configuração Automática

O arquivo `.csproj` já está configurado para incluir os arquivos como recursos embutidos.

## ✅ Verificação

Após adicionar os arquivos:

1. Compile o projeto: `dotnet build -c Release`
2. Os arquivos serão incluídos no executável
3. Quando executar, o ADB será extraído automaticamente para uma pasta temporária
4. Não será necessário ter ADB instalado no sistema!

## 📝 Notas

- Os arquivos do ADB serão extraídos para `%TEMP%\BypassBlueStacks_ADB\` quando o app iniciar
- Os arquivos serão limpos automaticamente quando o app fechar
- Se os arquivos não estiverem embutidos, o app tentará usar ADB do sistema (se disponível)

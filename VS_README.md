# Bypass BlueStacks - Visual Studio 2022

## 📋 Projeto C# Windows Forms

Projeto Visual Studio 2022 para compilar aplicação de bypass BlueStacks em modo Release.

## 🚀 Como Compilar

### Pré-requisitos

1. **Visual Studio 2022** (Community, Professional ou Enterprise)
2. **.NET 6.0 SDK** ou superior
3. **Workload**: Desktop development with C++ (opcional, para compilação nativa)

### Compilar em Release

#### Método 1: Visual Studio IDE

1. Abra `BypassBlueStacks.sln` no Visual Studio 2022
2. Selecione **Release** no dropdown de configuração (topo da tela)
3. Menu: **Build** > **Build Solution** (ou `Ctrl+Shift+B`)
4. O executável será gerado em: `bin\Release\net6.0-windows\BypassBlueStacks.exe`

#### Método 2: Linha de Comando

```bash
# Navegar até a pasta do projeto
cd /workspace

# Compilar em Release
dotnet build -c Release

# Ou usando MSBuild
msbuild BypassBlueStacks.csproj /p:Configuration=Release /p:Platform="Any CPU"
```

#### Método 3: Publish (Publicação)

```bash
# Criar versão publicada otimizada
dotnet publish -c Release -r win-x64 --self-contained false

# Ou self-contained (inclui .NET runtime)
dotnet publish -c Release -r win-x64 --self-contained true
```

## 📁 Estrutura do Projeto

```
/workspace/
├── BypassBlueStacks.csproj    # Arquivo de projeto
├── MainForm.cs                 # Formulário principal com toda a lógica
├── Program.cs                  # Ponto de entrada da aplicação
├── app.manifest                # Manifesto da aplicação
├── App.config                  # Configuração da aplicação
└── bin/Release/                # Executável compilado (após build)
```

## ⚙️ Configurações de Release

O projeto está configurado para Release com:

- ✅ **Otimizações habilitadas** (`Optimize=true`)
- ✅ **Sem símbolos de debug** (`DebugType=none`)
- ✅ **Sem informações de debug** (`DebugSymbols=false`)
- ✅ **Target Framework**: .NET 6.0 Windows
- ✅ **Windows Forms** habilitado

## 🎯 Características da Aplicação

- **Interface gráfica moderna** com tema escuro
- **Detecção automática** do BlueStacks
- **Conexão ADB** sem necessidade de root
- **4 perfis de dispositivos** pré-configurados
- **Daemon automático** para manter bypass ativo
- **Log em tempo real** das operações

## 🔧 Personalização

### Alterar Configurações de Build

Edite `BypassBlueStacks.csproj`:

```xml
<PropertyGroup Condition="'$(Configuration)|$(Platform)'=='Release|AnyCPU'">
  <DebugType>none</DebugType>
  <DebugSymbols>false</DebugSymbols>
  <Optimize>true</Optimize>
  <OutputPath>bin\Release\</OutputPath>
  <!-- Adicionar outras configurações aqui -->
</PropertyGroup>
```

### Adicionar Ícone

1. Adicione um arquivo `icon.ico` na raiz do projeto
2. O projeto já está configurado para usar: `<ApplicationIcon>icon.ico</ApplicationIcon>`

### Code Signing (Assinatura Digital)

Para assinar o executável:

```xml
<PropertyGroup>
  <SignAssembly>true</SignAssembly>
  <AssemblyOriginatorKeyFile>key.snk</AssemblyOriginatorKeyFile>
</PropertyGroup>
```

## 📦 Dependências

- **.NET 6.0 Windows Forms** - Framework de UI (incluído no SDK)
- Nenhuma dependência externa necessária

## 🐛 Troubleshooting

### Erro: "Target framework not found"
- Instale .NET 6.0 SDK: https://dotnet.microsoft.com/download

### Erro: "Visual Studio workload missing"
- Abra Visual Studio Installer
- Modifique instalação
- Adicione workload: ".NET desktop development"

### Executável não funciona em outros PCs
- Use `--self-contained true` para incluir runtime
- Ou instale .NET 6.0 Runtime no PC destino

## 📝 Notas

- O executável Release é otimizado e menor que Debug
- Não inclui símbolos de debug (mais difícil de fazer reverse engineering)
- Compilação Release é mais rápida na execução

## 🚀 Distribuição

Após compilar em Release:

1. Copie `BypassBlueStacks.exe` de `bin\Release\net6.0-windows\`
2. Distribua junto com ADB ou instrua usuários a instalarem ADB
3. Ou crie instalador usando WiX/Inno Setup

# Como Adicionar Ícone ao Executável

## 📝 Instruções

1. **Crie ou obtenha um arquivo `icon.ico`**
   - Formato: ICO (Windows Icon)
   - Tamanhos recomendados: 16x16, 32x32, 48x48, 256x256 pixels
   - Você pode usar ferramentas online para converter PNG/JPG para ICO

2. **Coloque o arquivo na raiz do projeto**
   ```
   /workspace/
   ├── icon.ico          ← Coloque aqui
   ├── BypassBlueStacks.csproj
   ├── MainForm.cs
   └── ...
   ```

3. **Compile o projeto**
   ```bash
   dotnet build -c Release
   ```

4. **Pronto!** O executável terá o ícone aplicado.

## 🛠️ Ferramentas para Criar Ícone

### Online (Gratuito)
- **ICO Convert**: https://icoconvert.com/
- **ConvertICO**: https://convertico.com/
- **Favicon.io**: https://favicon.io/favicon-converter/

### Software
- **GIMP** (gratuito): https://www.gimp.org/
- **Paint.NET** (gratuito): https://www.getpaint.net/
- **IcoFX** (gratuito): http://icofx.ro/

## 📋 Formato do Ícone

O arquivo `icon.ico` deve conter múltiplos tamanhos:
- 16x16 pixels (usado em listas)
- 32x32 pixels (usado no desktop)
- 48x48 pixels (usado em detalhes)
- 256x256 pixels (usado em visualizações grandes)

## ⚠️ Nota

- O ícone é **opcional** - se não existir, o executável funcionará normalmente sem ícone
- O arquivo deve estar na mesma pasta do `.csproj`
- Após adicionar o ícone, recompile o projeto

## ✅ Verificação

Após compilar, verifique se o ícone aparece:
1. Navegue até `bin\Release\net8.0-windows\`
2. Veja o arquivo `BypassBlueStacks.exe`
3. O ícone deve aparecer no explorador de arquivos

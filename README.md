# Win32 Window Capture & Embedding POC

## Overview
Este projeto é uma **Prova de Conceito (POC)** demonstrando como usar APIs nativas do Windows (Win32/user32.dll) para capturar e embeddar janelas de outras aplicações. O exemplo prático utiliza o Lossless Scaling como caso de uso.

## 🎯 Objetivo do POC
Demonstrar as capacidades das **APIs Win32** para:
- **Captura de Janelas**: Localizar e monitorar janelas de processos externos
- **Window Embedding**: Integrar janelas de outras aplicações como child windows
- **Controle de Processos**: Gerenciar o ciclo de vida de aplicações externas
- **Manipulação de Registry**: Verificar e modificar entradas do registro Windows

## 🔧 Tecnologias Demonstradas

### APIs Win32 Utilizadas
- **user32.dll**:
  - `SetParent()` - Embedding de janelas
  - `FindWindow()` - Localização de janelas por título/classe
  - `SetWindowPos()` - Posicionamento e redimensionamento
  - `EnumWindows()` - Enumeração de janelas do sistema
  - `GetWindowText()` - Obtenção de títulos de janelas

### Implementação C# WPF
- **.NET 8.0** com Windows Presentation Foundation
- **P/Invoke** para chamadas de API nativas
- **Async/Await** para operações não-bloqueantes
- **Event-driven architecture** para atualizações em tempo real

## 💡 Potenciais Aplicações

Esta POC pode ser base para projetos que precisem de:

### 🎮 Gaming & Media
- **Filtros em Tempo Real**: Aplicar shaders/filtros sobre jogos capturados
- **FSR Integration**: Implementar upscaling via FidelityFX Super Resolution
- **Streaming Tools**: Capturar e processar janelas para streaming
- **Game Overlays**: Adicionar interfaces sobre jogos

### 🖥️ Produtividade
- **Window Management**: Criar gerenciadores de janelas avançados
- **Screen Capture**: Sistemas de captura seletiva de aplicações
- **Remote Desktop**: Embedding de sessões remotas
- **Application Containers**: Executar apps em containers visuais

### 🔧 Desenvolvimento
- **Testing Frameworks**: Automatizar testes de UI em aplicações externas
- **Debugging Tools**: Criar ferramentas de debug visual
- **Application Wrappers**: Modernizar aplicações legadas
- **Multi-App Dashboards**: Centralizar múltiplas aplicações

## ⚠️ Considerações Técnicas

### Limitações
- **Windows Only**: APIs Win32 são específicas do Windows
- **Permissions**: Algumas operações requerem privilégios elevados
- **Compatibility**: Nem todas as aplicações permitem embedding
- **Performance**: Embedding pode impactar performance da aplicação host

## 🚀 Funcionalidades Implementadas

### 1. Window Embedding Real
```csharp
// Exemplo de embedding usando SetParent API
[DllImport("user32.dll")]
public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
```

### 2. Process Management
- Detecção automática do executável
- Inicialização controlada de processos
- Monitoramento de estado em tempo real
- Cleanup automático na finalização

### 3. Registry Operations  
- Verificação de chaves existentes
- Leitura e escrita de valores
- Aplicação automática de correções
- Backup de segurança antes de modificações

## 📁 Estrutura do Projeto

```
ScalingManagerWPF/
├── ProcessManager.cs      # Gerenciamento de processos
├── WindowEmbedder.cs      # Lógica de embedding de janelas
├── RegistryManager.cs     # Operações de registry
├── Win32Api.cs           # Declarações P/Invoke
├── MainWindow.xaml       # Interface WPF
├── MainWindow.xaml.cs    # Code-behind da UI
└── Scaling'Exemplo'/     # Executável de exemplo

```

## 🔨 Como Compilar e Executar

### Pré-requisitos
- Windows 10/11
- .NET 8.0 Runtime
- Visual Studio 2022 ou VS Code

### 📁 Setup do Executável de Exemplo
Para testar a aplicação, você precisa de um executável de exemplo. A aplicação procura nos seguintes caminhos (nesta ordem):

1. `ScalingManagerWPF/bin/Debug/net8.0-windows/Scaling/Scaling.exe`

2. `ScalingManagerWPF/Scaling/Scaling.exe`

**📦 Arquivo de Exemplo**: 
- Descompacte `ScalingManagerWPF/example-app.zip` para obter um executável de exemplo
- Crie a pasta `ScalingManagerWPF/Scaling/` e coloque o executável lá


**🔒 Senha do arquivo**: `scaler` (para arquivos protegidos)

### Compilação
```bash
cd ScalingManagerWPF
dotnet build
```

### Execução
```bash
dotnet run
```



### Segurança
- Registry operations requerem cuidado especial
- Validação de processos externos é essencial
- Cleanup adequado previne vazamentos de recursos

## 🎯 Próximos Passos

### Melhorias Potenciais
1. **Shader Pipeline**: Implementar filtros DirectX/OpenGL sobre janelas embedded
2. **FSR Integration**: Adicionar upscaling AMD FidelityFX
3. **Multi-Window**: Suporte para embedding simultâneo de múltiplas janelas
4. **Plugin System**: Arquitetura de plugins para filtros personalizados
5. **Performance Optimization**: Otimizações para jogos e aplicações pesadas

---

**Este POC demonstra que as APIs Win32 oferecem controle total sobre o ambiente Windows, possibilitando integração profunda entre aplicações e recursos avançados de manipulação de janelas.**
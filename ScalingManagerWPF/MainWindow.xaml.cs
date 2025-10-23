using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ScalingManager
{
    public partial class MainWindow : Window
    {
        private readonly ProcessManager _processManager;
        private readonly RegistryManager _registryManager;
        private readonly WindowEmbedder _windowEmbedder;
        private readonly DispatcherTimer _statusTimer;
        
        public MainWindow()
        {
            InitializeComponent();
            
            // Inicializar managers
            _processManager = ProcessManager.Instance;
            _registryManager = RegistryManager.Instance;
            _windowEmbedder = WindowEmbedder.Instance;
            
            // Setup event handlers
            SetupEventHandlers();
            
            // Timer para monitoramento de status
            _statusTimer = new DispatcherTimer();
            _statusTimer.Interval = TimeSpan.FromSeconds(2);
            _statusTimer.Tick += StatusTimer_Tick;
            _statusTimer.Start();
            
            // Inicialização automática
            Loaded += MainWindow_Loaded;
        }
        
        private void SetupEventHandlers()
        {
            // Process Manager events
            _processManager.ProcessStarted += OnProcessStarted;
            _processManager.ProcessStopped += OnProcessStopped;
            _processManager.LogMessage += OnLogMessage;
            
            // Registry Manager events
            _registryManager.LogMessage += OnLogMessage;
            
            // Window Embedder events
            _windowEmbedder.LogMessage += OnLogMessage;
            _windowEmbedder.EmbedStatusChanged += OnEmbedStatusChanged;
        }
        
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            AddLogMessage("🚀 Lossless Scaling Manager 2.0 iniciado");
            AddLogMessage("💡 Aplicação WPF nativa com window embedding real");
            
            // Verificar registry automaticamente
            await CheckRegistryAsync();
        }
        
        private async void CheckRegistryBtn_Click(object sender, RoutedEventArgs e)
        {
            await CheckRegistryAsync();
        }
        
        private async Task CheckRegistryAsync()
        {
            var result = await Task.Run(() => _registryManager.CheckSteamRegistry());
            
            Dispatcher.Invoke(() =>
            {
                RegistryStatusText.Text = $"Status: {(result.IsValid ? "✅ Válido" : "❌ Inválido")}";
                RegistryValueText.Text = $"Valor: {result.ActualValue ?? "N/A"}";
                
                if (!string.IsNullOrEmpty(result.Error))
                {
                    AddLogMessage($"❌ Registry: {result.Error}");
                }
            });
        }
        
        private async void FixRegistryBtn_Click(object sender, RoutedEventArgs e)
        {
            var result = await Task.Run(() => _registryManager.FixSteamRegistry());
            
            if (result)
            {
                await CheckRegistryAsync();
            }
        }
        
        private async void StartProcessBtn_Click(object sender, RoutedEventArgs e)
        {
            StartProcessBtn.IsEnabled = false;
            
            bool success = await _processManager.StartScalingAsync();
            
            if (success)
            {
                // Aguardar processo inicializar e tentar integrar automaticamente
                await Task.Delay(3000);
                await TryAutoEmbedAsync();
            }
            
            UpdateProcessButtons();
        }
        
        private void StopProcessBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_windowEmbedder.IsEmbedded)
            {
                _windowEmbedder.UnembedWindow();
            }
            
            _processManager.StopScaling();
            UpdateProcessButtons();
            UpdateEmbedButtons();
        }
        
        private async void EmbedBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_processManager.ProcessId.HasValue)
            {
                EmbedBtn.IsEnabled = false;
                
                var result = await _windowEmbedder.EmbedScalingWindowAsync(this, _processManager.ProcessId.Value);
                
                if (result.Success)
                {
                    // Ocultar placeholder e ajustar área
                    PlaceholderContent.Visibility = Visibility.Hidden;
                    EmbedArea.BorderBrush = System.Windows.Media.Brushes.LimeGreen;
                }
                
                UpdateEmbedButtons();
            }
        }
        
        private void UnembedBtn_Click(object sender, RoutedEventArgs e)
        {
            var result = _windowEmbedder.UnembedWindow();
            
            if (result.Success)
            {
                // Mostrar placeholder novamente
                PlaceholderContent.Visibility = Visibility.Visible;
                EmbedArea.BorderBrush = System.Windows.Media.Brushes.CornflowerBlue;
            }
            
            UpdateEmbedButtons();
        }
        
        private async Task TryAutoEmbedAsync()
        {
            if (_processManager.IsRunning && _processManager.ProcessId.HasValue)
            {
                AddLogMessage("🔄 Tentando integração automática...");
                
                await Task.Delay(1000); // Aguardar janela aparecer
                
                var result = await _windowEmbedder.EmbedScalingWindowAsync(this, _processManager.ProcessId.Value);
                
                if (result.Success)
                {
                    Dispatcher.Invoke(() =>
                    {
                        PlaceholderContent.Visibility = Visibility.Hidden;
                        EmbedArea.BorderBrush = System.Windows.Media.Brushes.LimeGreen;
                        UpdateEmbedButtons();
                    });
                }
                else
                {
                    AddLogMessage("⚠️ Integração automática falhou - use o botão manual");
                }
            }
        }
        
        private void StatusTimer_Tick(object sender, EventArgs e)
        {
            // Verificar status da janela integrada
            _windowEmbedder.CheckEmbeddedWindow();
            
            // Atualizar UI
            UpdateProcessButtons();
            UpdateEmbedButtons();
        }
        
        private void OnProcessStarted(object sender, ProcessEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                ProcessStatusText.Text = "Status: ✅ Executando";
                ProcessPidText.Text = $"PID: {e.ProcessId}";
                UpdateProcessButtons();
                UpdateEmbedButtons();
            });
        }
        
        private void OnProcessStopped(object sender, ProcessEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                ProcessStatusText.Text = "Status: ❌ Parado";
                ProcessPidText.Text = "PID: -";
                UpdateProcessButtons();
                UpdateEmbedButtons();
                
                // Mostrar placeholder se estava integrado
                if (!_windowEmbedder.IsEmbedded)
                {
                    PlaceholderContent.Visibility = Visibility.Visible;
                    EmbedArea.BorderBrush = System.Windows.Media.Brushes.CornflowerBlue;
                }
            });
        }
        
        private void OnEmbedStatusChanged(object sender, EmbedStatusEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                EmbedStatusText.Text = $"Status: {(e.IsEmbedded ? "🔗 Integrado" : "↗️ Independente")}";
                EmbedHandleText.Text = $"Handle: {(e.WindowHandle != IntPtr.Zero ? $"0x{e.WindowHandle.ToInt64():X}" : "-")}";
                UpdateEmbedButtons();
            });
        }
        
        private void OnLogMessage(object sender, string message)
        {
            Dispatcher.Invoke(() => AddLogMessage(message));
        }
        
        private void AddLogMessage(string message)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            LogTextBlock.Text += $"[{timestamp}] {message}\n";
            
            // Auto-scroll para a última mensagem
            LogScrollViewer.ScrollToEnd();
            
            // Limitar número de linhas para performance
            var lines = LogTextBlock.Text.Split('\n');
            if (lines.Length > 100)
            {
                LogTextBlock.Text = string.Join("\n", lines[^50..]);
            }
        }
        
        private void UpdateProcessButtons()
        {
            bool isRunning = _processManager.IsRunning;
            StartProcessBtn.IsEnabled = !isRunning;
            StopProcessBtn.IsEnabled = isRunning;
        }
        
        private void UpdateEmbedButtons()
        {
            bool processRunning = _processManager.IsRunning;
            bool isEmbedded = _windowEmbedder.IsEmbedded;
            
            EmbedBtn.IsEnabled = processRunning && !isEmbedded;
            UnembedBtn.IsEnabled = isEmbedded;
        }
        
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _statusTimer?.Stop();
            
            // Cleanup
            if (_windowEmbedder.IsEmbedded)
            {
                _windowEmbedder.UnembedWindow();
            }
            
            _processManager?.Cleanup();
        }
    }
}

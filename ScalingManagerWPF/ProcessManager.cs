using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace ScalingManager
{
    public class ProcessManager
    {
        private static ProcessManager _instance;
        public static ProcessManager Instance => _instance ??= new ProcessManager();
        
        private Process _scalingProcess;
        private string _scalingExePath;
        
        public event EventHandler<ProcessEventArgs> ProcessStarted;
        public event EventHandler<ProcessEventArgs> ProcessStopped;
        public event EventHandler<string> LogMessage;
        
        public bool IsRunning => _scalingProcess != null && !_scalingProcess.HasExited;
        public int? ProcessId => _scalingProcess?.Id;
        
        private ProcessManager()
        {
            FindScalingExecutable();
        }
        
        private void FindScalingExecutable()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            
            string[] possiblePaths = 
            {
                Path.Combine(baseDir, "Scaling", "LosslessScaling.exe"),
                Path.Combine(baseDir, "Scaling", "Scaling.exe"),
                Path.Combine(Directory.GetParent(baseDir)?.FullName ?? "", "Scaling", "LosslessScaling.exe"),
                Path.Combine(Directory.GetParent(baseDir)?.FullName ?? "", "Scaling", "Scaling.exe")
            };
            
            foreach (string path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    _scalingExePath = path;
                    LogMessage?.Invoke(this, $"✅ Executável encontrado: {path}");
                    return;
                }
            }
            
            LogMessage?.Invoke(this, "❌ Executável do Scaling não encontrado nos caminhos esperados");
        }
        
        public async Task<bool> StartScalingAsync()
        {
            try
            {
                if (IsRunning)
                {
                    LogMessage?.Invoke(this, "⚠️ Scaling.exe já está rodando");
                    return true;
                }
                
                if (string.IsNullOrEmpty(_scalingExePath))
                {
                    LogMessage?.Invoke(this, "❌ Caminho do executável não encontrado");
                    return false;
                }
                
                LogMessage?.Invoke(this, "🚀 Iniciando Scaling.exe...");
                
                var startInfo = new ProcessStartInfo
                {
                    FileName = _scalingExePath,
                    WorkingDirectory = Path.GetDirectoryName(_scalingExePath),
                    UseShellExecute = false,
                    CreateNoWindow = false
                };
                
                _scalingProcess = Process.Start(startInfo);
                
                if (_scalingProcess != null)
                {
                    _scalingProcess.EnableRaisingEvents = true;
                    _scalingProcess.Exited += OnProcessExited;
                    
                    // Aguardar processo inicializar
                    await Task.Delay(1000);
                    
                    LogMessage?.Invoke(this, $"✅ Scaling.exe iniciado (PID: {_scalingProcess.Id})");
                    ProcessStarted?.Invoke(this, new ProcessEventArgs(_scalingProcess.Id, true));
                    
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"❌ Erro ao iniciar Scaling: {ex.Message}");
                return false;
            }
        }
        
        public bool StopScaling()
        {
            try
            {
                if (!IsRunning)
                {
                    LogMessage?.Invoke(this, "⚠️ Scaling.exe não está rodando");
                    return true;
                }
                
                LogMessage?.Invoke(this, "🛑 Parando Scaling.exe...");
                
                _scalingProcess.Kill();
                _scalingProcess.WaitForExit(5000);
                
                LogMessage?.Invoke(this, "✅ Scaling.exe parado com sucesso");
                return true;
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"❌ Erro ao parar Scaling: {ex.Message}");
                return false;
            }
        }
        
        private void OnProcessExited(object sender, EventArgs e)
        {
            LogMessage?.Invoke(this, $"🔄 Scaling.exe encerrado (código: {_scalingProcess?.ExitCode})");
            ProcessStopped?.Invoke(this, new ProcessEventArgs(_scalingProcess?.Id ?? 0, false));
            _scalingProcess = null;
        }
        
        public void Cleanup()
        {
            if (IsRunning)
            {
                StopScaling();
            }
        }
    }
    
    public class ProcessEventArgs : EventArgs
    {
        public int ProcessId { get; }
        public bool IsRunning { get; }
        
        public ProcessEventArgs(int processId, bool isRunning)
        {
            ProcessId = processId;
            IsRunning = isRunning;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace ScalingManager
{
    public class WindowEmbedder
    {
        private static WindowEmbedder _instance;
        public static WindowEmbedder Instance => _instance ??= new WindowEmbedder();
        
        private IntPtr _scalingWindowHandle = IntPtr.Zero;
        private IntPtr _parentWindowHandle = IntPtr.Zero;
        private bool _isEmbedded = false;
        
        public event EventHandler<string> LogMessage;
        public event EventHandler<EmbedStatusEventArgs> EmbedStatusChanged;
        
        public bool IsEmbedded => _isEmbedded;
        public IntPtr ScalingWindowHandle => _scalingWindowHandle;
        
        private WindowEmbedder() { }
        
        public async Task<EmbedResult> EmbedScalingWindowAsync(Window parentWindow, int processId)
        {
            try
            {
                LogMessage?.Invoke(this, "🔍 Procurando janela do Scaling.exe...");
                
                // Obter handle da janela pai (WPF)
                var windowHelper = new WindowInteropHelper(parentWindow);
                _parentWindowHandle = windowHelper.Handle;
                
                if (_parentWindowHandle == IntPtr.Zero)
                {
                    return new EmbedResult { Success = false, Message = "Não foi possível obter handle da janela pai" };
                }
                
                // Encontrar janela do Scaling
                var findResult = await FindScalingWindowAsync(processId);
                if (!findResult.Success)
                {
                    return findResult;
                }
                
                _scalingWindowHandle = new IntPtr(findResult.WindowHandle);
                
                LogMessage?.Invoke(this, "🔗 Integrando janela do Scaling...");
                
                // Definir como janela filha
                IntPtr result = Win32Api.SetParent(_scalingWindowHandle, _parentWindowHandle);
                
                if (result != IntPtr.Zero)
                {
                    // Posicionar janela na área designada
                    PositionEmbeddedWindow(400, 500); // Largura e altura padrão
                    
                    // Mostrar janela
                    Win32Api.ShowWindow(_scalingWindowHandle, Win32Api.SW_SHOW);
                    
                    _isEmbedded = true;
                    
                    LogMessage?.Invoke(this, "✅ Janela integrada com sucesso!");
                    LogMessage?.Invoke(this, "🎮 Você pode agora interagir diretamente com o Scaling na área designada");
                    
                    EmbedStatusChanged?.Invoke(this, new EmbedStatusEventArgs(true, _scalingWindowHandle));
                    
                    return new EmbedResult 
                    { 
                        Success = true, 
                        Message = "Janela integrada com sucesso", 
                        WindowHandle = _scalingWindowHandle.ToInt64() 
                    };
                }
                else
                {
                    return new EmbedResult 
                    { 
                        Success = false, 
                        Message = "Falha ao definir relacionamento pai-filho" 
                    };
                }
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"❌ Erro ao integrar janela: {ex.Message}");
                return new EmbedResult { Success = false, Message = $"Erro: {ex.Message}" };
            }
        }
        
        public EmbedResult UnembedWindow()
        {
            try
            {
                if (!_isEmbedded || _scalingWindowHandle == IntPtr.Zero)
                {
                    return new EmbedResult { Success = true, Message = "Nenhuma janela integrada" };
                }
                
                LogMessage?.Invoke(this, "📤 Desintegrando janela...");
                
                // Verificar se a janela ainda existe
                if (!Win32Api.IsWindow(_scalingWindowHandle))
                {
                    _isEmbedded = false;
                    _scalingWindowHandle = IntPtr.Zero;
                    EmbedStatusChanged?.Invoke(this, new EmbedStatusEventArgs(false, IntPtr.Zero));
                    return new EmbedResult { Success = true, Message = "Janela já foi fechada" };
                }
                
                // Remover relacionamento pai-filho (tornar independente)
                Win32Api.SetParent(_scalingWindowHandle, IntPtr.Zero);
                
                // Restaurar janela
                Win32Api.ShowWindow(_scalingWindowHandle, Win32Api.SW_RESTORE);
                
                _isEmbedded = false;
                var oldHandle = _scalingWindowHandle;
                _scalingWindowHandle = IntPtr.Zero;
                
                LogMessage?.Invoke(this, "✅ Janela desintegrada - agora independente");
                EmbedStatusChanged?.Invoke(this, new EmbedStatusEventArgs(false, IntPtr.Zero));
                
                return new EmbedResult { Success = true, Message = "Janela desintegrada com sucesso" };
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"❌ Erro ao desintegrar janela: {ex.Message}");
                return new EmbedResult { Success = false, Message = $"Erro: {ex.Message}" };
            }
        }
        
        public void PositionEmbeddedWindow(int width, int height)
        {
            if (!_isEmbedded || _scalingWindowHandle == IntPtr.Zero)
                return;
            
            try
            {
                // Posicionar na área de embedding designada
                const int embedX = 20;   // Margem da esquerda
                const int embedY = 120;  // Abaixo dos controles
                
                Win32Api.SetWindowPos(_scalingWindowHandle, IntPtr.Zero,
                    embedX, embedY, width, height,
                    Win32Api.SWP_NOZORDER | Win32Api.SWP_NOACTIVATE | Win32Api.SWP_SHOWWINDOW);
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"⚠️ Erro ao posicionar janela: {ex.Message}");
            }
        }
        
        private async Task<EmbedResult> FindScalingWindowAsync(int processId)
        {
            return await Task.Run(() =>
            {
                var foundWindows = new List<IntPtr>();
                
                // Enumerar todas as janelas
                Win32Api.EnumWindows((hWnd, lParam) =>
                {
                    if (Win32Api.IsWindowVisible(hWnd))
                    {
                        uint windowProcessId = Win32Api.GetWindowProcessId(hWnd);
                        
                        if (windowProcessId == processId)
                        {
                            // Verificar se é uma janela principal
                            if (Win32Api.IsMainWindow(hWnd))
                            {
                                string title = Win32Api.GetWindowTitle(hWnd);
                                
                                // Verificar se tem um título significativo (provável janela principal)
                                if (!string.IsNullOrEmpty(title) && title.Length > 3)
                                {
                                    foundWindows.Add(hWnd);
                                }
                            }
                        }
                    }
                    return true; // Continuar enumeração
                }, IntPtr.Zero);
                
                if (foundWindows.Any())
                {
                    // Se encontrou múltiplas janelas, pegar a primeira que pareça ser a principal
                    var mainWindow = foundWindows.First();
                    
                    string windowTitle = Win32Api.GetWindowTitle(mainWindow);
                    LogMessage?.Invoke(this, $"🎯 Janela encontrada: \"{windowTitle}\"");
                    
                    return new EmbedResult 
                    { 
                        Success = true, 
                        Message = "Janela do Scaling encontrada", 
                        WindowHandle = mainWindow.ToInt64() 
                    };
                }
                else
                {
                    return new EmbedResult 
                    { 
                        Success = false, 
                        Message = "Janela do Scaling não encontrada. Aguarde o processo inicializar completamente." 
                    };
                }
            });
        }
        
        public void CheckEmbeddedWindow()
        {
            if (_isEmbedded && _scalingWindowHandle != IntPtr.Zero)
            {
                if (!Win32Api.IsWindow(_scalingWindowHandle) || !Win32Api.IsWindowVisible(_scalingWindowHandle))
                {
                    LogMessage?.Invoke(this, "⚠️ Janela integrada foi fechada ou não está mais visível");
                    _isEmbedded = false;
                    _scalingWindowHandle = IntPtr.Zero;
                    EmbedStatusChanged?.Invoke(this, new EmbedStatusEventArgs(false, IntPtr.Zero));
                }
            }
        }
    }
    
    public class EmbedResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public long WindowHandle { get; set; }
    }
    
    public class EmbedStatusEventArgs : EventArgs
    {
        public bool IsEmbedded { get; }
        public IntPtr WindowHandle { get; }
        
        public EmbedStatusEventArgs(bool isEmbedded, IntPtr windowHandle)
        {
            IsEmbedded = isEmbedded;
            WindowHandle = windowHandle;
        }
    }
}

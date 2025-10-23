using System;
using System.Windows;

namespace ScalingManager
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Criar e mostrar janela principal
            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
        
        protected override void OnExit(ExitEventArgs e)
        {
            // Limpeza ao fechar aplicação
            ProcessManager.Instance?.Cleanup();
            base.OnExit(e);
        }
    }
}

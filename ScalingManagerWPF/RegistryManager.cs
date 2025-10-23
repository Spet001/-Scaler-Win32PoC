using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;

namespace ScalingManager
{
    public class RegistryManager
    {
        private static RegistryManager _instance;
        public static RegistryManager Instance => _instance ??= new RegistryManager();
        
        public event EventHandler<string> LogMessage;
        
        private const string STEAM_REGISTRY_PATH = @"Software\Valve\Steam";
        private const string STEAM_EXE_KEY = "SteamExe";
        
        private RegistryManager() { }
        
        public RegistryCheckResult CheckSteamRegistry()
        {
            try
            {
                LogMessage?.Invoke(this, "🔍 Verificando chaves do Steam no registry...");
                
                using var key = Registry.CurrentUser.OpenSubKey(STEAM_REGISTRY_PATH);
                
                if (key == null)
                {
                    return new RegistryCheckResult
                    {
                        IsValid = false,
                        KeyPath = $"HKEY_CURRENT_USER\\{STEAM_REGISTRY_PATH}",
                        ExpectedValue = "Chave Steam válida",
                        ActualValue = null,
                        Error = "Chave do Steam não encontrada no registry"
                    };
                }
                
                var steamExeValue = key.GetValue(STEAM_EXE_KEY)?.ToString();
                
                if (string.IsNullOrEmpty(steamExeValue))
                {
                    return new RegistryCheckResult
                    {
                        IsValid = false,
                        KeyPath = $"HKEY_CURRENT_USER\\{STEAM_REGISTRY_PATH}\\{STEAM_EXE_KEY}",
                        ExpectedValue = "Caminho para steam.exe",
                        ActualValue = null,
                        Error = "Valor SteamExe não encontrado"
                    };
                }
                
                bool isValid = steamExeValue.ToLowerInvariant().Contains("steam.exe");
                
                var result = new RegistryCheckResult
                {
                    IsValid = isValid,
                    KeyPath = $"HKEY_CURRENT_USER\\{STEAM_REGISTRY_PATH}\\{STEAM_EXE_KEY}",
                    ExpectedValue = "Caminho válido para steam.exe",
                    ActualValue = steamExeValue
                };
                
                string status = isValid ? "✅ Válido" : "❌ Inválido";
                LogMessage?.Invoke(this, $"📝 Registry do Steam: {status}");
                
                return result;
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"❌ Erro ao verificar registry: {ex.Message}");
                return new RegistryCheckResult
                {
                    IsValid = false,
                    KeyPath = $"HKEY_CURRENT_USER\\{STEAM_REGISTRY_PATH}",
                    ExpectedValue = "Registry acessível",
                    ActualValue = null,
                    Error = $"Erro ao acessar registry: {ex.Message}"
                };
            }
        }
        
        public bool FixSteamRegistry()
        {
            try
            {
                LogMessage?.Invoke(this, "🔧 Aplicando correção do registry...");
                
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string regFilePath = Path.Combine(baseDir, "FirstRun.reg");
                
                if (!File.Exists(regFilePath))
                {
                    // Criar arquivo .reg básico se não existir
                    string regContent = @"Windows Registry Editor Version 5.00

[HKEY_CURRENT_USER\Software\Valve\Steam]
""SteamExe""=""C:\\Program Files (x86)\\Steam\\steam.exe""
""SteamPath""=""C:\\Program Files (x86)\\Steam""
";
                    File.WriteAllText(regFilePath, regContent);
                }
                
                var startInfo = new ProcessStartInfo
                {
                    FileName = "regedit.exe",
                    Arguments = $"/s \"{regFilePath}\"",
                    UseShellExecute = true,
                    Verb = "runas" // Executar como administrador
                };
                
                var process = Process.Start(startInfo);
                process?.WaitForExit();
                
                if (process?.ExitCode == 0)
                {
                    LogMessage?.Invoke(this, "✅ Registry corrigido com sucesso");
                    
                    // Verificar novamente após correção
                    var recheckResult = CheckSteamRegistry();
                    return recheckResult.IsValid;
                }
                else
                {
                    LogMessage?.Invoke(this, "❌ Falha ao aplicar correção do registry");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"❌ Erro ao corrigir registry: {ex.Message}");
                return false;
            }
        }
    }
    
    public class RegistryCheckResult
    {
        public bool IsValid { get; set; }
        public string KeyPath { get; set; }
        public string ExpectedValue { get; set; }
        public string ActualValue { get; set; }
        public string Error { get; set; }
    }
}

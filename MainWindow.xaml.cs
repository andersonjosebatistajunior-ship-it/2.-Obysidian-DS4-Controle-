using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace ObysidianDS4
{
    public partial class MainWindow : Window
    {
        private readonly ServicoLicencaDS4 _servicoLicenca = new();

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Se já estiver com licença válida ativa para esta máquina, inicia o DS4 direto!
            if (_servicoLicenca.VerificarLicencaAtiva(out string tipo, out int dias))
            {
                IniciarDS4CoreEFechar();
                return;
            }

            TxtHwid.Text = ServicoLicencaDS4.ObterHWID();
        }

        private void IniciarDS4CoreEFechar()
        {
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            string corePath = Path.Combine(appDir, "DS4_Core.exe");

            // Se não encontrar DS4_Core.exe, procura DS4Windows.exe
            if (!File.Exists(corePath))
            {
                corePath = Path.Combine(appDir, "DS4Windows.exe");
            }

            if (File.Exists(corePath))
            {
                try
                {
                    var psi = new ProcessStartInfo(corePath)
                    {
                        WorkingDirectory = appDir,
                        UseShellExecute = true
                    };
                    Process.Start(psi);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro ao iniciar o motor do DS4: " + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Arquivo executável do motor do DS4 (DS4_Core.exe) não foi encontrado nesta pasta.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            Application.Current.Shutdown();
        }

        private void BtnCopiarHwid_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(TxtHwid.Text);
                TxtStatus.Text = "✓ HWID copiado para a área de transferência!";
                TxtStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));
            }
            catch { }
        }

        private void BtnAtivar_Click(object sender, RoutedEventArgs e)
        {
            string chave = TxtChave.Text;
            if (_servicoLicenca.AtivarChave(chave, out string msg))
            {
                TxtStatus.Text = "✓ " + msg;
                TxtStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));
                MessageBox.Show(msg, "Obysidian DS4 — Ativação Concluída", MessageBoxButton.OK, MessageBoxImage.Information);
                IniciarDS4CoreEFechar();
            }
            else
            {
                TxtStatus.Text = "❌ " + msg;
                TxtStatus.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));
            }
        }
    }
}

using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace YimInjector
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private HttpClient client = new HttpClient();

        private static string dllPath = @"C:\Windows\Temp\YimMenu.dll";
        private static string downloadURL = "https://github.com/YimMenu/YimMenu/releases/download/nightly/YimMenu.dll";

        private Process? GTAProcess = null;

        public MainWindow()
        {
            InitializeComponent();

            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
                {
                    MessageBox.Show("Run YimInjector as Administrator!");
                    Environment.Exit(-1);
                }
            }

            
        }

        private async void injectButton_Click(object sender, RoutedEventArgs e)
        {
            injectButton.IsEnabled = false;

            if (File.Exists(dllPath))
            {
                File.Delete(dllPath);
            }

            statusLabel.Content = "STATUS: Downloading latest YimMenu DLL...";

            HttpResponseMessage response = await client.GetAsync(downloadURL);
            using (FileStream fs = new FileStream(dllPath, FileMode.CreateNew, FileAccess.Write))
            {
                await response.Content.CopyToAsync(fs);
            }

            Process[] processes = Process.GetProcessesByName("GTA5");
            if (processes.Length == 0)
            {
                statusLabel.Content = "STATUS: Run GTA first!";
                injectButton.IsEnabled = true;
                return;
            }

            Process process = processes[0];
            bool status = Injector.Inject(process, dllPath);
            if (!status)
            {
                MessageBox.Show("Injection failed");
                Environment.Exit(-2);
            }

            statusLabel.Content = "STATUS: Injected!";

            GTAProcess = process;

            GTAProcess.EnableRaisingEvents = true;
            GTAProcess.Exited += GTAProcess_Exited;
        }

        private void GTAProcess_Exited(object? sender, EventArgs e)
        {
            statusLabel.Content = "STATUS: Idle";
            injectButton.IsEnabled = true;
        }
    }
}
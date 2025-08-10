using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// Interaction logic for StartupWindow.xaml
    /// </summary>
    public partial class StartupWindow : Window
    {
        public StartupWindow()
        {
            InitializeComponent();
            Window.GetWindow(this).Closing += StartupWindow_Closing;
        }

        string hostAddress = "http://localhost:" + Application.Current.Resources["Port"];

        private async void StartupWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            string solutionPath = System.IO.Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName);
            string serverPath = System.IO.Path.Combine(solutionPath, "Service");

            string serverText = System.IO.File.ReadAllText(System.IO.Path.Combine(serverPath, "appsettings.json"));

            //Update the appsettings.json file with the parameterized port value
            if (!string.IsNullOrEmpty(Application.Current.Resources["Port"].ToString()))
            {
                serverText = serverText.Replace(Application.Current.Resources["Port"].ToString(), "{Port}");

                File.WriteAllText(System.IO.Path.Combine(serverPath, "appsettings.json"), serverText);
            }
           

            MessageBox.Show("Lütfen Görev Yöneticisinden Service.exe'yi sonlandırın. proto dosyasını verdiğiniz gibi bırakmak istediğimden yeni bir metot eklemedim.", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);

            using var channel = GrpcChannel.ForAddress(hostAddress);
            await channel.ShutdownAsync(); //Closing channel
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {

            if (!int.TryParse(Port_TextBox.Text, out int port))
            {
                MessageBox.Show("Lütfen 1000-9999 arasında sayısal bir değer girin!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (port < 1000 || port > 9999)
            {
                MessageBox.Show("Lütfen 1000-9999 arasında bir değer girin!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            string solutionPath = System.IO.Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName);

            string serverPath = System.IO.Path.Combine(solutionPath, "Service");
            string clientPath = System.IO.Path.Combine(solutionPath, "Client");

            // Read the file as one string. 
            string serverText = System.IO.File.ReadAllText(System.IO.Path.Combine(serverPath, "appsettings.json"));
            serverText = serverText.Replace("{Port}", port.ToString());

            File.WriteAllText(System.IO.Path.Combine(serverPath, "appsettings.json"), serverText);

            var procServer = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            //Start the process
            procServer.Start();

            procServer.StandardInput.WriteLine($"cd {serverPath}");
            procServer.StandardInput.WriteLine("dotnet run");

            await Task.Delay(5000);
            MessageBox.Show("Sunucu Başlatıldı", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);

            Application.Current.Resources["Port"] = port.ToString();

            SpecialButton.IsEnabled = true;
        }

        private async void SpecialButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = new MainWindow();
            await Task.Delay(5000); //Wait for server.
            mainWindow.Show();
        }

        private async void StopButton_Click(object sender, RoutedEventArgs e)
        {
            using var channel = GrpcChannel.ForAddress(hostAddress);
            await channel.ShutdownAsync();

            MessageBox.Show("Sunucu Durduruldu", "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}

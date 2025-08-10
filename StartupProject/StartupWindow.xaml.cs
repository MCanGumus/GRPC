using System.Diagnostics;
using System.IO;
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

namespace StartupProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            string port = Port_TextBox.Text;

            string solutionPath = System.IO.Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName);

            string serverPath = System.IO.Path.Combine(solutionPath, "Service");
            string clientPath = System.IO.Path.Combine(solutionPath, "Client");

            // Read the file as one string. 
            string serverText = System.IO.File.ReadAllText(System.IO.Path.Combine(serverPath, "appsettings.json"));
            serverText = serverText.Replace("{Port}", port);

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

            // Update the appsettings.json file with the parameterized port value
            serverText = serverText.Replace(port, "{Port}");

            File.WriteAllText(System.IO.Path.Combine(serverPath, "appsettings.json"), serverText);
        }

        private void SpecialButton_Click(object sender, RoutedEventArgs e)
        {
            string port = Port_TextBox.Text;

            string clientPath = System.IO.Path.Combine(Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName, "Client");

            string clientText = System.IO.File.ReadAllText(System.IO.Path.Combine(clientPath, "MainWindow.xaml.cs"));
            clientText = clientText.Replace("{Port}", port);

            File.WriteAllText(System.IO.Path.Combine(clientPath, "MainWindow.xaml.cs"), clientText);

            var procUI = new Process
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

            procUI.Start();

            procUI.StandardInput.WriteLine($"cd {clientPath}");
            procUI.StandardInput.WriteLine("dotnet run");

            // Update the MainWindow.xaml.cs file with the parameterized port value
            clientText = clientText.Replace(port, "{Port}");

            File.WriteAllText(System.IO.Path.Combine(clientPath, "MainWindow.xaml.cs"), clientText);
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
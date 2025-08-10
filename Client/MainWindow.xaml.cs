using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Service;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Queue<string> _lastValues = new();

        private CancellationTokenSource _cts1;
        private CancellationTokenSource _cts2;

        private readonly SemaphoreSlim _semaphore = new(1, 1);

        string hostAddress = "http://localhost:" + Application.Current.Resources["Port"];

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void StreamNotification_Click(object sender, RoutedEventArgs e)
        {
            _cts1 = new CancellationTokenSource();

            try
            {
                using var channel = GrpcChannel.ForAddress(hostAddress);
                var client = new NotificationProto.NotificationProtoClient(channel);

                using var call = client.StreamNotifications(new EmptyRequest(), cancellationToken: _cts1.Token);

                await foreach (var response in call.ResponseStream.ReadAllAsync(_cts1.Token))
                {
                    StreamNotificationStatus.Text = response.Message;
                    AddToList(response.Message);
                }
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
            {
                StreamNotificationStatus.Text = "İzleme Durduruldu";
            }
        }

        private void StreamNotificationStop_Click(object sender, RoutedEventArgs e)
        {
            _cts1?.Cancel();
        }

        private async void StreamContentNotification_Click(object sender, RoutedEventArgs e)
        {
            _cts2 = new CancellationTokenSource();
            string value = Value_TextBox.Text;

            if (string.IsNullOrEmpty(value))
            {
                MessageBox.Show("Lütfen bir değer girin!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var channel = GrpcChannel.ForAddress(hostAddress);
                var client = new NotificationProto.NotificationProtoClient(channel);

                using var call = client.StreamContentNotifications(new ContentRequest { Title = value }, cancellationToken: _cts2.Token);

                await foreach (var response in call.ResponseStream.ReadAllAsync(_cts2.Token))
                {
                    StreamContentNotificationStatus.Text = response.Message;
                    AddToList(response.Message);
                }
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
            {
                StreamContentNotificationStatus.Text = "İzleme Durduruldu";
            }
        }

        private void StreamContentNotificationStop_Click(object sender, RoutedEventArgs e)
        {
            _cts2?.Cancel();
        }

        private async void CutButton_Click(object sender, RoutedEventArgs e)
        {
            if ((_cts1 == null || _cts1.IsCancellationRequested) ||
                (_cts2 == null || _cts2.IsCancellationRequested))
            {
                MessageBox.Show("Her iki akış da çalışıyor olmalı!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(Cut_TextBox.Text))
            { 
                MessageBox.Show("Lütfen geçerli bir değer girin!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(Cut_TextBox.Text, out int index))
            {
                MessageBox.Show("Lütfen 0-9 arasında sayısal bir değer girin!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (index < 0 || index > 9)
            {
                MessageBox.Show("Lütfen 0-9 arasında bir değer girin!", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await _semaphore.WaitAsync();
            try
            {
                var list = _lastValues.Reverse().ToList();

                if (index < 0 || index >= list.Count)
                {
                    MessageBox.Show("Geçersiz index!", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                list.RemoveAt(index);

                _lastValues.Clear();
                foreach (var item in list.Reverse<string>())
                    _lastValues.Enqueue(item);

                Value_ListBox.ItemsSource = null;
                Value_ListBox.ItemsSource = _lastValues.Reverse();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private void IndexTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }

        private async void AddToList(string text)
        {
            await _semaphore.WaitAsync();
            try
            {
                if (_lastValues.Count >= 10)
                    _lastValues.Dequeue();

                _lastValues.Enqueue(text);

                Value_ListBox.ItemsSource = null;
                Value_ListBox.ItemsSource = _lastValues.Reverse();
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
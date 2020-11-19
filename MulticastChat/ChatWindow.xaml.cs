using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
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

namespace MulticastChat
{
    /// <summary>
    /// Логика взаимодействия для ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : Window
    {
        private readonly ObservableCollection<string> messages = new ObservableCollection<string>();
        private string username;
        public ChatWindow(string name)
        {
            InitializeComponent();

            username = name;
            chatBox.ItemsSource = messages;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MessageBox.Show($"Добро пожаловать, {username}");
            var recieverUdpClient = new UdpClient(8001);

            try
            {
                recieverUdpClient.JoinMulticastGroup(IPAddress.Parse("225.1.10.8"), 10);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }

            while (true)
            {
                var result = await recieverUdpClient.ReceiveAsync();
                messages.Add(Encoding.UTF8.GetString(result.Buffer));
            }
        }

        private async void SendMessage(object sender, RoutedEventArgs e)
        {
            UdpClient client = new UdpClient(); // создаем UdpClient для отправки
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("225.1.10.8"), 8001);
            string message = String.Empty;
            try
            {
                await Task.Run(() =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        message = messageTextBox.Text; // сообщение для отправки

                        });

                    message = $"{username}: {message}";
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    client.SendAsync(data, data.Length, endPoint); // отправка

                    Dispatcher.Invoke(() =>
                    {
                        messageTextBox.Text = null;
                    });
                });
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }

            client.Close();
        }
    }
}

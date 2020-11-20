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
        UdpClient client;
        public ChatWindow(string name)
        {
            InitializeComponent();

            username = name;
            chatBox.ItemsSource = messages;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            messages.Add($"Добро пожаловать, {username}");
            //var recieverUdpClient = new UdpClient(8001);

            try
            {
                client = new UdpClient(8001);
                //recieverUdpClient.JoinMulticastGroup(IPAddress.Parse("225.1.10.8"), 10);
                client.JoinMulticastGroup(IPAddress.Parse("225.1.10.8"), 10);

                Task receiveTask = new Task(ReceiveMessages);
                receiveTask.Start();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }

            //while (true)
            //{
            //    var result = await recieverUdpClient.ReceiveAsync();
            //    messages.Add(Encoding.UTF8.GetString(result.Buffer));
            //}
        }

        private async void SendMessage(object sender, RoutedEventArgs e)
        {
            UdpClient client = new UdpClient();
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("225.1.10.8"), 8001);
            string message = String.Empty;
            try
            {
                await Task.Run(() =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        message = messageTextBox.Text;

                    });

                    message = $"{username}: {message}";
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    client.SendAsync(data, data.Length, endPoint);

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

        private async void ReceiveMessages()
        {
            //while (true)
            //{
            //    var result = await client.ReceiveAsync();
            //    messages.Add(Encoding.UTF8.GetString(result.Buffer));
            //}

            bool alive = true;
            try
            {
                while (alive)
                {
                    //IPEndPoint remoteIp = null;
                    //byte[] data = client.Receive(ref remoteIp);
                    //string message = Encoding.UTF8.GetString(data);
                    //message = $"{username}: {message}";

                    //// добавляем полученное сообщение в текстовое поле
                    //messages.Add(message);

                    var result = await client.ReceiveAsync();
                    Dispatcher.Invoke(() =>
                    {
                        messages.Add(Encoding.UTF8.GetString(result.Buffer));
                    });

                }
            }
            catch (ObjectDisposedException)
            {
                if (!alive)
                    return;
                throw;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }
    }
}

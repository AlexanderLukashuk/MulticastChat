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
        private string userName;
        public ChatWindow(string name)
        {
            InitializeComponent();

            userName = name;
            chatBox.ItemsSource = messages;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Task.Run(() =>
                {
                    ReceiveMessage();
                });

            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private async void SendMessage(object sender, RoutedEventArgs e)
        {
            UdpClient client = new UdpClient();

            //try
            //{
            //    while (true)
            //    {
            //        string message = $"{userName}: {messageTextBox.Text}";
            //        byte[] data = Encoding.UTF8.GetBytes(message);
            //        client.Send(data, data.Length);
            //    }
            //}
            //catch (Exception exception)
            //{
            //    MessageBox.Show(exception.Message);
            //}

            if (!string.IsNullOrEmpty(messageTextBox.Text))
            {
                var datagrams = Encoding.UTF8.GetBytes($"{userName}: {messageTextBox.Text}");
                await client.SendAsync(datagrams, datagrams.Length, new IPEndPoint(IPAddress.Parse("225.1.10.8"), 8001));
            }

            client.Close();
        }

        private async void ReceiveMessage()
        {
            UdpClient receiver = new UdpClient(8001); // UdpClient для получения данных
            IPEndPoint remoteIp = null;
            //string localAddress = LocalIPAddress();
            try
            {
                receiver.JoinMulticastGroup(IPAddress.Parse("225.1.10.8"), 10);
                while (true)
                {
                    byte[] data = receiver.Receive(ref remoteIp); // получаем данные
                    //if (remoteIp.Address.ToString().Equals(localAddress))
                    //    continue;
                    string message = Encoding.UTF8.GetString(data);

                    var result = await receiver.ReceiveAsync();
                    messages.Add(Encoding.UTF8.GetString(result.Buffer));
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }

            receiver.Close();
        }

        private static string LocalIPAddress()
        {
            string localIP = "";
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }
    }
}

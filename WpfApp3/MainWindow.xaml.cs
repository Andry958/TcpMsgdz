using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Net.Sockets;
using System.Net;
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

namespace WpfApp3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IPEndPoint server;
        TcpClient client;
        NetworkStream ns = null;
        ObservableCollection<MessageInfo> messages;
        StreamWriter sw;
        StreamReader sr;
        string Name = null;

        public MainWindow()
        {
            InitializeComponent();
            string address = ConfigurationManager.AppSettings["serverAddress"]!;
            short port = short.Parse(ConfigurationManager.AppSettings["serverPort"]!);
            server = new IPEndPoint(IPAddress.Parse(address), port);
            messages = new ObservableCollection<MessageInfo>();
            txtName.Text = "";
            this.DataContext = messages;
        }

        private void SendBtn(object sender, RoutedEventArgs e)
        {
            string message = msgTextBox.Text;
            msgTextBox.Text = "";
            if (sw == null || string.IsNullOrWhiteSpace(Name))
                return;

            sw.WriteLine($"{Name}: {message}");
            sw.Flush();
        }

        private void SetNameBtn(object sender, RoutedEventArgs e)
        {
            Name = txtName.Text;
        }

        private void msgTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendBtn(sender, e);
            }
        }

        private void ConnectedBtn(object sender, RoutedEventArgs e)
        {
            try
            {
                client = new TcpClient();
                client.Connect(server);
                ns = client.GetStream();
                sw = new StreamWriter(ns);
                sr = new StreamReader(ns);
                Listener();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void Listener()
        {
            while (true)
            {
                try
                {
                    string? message = await sr.ReadLineAsync();
                    if (message != null)
                        messages.Add(new MessageInfo(message));
                }
                catch
                {
                    break;
                }
            }
        }

        private void DisconnectedBtn(object sender, RoutedEventArgs e)
        {
            sw.WriteLine("$<close>");
            sw.Flush();

            sw.Close();
            ns.Close();
            client.Close();

            sw = null;
        }
    }

    public class MessageInfo
    {
        public string Message { get; set; }
        private DateTime time;
        public string Time => time.ToLongTimeString();

        public MessageInfo(string message)
        {
            Message = message;
            time = DateTime.Now;
        }

        public override string ToString()
        {
            return $"{Message,-20}  ({Time})";
        }
    }
}
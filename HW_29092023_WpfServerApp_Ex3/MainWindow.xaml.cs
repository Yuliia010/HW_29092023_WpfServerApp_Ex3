using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Markup;

namespace HW_29092023_WpfServerApp_Ex3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int port = 8080;
        private string ip = "127.0.0.1";
        private IPEndPoint ipPoint;
        private Socket socket;
        private Socket client;
        private IPEndPoint clientEndPoint;

        private int isConnected = -1;

        public MainWindow()
        {
            InitializeComponent();
            Connection();
            StartGetData();
            CheckConnection();

        }

        private async Task CheckConnection()
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        if (client != null && client.Connected)
                        {
                            isConnected = 1;
                        }
                        else
                        {
                            isConnected = 0;
                        }

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            UpdateConnBtn();
                        });
                    }
                    catch
                    {
                        isConnected = 0;
                    }

                    await Task.Delay(1000);
                }
            });
        }

        private async Task StartGetData()
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        if (client != null && client.Connected)
                        {
                            byte[] data = new byte[256];
                            StringBuilder builder = new StringBuilder();
                            int bytes = 0;
                            do
                            {
                                bytes = await client.ReceiveAsync(data, SocketFlags.None);
                                builder.Append(Encoding.UTF8.GetString(data, 0, bytes));

                            } while (client.Available > 0);

                            if (builder.Length > 0)
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    tb_Answer.Text = $"At {DateTime.Now.ToShortTimeString()} a line was received from {clientEndPoint.Address}:{clientEndPoint.Port}: {builder.ToString()}";
                                });
                            }
                        }
                        else
                        {
                            isConnected = 0;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        isConnected = 0;
                    }
                    await Task.Delay(100);
                }
            });
        }

        private void UpdateConnBtn()
        {
            if (isConnected == 1)
            {
                lb_connect.Background = new SolidColorBrush(Colors.LightGreen);
                lb_connect.Content = "Connected";
                lb_client.Content = $"{clientEndPoint.Address}:{clientEndPoint.Port}";
            }
            else if (isConnected == 0 || isConnected == -1)
            {
                lb_connect.Background = new SolidColorBrush(Colors.IndianRed);
                lb_connect.Content = "Disconnected";
                lb_client.Content = "---";
            }
        }

        private async void btn_Send_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (tb_Message.Text.Length == 0)
                {
                    throw new Exception("Message is empty");
                }

                if (isConnected == 0 || client == null || !client.Connected)
                {
                    throw new Exception("Not connected to the client");
                }

                string message = tb_Message.Text;
                byte[] data = Encoding.UTF8.GetBytes(message);
                await client.SendAsync(data, SocketFlags.None);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            UpdateConnBtn();
        }

        private async Task Connection()
        {
            ipPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(ipPoint);
                socket.Listen();
                this.Title = "Server started. Waiting for connections...";

                do
                {
                    client = await socket.AcceptAsync();
                    clientEndPoint = (IPEndPoint)client.RemoteEndPoint;
                    this.Title = "Client connected. Waiting for data...";
                    isConnected = 1;
                    UpdateConnBtn();

                } while (true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }
        }

    }
}

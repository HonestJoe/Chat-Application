using Newtonsoft.Json;
using Protocol;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Client
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private Socket loginSocket;
        private EndPoint epServer;
        private byte[] dataStream;

        private delegate void DisplayMessageDelegate(string message);
        private DisplayMessageDelegate displayMessageDelegate = null;

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                loginSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                loginSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

                epServer = new IPEndPoint(IPAddress.Parse(GetLocalIP()), 30000);
                loginSocket.Connect(epServer);

                Message login = new Message();
                login.Who = username.Text.Trim();
                login.What = password.ToString().Trim();
                login.When = DateTime.Now.ToShortTimeString();
                login.Where = 0;
                login.Why = Protocol.Protocol.CREATE_ACCOUNT;

                string jsonMessage = JsonConvert.SerializeObject(login);

                var enc = new ASCIIEncoding();
                byte[] msg = new byte[1500];
                msg = enc.GetBytes(jsonMessage);

                
                loginSocket.BeginSendTo(msg, 0, msg.Length, SocketFlags.None, epServer, new AsyncCallback(this.SendData), null);

                dataStream = new byte[1024];

                loginSocket.BeginReceiveFrom(this.dataStream, 0, this.dataStream.Length, SocketFlags.None, ref epServer, new AsyncCallback(this.ReceiveData), null);


            }
            catch (Exception er)
            {
                MessageBox.Show(er.ToString());
            }

            if (dataStream.Length == 1024)
            {
                MessageBox.Show("Welcome " + username.Text.Trim());
                MainWindow main = new MainWindow(username.Text.Trim());
                main.Show();
            }
            else
            {
                MessageBox.Show("Incorrect username/password");
            }

            loginSocket.Close();
            Close();

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                loginSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                loginSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

                epServer = new IPEndPoint(IPAddress.Parse(GetLocalIP()), 30000);
                loginSocket.Connect(epServer);

                Message login = new Message();
                login.Who = username.Text.Trim();
                login.What = password.Password.Trim();
                login.When = DateTime.Now.ToShortTimeString();
                login.Where = 0;
                login.Why = Protocol.Protocol.LOGIN;

                string jsonMessage = JsonConvert.SerializeObject(login);

                var enc = new ASCIIEncoding();
                byte[] msg = new byte[1500];
                msg = enc.GetBytes(jsonMessage);


                loginSocket.BeginSendTo(msg, 0, msg.Length, SocketFlags.None, epServer, new AsyncCallback(this.SendData), null);

                dataStream = new byte[1024];

                loginSocket.BeginReceiveFrom(this.dataStream, 0, this.dataStream.Length, SocketFlags.None, ref epServer, new AsyncCallback(this.ReceiveData), null);


            }
            catch (Exception er)
            {
                MessageBox.Show(er.ToString());
            }

            if (dataStream.Length == 1024)
            {
                MessageBox.Show("Welcome " + username.Text.Trim());
                MainWindow main = new MainWindow(username.Text.Trim());
                main.Show();
            }
            else
            {
                MessageBox.Show("Incorrect username/password");
            }

            loginSocket.Close();
            Close();


        }

        #region Networking

        /// <summary>
        /// Return Your Own IP Address
        /// </summary>
        private string GetLocalIP()
        {
            IPHostEntry host;
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "127.0.0.1";
        }

        //private void MessageCallBack(IAsyncResult result)
        //{

        //}

        private void SendData(IAsyncResult ar)
        {
            try
            {
                loginSocket.EndSend(ar);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private void ReceiveData(IAsyncResult ar)
        {
            try
            {
                // Check If Data Exists
                int size = loginSocket.EndReceiveFrom(ar, ref epServer);

                if (size > 0)
                {
                    // Auxiliary Buffer
                    byte[] aux = new byte[1500];

                    // Retrieve Data
                    aux = (byte[])dataStream;

                    // Decode Byte Array
                    string jsonStr = Encoding.ASCII.GetString(aux);

                    // Deserialize JSON
                    Message message = JsonConvert.DeserializeObject<Message>(jsonStr);

                    // TODO: Handle Messange
                    // Update Message Box Through Delegate
                    if (!string.IsNullOrEmpty(message.What))
                        this.Dispatcher.Invoke(this.displayMessageDelegate, new object[] { "[" + message.When + "] " + message.Who + " : " + message.What });

                    // Reset data stream
                    this.dataStream = new byte[1500];

                    // Continue listening for broadcasts
                    loginSocket.BeginReceiveFrom(this.dataStream, 0, this.dataStream.Length, SocketFlags.None, ref epServer, new AsyncCallback(this.ReceiveData), null);
                }
            }
            catch (ObjectDisposedException) { }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        #endregion
    }
}

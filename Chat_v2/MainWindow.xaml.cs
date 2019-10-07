using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WebSocketSharp;
using Newtonsoft.Json;

namespace Chat_v2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {        
        private WebSocket wsClient;       

        public MainWindow()
        {
            InitializeComponent();           
        }

        private void WsClient_OnClose(object sender, CloseEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                textBlockStatus.Text = "Websocket closed: " + e.Reason;
            });
        }

        private void WsClient_OnError(object sender, ErrorEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                textBlockStatus.Text = "Error on WebSocket: " + e.Message;
            });
        }

        private void WsClient_OnMessage(object sender, MessageEventArgs e)
        {
            ChatMessage obj = JsonConvert.DeserializeObject<ChatMessage>(e.Data);

            this.Dispatcher.Invoke(() =>
            {
                textBlockStatus.Text = e.Data;
                textBlockMessages.Text += UnixTimeStampToDateTime(obj.time / 1000).ToShortTimeString() + " = " + obj.user + ": " + obj.message + '\n';
            });
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        private void WsClient_OnOpen(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                textBlockStatus.Text = "Client connected to server";
            });
        }
        
        private void ButtonSend_Click(object sender, RoutedEventArgs e)
        {
            string user = textBoxUsername.Text;
            string message = textBoxSendMessage.Text;
            ChatMessage chat = new ChatMessage() { user = user, message = message };
            string json = JsonConvert.SerializeObject(chat);
            if (wsClient != null)
                wsClient.Send(json);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (wsClient != null)
                wsClient.Close();
        }

        private void ButtonConnect_Click(object sender, RoutedEventArgs e)
        {
            wsClient = new WebSocket("ws://localhost:9011");
            wsClient.OnOpen += WsClient_OnOpen;
            wsClient.OnMessage += WsClient_OnMessage;
            wsClient.OnError += WsClient_OnError;
            wsClient.OnClose += WsClient_OnClose;
            wsClient.Connect();
        }

        private void ButtonDisconnect_Click(object sender, RoutedEventArgs e)
        {
            if (wsClient != null)
                wsClient.Close();
        }
    }
}

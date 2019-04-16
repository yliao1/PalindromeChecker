using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MyTcpClient
{
    /// <summary>
    /// A custom client class to make it simple to use.
    /// </summary>
    public class MyClient : IDisposable
    {
        private readonly TcpClient Client; // TcpClient from built-in API.
        private readonly Thread ClientThread; // The thread listening to any incoming message.
        private bool IsRunning = false; // The status of the thread.

        /// <summary>
        /// Event for processing incoming messages.
        /// </summary>
        /// <param name="sender">Client triggering the event.</param>
        /// <param name="data">The data received by this client.</param>
        public delegate void NewMessageDelegate(object sender, byte[] data);
        public event NewMessageDelegate NewMessageEvent;

        //public delegate void ClientClosedDelegate(object sender);
        //public event ClientClosedDelegate ClientClosedEvent;
        /// <summary>
        /// Initialize a new TcpClient from exsiting built-in TcpClient.
        /// </summary>
        /// <param name="client">New TcpClient</param>
        /// <param name="isConnected">If the client is already connected.</param>
        public MyClient(TcpClient client, bool isConnected)
        {
            IsRunning = isConnected;
            Client = client;
            ClientThread = new Thread(delegate () // Create a new thread to handle the event.
            {
                while (IsRunning)
                {
                    if (Client.GetStream().DataAvailable)
                    {
                        // When new data is available to be read.
                        byte[] data = new byte[Client.Available];
                        Client.GetStream().Read(data, 0, data.Length);
                        if (data.Length == 1)
                        {
                            // If it is an empty message.
                            NewMessageEvent?.Invoke(this, null);
                        }
                        else
                        {
                            // Send the received data to the handler.
                            var result = new byte[data.Length - 1];
                            Array.Copy(data, 1, result, 0, data.Length - 1);
                            NewMessageEvent?.Invoke(this, result);
                        }
                    }
                }
                Dispose();
            })
            {
                IsBackground = true
            };
            if (isConnected)
            {
                ClientThread.Start(); // Srart the thread.
            }
        }
        /// <summary>
        /// Create new custom client.
        /// </summary>
        public MyClient() : this(new TcpClient(), false)
        {

        }

        /// <summary>
        /// Connect to a server.
        /// </summary>
        /// <param name="ip">The IP address.</param>
        /// <param name="port">The port number.</param>
        public void Connect(IPAddress ip, int port)
        {
            if (IsRunning)
            {
                return;
            }
            IsRunning = true;
            Client.Connect(ip, port);
            ClientThread.Start();
        }

        /// <summary>
        /// When the client is ready to be closed.
        /// </summary>
        public void Dispose()
        {
            IsRunning = false;
            try
            {
                ClientThread.Abort();
            }
            catch
            {

            }
            Client.Close();
            Client.Dispose();
        }

        /// <summary>
        /// Send data.
        /// </summary>
        /// <param name="data">The data is going to be sent.</param>
        public void Send(byte[] data)
        {
            var sentData = new byte[data.Length + 1];
            Array.Copy(data, 0, sentData, 1, data.Length);
            sentData[0] = 50;
            Client.GetStream().Write(sentData, 0, sentData.Length);
        }

        /// <summary>
        /// Send string.
        /// </summary>
        /// <param name="data">The string is going to be sent.</param>
        public void SendString(string data)
        {
            Send(Encoding.Default.GetBytes(data));
        }

        ~MyClient()
        {
            Dispose();
        }
    }
}

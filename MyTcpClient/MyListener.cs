using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace MyTcpClient
{
    /// <summary>
    /// A custom listner class (server) to make it simple to use.
    /// </summary>
    public class MyListener : IDisposable
    {
        /// <summary>
        /// Built-in TcpListener
        /// </summary>
        private readonly TcpListener Listener; 
        /// <summary>
        /// Make more clients to enable the multi-thread functionality.
        /// </summary>
        private readonly List<MyClient> Clients = new List<MyClient>();
        /// <summary>
        /// Thread to accept connection request from clients.
        /// </summary>
        private readonly Thread ListenerThread;
        /// <summary>
        /// Status of the thread.
        /// </summary>
        private bool IsRunning = false;

        /// <summary>
        /// New message arrives.
        /// </summary>
        /// <param name="sender">The server that gets the new message.</param>
        /// <param name="client">The client that gets the new message.</param>
        /// <param name="data">The message</param>
        public delegate void NewMessageDelegate(object sender, object client, byte[] data);
        public event NewMessageDelegate NewMessageEvent = null;

        /// <summary>
        /// A new connection request from a client.
        /// </summary>
        /// <param name="sender">The server that get requested.</param>
        /// <param name="newClient">The new client that requests new connection.</param>
        public delegate void NewClientDelegate(object sender, object newClient);
        public event NewClientDelegate NewClientEvent = null;

        /// <summary>
        /// Initialize the server with a port number.
        /// </summary>
        /// <param name="port">Port number.</param>
        public MyListener(int port)
        {
            Listener = new TcpListener(IPAddress.Any, port); // Create new listner.
            ListenerThread = new Thread(delegate () // Thread handling connection requests.
            {
                try
                {
                    while (IsRunning)
                    {
                        var newClient = new MyClient(Listener.AcceptTcpClient(), true);
                        Clients.Add(newClient);
                        newClient.NewMessageEvent += NewClient_NewMessageEvent;
                        NewClientEvent?.Invoke(this, newClient);
                    }
                }
                catch
                {
                    Dispose();
                }
            })
            {
                IsBackground = true
            };
        }
        /// <summary>
        /// New message arrives. It will resend the message to an handler.
        /// </summary>
        /// <param name="sender">The server that gets the message.</param>
        /// <param name="data">The data that is received.</param>
        private void NewClient_NewMessageEvent(object sender, byte[] data)
        {
            NewMessageEvent?.Invoke(this, sender, data);
        }

        /// <summary>
        /// Start listening
        /// </summary>
        public void Start()
        {
            IsRunning = true;
            Listener.Start();
            ListenerThread.Start();
        }

        /// <summary>
        /// When the server is ready to be closed.
        /// </summary>
        public void Dispose()
        {
            IsRunning = false;
            try
            {
                ListenerThread.Abort();
            }
            catch
            {

            }
            Listener.Stop();
        }
        ~MyListener()
        {
            Dispose();
        }
    }
}

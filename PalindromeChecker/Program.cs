using System;
using System.Net;
using System.Text;
using MyTcpClient;

using static System.Console;

namespace PalindromeChecker
{
    class Program
    {
        
        static void Main(string[] args)
        {
            using (MyClient client = new MyClient())
            {
                // Handle the parameters from user input.
                string ip = null;
                int port = -1;
                if (args.Length < 2)
                {
                    WriteLine("Please enter IP and port.");
                }
                foreach (var item in args)
                {
                    if (item.Substring(0, 5) == "-port")
                    {
                        port = Convert.ToInt32(item.Split('=')[1]);
                    }
                    else if (item.Substring(0, 3) == "-ip")
                    {
                        ip = item.Split("=")[1];
                    }
                }
                if (string.IsNullOrEmpty(ip) || port == -1)
                {
                    WriteLine("Invalid parameters!");
                    return;
                }
                // Connect this client to the server.
                client.Connect(IPAddress.Parse(ip), port);
                client.NewMessageEvent += Client_NewMessageEvent;
                string message = ReadLine();
                // Handle user input. The connection will be dropped when the 
                // input is null or empty string.
                while (!string.IsNullOrEmpty(message))
                {
                    client.SendString(message); // Send the string to the server.
                    message = ReadLine(); // Get user input.
                }
                client.SendString(string.Empty); // Send empty string to the server, which 
                // tells the server to drop the connection.
            }
            WriteLine("Press any key to exit...");
            ReadKey();
        }
        /// <summary>
        /// Handle the message received from the server.
        /// </summary>
        /// <param name="sender">The client that gets the message.</param>
        /// <param name="data">The data from the server.</param>
        private static void Client_NewMessageEvent(object sender, byte[] data)
        {
            WriteLine(Encoding.Default.GetString(data));
        }
    }
}

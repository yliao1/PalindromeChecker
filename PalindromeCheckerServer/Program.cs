using System;
using System.Text;
using System.Threading;
using MyTcpClient;

using static System.Console;

namespace PalindromeCheckerServer
{
    class Program
    {
        private static bool IsRunning = true;
        private static int ActiveUsers = 0;
        /// <summary>
        /// This is for thread lock. It will protect the multi-thread functionality.
        /// </summary>
        private static object lockObj = new object();
        static void Main(string[] args)
        {
            // Handle the parameters from command line.
            int port = 1200; // Default port
            if (args.Length >= 1)
            {
                bool isValid = false;
                foreach (var item in args)
                {
                    if (item.Substring(0, 5) == "-port")
                    {
                        try
                        {
                            port = Convert.ToInt32(item.Split('=')[1]);
                        }
                        catch
                        {
                            WriteLine("Invalid parameter(s)!");
                            return;
                        }
                        isValid = true;
                        break;
                    }
                }
                if (!isValid)
                {
                    WriteLine("Invalid parameter(s)!");
                    return;
                }
            }
            using (MyListener Server = new MyListener(port))
            {
                // Create the server.
                WriteLine("Server is ready!");
                Server.NewMessageEvent += Server_NewMessageEvent;
                Server.NewClientEvent += Server_NewClientEvent;
                Server.Start();
                while (true)
                {
                    Thread.Sleep(1000);
                    lock (lockObj) // Lock the varible, so other threads can't modify it.
                    {
                        WriteLine($"Current server status:{(IsRunning ? "Active" : "Offline")}");
                        if (!IsRunning)
                        {
                            break; // Close the program when all connections are dropped.
                        }
                    }
                }
                WriteLine("None is online. The server will be closed.");
                WriteLine("Server is closed. You can exit now...");
                ReadKey();
                return;
            }
        }
        /// <summary>
        /// Handle the event of new connection requests.
        /// </summary>
        /// <param name="sender">The server that gets new request.</param>
        /// <param name="newClient">Incoming client.</param>
        private static void Server_NewClientEvent(object sender, object newClient)
        {
            ActiveUsers++;
            WriteLine($"A clinet joined the server. Active users: {ActiveUsers}");
            (newClient as MyClient).Send(Encoding.Default.GetBytes("Welcome!"));
        }
        /// <summary>
        /// Handle the message from clients and check if it is a palindrome.
        /// </summary>
        /// <param name="sender">The server that gets the message.</param>
        /// <param name="client">The client that gets the message.</param>
        /// <param name="data">Incoming data. If the data is null, it means disconnection
        /// request.</param>
        private static void Server_NewMessageEvent(object sender, object client, byte[] data)
        {
            if (data == null) // If it is a disconnection request. 
            {
                WriteLine($"A clinet left the server. Active users: {--ActiveUsers}");
                if (ActiveUsers < 1) // If none is online.
                {
                    lock (lockObj)
                    {
                        IsRunning = false; // Change the status of the problem to false.
                    }
                }
                return;
            }
            string text = Encoding.Default.GetString(data); // Convert the binary data to a
                                                            //string
            Write($"Get string: {text} "); // Print the string.
            if (string.IsNullOrEmpty(text)) // If it is an empty string, 
                                            // it also means disconnection request.
            {
                WriteLine($"A clinet left the server. Active users: {--ActiveUsers}");
                if (ActiveUsers < 1)
                {
                    lock (lockObj)
                    {
                        IsRunning = false;
                    }
                }
                return;
            }
            // Get all letters and numbers of the string.
            string normalizedText = "";
            foreach (var item in text)
            {
                if (char.IsLetterOrDigit(item))
                {
                    normalizedText += char.ToLower(item);
                }
            }
            Write($"Normalized text: {normalizedText} ");
            // Check if it is a palindrome.
            int l = 0, r = normalizedText.Length - 1;
            while (l <= r)
            {
                // Send back the result to the client.
                if (normalizedText[l] != normalizedText[r])
                {
                    (client as MyClient).SendString("No");
                    WriteLine("Response: No");
                    return;
                }
                l++;
                r--;
            }
            // Send back the result to the client.
            (client as MyClient).SendString("Yes");
            WriteLine("Response: Yes");
        }
    }
}

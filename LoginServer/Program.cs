using Database.Context;
using Networking;
using System.Net;
using System.Net.Sockets;

namespace LoginServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("===================================");
            Console.WriteLine("          LOGINSERVER              ");
            Console.WriteLine("===================================");

            using LoginDatabaseContext loginDatabase = new();
            loginDatabase.Database.EnsureCreated();

            IPEndPoint endpoint = IPEndPoint.Parse("127.0.0.1:3724");
            TcpListener listener = new(endpoint);
            listener.Start();

            while (true)
            {
                StartSocket(listener.AcceptTcpClient());
            }
        }

        private static void StartSocket(TcpClient client)
        {
            LoginSocket socket = new(client);
            socket.Start();

            /*
            // Debugging to make sure the sessions ans sockets are getting killed properly
            WeakReference reference1 = new(socket);
            WeakReference reference2 = new(socket.Session);

            Task.Run(async() =>
            {
                while (true)
                {
                    await Task.Delay(1000);
                    Console.WriteLine($"[Is socket alive] {reference1.IsAlive}");
                    Console.WriteLine($"[Is session alive] {reference2.IsAlive}");
                    GC.Collect();
                }
            });
            */
        }
    }
}

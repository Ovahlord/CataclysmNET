using System.Net.Sockets;
using System.Net;

namespace RealmServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("===================================");
            Console.WriteLine("          REALMSERVER              ");
            Console.WriteLine("===================================");

            IPEndPoint endpoint = IPEndPoint.Parse("127.0.0.1:8085");
            TcpListener listener = new(endpoint);
            listener.Start();

            Task.Run(async () =>
            {
                while (true)
                {
                    StartSocket(await listener.AcceptTcpClientAsync());
                }
            });

            Console.ReadLine();
        }

        private static void StartSocket(TcpClient client)
        {
            Console.WriteLine("created socket");
            RealmSocket socket = new(client);
            socket.Start();
            socket.SendConnectionInitialize();
        }
    }
}

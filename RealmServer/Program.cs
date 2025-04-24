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

            while (true)
            {
                StartSocket(listener.AcceptTcpClient());
            }
        }

        private static void StartSocket(TcpClient client)
        {
            RealmSocket socket = new(client);
            socket.Start();
        }
    }
}

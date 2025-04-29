using System.Net.Sockets;
using System.Net;
using Core.Misc;

namespace RealmServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(Banner.BannerString);
            Console.WriteLine("===================================");
            Console.WriteLine("          REALMSERVER              ");
            Console.WriteLine("===================================");

            IPEndPoint endpoint = IPEndPoint.Parse("127.0.0.1:8085");
            IPEndPoint endpoint2 = IPEndPoint.Parse("127.0.0.1:140");
            TcpListener listener = new(endpoint);
            listener.Start();

            TcpListener listener2 = new(endpoint2);
            listener2.Start();


            Task.Run(async () =>
            {
                while (true)
                {
                    StartSocket(await listener.AcceptTcpClientAsync());
                }
            });

            Task.Run(async () =>
            {
                while (true)
                {
                    StartSocket(await listener2.AcceptTcpClientAsync());
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

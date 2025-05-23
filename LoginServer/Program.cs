﻿using Database.LoginDatabase;
using Database.RealmDatabase;
using LoginServer.Networking;
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

            using RealmDatabaseContext realmDatabase = new();
            realmDatabase.Database.EnsureCreated();

            IPEndPoint endpoint = IPEndPoint.Parse("127.0.0.1:3724");
            TcpListener listener = new(endpoint);
            listener.Start();

            RealmsStatusManager.Initialize();

            while (true)
            {
                StartSocket(listener.AcceptTcpClient());
            }
        }

        private static void StartSocket(TcpClient client)
        {
            LoginSocket socket = new(client);
            socket.Open();
        }
    }
}

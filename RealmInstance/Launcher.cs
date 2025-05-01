using Database.RealmDatabase;
using Database.RealmDatabase.Tables;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Sockets;

namespace RealmInstance
{
    public sealed class Launcher
    {
        public async Task Launch(int realmId)
        {
            try
            {
                using RealmDatabaseContext realmDatabase = new();
                Realms? realmInfo = await realmDatabase.Realms.FirstOrDefaultAsync(realm => realm.Id == realmId);

                if (realmInfo == null)
                    throw new Exception($"Data for RealmInstance (realm Id {realmId}) could not be loaded from database");

                if (!IPEndPoint.TryParse(realmInfo.FirstSocketAddress, out IPEndPoint? firstSocketEndPoint)
                    || !IPEndPoint.TryParse(realmInfo.SecondSocketAddress, out IPEndPoint? secondSocketEndPoint))
                    throw new Exception($"Could not parse one of the provided realm socket addresses for realm Id {realmId}");

                TcpListener socketListener1 = new(firstSocketEndPoint);
                TcpListener socketListener2 = new(secondSocketEndPoint);

                socketListener1.Start();
                socketListener2.Start();

                Console.WriteLine($"Realm '{realmInfo.Name}' is now listening to connections on {firstSocketEndPoint} and {secondSocketEndPoint}");

                Task.WhenAll(ListenToConnectionsAsync(socketListener1), ListenToConnectionsAsync(socketListener2)).Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async Task ListenToConnectionsAsync(TcpListener listener)
        {
            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();

                // We delay the session creation for a second to make sure it's not just a ping from the LoginServer (or someone else being funny)
                await Task.Delay(1000);
                if (!client.Connected)
                    continue;

                RealmSocket socket = new(client);
                socket.Start();
                socket.SendConnectionInitialize();
            }
        }
    }
}

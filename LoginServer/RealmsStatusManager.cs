using Database.RealmDatabase;
using Database.RealmDatabase.Tables;
using Shared.Enums;
using System.Net;
using System.Net.Sockets;

namespace LoginServer
{
    public sealed class RealmsStatusManager
    {
        public static List<Realms> RealmCache { get; private set; } = [];

        public static void Initialize()
        {
            Task.Run(async() =>
            {
                while (true)
                {
                    UpdateRealmCache();

                    // The client updates their realm list every 5 seconds, so we should do the same
                    await Task.Delay(5000);
                }
            });
        }

        private static void UpdateRealmCache()
        {
            using RealmDatabaseContext realmDatabase = new();
            List<Realms> realms = realmDatabase.Realms.ToList();

            // Ping each realm to determine their connection state
            //List<Task> connectTasks = [];
            //foreach (Realms realm in realms)
            //{
            //    connectTasks.Add(Task.Run(async () =>
            //    {
            //        if (IPEndPoint.TryParse(realm.Address, out IPEndPoint? realmEndpoint))
            //        {
            //            using TcpClient client = new();
            //            // We give every realm 1 second to respond, otherwise we will mark the realm as offline.
            //            // This will detect offline realms as well as blocking realms under heavy load.
            //
            //            Task connectTask = client.ConnectAsync(realmEndpoint);
            //            Task timeoutTask = Task.Delay(1000);
            //            Task completedTask = await Task.WhenAny(connectTask, timeoutTask);
            //
            //            if (completedTask == timeoutTask || !client.Connected)
            //                realm.Flags |= (byte)RealmFlags.Offline;
            //        }
            //        else
            //            realm.Flags |= (byte)RealmFlags.Offline;
            //    }));
            //}
            //
            //Task.WaitAll(connectTasks);
            RealmCache = realms;
            //Console.WriteLine($"Updated status of {RealmCache.Count} realms");
        }
    }
}

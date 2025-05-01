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
                // Loading realm settings from database
                using RealmDatabaseContext realmDatabase = new();
                Realms? realmInfo = await realmDatabase.Realms.FirstOrDefaultAsync(realm => realm.Id == realmId);

                if (realmInfo == null)
                    throw new Exception($"Data for RealmInstance (realm Id {realmId}) could not be loaded from database");

                // Store realm configuation
                Realm.Instance.Initialize(realmInfo);
                Realm.Instance.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}

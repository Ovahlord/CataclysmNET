using Database.RealmDatabase;
using Database.RealmDatabase.Tables;
using Microsoft.EntityFrameworkCore;

namespace WorldInstance
{
    public sealed class Launcher
    {
        public async Task Launch(int realmId, int mapRecId)
        {
            try
            {
                // Loading realm settings from database
                using RealmDatabaseContext realmDatabase = new();
                Realms? realmInfo = await realmDatabase.Realms.FirstOrDefaultAsync(realm => realm.Id == realmId);

                if (realmInfo == null)
                    throw new Exception($"Data for RealmInstance (realm Id {realmId}) could not be loaded from database");

                World.Instance.Initialize(realmInfo, mapRecId);
                World.Instance.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}

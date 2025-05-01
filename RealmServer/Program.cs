using Database.RealmDatabase;
using Database.RealmDatabase.Tables;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace RealmServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("===================================");
            Console.WriteLine("          REALMSERVER              ");
            Console.WriteLine("===================================");
            Console.WriteLine();

            _ = StartRealms();

            Console.ReadLine();
        }

        private static async Task StartRealms()
        {
            Console.WriteLine("Starting realms...");

            try
            {
                // Load realm definitions from database
                using RealmDatabaseContext realmDatabase = new();
                List<Realms> realms = await realmDatabase.Realms.ToListAsync();
                if (realms.Count == 0)
                {
                    Console.WriteLine("Realms table is empty. Cannot create realms.");
                    return;
                }

                Console.WriteLine($"Loaded {realms.Count} definition(s) for realms.");

                // Prepare hosting process start info
                ProcessStartInfo hostStartInfo = new()
                {
                    //CreateNoWindow = true,
                    FileName = "InstanceHost"
                };

                if (OperatingSystem.IsWindows())
                    hostStartInfo.FileName += ".exe";

                foreach (Realms realm in realms)
                {
                    StartRealm(realm, hostStartInfo);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void StartRealm(Realms realm, ProcessStartInfo hostStartInfo)
        {
            Console.WriteLine($"Starting hosting process for realm '{realm.Name}'...");

            hostStartInfo.Arguments = $"Realm {realm.Id}";
            Process? process = Process.Start(hostStartInfo);
            if (process == null)
                Console.WriteLine("Process could not be started!");
        }
    }
}

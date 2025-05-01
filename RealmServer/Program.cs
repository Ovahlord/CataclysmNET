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

            StartRealms();

            Console.ReadLine();
        }

        private static void StartRealms()
        {
            Console.WriteLine("Starting realms...");

            try
            {
                // Load realm definitions from database
                using RealmDatabaseContext realmDatabase = new();
                List<Realms> realms = realmDatabase.Realms.ToList();
                if (realms.Count == 0)
                {
                    Console.WriteLine("Realms table is empty. Cannot create realms.");
                    return;
                }

                Console.WriteLine($"Loaded {realms.Count} definition(s) for realms.");

                // Prepare hosting process start info
                ProcessStartInfo hostStartInfo = new()
                {
                    FileName = "InstanceHost"
                };

                // Linux systems don't use .exe files, so we only add the extension on windows platforms
                if (OperatingSystem.IsWindows())
                    hostStartInfo.FileName += ".exe";

                // Spawning realms
                foreach (Realms realm in realms)
                {
                    StartRealmProcess(realm, hostStartInfo);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Attempts to start the hosting process that will run the realm
        /// </summary>
        /// <param name="realm"></param>
        /// <param name="hostStartInfo"></param>
        private static void StartRealmProcess(Realms realm, ProcessStartInfo hostStartInfo)
        {
            Console.WriteLine($"Starting hosting process for realm '{realm.Name}'...");

            hostStartInfo.Arguments = $"Realm {realm.Id}";
            Process? process = Process.Start(hostStartInfo);
            if (process == null)
                Console.WriteLine("Process could not be started!");
        }
    }
}

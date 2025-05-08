using Database.RealmDatabase;
using Database.RealmDatabase.Tables;
using System.Diagnostics;

namespace WorldServer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("===================================");
            Console.WriteLine("           WORLD SERVER            ");
            Console.WriteLine("===================================");
            Console.WriteLine();

            StartWorlds();

            Console.ReadLine();
        }

        private static void StartWorlds()
        {
            Console.WriteLine("Starting worlds...");

            try
            {
                // Load realm definitions from database
                using RealmDatabaseContext realmDatabase = new();
                List<Realms> realms = realmDatabase.Realms.ToList();
                if (realms.Count == 0)
                {
                    Console.WriteLine("Realms table is empty. Cannot create worlds for realms.");
                    return;
                }

                Console.WriteLine($"Loaded {realms.Count} definition(s) for realms.");

                // Prepare hosting process start info
                ProcessStartInfo hostStartInfo = new("InstanceHost");

                // Linux systems don't use .exe files, so we only add the extension on windows platforms
                if (OperatingSystem.IsWindows())
                    hostStartInfo.FileName += ".exe";

                // Spawning realms
                foreach (Realms realm in realms)
                {
                    StartWorldProcess(realm, 0, hostStartInfo);
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
        private static void StartWorldProcess(Realms realm, int mapRecId, ProcessStartInfo hostStartInfo)
        {
            Console.WriteLine($"Starting hosting process for Map 'NYI' on realm '{realm.Name}'...");

            hostStartInfo.Arguments = $"World {realm.Id} {mapRecId}";
            Process? process = Process.Start(hostStartInfo);
            if (process == null)
                Console.WriteLine("Process could not be started!");
        }
    }
}

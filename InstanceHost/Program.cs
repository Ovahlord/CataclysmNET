using System.Reflection;

namespace InstanceHost
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                    throw new Exception($"Provided too few launch parameters");

                switch (args[0])
                {
                    case "Realm":
                    {
                        if (int.TryParse(args[1], out int realmId))
                            LaunchRealmInstance(realmId);
                        break;
                    }
                    case "World":
                    {
                        if (int.TryParse(args[1], out int realmId) && int.TryParse(args[1], out int mapId))
                            LaunchWorldInstance(realmId, mapId);
                            break;
                    }
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            Console.ReadLine();
        }

        private static void LaunchRealmInstance(int realmId)
        {
            RealmInstance.Launcher realmLauncher = new();
            _= realmLauncher.Launch(realmId);
        }

        private static void LaunchWorldInstance(int realmId, int mapId)
        {

        }
    }
}

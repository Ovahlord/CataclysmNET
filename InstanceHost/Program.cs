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

                if (args[0].Equals("Realm"))
                {
                    if (int.TryParse(args[1], out int realmId))
                        LaunchRealmInstance(realmId);
                }

                if (args[0].Equals("World"))
                {
                    //LaunchWorldInstance();
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
            Assembly? assembly = Assembly.LoadFrom("RealmInstance.dll");
            Type? instanceType = assembly.GetType("RealmInstance.Launcher");
            MethodInfo? method = instanceType?.GetMethod("Launch");

            if (assembly == null || instanceType == null || method == null)
                throw new Exception("RealmInstance assembly could not be processed");

            object? realmInstance = Activator.CreateInstance(instanceType);
            if (realmInstance == null)
                throw new Exception("RealmInstance could not be created");

            method.Invoke(realmInstance, [realmId]);
        }

        private static void LaunchWorldInstance(int realmId, int mapId)
        {

        }
    }
}

using Database.RealmDatabase.Tables;
using System.Net;
using System.Net.Sockets;

namespace WorldInstance
{
    internal class World
    {
        private Realms? _realmInfo;
        private IPEndPoint? _connectionEndpoint;
        private TcpListener? _connectionListener;

        private static readonly Lazy<World> _instance = new(() => new World());
        public static World Instance => _instance.Value;

        public void Initialize(Realms realmInfo)
        {
            _realmInfo = realmInfo;
        }

        public void Open()
        {

        }
    }
}

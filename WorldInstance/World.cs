using Database.RealmDatabase.Tables;
using Game.Networking;
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

        public void Initialize(Realms realmInfo, int mapRecId)
        {
            _realmInfo = realmInfo;

            // Setting up the connection end point
            if (!IPEndPoint.TryParse($"{realmInfo.SocketAddress}:{realmInfo.FirstWorldSocketPort + mapRecId}", out _connectionEndpoint))
                throw new Exception($"Could not parse one of the provided world socket addresses for realm Id {realmInfo.Id}");

            Console.WriteLine($"Initialized world connection listener for map {mapRecId} on endpoint: {_connectionEndpoint}");

            _connectionListener = new(_connectionEndpoint);
        }

        public void Open()
        {
            if (_connectionListener == null)
                throw new Exception("Tried to open an uninitialized world instance");

            _connectionListener.Start();

            _ = ListenToConnectionsAsync();
        }

        public void Stop()
        {
            if (_connectionListener == null)
                throw new Exception("Tried to close an uninitialized world instance");

            _connectionListener.Stop();
        }

        private async Task ListenToConnectionsAsync()
        {
            if (_connectionListener == null)
                throw new Exception("The connection listeners is null!");

            try
            {
                while (true)
                {
                    TcpClient client = await _connectionListener.AcceptTcpClientAsync();
                    WorldSocket socket = new(client);
                    socket.Open();
                    socket.SendConnectionInitialize();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[World '{_realmInfo?.Name}' MapRecId 'NYI'] exception: {ex}");
            }
        }
    }
}

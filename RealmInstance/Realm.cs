using Database.RealmDatabase.Tables;
using MySqlX.XDevAPI;
using System.Net;
using System.Net.Sockets;

namespace RealmInstance
{
    public sealed class Realm
    {
        private Realms? _realmInfo;
        private IPEndPoint? _firstConnectionEndPoint;
        private IPEndPoint? _secondConnectionEndPoint;
        private TcpListener? _connectionListener1;
        private TcpListener? _connectionListener2;

        private static readonly Lazy<Realm> _instance = new(() => new Realm());
        public static Realm Instance => _instance.Value;

        public void Initialize(Realms realmInfo)
        {
            _realmInfo = realmInfo;

            // Setting up the connection end points
            if (!IPEndPoint.TryParse($"{realmInfo.SocketAddress}:{realmInfo.FirstRealmSocketPort}", out _firstConnectionEndPoint)
                || !IPEndPoint.TryParse($"{realmInfo.SocketAddress}:{realmInfo.SecondRealmSocketPort}", out _secondConnectionEndPoint))
                throw new Exception($"Could not parse one of the provided realm socket addresses for realm Id {realmInfo.Id}");

            _connectionListener1 = new(_firstConnectionEndPoint);
            _connectionListener2 = new(_secondConnectionEndPoint);
        }

        public void Open()
        {
            if (_connectionListener1 == null || _connectionListener2 == null)
                throw new Exception("Tried to open an uninitialized realm");

            _connectionListener1.Start();
            _connectionListener2.Start();

            _ = ListenToConnectionsAsync();

            Console.WriteLine($"[Realm '{_realmInfo?.Name}'] is now open!");
        }

        public void Close()
        {
            if (_connectionListener1 == null || _connectionListener2 == null)
                throw new Exception("Tried to close an uninitialized realm");

            _connectionListener1.Stop();
            _connectionListener2.Stop();
        }

        public IPEndPoint GetConnectToEndPoint()
        {
            if (_secondConnectionEndPoint == null)
                throw new Exception("Tried to retrieve the endpoint for SMSG_CONNECT_TO from an uninitialized realm");

            return _secondConnectionEndPoint;
        }

        public IPEndPoint GetConnectToEndPointForMapInstance(int mapRecId)
        {
            if (_realmInfo == null)
                throw new Exception("Tried to retrieve the endpoint for SMSG_CONNECT_TO from an uninitialized realm");

            return IPEndPoint.Parse($"{_realmInfo.SocketAddress}:{_realmInfo.FirstWorldSocketPort + mapRecId}");
        }

        public int GetId()
        {
            if (_realmInfo == null)
                throw new Exception("Tried to retrieve the realm Id from an uninitialized realm");

            return _realmInfo.Id;
        }

        private async Task ListenToConnectionsAsync()
        {
            if (_connectionListener1 == null || _connectionListener2 == null)
                throw new Exception("One of the connection listeners is null!");

            try
            {
                while (true)
                {
                    Task<TcpClient> acceptTask1 = _connectionListener1.AcceptTcpClientAsync();
                    Task<TcpClient> acceptTask2 = _connectionListener2.AcceptTcpClientAsync();
                    Task<TcpClient> completedTask = await Task.WhenAny(acceptTask1, acceptTask2);

                    TcpClient client = await completedTask;
                    RealmSocket socket = new(client);
                    socket.Open();
                    socket.SendConnectionInitialize();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Realm '{_realmInfo?.Name}'] exception: {ex}");
            }
        }
    }
}

using Packets;

namespace Core.Networking
{
    public abstract class BaseSession(BaseSocket socket)
    {
        protected readonly CancellationTokenSource _cancellationTokenSource = new();
        protected BaseSocket Socket => socket;

        public abstract void HandlePacket(int opcode, byte[] payload);

        public async Task SendPacketAsync(ServerPacket packet, CancellationToken cancellationToken = default)
        {
            await Socket.SendPacketAsync(packet, cancellationToken);
        }

        public void SendPacket(ServerPacket packet)
        {
            Task.Run(() => SendPacketAsync(packet), _cancellationTokenSource.Token);
        }

        public void Close()
        {
            Socket.Close();
            _cancellationTokenSource.Cancel();
        }
    }
}

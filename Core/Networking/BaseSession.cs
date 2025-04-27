using Core.Packets;
using Core.Packets.Opcodes;

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
            if (this is GameSession session)
                Console.WriteLine($"[{GetType().Name}] (Id: {session.SessionId}) Sending packet with opcode {(ServerOpcode)packet.Cmd}");

            Task.Run(() => SendPacketAsync(packet), _cancellationTokenSource.Token);
        }

        public virtual void Close()
        {
            if (_cancellationTokenSource.IsCancellationRequested)
                return;

            Socket.Close();
            _cancellationTokenSource.Cancel();
        }
    }
}

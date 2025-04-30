using Core.Packets;
using Core.Packets.Opcodes;

namespace Core.Networking
{
    public abstract class BaseSession(BaseSocket socket)
    {
        protected readonly CancellationTokenSource _cancellationTokenSource = new();
        protected BaseSocket Socket => socket;

        public abstract void HandlePacket(int opcode, byte[] payload);

        public async Task SendPacketAsync(ServerPacket packet)
        {
            Console.WriteLine($"[{GetType().Name}] Sending packet for {(ServerOpcode)packet.Cmd}");
            await Socket.SendPacketAsync(packet, _cancellationTokenSource.Token);
        }

        public void SendPacket(ServerPacket packet)
        {
            _ = SendPacketAsync(packet);
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

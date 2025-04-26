using Packets;

namespace Core.Networking
{
    public abstract class BaseSession(BaseSocket socket)
    {
        protected readonly CancellationTokenSource _cancellationTokenSource = new();
        protected BaseSocket Socket => socket;

        public abstract void HandlePacket(int opcode, byte[] payload);
        public abstract void SendPacket(ServerPacket packet);
    }
}

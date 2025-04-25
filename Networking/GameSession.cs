using Packets;
using Packets.GamePackets;
using Packets.Opcodes;
using Shared.Enums;

namespace Networking
{
    /// <summary>
    /// A special derived base socket which serves as base class for Realms and Worlds.
    /// This class performs additional packet de/encryption which is required for game connections.
    /// </summary>
    public abstract class GameSession(BaseSocket socket) : BaseSession(socket)
    {
        public override void HandlePacket(int opcode, byte[] payload)
        {
            Console.WriteLine($"[{GetType().Name}] Called packet handler for opcode: {(ClientOpcode)opcode}\n");
            CallPacketHandler((ClientOpcode)opcode, payload);
        }

        public override void SendPacket(ServerPacket packet)
        {
            try
            {
                Task.Run(() => Socket.SendPacketAsync(packet), _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException) { }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// This method contains the packet handlers which Realm and World both share. Overrides should implement the server specific packets
        /// </summary>
        protected virtual void CallPacketHandler(ClientOpcode opcode, byte[] payload)
        {
            switch (opcode)
            {
                case ClientOpcode.CMSG_LOG_DISCONNECT: HandleLogDisconnect(payload); break;
                default:
                    break;
            }
        }

        #region Packet Handlers

        public void HandleLogDisconnect(ClientLogDisconnect logDisconnect)
        {
            Console.WriteLine($"[{GetType().Name}] Client disconnected for reason: {(LogDisconnectReason)logDisconnect.Reason}");
        }

        #endregion
    }
}
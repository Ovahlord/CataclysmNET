using Packets;
using Packets.GamePackets;
using Packets.Opcodes;
using Shared.Enums;
using System.Security.Cryptography;
using System.Numerics;
using System.Buffers.Binary;

namespace Networking
{
    /// <summary>
    /// A special derived base socket which serves as base class for Realms and Worlds.
    /// This class performs additional packet de/encryption which is required for game connections.
    /// </summary>
    public abstract class GameSession(BaseSocket socket) : BaseSession(socket)
    {
        private readonly BigInteger _encryptSeed = new(RandomNumberGenerator.GetBytes(16), true);
        private readonly BigInteger _decryptSeed = new(RandomNumberGenerator.GetBytes(16), true);
        private readonly uint _authSeed = BinaryPrimitives.ReadUInt32LittleEndian(RandomNumberGenerator.GetBytes(4));

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

        private void HandleLogDisconnect(ClientLogDisconnect logDisconnect)
        {
            Console.WriteLine($"[{GetType().Name}] Client disconnected for reason: {(LogDisconnectReason)logDisconnect.Reason}");
        }

        #endregion

        #region Packet sending methods

        public void SendAuthChallenge()
        {
            ServerAuthChallenge packet = new()
            {
                Challenge = _authSeed,
                DosZeroBits = 1
            };

            for (int i = 0; i < 4; ++i)
            {
                packet.DosChallenge[i] = BitConverter.ToUInt32(_encryptSeed.ToByteArray(true), i * 4);
                packet.DosChallenge[i + 4] = BitConverter.ToUInt32(_decryptSeed.ToByteArray(true), i * 4);
            }

            SendPacket(packet);
        }

        #endregion
    }
}

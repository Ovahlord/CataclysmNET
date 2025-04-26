using Packets;
using Packets.GamePackets;
using Shared.Enums;
using System.Security.Cryptography;
using System.Numerics;
using System.Buffers.Binary;
using Database.LoginDatabase;
using Database.LoginDatabase.Tables;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Digests;
using System.Text;
using Core.Packets.Opcodes;
using Org.BouncyCastle.Bcpg;

namespace Core.Networking
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
        private GameAccounts? _gameAccount = null;

        public override void HandlePacket(int opcode, byte[] payload)
        {
            Console.WriteLine($"[{GetType().Name}] Called packet handler for opcode: {(ClientOpcode)opcode}\n");
            CallPacketHandler((ClientOpcode)opcode, payload);
        }

        /// <summary>
        /// This method contains the packet handlers which Realm and World both share. Overrides should implement the server specific packets
        /// </summary>
        protected virtual void CallPacketHandler(ClientOpcode opcode, byte[] payload)
        {
            switch (opcode)
            {
                case ClientOpcode.CMSG_LOG_DISCONNECT: HandleLogDisconnect(payload); break;
                case ClientOpcode.CMSG_AUTH_SESSION: HandleAuthSession(payload); break;
                default:
                    break;
            }
        }

        #region Packet Handlers

        private void HandleLogDisconnect(ClientLogDisconnect logDisconnect)
        {
            Console.WriteLine($"[{GetType().Name}] Client disconnected for reason: {(LogDisconnectReason)logDisconnect.Reason}");
        }

        private void HandleAuthSession(ClientAuthSession authSession)
        {
            Task.Run(async () =>
            {
                using LoginDatabaseContext loginDatabase = new();
                _gameAccount = await loginDatabase.GameAccounts.FirstOrDefaultAsync(ga => ga.Login == authSession.Account);

                // Hacking attempt - no account exists
                if (_gameAccount == null)
                {
                    SendAuthResponseError(ResponseCodes.AUTH_UNKNOWN_ACCOUNT);
                    //Socket.Close();
                    return;
                }

                if (Socket is GameSocket socket)
                    socket.InitializePacketCrypt(_gameAccount.SessionKey);

                // Check that Key and account name are the same on client and server
                uint t = 0;
                byte[] accountBytes = Encoding.UTF8.GetBytes(authSession.Account);

                Sha1Digest sha = new();
                sha.BlockUpdate(accountBytes, 0, accountBytes.Length);
                sha.BlockUpdate(BitConverter.GetBytes(t), 0, 4);
                sha.BlockUpdate(BitConverter.GetBytes(authSession.LocalChallenge), 0, 4);
                sha.BlockUpdate(BitConverter.GetBytes(_authSeed), 0, 4);
                sha.BlockUpdate(_gameAccount.SessionKey, 0, _gameAccount.SessionKey.Length);

                byte[] hash = new byte[sha.GetDigestSize()];
                sha.DoFinal(hash, 0);

                // Make sure that the auth seed of the server and client match
                if (!hash.SequenceEqual(authSession.Digest))
                {
                    Console.WriteLine("[{GetType().Name}] hash verification failed");
                    SendAuthResponseError(ResponseCodes.AUTH_FAILED);
                    //Socket.Close();
                    return;
                }

                // All checks have passed. Send the response and await new packets
                SendAuthResponseSuccess();

            }, _cancellationTokenSource.Token);
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

        private void SendAuthResponseError(ResponseCodes error)
        {
            ServerAuthResponse packet = new((byte)error);
            SendPacket(packet);
        }

        private void SendAuthResponseSuccess()
        {
            if (_gameAccount == null)
                return;

            ServerAuthResponse packet = new((byte)ResponseCodes.AUTH_OK)
            {
                SuccessInfo = new(_gameAccount.ExpansionLevel, _gameAccount.ExpansionLevel)
            };

            SendPacket(packet);
        }

        #endregion
    }
}

using Core.Packets.GamePackets;
using Core.Packets.GamePackets.Helpers;
using Core.Packets.Opcodes;
using Database.LoginDatabase;
using Database.LoginDatabase.Tables;
using Microsoft.EntityFrameworkCore;
using Packets.GamePackets;
using Shared.Enums;
using System.Buffers.Binary;
using System.Net;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

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
        private readonly byte[] _authSeed = RandomNumberGenerator.GetBytes(4);
        protected GameAccounts? _gameAccount = null;

        public int SessionId { get; private set; } = GameSessionManager.SessionId;

        public override void HandlePacket(int opcode, byte[] payload)
        {
            Console.WriteLine($"[{GetType().Name}] (Id: {SessionId}) Called packet handler for opcode: {(ClientOpcode)opcode} (Size: {payload.Length})");
            CallPacketHandler((ClientOpcode)opcode, payload);
        }

        /// <summary>
        /// This method contains the packet handlers which Realm and World both share. Overrides should implement the server specific packets
        /// </summary>
        protected virtual void CallPacketHandler(ClientOpcode opcode, byte[] payload)
        {
            switch (opcode)
            {
                case ClientOpcode.CMSG_LOG_DISCONNECT:          HandleLogDisconnect(payload); break;
                case ClientOpcode.CMSG_AUTH_SESSION:            HandleAuthSession(payload); break;
                case ClientOpcode.CMSG_AUTH_CONTINUED_SESSION:  HandleAuthContinuedSession(payload); break;
                case ClientOpcode.CMSG_SUSPEND_COMMS_ACK:       HandleClientSuspendCommsAck(payload); break;
                case ClientOpcode.CMSG_PING:                    HandleClientPing(payload); break;
                default:
                    break;
            }
        }

        public override void Close()
        {
            if (_cancellationTokenSource.IsCancellationRequested)
                return;

            if (_gameAccount != null)
                GameSessionManager.SetActiveSession(_gameAccount.Id, null);

            base.Close();
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
                    //Close();
                    return;
                }

                GameSessionManager.SetActiveSession(_gameAccount.Id, this);

                if (Socket is GameSocket socket)
                    socket.InitializePacketCrypt(_gameAccount.SessionKey);

                // Check that Key and account name are the same on client and server
                uint t = 0;
                byte[] accountBytes = Encoding.UTF8.GetBytes(authSession.Account);

                using SHA1 sha = SHA1.Create();
                sha.TransformBlock(accountBytes, 0, accountBytes.Length, null, 0);
                sha.TransformBlock(BitConverter.GetBytes(t), 0, 4, null, 0);
                sha.TransformBlock(BitConverter.GetBytes(authSession.LocalChallenge), 0, 4, null, 0);
                sha.TransformBlock(_authSeed, 0, _authSeed.Length, null, 0);
                sha.TransformFinalBlock(_gameAccount.SessionKey, 0, _gameAccount.SessionKey.Length);

                byte[]? hash = sha.Hash;
                if (hash == null)
                {
                    SendAuthResponseError(ResponseCodes.AUTH_REJECT);
                    return;
                }

                // Make sure that the auth seed of the server and client match
                if (!hash.SequenceEqual(authSession.Digest))
                {
                    Console.WriteLine($"[{GetType().Name}] hash verification failed");
                    SendAuthResponseError(ResponseCodes.AUTH_FAILED);
                    //Close();
                    return;
                }

                Console.WriteLine($"[{GetType().Name}] HandleAuthSession successfully authenticated");

                // All checks have passed. Send the response and await new packets
                SendAuthResponseSuccess();

                // And finally have the client connect to the actual active socket while this one serves as fallback
                SendConnectTo();

            }, _cancellationTokenSource.Token);
        }

        private void HandleAuthContinuedSession(ClientAuthContinuedSession clientAuthContinuedSession)
        {
            ConnectToKey connectToKey = new()
            {
                Raw = clientAuthContinuedSession.Key
            };

            Task.Run(async () =>
            {
                using LoginDatabaseContext loginDatabase = new();
                _gameAccount = await loginDatabase.GameAccounts.FirstOrDefaultAsync(ga => ga.Id == connectToKey.AccountId);
                if (_gameAccount == null)
                {
                    Close();
                    return;
                }

                if (Socket is GameSocket socket)
                    socket.InitializePacketCrypt(_gameAccount.SessionKey, _encryptSeed.ToByteArray(true), _decryptSeed.ToByteArray(true));

                byte[] login = Encoding.UTF8.GetBytes(_gameAccount.Login.ToUpper());

                using SHA1 sha = SHA1.Create();
                sha.TransformBlock(login, 0, login.Length, null, 0);
                sha.TransformBlock(_gameAccount.SessionKey, 0, _gameAccount.SessionKey.Length, null, 0);
                sha.TransformFinalBlock(_authSeed, 0, _authSeed.Length);

                byte[]? hash = sha.Hash;
                // Make sure that the auth seed of the server and client match
                if (hash == null || !hash.SequenceEqual(clientAuthContinuedSession.Digest))
                {
                    Console.WriteLine($"[{GetType().Name}] hash verification failed");
                    SendAuthResponseError(ResponseCodes.AUTH_FAILED);
                    //Close();
                    return;
                }

                Console.WriteLine($"[{GetType().Name}] HandleAuthContinuedSession successfully authenticated");

                GameSessionManager.InitiateSessionJump(_gameAccount.Id, this);
            });
        }

        private void HandleClientSuspendCommsAck(ClientSuspendCommsAck clientSuspendCommsAck)
        {
            if (_gameAccount == null)
                return;

            Console.WriteLine($"ClientSuspendCommsAck Serial: {clientSuspendCommsAck.Serial}");
            GameSessionManager.FinalizeSessionJump(_gameAccount.Id);
        }

        private void HandleClientPing(ClientPing clientPing)
        {
            SendPacket(new ServerPong(clientPing.Serial));
        }

        #endregion

        #region Packet sending methods

        public void SendAuthChallenge()
        {
            ServerAuthChallenge packet = new()
            {
                Challenge = BinaryPrimitives.ReadUInt32LittleEndian(_authSeed),
                DosZeroBits = 1
            };

            for (int i = 0; i < 4; ++i)
            {
                packet.DosChallenge[i] = BitConverter.ToUInt32(_encryptSeed.ToByteArray(true), i * 4);
                packet.DosChallenge[i + 4] = BitConverter.ToUInt32(_decryptSeed.ToByteArray(true), i * 4);
            }

            SendPacket(packet);
        }

        public void SendSuspendComms()
        {
            ServerSuspendComms packet = new(20);
            SendPacket(packet);
        }

        public void SendResumeComms()
        {
            ServerResumeComms packet = new();
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

        private void SendConnectTo()
        {
            if (_gameAccount == null)
                return;

            ConnectToKey key = new()
            {
                AccountId = (uint)_gameAccount.Id,
                ConnectionType = 0,
                Key = (uint)RandomNumberGenerator.GetInt32(0x7FFFFFFF)
            };

            ServerConnectTo packet = new()
            {
                Serial = 14,
                Payload = new()
                {
                    Where = IPEndPoint.Parse("127.0.0.1:140")
                },
                Key = key.Raw
            };

            SendPacket(packet);
        }

        #endregion
    }
}

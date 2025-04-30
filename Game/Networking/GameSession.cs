using Core.Enums;
using Core.Networking;
using Core.Packets.Opcodes;
using Database.LoginDatabase;
using Database.LoginDatabase.Tables;
using Game.Packets;
using Game.Packets.Helpers;
using Microsoft.EntityFrameworkCore;
using Shared.Enums;
using System.Buffers.Binary;
using System.Net;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace Game.Networking
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
        protected int _sessionIndex = 0;

        public override void HandlePacket(int opcode, byte[] payload)
        {
            Console.WriteLine($"[{GetType().Name}] Called packet handler for opcode: {(ClientOpcode)opcode} (Size: {payload.Length})");
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
                case ClientOpcode.CMSG_AUTH_SESSION:            _ = HandleAuthSession(payload); break;
                case ClientOpcode.CMSG_AUTH_CONTINUED_SESSION:  _ = HandleAuthContinuedSession(payload); break;
                case ClientOpcode.CMSG_SUSPEND_COMMS_ACK:       HandleClientSuspendCommsAck(payload); break;
                case ClientOpcode.CMSG_PING:                    _ = HandleClientPing(payload); break;
                default:
                    break;
            }
        }

        public override void Close()
        {
            if (_cancellationTokenSource.IsCancellationRequested)
                return;

            base.Close();
        }

        #region Packet Handlers

        private void HandleLogDisconnect(ClientLogDisconnect logDisconnect)
        {
            Console.WriteLine($"[{GetType().Name}] Client disconnected for reason: {(LogDisconnectReason)logDisconnect.Reason}");
        }

        private async Task HandleAuthSession(ClientAuthSession authSession)
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
        }

        private async Task HandleAuthContinuedSession(ClientAuthContinuedSession clientAuthContinuedSession)
        {
            ConnectToKey connectToKey = new(clientAuthContinuedSession.Key);

            // Try to retrieve the game account of the user from the key
            using LoginDatabaseContext loginDatabase = new();
            _gameAccount = await loginDatabase.GameAccounts.FirstOrDefaultAsync(ga => ga.Id == connectToKey.AccountId);
            if (_gameAccount == null)
            {
                Close();
                return;
            }

            _sessionIndex = 1;

            // Mark our current session as target session as we prepare to transition over to it
            GameSessionManager.SetTargetSession(_gameAccount.Id, this);

            // Initialize packet header encryption
            if (Socket is GameSocket socket)
                socket.InitializePacketCrypt(_gameAccount.SessionKey, _encryptSeed.ToByteArray(true), _decryptSeed.ToByteArray(true));

            // Validate the hash that we have received from the client
            byte[] login = Encoding.UTF8.GetBytes(_gameAccount.Login.ToUpper());

            using SHA1 sha = SHA1.Create();
            sha.TransformBlock(login, 0, login.Length, null, 0);
            sha.TransformBlock(_gameAccount.SessionKey, 0, _gameAccount.SessionKey.Length, null, 0);
            sha.TransformFinalBlock(_authSeed, 0, _authSeed.Length);

            byte[]? hash = sha.Hash;
            if (hash == null || !hash.SequenceEqual(clientAuthContinuedSession.Digest))
            {
                Console.WriteLine($"[{GetType().Name}] hash verification failed");
                SendAuthResponseError(ResponseCodes.AUTH_FAILED);
                //Close();
                return;
            }

            Console.WriteLine($"[{GetType().Name}] HandleAuthContinuedSession successfully authenticated");

            // We have passed the authentication, inform the currently active session to drop its communication
            GameSessionManager.GetActiveSession(_gameAccount.Id)?.SendSuspendComms();
        }

        private void HandleClientSuspendCommsAck(ClientSuspendCommsAck clientSuspendCommsAck)
        {
            if (_gameAccount == null)
                return;

            Console.WriteLine($"ClientSuspendCommsAck Serial: {clientSuspendCommsAck.Serial}");

            GameSession? targetSession = GameSessionManager.GetTargetSession(_gameAccount.Id);
            if (targetSession == null)
                return;

            GameSessionManager.SetActiveSession(_gameAccount.Id, targetSession);
            targetSession.SendResumeComms();
        }

        private async Task HandleClientPing(ClientPing clientPing)
        {
            await SendPacketAsync(new ServerPong(clientPing.Serial));
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

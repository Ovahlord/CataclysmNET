﻿using Core.Enums;
using Core.Networking;
using Core.Packets;
using Core.Packets.Opcodes;
using Database.LoginDatabase;
using Database.LoginDatabase.Tables;
using Game.Enums;
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
        private readonly byte[] _encryptSeed = RandomNumberGenerator.GetBytes(16);
        private readonly byte[] _decryptSeed = RandomNumberGenerator.GetBytes(16);
        private readonly byte[] _authSeed = RandomNumberGenerator.GetBytes(4);
        protected GameAccounts? _gameAccount = null;

        public override Task? HandlePacket(int opcode, byte[] payload)
        {
            Console.WriteLine($"[{GetType().Name}] Received {(ClientOpcode)opcode} (Size: {payload.Length})");

            switch ((ClientOpcode)opcode)
            {
                case ClientOpcode.CMSG_LOG_DISCONNECT:          return HandleLogDisconnect(payload);
                case ClientOpcode.CMSG_AUTH_SESSION:            return HandleAuthSession(payload);
                case ClientOpcode.CMSG_AUTH_CONTINUED_SESSION:  return HandleAuthContinuedSession(payload);
                case ClientOpcode.CMSG_SUSPEND_COMMS_ACK:       return HandleSuspendCommsAck(payload);
                case ClientOpcode.CMSG_PING:                    return HandlePing(payload);
                default:
                    return null;
            }
        }

        /// <summary>
        /// This method is being invoked when the client has successfully authenticated in CMSG_AUTH_SESSION and CMSG_AUTH_CONTINUED_SESSION
        /// </summary>
        protected virtual void OnSessionAuthenticated(bool requestedByServer)
        {
            if (_gameAccount == null)
                return;

            if (requestedByServer)
            {
                GameSession? activeSession = GameSessionManager.GetActiveSession(_gameAccount.Id);

                // If we already have an active game session while connecting, we switch over by starting with a suspend comms packet
                if (activeSession != null)
                    GameSessionManager.GetActiveSession(_gameAccount.Id)?.DelayedSuspendComms();
                else // otherwise, if this is our very first connection (only possible for world connections) we allow a direct activation for the first time
                    GameSessionManager.SetActiveSession(_gameAccount.Id, this);
            }
        }

        public override void SendSuspendComms()
        {
            SendPacket(new ServerSuspendComms(20));
        }

        #region Packet Handlers

        private Task HandleLogDisconnect(ClientLogDisconnect logDisconnect)
        {
            Console.WriteLine($"[{GetType().Name}] Client disconnected for reason: {(LogDisconnectReason)logDisconnect.Reason}");
            return Task.CompletedTask;
        }

        private async Task HandleAuthSession(ClientAuthSession authSession)
        {
            using LoginDatabaseContext loginDatabase = new();
            _gameAccount = await loginDatabase.GameAccounts.FirstOrDefaultAsync(ga => ga.Login == authSession.Account);

            // Hacking attempt - no account exists
            if (_gameAccount == null)
            {
                SendAuthResponseError(ResponseCodes.AUTH_UNKNOWN_ACCOUNT);
                DelayedClose();
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
                DelayedClose();
                return;
            }

            // Make sure that the auth seed of the server and client match
            if (!hash.SequenceEqual(authSession.Digest))
            {
                Console.WriteLine($"[{GetType().Name}] hash verification failed");
                SendAuthResponseError(ResponseCodes.AUTH_FAILED);
                DelayedClose();
                return;
            }

            Console.WriteLine($"[{GetType().Name}] HandleAuthSession successfully authenticated");

            // All checks have passed. Send the response and await new packets
            SendAuthResponseSuccess();

            // Invoke a virtual method that allows realm and world sessions to continue with the login process
            OnSessionAuthenticated(false);
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

            // Mark our current session as target session as we prepare to transition over to it
            GameSessionManager.SetTargetSession(_gameAccount.Id, this);

            // Initialize packet header encryption
            if (Socket is GameSocket socket)
                socket.InitializePacketCrypt(_gameAccount.SessionKey, _encryptSeed, _decryptSeed);

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
                DelayedClose();
                return;
            }

            Console.WriteLine($"[{GetType().Name}] HandleAuthContinuedSession successfully authenticated");

            OnSessionAuthenticated(true);
        }

        private Task HandleSuspendCommsAck(ClientSuspendCommsAck clientSuspendCommsAck)
        {
            if (_gameAccount == null)
                return Task.CompletedTask;

            // The client has noted our suspend comms request. At this point we now have to continue on the new socket.

            GameSession? targetSession = GameSessionManager.GetTargetSession(_gameAccount.Id);
            if (targetSession == null)
                return Task.CompletedTask;

            GameSessionManager.SetActiveSession(_gameAccount.Id, targetSession);
            targetSession.SendResumeComms();
            return Task.CompletedTask;
        }

        private Task HandlePing(ClientPing clientPing)
        {
            SendPacket(new ServerPong(clientPing.Serial));
            return Task.CompletedTask;
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
                packet.DosChallenge[i] = BitConverter.ToUInt32(_encryptSeed, i * 4);
                packet.DosChallenge[i + 4] = BitConverter.ToUInt32(_decryptSeed, i * 4);
            }

            SendPacket(packet);
        }

        public void SendResumeComms()
        {
            SendPacket(new ServerResumeComms());
        }

        private void SendAuthResponseError(ResponseCodes error)
        {
            SendPacket(new ServerAuthResponse((byte)error));
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

        protected void SendConnectTo(IPEndPoint where, ConnectToConnectionType connectionType)
        {
            if (_gameAccount == null)
                return;

            ConnectToKey key = new()
            {
                AccountId = (uint)_gameAccount.Id,
                ConnectionType = (byte)connectionType,
                Key = (uint)RandomNumberGenerator.GetInt32(0x7FFFFFFF)
            };

            ServerConnectTo packet = new()
            {
                Serial = 14,
                Payload = new(where),
                Key = key.Raw
            };

            SendPacket(packet);
        }

        #endregion
    }
}

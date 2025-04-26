using Database.LoginDatabase;
using Database.LoginDatabase.Tables;
using Database.RealmDatabase.Tables;
using LoginServer.Enums;
using Microsoft.EntityFrameworkCore;
using Packets;
using Packets.LoginPackets;
using Shared.Enums;
using System.Text;
using Core.Networking;
using Core.Cryptography;
using Core.Packets.Opcodes;

namespace LoginServer
{
    public sealed class LoginSession(BaseSocket socket) : BaseSession(socket)
    {
        private LoginState _loginState = LoginState.None;
        private SRP6? _srp6 = null;
        private GameAccounts? _gameAccount = null;
        private readonly static byte[] _versionChallenge = [0xBA, 0xA3, 0x1E, 0x99, 0xA0, 0x0B, 0x21, 0x57, 0xFC, 0x37, 0x3F, 0xB3, 0x69, 0xCD, 0xD2, 0xF1];

        public override void HandlePacket(int opcode, byte[] payload)
        {
            Console.WriteLine($"[{GetType().Name}] Called packet handler for opcode: {(LoginOpcode)opcode}\n");
            CallPacketHandler((LoginOpcode)opcode, payload);
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

        private void CallPacketHandler(LoginOpcode opcode, byte[] payload)
        {
            switch (opcode)
            {
                case LoginOpcode.AuthLogonChallenge:    HandleAuthLogonChallenge(payload); break;
                case LoginOpcode.AuthLogonProof:        HandleAuthLogonProof(payload); break;
                case LoginOpcode.RealmList:             HandleRealmList(payload); break;
                default:
                    break;
            }
        }

        private void SendLoginFailed(LoginOpcode cmd, LoginResult result)
        {
            switch (cmd)
            {
                case LoginOpcode.AuthLogonChallenge:
                {
                    ServerAuthLogonChallenge packet = new()
                    {
                        Error = (byte)result
                    };
                    SendPacket(packet);
                    break;
                }
                case LoginOpcode.AuthLogonProof:
                {
                    ServerAuthLogonProof packet = new()
                    {
                        Error = (byte)result
                    };
                    SendPacket(packet);
                    break;
                }
                default:
                    break;
            }
        }

        #region Packet Handlers
        private void HandleAuthLogonChallenge(ClientAuthLogonChallenge logonChallenge)
        {
            // We only allow one login attempt per session. The client disconnects after failed attempts so when this packet comes again, we know it's a cheater.
            if (_loginState != LoginState.None)
                return;

            _loginState = LoginState.Challenge;

            Task.Run(async () =>
            {
                Array.Reverse(logonChallenge.GameName);
                Array.Reverse(logonChallenge.OS);
                Array.Reverse(logonChallenge.Platform);
                Array.Reverse(logonChallenge.Locale);

                string gameName = Encoding.UTF8.GetString(logonChallenge.GameName);
                string os = Encoding.UTF8.GetString(logonChallenge.OS);
                string platform = Encoding.UTF8.GetString(logonChallenge.Platform);
                string locale = Encoding.UTF8.GetString(logonChallenge.Locale);

                Console.WriteLine($"[{GetType().Name}] |========== ClientAuthLogonChallenge =============");
                Console.WriteLine($"[{GetType().Name}] | Login: {logonChallenge.Login}");
                Console.WriteLine($"[{GetType().Name}] | Game Name: {gameName}");
                Console.WriteLine($"[{GetType().Name}] | OS: {os}");
                Console.WriteLine($"[{GetType().Name}] | Platform: {platform}");
                Console.WriteLine($"[{GetType().Name}] | Locale: {locale}");
                Console.WriteLine($"[{GetType().Name}] | Version: {logonChallenge.MajorVersion}.{logonChallenge.MinorVersion}.{logonChallenge.BugfixVersion}.{logonChallenge.Build}");
                Console.WriteLine($"[{GetType().Name}] |=================================================");

                using LoginDatabaseContext loginDatabase = new();
                _gameAccount = await loginDatabase.GameAccounts.FirstOrDefaultAsync(ga => ga.Login.Equals(logonChallenge.Login));

                // No game account with the provided login name exists
                if (_gameAccount == null)
                {
                    /*
                    SendLoginFailed(LoginOpcode.AuthLogonChallenge, LoginResult.WowFailUnknownAccount);
                    return;
                    */

                    // For now we create a new account for that login to allow testing. This should be removed once we have a registration service.
                    (byte[] salt, byte[] verifier) = SRP6.GenerateRegistrationData(logonChallenge.Login, "test");

                    _gameAccount = new()
                    {
                        Login = logonChallenge.Login,
                        Salt = salt,
                        Verifier = verifier,
                        ExpansionLevel = 3
                    };

                    await loginDatabase.AddAsync(_gameAccount);
                    await loginDatabase.SaveChangesAsync();

                    Console.WriteLine($"[{GetType().Name}] No game account for user {logonChallenge.Login} existed. Created a new account with password 'test' for it.");
                }

                _srp6 = new(logonChallenge.Login, _gameAccount.Salt, _gameAccount.Verifier);

                // Game account has been found. Now we will send the challenge back to the client with our SRP6 data to encrypt the password.
                ServerAuthLogonChallenge packet = new()
                {
                    Error = (byte)LoginResult.WowSuccess,
                    Srp6Data = new(_srp6.B, SRP6.g, SRP6.N, _srp6.s, _versionChallenge)
                };

                SendPacket(packet);
            }, _cancellationTokenSource.Token);
        }

        private void HandleAuthLogonProof(ClientAuthLogonProof logonProof)
        {
            if (_loginState != LoginState.Challenge)
                return;

            _loginState = LoginState.Proof;

            Task.Run(async () =>
            {
                if (_srp6 == null || _gameAccount == null)
                {
                    SendLoginFailed(LoginOpcode.AuthLogonProof, LoginResult.WowFailFailNoaccess);
                    return;
                }

                if (!_srp6.VerifyChallengeResponse(logonProof.A, logonProof.ClientM, out byte[]? sessionKey) || sessionKey == null)
                {
                    SendLoginFailed(LoginOpcode.AuthLogonProof, LoginResult.WowFailIncorrectPassword);
                    return;
                }

                using LoginDatabaseContext loginDatabase = new();
                _gameAccount = await loginDatabase.GameAccounts.FirstOrDefaultAsync(ga => ga.Login.Equals(_gameAccount.Login));

                if (_gameAccount == null)
                {
                    SendLoginFailed(LoginOpcode.AuthLogonProof, LoginResult.WowFailFailNoaccess);
                    return;
                }

                _gameAccount.SessionKey = sessionKey;
                await loginDatabase.SaveChangesAsync();

                ServerAuthLogonProof packet = new()
                {
                    M2 = SRP6.GetSessionVerifier(logonProof.A, logonProof.ClientM, _gameAccount.SessionKey),
                    Error = 0,
                    AccountFlags = 0,
                    SurveyId = 0,
                    LoginFlags = 0
                };

                SendPacket(packet);
            }, _cancellationTokenSource.Token);
        }

        public void HandleRealmList(ClientRealmList realmList)
        {
            // realmList comes with 32 bits of unk data. We ignore it for now
            ServerRealmList packet = new();
            foreach (Realms realm in RealmsStatusManager.RealmCache)
            {
                packet.RealmList.Add(new()
                {
                    Id = (byte)realm.Id,
                    Name = realm.Name,
                    Flags = (RealmFlags)realm.Flags,
                    RealmType = realm.RealmType,
                    TimeZone = realm.TimeZone,
                    RealmServerAddress = realm.Address
                });
            }

            SendPacket(packet);
        }

        #endregion
    }
}

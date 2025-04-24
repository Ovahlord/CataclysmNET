using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;
using Cryptography;

namespace Packets
{
    public class PacketCrypt
    {
        public bool Initialized { get; private set; } = false;

        private readonly ARC4 _clientDecrypt = new();
        private readonly ARC4 _serverEncrypt = new();

        public void Init(byte[] K)
        {
            Console.WriteLine("[PacketCrypt] Initializing packet crypt");

            byte[] serverEncryptionKey = [0xCC, 0x98, 0xAE, 0x04, 0xE8, 0x97, 0xEA, 0xCA, 0x12, 0xDD, 0xC0, 0x93, 0x42, 0x91, 0x53, 0x57];
            byte[] serverDecryptionKey = [0xC2, 0xB3, 0x72, 0x3C, 0xC6, 0xAE, 0xD9, 0xB5, 0x34, 0x3C, 0x53, 0xEE, 0x2F, 0x43, 0x67, 0xCE];

            _serverEncrypt.Init(GetHMACSHA1Digest(serverEncryptionKey, K));
            _clientDecrypt.Init(GetHMACSHA1Digest(serverDecryptionKey, K));

            // Drop first 1024 bytes, as WoW uses ARC4-drop1024.
            byte[] syncBuf = new byte[1024];
            _serverEncrypt.UpdateData(syncBuf);
            _clientDecrypt.UpdateData(syncBuf);

            Initialized = true;
        }

        private byte[] GetHMACSHA1Digest(byte[] key, byte[] data)
        {
            HMac hmac = new(new Sha1Digest());
            hmac.Init(new KeyParameter(key));
            hmac.BlockUpdate(data, 0, data.Length);
            byte[] result = new byte[hmac.GetMacSize()];
            hmac.DoFinal(result, 0);
            return result;
        }

        public void DecryptRecv(byte[] data)
        {
            _clientDecrypt.UpdateData(data);
        }

        public void EncryptSend(byte[] data)
        {
            _serverEncrypt.UpdateData(data);
        }
    }
}

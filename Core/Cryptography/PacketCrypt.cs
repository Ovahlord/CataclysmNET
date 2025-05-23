﻿using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;

namespace Core.Cryptography
{
    public class PacketCrypt
    {
        private static readonly byte[] authSessionEncryptionKey = [0xCC, 0x98, 0xAE, 0x04, 0xE8, 0x97, 0xEA, 0xCA, 0x12, 0xDD, 0xC0, 0x93, 0x42, 0x91, 0x53, 0x57];
        private static readonly byte[] authSessionDecryptionKey = [0xC2, 0xB3, 0x72, 0x3C, 0xC6, 0xAE, 0xD9, 0xB5, 0x34, 0x3C, 0x53, 0xEE, 0x2F, 0x43, 0x67, 0xCE];

        public PacketCrypt(byte[] sessionKey)
        {
            Init(sessionKey, authSessionEncryptionKey, authSessionDecryptionKey);
        }

        public PacketCrypt(byte[] sessionKey, byte[] encryptionKey, byte[] decryptionKey)
        {
            Init(sessionKey, encryptionKey, decryptionKey);
        }

        private readonly ARC4 _clientDecrypt = new();
        private readonly ARC4 _serverEncrypt = new();

        private void Init(byte[] K, byte[] encryptionKey, byte[] decryptionKey)
        {
            _serverEncrypt.Init(GetHMACSHA1Digest(encryptionKey, K));
            _clientDecrypt.Init(GetHMACSHA1Digest(decryptionKey, K));

            // Drop first 1024 bytes, as WoW uses ARC4-drop1024.
            byte[] syncBuf = new byte[1024];
            _serverEncrypt.UpdateData(syncBuf);
            _clientDecrypt.UpdateData(syncBuf);
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

using Core.Packets.Opcodes;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;
using Packets.GamePackets.Substructures;
using System.Numerics;
using System.Text;

namespace Core.Packets.GamePackets
{
    public sealed class ServerConnectTo : ServerPacket
    {
        #region Binary Data
        private static readonly byte[] P =
        [
            0x7D, 0xBD, 0xB9, 0xE1, 0x2D, 0xAE, 0x42, 0x56, 0x6E, 0x2B, 0xE2, 0x89, 0xD9, 0xBB, 0x0C, 0x1F,
            0x67, 0x28, 0xC1, 0x4D, 0x91, 0x3C, 0xAD, 0x5F, 0xF0, 0x43, 0x86, 0x5C, 0x27, 0xDC, 0x58, 0xB3,
            0x0E, 0x75, 0x77, 0x78, 0x49, 0x35, 0xE7, 0xE7, 0xDF, 0xFD, 0x74, 0xAB, 0x4E, 0xFE, 0xD3, 0xAB,
            0x6B, 0x96, 0xF7, 0x89, 0xB2, 0x5A, 0x6A, 0x25, 0x03, 0x5A, 0x92, 0x1A, 0xF1, 0xFC, 0x05, 0x4E,
            0xCE, 0xDD, 0x37, 0xA4, 0x02, 0x53, 0x76, 0xCB, 0xC2, 0xD9, 0x63, 0xCB, 0x51, 0x94, 0xEC, 0x5C,
            0x39, 0xCC, 0xB2, 0x17, 0x0C, 0xA3, 0x43, 0x9A, 0xD0, 0x83, 0x27, 0x67, 0x52, 0x64, 0x37, 0x0E,
            0x38, 0xB7, 0x9B, 0xF4, 0x2D, 0xB8, 0x0F, 0x30, 0x72, 0xD3, 0x15, 0xF3, 0x2C, 0x39, 0x55, 0x72,
            0x2C, 0x55, 0x80, 0x63, 0xA0, 0xA1, 0x6F, 0x28, 0xF3, 0xF3, 0x5A, 0x6F, 0x68, 0x59, 0xB3, 0xF3
        ];

        private static readonly byte[] Q =
        [
            0x0B, 0x1A, 0x13, 0x07, 0x12, 0xEF, 0xDD, 0x97, 0x01, 0x9A, 0x21, 0x7D, 0xFA, 0xA3, 0xB7, 0xE2,
            0x39, 0x2E, 0x04, 0x92, 0x96, 0x45, 0x2A, 0xEB, 0x57, 0x03, 0xAC, 0xB1, 0x83, 0xCD, 0x25, 0x4F,
            0x2C, 0xA9, 0xA1, 0x54, 0x26, 0x54, 0xCF, 0xE6, 0x1B, 0x53, 0x51, 0x3A, 0xC1, 0x15, 0xF4, 0x17,
            0xBB, 0x17, 0x1F, 0x37, 0x66, 0x36, 0x1A, 0xD4, 0xB1, 0x5B, 0x49, 0xA8, 0xF1, 0x02, 0xB0, 0x42,
            0xA9, 0x66, 0xA0, 0xE2, 0x52, 0x2C, 0x8C, 0x89, 0xA2, 0xDD, 0xA6, 0xF1, 0xA3, 0xDF, 0xB6, 0x80,
            0x63, 0xB8, 0x10, 0xDA, 0xDE, 0x84, 0x56, 0xFA, 0xFB, 0x72, 0x65, 0x5E, 0xA3, 0x9C, 0x78, 0x65,
            0xD0, 0x73, 0x07, 0x34, 0x1D, 0xE1, 0x4D, 0x77, 0xE8, 0x00, 0x0F, 0x80, 0x1C, 0x5A, 0x21, 0x55,
            0x0A, 0x8C, 0xF4, 0x93, 0xF5, 0xF8, 0x40, 0xF2, 0x40, 0xEA, 0x52, 0x12, 0x40, 0xF0, 0xBF, 0xFA
        ];

        private static readonly byte[] DP =
        [
            0xE1, 0xA6, 0x22, 0xAB, 0xFF, 0x57, 0x83, 0x45, 0x3F, 0x93, 0x76, 0xC8, 0xFA, 0xD9, 0x17, 0xE1,
            0x49, 0x73, 0xC2, 0x13, 0x28, 0x0B, 0x1F, 0xE2, 0x9A, 0xF4, 0x7F, 0x7C, 0x37, 0x56, 0xA1, 0xDF,
            0x51, 0x97, 0x2F, 0x15, 0x10, 0x97, 0xCD, 0x2A, 0x40, 0x09, 0xFC, 0x0A, 0xC3, 0x3F, 0x88, 0x86,
            0xA9, 0x51, 0x13, 0xE1, 0x76, 0xCF, 0xA8, 0x37, 0x9A, 0x91, 0x3B, 0xD0, 0x70, 0xA1, 0xD7, 0x03,
            0x71, 0x59, 0x6C, 0xB3, 0x41, 0xB8, 0x32, 0x68, 0x56, 0xC8, 0xB8, 0xD1, 0xF9, 0x1D, 0x04, 0xC5,
            0x13, 0xB5, 0x8E, 0x57, 0x73, 0x02, 0x97, 0x7B, 0x33, 0x60, 0x68, 0xA9, 0xC2, 0x40, 0x96, 0x3C,
            0x57, 0x4E, 0x4F, 0xC0, 0xAB, 0x21, 0x5C, 0xBA, 0x7D, 0x65, 0xAA, 0x1B, 0xD6, 0x43, 0x06, 0xCE,
            0x3E, 0x0C, 0xB9, 0xB2, 0x82, 0xB0, 0xC9, 0x54, 0x59, 0x32, 0xC5, 0x88, 0x08, 0x9C, 0x9B, 0xBF
        ];

        private static readonly byte[] DQ =
        [
            0xE3, 0xB1, 0xED, 0x52, 0xEF, 0xE6, 0x88, 0x40, 0x50, 0x89, 0x4C, 0x99, 0xE5, 0xF7, 0xED, 0x03,
            0x1C, 0x54, 0x11, 0x24, 0x2F, 0x9D, 0xE8, 0xE6, 0x39, 0xFA, 0x19, 0xF4, 0x06, 0x55, 0x0B, 0x8B,
            0x95, 0xC8, 0xB1, 0xE2, 0x7C, 0x75, 0x3B, 0x2A, 0x40, 0xC3, 0xE7, 0xE0, 0x25, 0x18, 0xBF, 0xB5,
            0x03, 0x1B, 0x5A, 0x57, 0x92, 0x3C, 0x85, 0x7D, 0x7F, 0x43, 0x56, 0x1F, 0x1E, 0x80, 0xC3, 0xBA,
            0xF0, 0x53, 0xD7, 0x6A, 0xD0, 0xF2, 0xDD, 0x9C, 0xC6, 0x53, 0xE7, 0xB4, 0xD3, 0x9D, 0xAB, 0xBF,
            0xE0, 0x97, 0x50, 0x92, 0x23, 0xB9, 0xB7, 0xDC, 0xAA, 0xC4, 0x20, 0x93, 0x5A, 0xF5, 0xDE, 0x76,
            0x28, 0x93, 0x91, 0x44, 0x1E, 0x4C, 0x15, 0x2F, 0x7F, 0x45, 0x3C, 0x3B, 0x7D, 0x36, 0x3B, 0x24,
            0xC7, 0x8C, 0x65, 0x43, 0xAE, 0x65, 0x84, 0xBC, 0xF9, 0x76, 0x4E, 0x3C, 0x44, 0x05, 0xBC, 0xFA
        ];

        private static readonly byte[] InverseQ =
        [
            0x63, 0xC1, 0x14, 0x2B, 0x57, 0x0B, 0x8A, 0x3C, 0x27, 0xDB, 0x96, 0x82, 0x27, 0xEB, 0xF6, 0x45,
            0x6D, 0x07, 0x50, 0xE8, 0x4A, 0xD4, 0xB6, 0x7A, 0x3C, 0x8B, 0x4D, 0x65, 0xF0, 0x50, 0x70, 0x84,
            0x71, 0x2B, 0xC6, 0x6D, 0x28, 0x2D, 0x76, 0x38, 0x73, 0x93, 0xDB, 0x44, 0xD7, 0xC0, 0x7F, 0xD9,
            0x57, 0x18, 0x28, 0x57, 0xF1, 0x13, 0x38, 0xA4, 0x91, 0x67, 0x1E, 0x13, 0x73, 0x55, 0xFC, 0x7B,
            0xAF, 0x50, 0xFA, 0xFD, 0x16, 0x12, 0x6F, 0xA4, 0x95, 0x15, 0x9C, 0x07, 0x18, 0xA6, 0x46, 0xFD,
            0xB3, 0xCF, 0xA5, 0x0E, 0x05, 0x30, 0xEC, 0x2C, 0xCD, 0x62, 0xDD, 0x6F, 0xB1, 0xFE, 0x6C, 0x05,
            0x2F, 0x11, 0xA6, 0xA0, 0x98, 0xAC, 0x9B, 0x15, 0xF0, 0x04, 0xC4, 0x7B, 0x79, 0xAA, 0x51, 0x25,
            0x2A, 0x84, 0x73, 0xE6, 0x77, 0x47, 0xA3, 0xEB, 0xCF, 0x6D, 0xC8, 0x96, 0x3A, 0x1B, 0x02, 0x52
        ];

        private static readonly byte[] WherePacketHmac =
        [
            0x2C, 0x1F, 0x1D, 0x80, 0xC3, 0x8C, 0x23, 0x64, 0xDA, 0x90, 0xCA, 0x8E, 0x2C, 0xFC, 0x0C, 0xCE,
            0x09, 0xD3, 0x62, 0xF9, 0xF3, 0x8B, 0xBE, 0x9F, 0x19, 0xEF, 0x58, 0xA1, 0x1C, 0x34, 0x14, 0x41,
            0x3F, 0x23, 0xFD, 0xD3, 0xE8, 0x14, 0xEC, 0x2A, 0xFD, 0x4F, 0x95, 0xBA, 0x30, 0x7E, 0x56, 0x5D,
            0x83, 0x95, 0x81, 0x69, 0xB0, 0x5A, 0xB4, 0x9D, 0xA8, 0x55, 0xFF, 0xFC, 0xEE, 0x58, 0x0A, 0x2F
        ];

        #endregion

        private static readonly byte[] _haiku = Encoding.UTF8.GetBytes("World torn asunder\nDarkness descends on the land\nDeathwing has returned\n\0\0");

        private static readonly byte[] _piDigits =
        [
            0x31, 0x41, 0x59, 0x26, 0x53, 0x58, 0x97, 0x93, 0x23, 0x84,
            0x62, 0x64, 0x33, 0x83, 0x27, 0x95, 0x02, 0x88, 0x41, 0x97,
            0x16, 0x93, 0x99, 0x37, 0x51, 0x05, 0x82, 0x09, 0x74, 0x94,
            0x45, 0x92, 0x30, 0x78, 0x16, 0x40, 0x62, 0x86, 0x20, 0x89,
            0x98, 0x62, 0x80, 0x34, 0x82, 0x53, 0x42, 0x11, 0x70, 0x67,
            0x98, 0x21, 0x48, 0x08, 0x65, 0x13, 0x28, 0x23, 0x06, 0x64,
            0x70, 0x93, 0x84, 0x46, 0x09, 0x55, 0x05, 0x82, 0x23, 0x17,
            0x25, 0x35, 0x94, 0x08, 0x12, 0x84, 0x81, 0x11, 0x74, 0x50,
            0x28, 0x41, 0x02, 0x70, 0x19, 0x38, 0x52, 0x11, 0x05, 0x55,
            0x96, 0x44, 0x62, 0x29, 0x48, 0x95, 0x49, 0x30, 0x38, 0x19,
            0x64, 0x42, 0x88, 0x10, 0x97, 0x56, 0x65, 0x93, 0x34, 0x46,
            0x12, 0x84, 0x75, 0x64, 0x82, 0x33, 0x78, 0x67, 0x83, 0x16,
            0x52, 0x71, 0x20, 0x19, 0x09, 0x14, 0x56, 0x48, 0x56, 0x69,
            0x23, 0x46, 0x03, 0x48, 0x61, 0x04, 0x54, 0x32, 0x66, 0x48,
            0x21, 0x33
        ];

        private static readonly BigInteger _p = new(P, true);
        private static readonly BigInteger _q = new(Q, true);
        private static readonly BigInteger _dmp1 = new(DP, true);
        private static readonly BigInteger _dmq1 = new(DQ, true);
        private static readonly BigInteger _iqmp = new(InverseQ, true);


        public ulong Key { get; set; }
        public uint Serial { get; set; }
        public required ConnectPayload Payload { get; set; }
        public byte Con { get; set; }

        public ServerConnectTo() : base(1024, (int)ServerOpcode.SMSG_CONNECT_TO)
        {
        }

        public override ServerPacket Write()
        {
            byte[] address = new byte[16];
            byte[] addressBytes = Payload.Where.Address.GetAddressBytes();
            Buffer.BlockCopy(addressBytes, 0, address, 0, addressBytes.Length);

            uint addressType = 3;
            if (Payload.Where.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                addressType = 1;
            }
            else if (Payload.Where.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
            {
                addressType = 2;
            }

            int port = Payload.Where.Port;

            HMac hmac = new(new Sha1Digest());
            hmac.Init(new KeyParameter(WherePacketHmac));

            hmac.BlockUpdate(address, 0, 16);
            hmac.BlockUpdate(BitConverter.GetBytes(addressType), 0, 4);
            hmac.BlockUpdate(BitConverter.GetBytes(Payload.Where.Port), 0, 2);
            hmac.BlockUpdate(_haiku, 0, 73);
            hmac.BlockUpdate(_piDigits, 0, _piDigits.Length);
            hmac.BlockUpdate([Payload.XorMagic], 0, 1);

            byte[] hmacResult = new byte[hmac.GetMacSize()];
            hmac.DoFinal(hmacResult, 0);

            WriteByte(_piDigits[30]);
            WriteByte(_haiku[31]);
            WriteByte(_piDigits[21]);
            WriteByte(_piDigits[26]);
            WriteByte(_haiku[3]);
            WriteByte(_piDigits[78]);
            WriteByte(_haiku[16]);
            WriteByte(_haiku[55]);
            WriteByte(_haiku[54]);
            WriteByte(_piDigits[68]);
            WriteByte(_piDigits[59]);
            WriteByte(_piDigits[115]);
            WriteByte(_haiku[65]);
            WriteByte(hmacResult[15]);
            WriteByte(_haiku[25]);
            WriteByte(_piDigits[32]);
            WriteByte(_piDigits[101]);
            WriteByte(_haiku[37]);
            WriteByte(_haiku[20]);
            WriteByte(_haiku[29]);
            WriteByte(_piDigits[88]);
            WriteByte(_haiku[9]);
            WriteByte(_haiku[63]);
            WriteByte(_piDigits[3]);
            WriteByte(_piDigits[22]);
            WriteByte(_haiku[14]);
            WriteByte(_piDigits[38]);
            WriteByte(_piDigits[46]);
            WriteByte(address[1]);
            WriteByte(_piDigits[94]);
            WriteByte(_piDigits[96]);
            WriteByte(_piDigits[137]);
            WriteByte(_haiku[42]);
            WriteByte(_haiku[21]);
            WriteByte(_piDigits[13]);
            WriteByte(hmacResult[14]);
            WriteByte(_piDigits[63]);
            WriteByte(_piDigits[16]);
            WriteByte(_haiku[5]);
            WriteByte(_piDigits[58]);
            WriteByte(_haiku[67]);
            WriteByte(_haiku[53]);
            WriteByte(_piDigits[79]);
            WriteByte(address[14]);
            WriteByte(address[9]);
            WriteByte(_piDigits[125]);
            WriteByte(_haiku[24]);
            WriteByte(_haiku[6]);
            WriteByte(_piDigits[140]);
            WriteByte(_haiku[8]);
            WriteByte(_piDigits[112]);
            WriteByte(_piDigits[133]);
            WriteByte(_piDigits[74]);
            WriteByte(_piDigits[135]);
            WriteByte(_piDigits[50]);
            WriteByte(_piDigits[14]);
            WriteByte(_piDigits[85]);
            WriteByte(hmacResult[19]);
            WriteByte(_piDigits[52]);
            WriteByte(hmacResult[17]);
            WriteByte(_piDigits[136]);
            WriteByte(_piDigits[71]);
            WriteByte(address[6]);
            WriteByte(_piDigits[87]);
            WriteByte(_piDigits[28]);
            WriteByte(_piDigits[105]);
            WriteByte(_haiku[32]);
            WriteByte(_piDigits[75]);
            WriteByte(_haiku[46]);
            WriteByte(_piDigits[5]);
            WriteByte(_piDigits[104]);
            WriteByte(_haiku[17]);
            WriteByte(_piDigits[64]);
            WriteByte(_haiku[22]);
            WriteByte(address[3]);
            WriteByte((byte)(port & 0xFF));
            WriteByte(_haiku[23]);
            WriteByte(_piDigits[0]);
            WriteByte(address[5]);
            WriteByte(_piDigits[110]);
            WriteByte(_piDigits[109]);
            WriteByte(_piDigits[93]);
            WriteByte(_haiku[10]);
            WriteByte(Payload.XorMagic);
            WriteByte(_haiku[26]);
            WriteByte(_haiku[13]);
            WriteByte(_piDigits[90]);
            WriteByte(_piDigits[72]);
            WriteByte(_piDigits[6]);
            WriteByte(_piDigits[54]);
            WriteByte(address[0]);
            WriteByte(_piDigits[23]);
            WriteByte(_piDigits[100]);
            WriteByte(_haiku[39]);
            WriteByte(_piDigits[86]);
            WriteByte(_piDigits[82]);
            WriteByte(_haiku[56]);
            WriteByte(_piDigits[95]);
            WriteByte(hmacResult[18]);
            WriteByte(_piDigits[113]);
            WriteByte(_haiku[38]);
            WriteByte(hmacResult[8]);
            WriteByte(_piDigits[92]);
            WriteByte(_piDigits[42]);
            WriteByte(_piDigits[120]);
            WriteByte(_piDigits[55]);
            WriteByte(_piDigits[124]);
            WriteByte(_haiku[30]);
            WriteByte(_piDigits[4]);
            WriteByte(_haiku[18]);
            WriteByte(_piDigits[123]);
            WriteByte(address[8]);
            WriteByte(_piDigits[61]);
            WriteByte(_piDigits[122]);
            WriteByte(_haiku[19]);
            WriteByte(_piDigits[53]);
            WriteByte(address[2]);
            WriteByte(hmacResult[11]);
            WriteByte(_piDigits[31]);
            WriteByte(_piDigits[36]);
            WriteByte(_haiku[2]);
            WriteByte(_haiku[57]);
            WriteByte(_haiku[40]);
            WriteByte(_piDigits[70]);
            WriteByte(_haiku[34]);
            WriteByte(_piDigits[132]);
            WriteByte(_piDigits[20]);
            WriteByte(_piDigits[107]);
            WriteByte(_piDigits[141]);
            WriteByte(_piDigits[97]);
            WriteByte(hmacResult[2]);
            WriteByte(_haiku[60]);
            WriteByte(_piDigits[102]);
            WriteByte(_piDigits[116]);
            WriteByte(_piDigits[49]);
            WriteByte(_piDigits[37]);
            WriteByte(_piDigits[48]);
            WriteByte(_piDigits[18]);
            WriteByte(_haiku[69]);
            WriteByte(hmacResult[12]);
            WriteByte(_piDigits[65]);
            WriteByte(hmacResult[3]);
            WriteByte(_haiku[27]);
            WriteByte(_piDigits[118]);
            WriteByte(_piDigits[44]);
            WriteByte(_haiku[50]);
            WriteByte(_haiku[59]);
            WriteByte(_piDigits[81]);
            WriteByte(_piDigits[51]);
            WriteByte(address[4]);
            WriteByte(_piDigits[12]);
            WriteByte(_piDigits[27]);
            WriteByte(address[11]);
            WriteByte(_piDigits[40]);
            WriteByte(_piDigits[139]);
            WriteByte(_haiku[51]);
            WriteByte(_haiku[64]);
            WriteByte(_piDigits[111]);
            WriteByte(_piDigits[131]);
            WriteByte(_haiku[1]);
            WriteByte(_haiku[49]);
            WriteByte(_piDigits[41]);
            WriteByte(_haiku[28]);
            WriteByte(_piDigits[77]);
            WriteByte(_piDigits[76]);
            WriteByte(_piDigits[8]);
            WriteByte(address[12]);
            WriteByte(_haiku[62]);
            WriteByte(_piDigits[19]);
            WriteByte(_piDigits[17]);
            WriteByte(_piDigits[24]);
            WriteByte(_haiku[72]);
            WriteByte(hmacResult[13]);
            WriteByte(_haiku[61]);
            WriteByte(_piDigits[29]);
            WriteByte(_piDigits[15]);
            WriteByte(address[7]);
            WriteByte(_piDigits[121]);
            WriteByte(_piDigits[69]);
            WriteByte(address[13]);
            WriteByte(_haiku[35]);
            WriteByte(_piDigits[103]);
            WriteByte(_piDigits[39]);
            WriteByte(hmacResult[5]);
            WriteByte(_haiku[4]);
            WriteByte(_piDigits[34]);
            WriteByte(_piDigits[56]);
            WriteByte((byte)(port >> 8 & 0xFF));
            WriteByte(hmacResult[10]);
            WriteByte(_piDigits[80]);
            WriteByte(_piDigits[130]);
            WriteByte(_haiku[12]);
            WriteByte(_piDigits[134]);
            WriteByte(_piDigits[33]);
            WriteByte(_piDigits[25]);
            WriteByte(_piDigits[73]);
            WriteByte(_piDigits[138]);
            WriteByte(_piDigits[9]);
            WriteByte(_piDigits[66]);
            WriteByte(_piDigits[1]);
            WriteByte(_haiku[45]);
            WriteByte(_piDigits[126]);
            WriteByte(_piDigits[67]);
            WriteByte(_haiku[33]);
            WriteByte(_piDigits[10]);
            WriteByte(hmacResult[4]);
            WriteByte(hmacResult[9]);
            WriteByte(_haiku[44]);
            WriteByte(_piDigits[60]);
            WriteByte(_piDigits[98]);
            WriteByte(_piDigits[91]);
            WriteByte(hmacResult[1]);
            WriteByte((byte)addressType);
            WriteByte(_piDigits[11]);
            WriteByte(_piDigits[83]);
            WriteByte(hmacResult[0]);
            WriteByte(_haiku[52]);
            WriteByte(_haiku[43]);
            WriteByte(_piDigits[47]);
            WriteByte(_haiku[11]);
            WriteByte(_piDigits[129]);
            WriteByte(_haiku[0]);
            WriteByte(_piDigits[57]);
            WriteByte(_piDigits[7]);
            WriteByte(hmacResult[7]);
            WriteByte(_haiku[15]);
            WriteByte(_haiku[58]);
            WriteByte(_haiku[66]);
            WriteByte(_piDigits[127]);
            WriteByte(_haiku[41]);
            WriteByte(address[10]);
            WriteByte(_haiku[71]);
            WriteByte(_piDigits[99]);
            WriteByte(_piDigits[117]);
            WriteByte(_piDigits[62]);
            WriteByte(_piDigits[89]);
            WriteByte(_haiku[70]);
            WriteByte(hmacResult[6]);
            WriteByte(_piDigits[114]);
            WriteByte(_piDigits[106]);
            WriteByte(_piDigits[108]);
            WriteByte(_haiku[48]);
            WriteByte(_haiku[7]);
            WriteByte(_piDigits[2]);
            WriteByte(_piDigits[43]);
            WriteByte(_haiku[36]);
            WriteByte(_piDigits[45]);
            WriteByte(_piDigits[119]);
            WriteByte(_haiku[47]);
            WriteByte(_haiku[68]);
            WriteByte(hmacResult[16]);
            WriteByte(_piDigits[128]);
            WriteByte(address[15]);
            WriteByte(_piDigits[35]);
            WriteByte(_piDigits[84]);

            BigInteger bnData = new(GetRawPacket(), true);
            Clear();

            // Perform the operations
            BigInteger m1 = BigInteger.ModPow(bnData % _p, _dmp1, _p);
            BigInteger m2 = BigInteger.ModPow(bnData % _q, _dmq1, _q);
            BigInteger h = _iqmp * (m1 - m2) % _p;

            // Ensure h is positive
            if (h < 0)
                h += _p;

            // Calculate the final value of m
            BigInteger m = m2 + h * _q;

            WriteUInt64(Key);
            WriteUInt32(Serial);
            WriteBytes(m.ToByteArray(true));
            WriteByte(Con);

            return this;
        }
    }
}

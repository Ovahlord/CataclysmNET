using Core.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packets.LoginPackets
{
    public sealed class ClientAuthLogonProof(byte[] buffer) : ClientPacket(buffer)
    {
        public static implicit operator ClientAuthLogonProof(byte[] buffer)
        {
            return new ClientAuthLogonProof(buffer);
        }

        public byte[] A { get; private set; } = [];
        public byte[] ClientM { get; private set; } = [];
        public byte[] CrcHash { get; private set; } = [];
        public byte NumberOfKeys { get; private set; }
        public byte SecurityFlags { get; private set; }

        protected override void Read()
        {
            A = ReadBytes(32);
            ClientM = ReadBytes(20);
            CrcHash = ReadBytes(20);
            NumberOfKeys = ReadByte();
            SecurityFlags = ReadByte();
        }
    }
}

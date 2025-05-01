using System.Net;

namespace Game.Packets.Substructures
{
    public sealed class ConnectPayload(IPEndPoint where)
    {
        public IPEndPoint Where { get; set; } = where;
        public uint Adler32 { get; } = 0xA0A66C10;
        public byte XorMagic { get; } = 0x2A;
    }
}

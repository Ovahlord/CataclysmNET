using System.Net;

namespace Game.Packets.Substructures
{
    public sealed class ConnectPayload
    {
        public required IPEndPoint Where { get; set; }
        public uint Adler32 { get; } = 0xA0A66C10;
        public byte XorMagic { get; } = 0x2A;
    }
}

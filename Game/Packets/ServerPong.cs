using Core.Packets;
using Core.Packets.Opcodes;

namespace Game.Packets
{
    public sealed class ServerPong : ServerPacket
    {
        public uint Serial { get; set; }

        public ServerPong(uint serial) : base(4, (int)ServerOpcode.SMSG_PONG)
        {
            Serial = serial;
        }

        public override ServerPacket Write()
        {
            WriteUInt32(Serial);
            return this;
        }
    }
}

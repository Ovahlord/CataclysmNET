using Core.Packets;
using Core.Packets.Opcodes;
using Core.Enums;

namespace Game.Packets.Substructures
{
    public sealed class ServerCreateChar : ServerPacket
    {
        public ServerCreateChar() : base(1, (int)ServerOpcode.SMSG_CREATE_CHAR) { }

        public ResponseCodes Code { get; set; }

        public override ServerPacket Write()
        {
            WriteByte((byte)Code);
            return this;
        }
    }
}

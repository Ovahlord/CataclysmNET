using Packets.GamePackets.Substructures;
using Packets.Opcodes;

namespace Packets.GamePackets
{
    public class ServerAuthResponse : ServerPacket
    {
        public AuthSuccessInfo? SuccessInfo { get; set; }
        public AuthWaitInfo? WaitInfo { get; set; }
        public byte Result { get; set; }

        public ServerAuthResponse() : base(50, (int)ServerOpcode.SMSG_AUTH_RESPONSE) { }
        public ServerAuthResponse(byte result) : base(50, (int)ServerOpcode.SMSG_AUTH_RESPONSE)
        {
            Result = result;
        }

        public override ServerPacket Write()
        {
            WriteBit(WaitInfo != null);

            if (WaitInfo != null)
                WriteBit(WaitInfo.HasFCM);

            WriteBit(SuccessInfo != null);
            FlushBits();

            if (SuccessInfo != null)
            {
                WriteUInt32(SuccessInfo.TimeRemain);
                WriteByte(SuccessInfo.ActiveExpansionLevel);
                WriteUInt32(SuccessInfo.TimeSecondsUntilPCKick);
                WriteByte(SuccessInfo.AccountExpansionLevel);
                WriteUInt32(SuccessInfo.TimeRested);
                WriteByte(SuccessInfo.TimeOptions);
            }

            WriteByte(Result);

            if (WaitInfo != null)
                WriteUInt32(WaitInfo.WaitCount);

            return this;
        }
    }
}

using Packets.Opcodes;

namespace Packets.LoginPackets
{
    public sealed class ServerAuthLogonProof : ServerPacket
    {
        public byte Error { get; set; }
        public byte[]? M2 { get; set; }
        public uint? AccountFlags { get; set; }
        public uint? SurveyId { get; set; }
        public ushort? LoginFlags { get; set; }

        public ServerAuthLogonProof() : base(256, (int)LoginOpcode.AuthLogonProof)
        {
        }

        public override ServerPacket Write()
        {
            WriteByte((byte)Cmd);
            WriteByte(Error);

            if (M2 != null)
                WriteBytes(M2);

            if (AccountFlags.HasValue)
                WriteUInt32(AccountFlags.Value);

            if (SurveyId.HasValue)
                WriteUInt32(SurveyId.Value);

            if (LoginFlags.HasValue)
                WriteUInt16(LoginFlags.Value);

            return this;
        }
    }
}

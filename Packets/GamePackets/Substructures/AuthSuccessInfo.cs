namespace Packets.GamePackets.Substructures
{
    public sealed class AuthSuccessInfo
    {
        public AuthSuccessInfo()
        {
        }

        public AuthSuccessInfo(byte accountExpansionLevel, byte activeExpansionLevel)
        {
            AccountExpansionLevel = accountExpansionLevel;
            ActiveExpansionLevel = activeExpansionLevel;
        }

        public uint TimeRemain { get; set; }
        public uint TimeRested { get; set; }
        public uint TimeSecondsUntilPCKick { get; set; }
        public byte AccountExpansionLevel { get; set; }
        public byte ActiveExpansionLevel { get; set; }
        public byte TimeOptions { get; set; }
    }
}

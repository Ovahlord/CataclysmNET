namespace Packets.GamePackets.Substructures
{
    public sealed class AuthWaitInfo
    {
        public uint WaitCount { get; set; }
        public bool HasFCM { get; set; }
    }
}

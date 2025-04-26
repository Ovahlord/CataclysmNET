namespace Packets.GamePackets.Substructures
{
    public sealed class CharacterListItem
    {
        public uint DisplayID { get; set; }
        public uint DisplayEnchantID { get; set; }
        public byte InvType { get; set; }
    }
}

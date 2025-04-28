namespace Database.RealmDatabase.Tables
{
    public sealed class Characters
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public byte RaceId { get; set; }
        public byte ClassId { get; set; }
        public byte SexId { get; set; }
        public byte SkinId { get; set; }
        public byte FaceId { get; set; }
        public byte HairStyleId { get; set; }
        public byte HairColorId { get; set; }
        public byte FacialHairStyleId { get; set; }
        public byte OutfitId { get; set; }
    }
}

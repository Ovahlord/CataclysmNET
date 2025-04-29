using Microsoft.EntityFrameworkCore;

namespace Database.RealmDatabase.Tables
{
    [PrimaryKey(nameof(CharacterId))]
    public sealed class CharacterStats
    {
        public int CharacterId { get; set; }
        public byte ExperienceLevel { get; set; }
        public int Experience { get; set; }
        public DateTime CreationDate { get; set; }
    }
}

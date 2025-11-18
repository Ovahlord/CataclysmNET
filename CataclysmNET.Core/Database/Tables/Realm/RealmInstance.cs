using System.ComponentModel.DataAnnotations;

namespace CataclysmNET.Core.Database.Tables.Realm
{
    public class RealmInstance
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public string EntryEndpointAddress { get; set; } = "127.0.0.1:8085";
        [MaxLength(50)]
        public string StepEndpointAddress { get; set; } = "127.0.0.1:8086";
        [MaxLength(32)]
        public string RealmName { get; set; } = "Default Realm";
        public byte RealmType { get; set; } = 1;
        public byte TimeZoneOffset { get; set; } = 1;
        public byte Region { get; set; } = 2;
        public int MaxPlayers { get; set; } = 1000;
        public int GameBuild { get; set; } = 15595;
        public bool RecommendedForNewPlayers { get; set; } = false;
        public bool LockedForNewPlayers { get; set; } = false;
    }
}

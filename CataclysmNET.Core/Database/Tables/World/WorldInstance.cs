using System.ComponentModel.DataAnnotations;

namespace CataclysmNET.Core.Database.Tables.World
{
    public class WorldInstance
    {
        public int RealmInstanceId { get; set; }
        public int MapId { get; set; }
        [MaxLength(50)]
        public string EndPointAddress { get; set; } = "127.0.0.1:8087";
    }
}

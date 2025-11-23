using System.ComponentModel.DataAnnotations;

namespace CataclysmNET.Core.Database.Tables.Login
{
    public class GameAccount
    {
        public long Id { get; set; }
        [MaxLength(32)]
        public string Name { get; set; } = string.Empty;
        public byte[] Salt { get; set; } = [];
        public byte[] Verifier { get; set; } = [];
        public byte[] SessionKey { get; set; } = [];
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; } = DateTime.Now;
        public int ExpansionLevel { get; set; }
    }
}

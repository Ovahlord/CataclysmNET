using System.ComponentModel.DataAnnotations;

namespace CataclysmNET.Core.Database.Tables.Login
{
    public class LoginInstance
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public string EndPointAddress { get; set; } = "127.0.0.1:3724";
    }
}

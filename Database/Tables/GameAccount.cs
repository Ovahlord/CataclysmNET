using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Tables
{
    public sealed class GameAccount
    {
        public int Id { get; set; }
        public string Login { get; set; } = string.Empty;
        public byte[] Salt { get; set; } = [];
        public byte[] Verifier { get; set; } = [];
        public byte[] SessionKey { get; set; } = [];
        public byte ExpansionLevel { get; set; } = 0;
    }
}

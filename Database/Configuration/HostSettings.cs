using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database.Configuration
{
    public sealed class HostSettings
    {
        public required string Host { get; set; } = null!;
        public required string Port { get; set; } = null!;
        public required string User { get; set; } = null!;
        public required string Password { get; set; } = null!;
        public required string Database { get; set; } = null!;
    }
}

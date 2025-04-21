using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginServer.Enums
{
    public enum LoginState
    {
        None            = 0,
        Challenge       = 1,
        Proof           = 2,
        Authenticated   = 3
    }
}

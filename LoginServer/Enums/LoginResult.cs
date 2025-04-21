using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoginServer.Enums
{
    public enum LoginResult
    {
        WowSuccess                  = 0x00,
        WoWFailBanned               = 0x03,
        WowFailUnknownAccount       = 0x04,
        WowFailIncorrectPassword    = 0x05,
        WowFailAlreadyOnline        = 0x06,
        WowFailNoTime               = 0x07,
        WowFailDbBusy               = 0x08,
        WowFailVersionInvalid       = 0x09,
        WowFailVersionUpdate        = 0x0A,
        WowFailInvalidServer        = 0x0B,
        WowFailSuspended            = 0x0C,
        WowFailFailNoaccess         = 0x0D,
        WoWSuccessSurvey            = 0x0E
    }
}

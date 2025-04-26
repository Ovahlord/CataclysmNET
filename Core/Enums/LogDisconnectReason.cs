namespace Shared.Enums
{
    /// <summary>
    /// The enum for the reason field received in CMSG_LOG_DISCONNECT
    /// </summary>
    public enum LogDisconnectReason
    {
        Unknown                 = 0,
        ClientLost              = 1,
        Flood                   = 2,
        ProtocolError           = 3,
        AuthError               = 4,
        Cheat                   = 5,
        Tmeout                  = 6,
        Bug                     = 7,
        Kick                    = 8,
        MoveEnforcement         = 9,
        WardenTimeout           = 10,
        ServerRequest           = 11,
        Reconnecting            = 12,
        Shutdown                = 13,
        UserRequest             = 14,
        paired                  = 15,
        StreamingFailure        = 16,
        ZlibError               = 17,
        Logout                  = 18,
        InvalidExpansion        = 19,
        BadInstanceSave         = 20,
        Unused                  = 21,
        PlayedTimeRestriction   = 22,
        VersionKick             = 23,
        EncryptionError         = 24,
        NameReservationOnly     = 25,
        Idle                    = 26,
        EraChoiceError          = 27,
        Redirect                = 28,
        NoHandshake             = 29
    }
}

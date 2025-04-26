namespace Core.Packets.Opcodes
{
    public enum LoginOpcode
    {
        AuthLogonChallenge  = 0x00,
        AuthLogonProof      = 0x01,
        ReconnectChallenge  = 0x02, // NYI
        ReconnectProof      = 0x03, // NYI
        RealmList           = 0x10,
        XferInitiate        = 0x30, // NYI
        XferData            = 0x31, // NYI
        XferAccept          = 0x32, // NYI
        XferResume          = 0x33, // NYI
        XferCancel          = 0x34  // NYI
    }
}

﻿namespace Shared.Enums
{
    [Flags]
    public enum RealmFlags
    {
        None            = 0x00,
        VersionMismatch = 0x01,
        Offline         = 0x02,
        SpecifyBuild    = 0x04,
        Unk1            = 0x08,
        Unk2            = 0x10,
        Recommended     = 0x20,
        New             = 0x40,
        Full            = 0x80
    };
}

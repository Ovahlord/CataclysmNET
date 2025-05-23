﻿using Core.Packets;
using Core.Packets.Opcodes;

namespace Game.Packets
{
    public sealed class ServerSuspendComms : ServerPacket
    {
        public uint Serial { get; set; }

        public ServerSuspendComms(uint serial) : base(4, (int)ServerOpcode.SMSG_SUSPEND_COMMS)
        {
            Serial = serial;
        }

        public override ServerPacket Write()
        {
            WriteUInt32(Serial);
            return this;
        }
    }
}

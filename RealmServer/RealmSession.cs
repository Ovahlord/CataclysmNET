using Networking;
using Packets.GamePackets;
using Packets.Opcodes;
using Shared.Enums;

namespace RealmServer
{
    public sealed class RealmSession(BaseSocket socket) : GameSession(socket)
    {
        protected override void CallPacketHandler(ClientOpcode opcode, byte[] payload)
        {
            switch (opcode)
            {
                default:
                    base.CallPacketHandler(opcode, payload);
                    break;
            }
        }

        #region Packet Handlers

        #endregion
    }
}

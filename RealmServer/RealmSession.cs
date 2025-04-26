using Core.Networking;
using Core.Packets.Opcodes;

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

using Core.Networking;
using Core.Packets.Opcodes;
using Game.Networking;

namespace WorldInstance
{
    public sealed class WorldSession(BaseSocket socket) : GameSession(socket)
    {
        public override Task? HandlePacket(int opcode, byte[] payload)
        {
            switch ((ClientOpcode)opcode)
            {
                default:
                    return base.HandlePacket(opcode, payload);
            }
        }

        protected override void OnSessionAuthenticated(bool connectionRequested)
        {


            base.OnSessionAuthenticated(connectionRequested);
        }

        #region Packet Handlers

        #endregion
    }
}

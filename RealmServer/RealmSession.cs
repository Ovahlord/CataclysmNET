using Core.Networking;
using Core.Packets.GamePackets;
using Core.Packets.Opcodes;

namespace RealmServer
{
    public sealed class RealmSession(BaseSocket socket) : GameSession(socket)
    {
        protected override void CallPacketHandler(ClientOpcode opcode, byte[] payload)
        {
            switch (opcode)
            {
                case ClientOpcode.CMSG_ENUM_CHARACTERS: HandleEnumCharacters(payload); break;
                default:
                    base.CallPacketHandler(opcode, payload);
                    break;
            }
        }

        #region Packet Handlers

        private void HandleEnumCharacters(ClientEnumCharacters enumCharacters)
        {
            ServerEnumCharactersResult packet = new()
            {
                Success = true
            };
            SendPacket(packet);
        }

        #endregion
    }
}

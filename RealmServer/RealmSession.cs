using Core.Networking;
using Core.Packets.GamePackets;
using Core.Packets.Opcodes;
using Database.RealmDatabase;
using Database.RealmDatabase.Tables;
using Packets.GamePackets.Substructures;

namespace RealmServer
{
    public sealed class RealmSession(BaseSocket socket) : GameSession(socket)
    {
        protected override void CallPacketHandler(ClientOpcode opcode, byte[] payload)
        {
            switch (opcode)
            {
                case ClientOpcode.CMSG_ENUM_CHARACTERS:     HandleEnumCharacters(payload); break;
                case ClientOpcode.CMSG_CREATE_CHARACTER:    HandleCreateCharacter(payload); break;
                default:
                    base.CallPacketHandler(opcode, payload);
                    break;
            }
        }

        #region Packet Handlers

        private void HandleEnumCharacters(ClientEnumCharacters enumCharacters)
        {
            ServerEnumCharactersResult packet = new();

            if (_gameAccount == null)
            {
                packet.Success = false;
                SendPacket(packet);
                return;
            }

            using RealmDatabaseContext realmDatabase = new();
            IEnumerable<RealmCharacters> realmCharacters = realmDatabase.RealmCharacters.Where(rc => rc.GameAccountId == _gameAccount.Id);
            if (!realmCharacters.Any())
            {
                packet.Success = false;
                SendPacket(packet);
                return;
            }

            IEnumerable<Characters> characters = realmDatabase.Characters.Where(c => realmCharacters.Select(rc => rc.CharacterId).Contains(c.Id));
            byte listPosition = 0;
            foreach (Characters character in characters)
            {
                packet.Characters.Add(new CharacterListEntry()
                {
                     Name = character.Name,
                     Guid = (ulong)character.Id,
                     MapID = 0,
                     ClassID = character.ClassId,
                     FaceID = character.FaceId,
                     FacialHair = character.FacialHairStyleId,
                     HairColor = character.HairColorId,
                     HairStyle = character.HairStyleId,
                     ListPosition = listPosition,
                     RaceID = character.RaceId,
                     SexID = character.SexId,
                     SkinID = character.SkinId,
                });

                ++listPosition;
            }

            SendPacket(packet);
        }

        private void HandleCreateCharacter(ClientCreateCharacter createCharacter)
        {

        }

        #endregion
    }
}

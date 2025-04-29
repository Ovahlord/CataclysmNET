using Core.Enums;
using Core.Networking;
using Core.Packets.Opcodes;
using Database.RealmDatabase;
using Database.RealmDatabase.Tables;
using Game.Networking;
using Game.Objects;
using Game.Packets;
using Game.Packets.Substructures;

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
            // No characters have been found
            if (!realmCharacters.Any())
            {
                packet.Success = true;
                SendPacket(packet);
                return;
            }

            packet.Success = true;

            IEnumerable<Characters> characters = realmDatabase.Characters.Where(c => realmCharacters.Select(rc => rc.CharacterId).Contains(c.Id));
            byte listPosition = 0;
            foreach (Characters character in characters)
            {
                CharacterListItem[] inventoryItems = new CharacterListItem[23];
                Array.Fill(inventoryItems, new CharacterListItem());

                packet.Characters.Add(new CharacterListEntry()
                {
                     Name = character.Name,
                     Guid = new ObjectGuid(HighGuid.Player, 0, (uint)character.Id),
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
                     InventoryItems = inventoryItems
                });

                ++listPosition;
            }

            SendPacket(packet);
        }

        private void HandleCreateCharacter(ClientCreateCharacter createCharacter)
        {
            if (_gameAccount == null)
                return;

            Characters newCharacter = new()
            {
                Name = createCharacter.Name,
                RaceId = createCharacter.RaceID,
                ClassId = createCharacter.ClassID,
                SexId = createCharacter.SexID,
                SkinId = createCharacter.SkinID,
                FaceId = createCharacter.FaceID,
                HairStyleId = createCharacter.HairStyleID,
                HairColorId = createCharacter.HairColorID,
                FacialHairStyleId = createCharacter.FacialHairStyleID,
                OutfitId = createCharacter.OutfitID
            };

            Console.WriteLine($"{newCharacter.Id}");

            Task.Run(async () =>
            {
                using RealmDatabaseContext realmDatabase = new();
                realmDatabase.Characters.Add(newCharacter);
                await realmDatabase.SaveChangesAsync();

                Console.WriteLine($"{newCharacter.Id}");

                RealmCharacters realmCharacter = new()
                {
                    RealmId = 1,
                    GameAccountId = _gameAccount.Id,
                    CharacterId = newCharacter.Id
                };

                realmDatabase.RealmCharacters.Add(realmCharacter);
                await realmDatabase.SaveChangesAsync();

                ServerCreateChar packet = new()
                {
                    Code = ResponseCodes.CHAR_CREATE_SUCCESS
                };

                SendPacket(packet);
            });
        }

        #endregion
    }
}

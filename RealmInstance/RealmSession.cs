using Core.Enums;
using Core.Networking;
using Core.Packets.Opcodes;
using Database.RealmDatabase;
using Database.RealmDatabase.Tables;
using Game.Networking;
using Game.Enums;
using Game.Objects;
using Game.Packets;
using Game.Packets.Substructures;
using Microsoft.EntityFrameworkCore;
using Core.Packets;

namespace RealmInstance
{
    public sealed class RealmSession(BaseSocket socket) : GameSession(socket)
    {
        public override Task? HandlePacket(int opcode, byte[] payload)
        {
            switch ((ClientOpcode)opcode)
            {
                case ClientOpcode.CMSG_ENUM_CHARACTERS:     return HandleEnumCharacters(payload);
                case ClientOpcode.CMSG_CREATE_CHARACTER:    return HandleCreateCharacter(payload);
                default:
                    return base.HandlePacket(opcode, payload);
            }
        }

        protected override void OnSessionAuthenticated()
        {
            SendConnectTo(Realm.Instance.GetConnectToEndPoint(), ConnectToConnectionType.Realm);
        }

        #region Packet Handlers

        private async Task HandleEnumCharacters(ClientEnumCharacters enumCharacters)
        {
            ServerEnumCharactersResult packet = new();

            if (_gameAccount == null)
            {
                packet.Success = false;
                SendPacket(packet);

                return;
            }

            using RealmDatabaseContext realmDatabase = new();

            var query = from rc in realmDatabase.Set<RealmCharacters>()
                        join c in realmDatabase.Set<Characters>()
                            on rc.CharacterId equals c.Id
                        join cs in realmDatabase.Set<CharacterStats>()
                            on c.Id equals cs.CharacterId
                        where (rc.GameAccountId == _gameAccount.Id && rc.RealmId == Realm.Instance.GetId())
                        select new
                        {
                            RealmCharacters = rc,
                            Character = c,
                            Characterstats = cs,
                        };

            var result = await query.ToListAsync();
            if (result.Count == 0)
            {
                packet.Success = true;
                SendPacket(packet);
                return;
            }

            packet.Success = true;

            foreach (var character in result)
            {
                CharacterListItem[] inventoryItems = new CharacterListItem[23];
                Array.Fill(inventoryItems, new CharacterListItem());

                packet.Characters.Add(new CharacterListEntry()
                {
                     Name = character.Character.Name,
                     Guid = new ObjectGuid(HighGuid.Player, 0, (uint)character.Character.Id),
                     MapID = 0,
                     ClassID = character.Character.ClassId,
                     FaceID = character.Character.FaceId,
                     FacialHair = character.Character.FacialHairStyleId,
                     HairColor = character.Character.HairColorId,
                     HairStyle = character.Character.HairStyleId,
                     ListPosition = character.RealmCharacters.ListPosition,
                     RaceID = character.Character.RaceId,
                     SexID = character.Character.SexId,
                     SkinID = character.Character.SkinId,
                     InventoryItems = inventoryItems,
                     ExperienceLevel = character.Characterstats.ExperienceLevel
                });
            }

            SendPacket(packet);
        }

        private async Task HandleCreateCharacter(ClientCreateCharacter createCharacter)
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
                OutfitId = createCharacter.OutfitID,
            };

            // Save the new character to database
            using RealmDatabaseContext realmDatabase = new();
            realmDatabase.Characters.Add(newCharacter);
            await realmDatabase.SaveChangesAsync();

            CharacterStats characterStats = new()
            {
                CharacterId = newCharacter.Id,
                ExperienceLevel = 1,
                CreationDate = DateTime.UtcNow
            };

            RealmCharacters realmCharacter = new()
            {
                RealmId = Realm.Instance.GetId(),
                GameAccountId = _gameAccount.Id,
                CharacterId = newCharacter.Id
            };

            // And finally we store the stats and realm characters
            realmDatabase.CharacterStats.Add(characterStats);
            realmDatabase.RealmCharacters.Add(realmCharacter);
            await realmDatabase.SaveChangesAsync();

            ServerCreateChar packet2 = new()
            {
                Code = ResponseCodes.CHAR_CREATE_SUCCESS
            };

            SendPacket(packet2);
        }

        #endregion
    }
}

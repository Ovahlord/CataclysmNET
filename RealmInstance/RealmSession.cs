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
            List<RealmCharacters> realmCharacters = await realmDatabase.RealmCharacters
                .Include(rc => rc.Character)
                .ThenInclude(c => c != null? c.CharacterStats : null)
                .Where(rc => rc.GameAccountId == _gameAccount.Id && rc.RealmId == Realm.Instance.GetId()).ToListAsync();

            if (realmCharacters.Count == 0)
            {
                packet.Success = true;
                SendPacket(packet);
                return;
            }

            packet.Success = true;

            foreach (RealmCharacters realmCharacter in realmCharacters)
            {
                CharacterListItem[] inventoryItems = new CharacterListItem[23];
                Array.Fill(inventoryItems, new CharacterListItem());

                if (realmCharacter.Character == null || realmCharacter.Character.CharacterStats == null)
                    continue;

                packet.Characters.Add(new CharacterListEntry()
                {
                     Name = realmCharacter.Character.Name,
                     Guid = new ObjectGuid(HighGuid.Player, 0, (uint)realmCharacter.Character.Id),
                     MapID = 0,
                     ClassID = realmCharacter.Character.ClassId,
                     FaceID = realmCharacter.Character.FaceId,
                     FacialHair = realmCharacter.Character.FacialHairStyleId,
                     HairColor = realmCharacter.Character.HairColorId,
                     HairStyle = realmCharacter.Character.HairStyleId,
                     ListPosition = realmCharacter.ListPosition,
                     RaceID = realmCharacter.Character.RaceId,
                     SexID = realmCharacter.Character.SexId,
                     SkinID = realmCharacter.Character.SkinId,
                     InventoryItems = inventoryItems,
                     ExperienceLevel = realmCharacter.Character.CharacterStats.ExperienceLevel
                });
            }

            SendPacket(packet);
        }

        private async Task HandleCreateCharacter(ClientCreateCharacter createCharacter)
        {
            if (_gameAccount == null)
                return;

            // Normalize the character name - capitalize first letter and lowercase the remaining letters
            string characterName = char.ToUpper(createCharacter.Name[0]) + createCharacter.Name.Substring(1).ToLower();

            Characters newCharacter = new()
            {
                Name = characterName,
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
                CharacterId = newCharacter.Id,
            };

            newCharacter.CharacterStats = characterStats;

            // And finally we store the stats and realm characters
            realmDatabase.RealmCharacters.Add(realmCharacter);
            realmDatabase.CharacterStats.Add(characterStats);
            await realmDatabase.SaveChangesAsync();

            ServerCreateChar packet = new()
            {
                Code = ResponseCodes.CHAR_CREATE_SUCCESS
            };

            SendPacket(packet);
        }

        #endregion
    }
}

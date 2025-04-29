using Core.Packets;
using Core.Packets.Opcodes;
using Game.Packets.Substructures;

namespace Game.Packets
{
    public sealed class ServerEnumCharactersResult : ServerPacket
    {
        public bool Success { get; set; }
        public List<CharacterListEntry> Characters { get; set; } = [];
        public List<RestrictedFactionChangeRule> FactionChangeRestrictions { get; set; } = [];

        public ServerEnumCharactersResult() : base(2048, (int)ServerOpcode.SMSG_ENUM_CHARACTERS_RESULT)
        {
        }

        public override ServerPacket Write()
        {
            WriteBits((ulong)FactionChangeRestrictions.Count, 23);
            WriteBit(Success);
            WriteBits((ulong)Characters.Count, 17);

            foreach (CharacterListEntry character in Characters)
            {
                WriteBit(character.Guid[3] != 0);
                WriteBit(character.GuildGUID[1] != 0);
                WriteBit(character.GuildGUID[7] != 0);
                WriteBit(character.GuildGUID[2] != 0);
                WriteBits((uint)character.Name.Length, 7);
                WriteBit(character.Guid[4] != 0);
                WriteBit(character.Guid[7] != 0);
                WriteBit(character.GuildGUID[3] != 0);
                WriteBit(character.Guid[5] != 0);
                WriteBit(character.GuildGUID[6] != 0);
                WriteBit(character.Guid[1] != 0);
                WriteBit(character.GuildGUID[5] != 0);
                WriteBit(character.GuildGUID[4] != 0);
                WriteBit(character.FirstLogin);
                WriteBit(character.Guid[0] != 0);
                WriteBit(character.Guid[2] != 0);
                WriteBit(character.Guid[6] != 0);
                WriteBit(character.GuildGUID[0] != 0);
            }

            FlushBits();

            foreach (CharacterListEntry character in Characters)
            {
                WriteByte(character.ClassID);

                foreach (CharacterListItem characterItem in character.InventoryItems)
                {
                    WriteByte(characterItem.InvType);
                    WriteUInt32(characterItem.DisplayID);
                    WriteUInt32(characterItem.DisplayEnchantID);
                }

                WriteUInt32(character.PetCreatureFamilyID);
                WriteByteSeq(character.GuildGUID[2]);
                WriteByte(character.ListPosition);
                WriteByte(character.HairStyle);
                WriteByteSeq(character.GuildGUID[3]);
                WriteUInt32(character.PetCreatureDisplayID);
                WriteUInt32(character.Flags);
                WriteByte(character.HairColor);
                WriteByteSeq(character.Guid[4]);
                WriteInt32(character.MapID);
                WriteByteSeq(character.GuildGUID[5]);
                WriteFloat(character.PreloadPos[2]);
                WriteByteSeq(character.GuildGUID[6]);
                WriteUInt32(character.PetExperienceLevel);
                WriteByteSeq(character.Guid[3]);
                WriteFloat(character.PreloadPos[1]);
                WriteUInt32(character.Flags2);
                WriteByte(character.FacialHair);
                WriteByteSeq(character.Guid[7]);
                WriteByte(character.SexID);
                WriteString(character.Name); // validate if correct method
                WriteByte(character.FaceID);
                WriteByteSeq(character.Guid[0]);
                WriteByteSeq(character.Guid[2]);
                WriteByteSeq(character.GuildGUID[1]);
                WriteByteSeq(character.GuildGUID[7]);
                WriteFloat(character.PreloadPos[0]);
                WriteByte(character.SkinID);
                WriteByte(character.RaceID);
                WriteByte(character.ExperienceLevel);
                WriteByteSeq(character.Guid[6]);
                WriteByteSeq(character.GuildGUID[4]);
                WriteByteSeq(character.GuildGUID[0]);
                WriteByteSeq(character.Guid[5]);
                WriteByteSeq(character.Guid[1]);
                WriteInt32(character.ZoneID);
            }

            foreach (RestrictedFactionChangeRule factionChangeRule in FactionChangeRestrictions)
            {
                WriteInt32(factionChangeRule.Mask);
                WriteByte(factionChangeRule.RaceID);
            }

            return this;
        }
    }
}

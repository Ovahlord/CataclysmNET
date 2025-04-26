using Core.Packets.Opcodes;
using Packets.GamePackets.Substructures;

namespace Core.Packets.GamePackets
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

            /*
            for (CharacterInfo const& charInfo : Characters)
            {
                _worldPacket.WriteBit(charInfo.Guid[3]);
                _worldPacket.WriteBit(charInfo.GuildGUID[1]);
                _worldPacket.WriteBit(charInfo.GuildGUID[7]);
                _worldPacket.WriteBit(charInfo.GuildGUID[2]);
                _worldPacket.WriteBits(charInfo.Name.length(), 7);
                _worldPacket.WriteBit(charInfo.Guid[4]);
                _worldPacket.WriteBit(charInfo.Guid[7]);
                _worldPacket.WriteBit(charInfo.GuildGUID[3]);
                _worldPacket.WriteBit(charInfo.Guid[5]);
                _worldPacket.WriteBit(charInfo.GuildGUID[6]);
                _worldPacket.WriteBit(charInfo.Guid[1]);
                _worldPacket.WriteBit(charInfo.GuildGUID[5]);
                _worldPacket.WriteBit(charInfo.GuildGUID[4]);
                _worldPacket.WriteBit(charInfo.FirstLogin);
                _worldPacket.WriteBit(charInfo.Guid[0]);
                _worldPacket.WriteBit(charInfo.Guid[2]);
                _worldPacket.WriteBit(charInfo.Guid[6]);
                _worldPacket.WriteBit(charInfo.GuildGUID[0]);
            }
           */
            FlushBits();

            /*
            for (CharacterInfo const&charInfo : Characters)
            {
                _worldPacket << uint8(charInfo.ClassID);

                for (WorldPackets::Character::EnumCharactersResult::CharacterInfo::VisualItemInfo const&visualItem : charInfo.VisualItems)
                {
                    _worldPacket << uint8(visualItem.InvType);
                    _worldPacket << uint32(visualItem.DisplayID);
                    _worldPacket << uint32(visualItem.DisplayEnchantID);
                }

                _worldPacket << uint32(charInfo.PetCreatureFamilyID);
                _worldPacket.WriteByteSeq(charInfo.GuildGUID[2]);
                _worldPacket << uint8(charInfo.ListPosition);
                _worldPacket << uint8(charInfo.HairStyle);
                _worldPacket.WriteByteSeq(charInfo.GuildGUID[3]);
                _worldPacket << uint32(charInfo.PetCreatureDisplayID);
                _worldPacket << uint32(charInfo.Flags);
                _worldPacket << uint8(charInfo.HairColor);
                _worldPacket.WriteByteSeq(charInfo.Guid[4]);
                _worldPacket << int32(charInfo.MapID);
                _worldPacket.WriteByteSeq(charInfo.GuildGUID[5]);
                _worldPacket << float(charInfo.PreloadPos.GetPositionZ());
                _worldPacket.WriteByteSeq(charInfo.GuildGUID[6]);
                _worldPacket << uint32(charInfo.PetExperienceLevel);
                _worldPacket.WriteByteSeq(charInfo.Guid[3]);
                _worldPacket << float(charInfo.PreloadPos.GetPositionY());
                _worldPacket << uint32(charInfo.Flags2);
                _worldPacket << uint8(charInfo.FacialHair);
                _worldPacket.WriteByteSeq(charInfo.Guid[7]);
                _worldPacket << uint8(charInfo.SexID);
                _worldPacket.WriteString(charInfo.Name);
                _worldPacket << uint8(charInfo.FaceID);
                _worldPacket.WriteByteSeq(charInfo.Guid[0]);
                _worldPacket.WriteByteSeq(charInfo.Guid[2]);
                _worldPacket.WriteByteSeq(charInfo.GuildGUID[1]);
                _worldPacket.WriteByteSeq(charInfo.GuildGUID[7]);
                _worldPacket << float(charInfo.PreloadPos.GetPositionX());
                _worldPacket << uint8(charInfo.SkinID);
                _worldPacket << uint8(charInfo.RaceID);
                _worldPacket << uint8(charInfo.ExperienceLevel);
                _worldPacket.WriteByteSeq(charInfo.Guid[6]);
                _worldPacket.WriteByteSeq(charInfo.GuildGUID[4]);
                _worldPacket.WriteByteSeq(charInfo.GuildGUID[0]);
                _worldPacket.WriteByteSeq(charInfo.Guid[5]);
                _worldPacket.WriteByteSeq(charInfo.Guid[1]);
                _worldPacket << int32(charInfo.ZoneID);
            }

            for (RestrictedFactionChangeRuleInfo const&rule : FactionChangeRestrictions)
            {
                _worldPacket << int32(rule.Mask);
                _worldPacket << uint8(rule.Race);
            }
            */

            return this;
        }
    }
}

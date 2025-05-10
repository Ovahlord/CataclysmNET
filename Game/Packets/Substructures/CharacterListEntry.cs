﻿using Game.Entities.Object;

namespace Game.Packets.Substructures
{
    public sealed class CharacterListEntry
    {
        public float[] PreloadPos { get; set; } = new float[3];
        public ObjectGuid Guid { get; set; } = ObjectGuid.Empty;
        public ObjectGuid GuildGUID { get; set; } = ObjectGuid.Empty;
        public uint Flags { get; set; }
        public uint Flags2 { get; set; }
        public int MapID { get; set; }
        public uint PetCreatureDisplayID { get; set; }
        public uint PetCreatureFamilyID { get; set; }
        public uint PetExperienceLevel { get; set; }
        public int ZoneID { get; set; }
        public byte ClassID { get; set; }
        public byte ExperienceLevel { get; set; }
        public byte FaceID { get; set; }
        public byte FacialHair { get; set; }
        public byte HairColor { get; set; }
        public byte HairStyle { get; set; }
        public byte ListPosition { get; set; }
        public byte RaceID { get; set; }
        public byte SexID { get; set; }
        public byte SkinID { get; set; }
        public bool FirstLogin { get; set; }
        public string Name { get; set; } = string.Empty;
        public CharacterListItem[] InventoryItems { get; set; } = new CharacterListItem[23];
    }
}

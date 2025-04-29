using Core.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Packets
{
    public sealed class ClientCreateCharacter(byte[] buffer) : ClientPacket(buffer)
    {
        public static implicit operator ClientCreateCharacter(byte[] buffer)
        {
            return new ClientCreateCharacter(buffer);
        }

        public string Name { get; private set; } = string.Empty;
        public byte RaceID { get; private set; }
        public byte ClassID { get; private set; }
        public byte SexID { get; private set; }
        public byte SkinID { get; private set; }
        public byte FaceID { get; private set; }
        public byte HairStyleID { get; private set; }
        public byte HairColorID { get; private set; }
        public byte FacialHairStyleID { get; private set; }
        public byte OutfitID { get; private set; }

        protected override void Read()
        {
            Name = ReadCString();
            RaceID = ReadByte();
            ClassID = ReadByte();
            SexID = ReadByte();
            SkinID = ReadByte();
            FaceID = ReadByte();
            HairStyleID = ReadByte();
            HairColorID = ReadByte();
            FacialHairStyleID = ReadByte();
            OutfitID = ReadByte();
        }
    }
}

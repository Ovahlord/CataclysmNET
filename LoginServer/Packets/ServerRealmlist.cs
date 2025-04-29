using Core.Packets;
using Core.Packets.Opcodes;
using Shared.Enums;

namespace Packets.LoginPackets
{
    public sealed class RealmInfo
    {
        public byte Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public byte RealmType { get; set; }
        public string RealmServerAddress { get; set; } = string.Empty;
        public float PopulationLevel { get; set; }
        public byte TimeZone { get; set; }
        public byte MajorVersion { get; set; }
        public byte MinorVersion { get; set; }
        public byte BugfixVersion { get; set; }
        public ushort Build { get; set; }
        public RealmFlags Flags { get; set; }
        public bool Locked { get; set; }
        public byte Characters { get; set; }
    }

    public class ServerRealmList : ServerPacket
    {
        public List<RealmInfo> RealmList { get; set; } = [];

        public ServerRealmList() : base(1024, (int)LoginOpcode.RealmList) { }

        public override ServerPacket Write()
        {
            foreach (RealmInfo realm in RealmList)
            {
                WriteByte(realm.RealmType);
                WriteBool(realm.Locked);
                WriteByte(((byte)realm.Flags));
                WriteCString(realm.Name);
                WriteCString(realm.RealmServerAddress);
                WriteFloat(realm.PopulationLevel);
                WriteByte(realm.Characters);
                WriteByte(realm.TimeZone);
                WriteByte(realm.Id);

                if (realm.Flags.HasFlag(RealmFlags.SpecifyBuild))
                {
                    WriteByte(realm.MajorVersion);
                    WriteByte(realm.MinorVersion);
                    WriteByte(realm.BugfixVersion);
                    WriteUInt16(realm.Build);
                }
            }

            WriteByte(0x10);
            WriteByte(0x00);

            byte[] realmBuffer = GetRawPacket();
            Clear();

            WriteUInt32(0);
            WriteUInt16((UInt16)RealmList.Count);

            byte[] realmlistSizeBuffer = GetRawPacket();
            Clear();

            WriteByte((byte)Cmd);
            WriteUInt16((UInt16)(realmBuffer.Length + realmlistSizeBuffer.Length));
            WriteBytes(realmlistSizeBuffer);
            WriteBytes(realmBuffer);

            return this;
        }
    }
}

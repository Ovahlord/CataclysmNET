using System;

namespace Game.Objects
{
    public enum TypeId
    {
        Object          = 0,
        Item            = 1,
        Container       = 2,
        Unit            = 3,
        Player          = 4,
        GameObject      = 5,
        DynamicObject   = 6,
        Corpse          = 7,
        AreaTrigger     = 8
    }

    [Flags]
    public enum TypeMask
    {
        Object          = 0x0001,
        Item            = 0x0002,
        Container       = 0x0006, // ITEM | 0x0004
        Unit            = 0x0008, // creature
        Player          = 0x0010,
        GameObject      = 0x0020,
        DynamicObject   = 0x0040,
        Corpse          = 0x0080,
        AreaTrigger     = 0x0100,
        Seer = Player | Unit | DynamicObject
    }

    public enum HighGuid
    {
        Item            = 0x400,
        Container       = 0x400,
        Player          = 0x000,
        GameObject      = 0xF11,
        Transport       = 0xF12,
        Unit            = 0xF13,
        Pet             = 0xF14,
        Vehicle         = 0xF15,
        DynamicObject   = 0xF10,
        Corpse          = 0xF101,
        AreaTrigger     = 0xF102,
        BattleGround    = 0x1F1,
        Mo_Transport    = 0x1FC,
        Instance        = 0x1F4,
        Group           = 0x1F5,
        Guild           = 0x1FF
    }

    public sealed class ObjectGuid
    {
        public static implicit operator ulong(ObjectGuid guid)
        {
            return guid.RawValue;
        }

        public static implicit operator ObjectGuid(ulong raw)
        {
            return new ObjectGuid(raw);
        }

        public static readonly ObjectGuid Empty = new(0);

        public ulong RawValue { get; private set; }

        public ObjectGuid()
        {
            RawValue = 0;
        }

        public ObjectGuid(ulong guid)
        {
            RawValue = guid;
        }

        public ObjectGuid(HighGuid high, uint entry, uint counter)
        {
            RawValue = counter != 0 ? counter | ((ulong)entry << 32) | ((ulong)high << ((high == HighGuid.Corpse || high == HighGuid.AreaTrigger) ? 48 : 52)) : 0;
        }

        public ObjectGuid(HighGuid high, uint counter)
        {
            RawValue = counter != 0 ? counter | ((ulong)high << ((high == HighGuid.Corpse || high == HighGuid.AreaTrigger) ? 48 : 52)) : 0;
        }

        public HighGuid GetHigh()
        {
            HighGuid temp = (HighGuid)((RawValue >> 48) & 0xFFFF);
            return (temp == HighGuid.Corpse || temp == HighGuid.AreaTrigger) ? temp : (HighGuid)(((uint)temp >> 4) & 0xFFF);
        }

        public uint GetEntry()
        {
            return HasEntry() ? (uint)((RawValue >> 32) & 0xFFFFF) : 0;
        }

        public uint GetCounter()
        {
            return (uint)(RawValue & 0xFFFFFFFF);
        }

        public bool IsEmpty()
        {
            return RawValue == 0;
        }

        public bool IsCreature()
        {
            return GetHigh() == HighGuid.Unit;
        }

        public bool IsPlayer()
        {
            return !IsEmpty() && GetHigh() == HighGuid.Player;
        }

        public bool IsUnit()
        {
            return IsCreature() || IsPlayer();
        }

        public TypeId GetTypeId()
        {
            switch (GetHigh())
            {
                case HighGuid.Item: return TypeId.Item;
                case HighGuid.Unit:
                case HighGuid.Pet:
                case HighGuid.Vehicle: return TypeId.Unit;
                case HighGuid.Player: return TypeId.Player;
                case HighGuid.GameObject: return TypeId.GameObject;
                case HighGuid.DynamicObject: return TypeId.DynamicObject;
                case HighGuid.Corpse: return TypeId.Corpse;
                case HighGuid.AreaTrigger: return TypeId.AreaTrigger;
                case HighGuid.Mo_Transport: return TypeId.GameObject;
                default:
                    return TypeId.Object;
            }
        }

        public override bool Equals(object? obj)
        {
            if (obj is ObjectGuid other)
                return RawValue == other.RawValue;

            return false;
        }

        public override int GetHashCode()
        {
            return RawValue.GetHashCode();
        }

        public override string ToString()
        {
            return RawValue.ToString("X16");
        }

        public byte this[int index]
        {
            get
            {
                if (index < 0 || index > 7)
                    throw new ArgumentOutOfRangeException("index must be within byte range (0 - 7)");

                return (byte)((RawValue >> (index * 8)) & 0xFF);
            }
        }

        private bool HasEntry()
        {
            switch (GetHigh())
            {
                case HighGuid.Item:
                case HighGuid.Player:
                case HighGuid.DynamicObject:
                case HighGuid.Corpse:
                case HighGuid.Mo_Transport:
                case HighGuid.Instance:
                case HighGuid.Group:
                    return false;
                default:
                    return true;
            }
        }

    }

    public class PackedGuid
    {
        private static readonly int PacketGuidMinBufferSize = 9;

        private readonly byte[] _packedGuid = new byte[PacketGuidMinBufferSize];
        private int _packedSize;

        public PackedGuid()
        {
            _packedSize = 1;
        }

        public PackedGuid(ObjectGuid guid)
        {
            Set(guid);
        }

        public void Set(ObjectGuid guid)
        {
            _packedSize = 1;
            ulong raw = guid.RawValue;

            for (byte i = 0; i < 8; ++i)
            {
                byte val = (byte)((raw >> (i * 8)) & 0xFF);
                if (val != 0)
                {
                    _packedGuid[0] |= (byte)(1 << i);
                    ++_packedSize;
                }
            }
        }

        public int Size => _packedSize;
    }
}

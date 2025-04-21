using System.Text;

namespace Packets
{
    public sealed class ClientPacketHeader
    {
        public ushort Size { get; set; }
        public uint Cmd { get; set; }
    }

    public abstract class ClientPacket
    {
        private readonly BinaryReader _binaryReader;
        private byte _bitPosition = 8;
        private byte _currentBitValue = 0;

        public ClientPacket(byte[] buffer)
        {
            _binaryReader = new(new MemoryStream(buffer));
            ReadPacket();
        }

        private bool CanRead() { return _binaryReader.BaseStream.Position < _binaryReader.BaseStream.Length; }
        protected byte ReadByte() { return _binaryReader.ReadByte(); }
        protected sbyte ReadSByte() { return _binaryReader.ReadSByte(); }
        protected short ReadInt16() { return _binaryReader.ReadInt16(); }
        protected ushort ReadUInt16() { return _binaryReader.ReadUInt16(); }
        protected int ReadInt32() { return _binaryReader.ReadInt32(); }
        protected uint ReadUInt32() { return _binaryReader.ReadUInt32(); }
        protected long ReadInt64() { return _binaryReader.ReadInt64(); }
        protected ulong ReadUInt64() { return _binaryReader.ReadUInt64(); }
        protected float ReadFloat() { return _binaryReader.ReadSingle(); }
        protected double ReadDouble() { return _binaryReader.ReadDouble(); }
        protected byte[] ReadBytes(int bytes) { return _binaryReader.ReadBytes(bytes); }

        protected string ReadString(int length)
        {
            return Encoding.UTF8.GetString(ReadBytes(length).Where(b => b != 0).ToArray());
        }

        protected string ReadCString()
        {
            List<byte> bytes = [];

            byte b;
            while (CanRead() && (b = ReadByte()) != 0)
            {
                bytes.Add(b);
            }

            return Encoding.UTF8.GetString(bytes.ToArray());
        }

        // Bit reading methods
        protected bool ReadBit()
        {
            ++_bitPosition;
            if (_bitPosition > 7)
            {
                _bitPosition = 0;
                _currentBitValue = ReadByte();
            }

            return (_currentBitValue >> 7 - _bitPosition & 1) != 0;
        }

        protected uint ReadBits(int bits)
        {
            uint value = 0;
            for (int i = bits - 1; i >= 0; --i)
            {
                if (ReadBit())
                    value |= (uint)(1 << i);
            }

            return value;
        }

        void ReadByteSeq(ref byte b)
        {
            if (b != 0)
                b ^= ReadByte();
        }

        public void ReadPacket()
        {
            Read();
            // Packet has not been fully read
            if (CanRead())
                Console.WriteLine($"[ClientPacket] packet has not been fully read. Remaining bytes: {_binaryReader.BaseStream.Length - _binaryReader.BaseStream.Position}");
        }

        protected abstract void Read();
    }
}

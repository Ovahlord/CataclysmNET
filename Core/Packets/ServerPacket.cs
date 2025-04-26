using System.Text;

namespace Core.Packets
{
    public abstract class ServerPacket
    {
        public int Cmd { get; private set; }

        private readonly BinaryWriter _binaryWriter;

        private readonly byte[] _buffer;
        private byte _bitPosition = 8;
        private byte _currentBitValue = 0;

        public ServerPacket(int bufferSize = 1024, int cmd = 0)
        {
            _buffer = new byte[bufferSize];
            _binaryWriter = new(new MemoryStream(_buffer));
            Cmd = cmd;
        }

        protected long Size { get { return _binaryWriter.BaseStream.Position; } }

        protected void WriteByte(byte value) { _binaryWriter.Write(value); }
        protected void WriteBool(bool value) { _binaryWriter.Write(value); }
        protected void WriteSByte(sbyte value) { _binaryWriter.Write(value); }
        protected void WriteInt16(short value) { _binaryWriter.Write(value); }
        protected void WriteUInt16(ushort value) { _binaryWriter.Write(value); }
        protected void WriteInt32(int value) { _binaryWriter.Write(value); }
        protected void WriteUInt32(uint value) { _binaryWriter.Write(value); }
        protected void WriteInt64(long value) { _binaryWriter.Write(value); }
        protected void WriteUInt64(ulong value) { _binaryWriter.Write(value); }
        protected void WriteFloat(float value) { _binaryWriter.Write(value); }
        protected void WriteDouble(double value) { _binaryWriter.Write(value); }
        protected void WriteBytes(byte[] value) { _binaryWriter.Write(value); }
        protected void WriteString(string value)
        {
            WriteBytes(Encoding.UTF8.GetBytes(value));
        }
        protected void WriteCString(string value)
        {
            WriteBytes(Encoding.UTF8.GetBytes(value));
            WriteByte(0);
        }

        protected void FlushBits()
        {
            if (_bitPosition == 0)
                return;

            _bitPosition = 8;
            WriteByte(_currentBitValue);
            _currentBitValue = 0;
        }

        protected bool WriteBit(bool bit)
        {
            --_bitPosition;
            if (bit)
                _currentBitValue |= (byte)(1 << _bitPosition);

            if (_bitPosition == 0)
            {
                _bitPosition = 8;
                WriteByte(_currentBitValue);
                _currentBitValue = 0;
            }

            return bit;
        }

        protected void WriteBits(ulong value, int bits)
        {
            value &= (1UL << bits) - 1;

            if (bits > _bitPosition)
            {
                _currentBitValue |= (byte)(value >> bits - _bitPosition);
                bits -= _bitPosition;
                _bitPosition = 8;
                WriteByte(_currentBitValue);

                while (bits >= 8)
                {
                    bits -= 8;
                    WriteByte((byte)(value >> bits));
                }

                _bitPosition = (byte)(8 - bits);
                _currentBitValue = (byte)((value & (1UL << bits) - 1) << _bitPosition);
            }
            else
            {
                _bitPosition -= (byte)bits;
                _currentBitValue |= (byte)(value << _bitPosition);

                if (_bitPosition == 0)
                {
                    _bitPosition = 8;
                    WriteByte(_currentBitValue);
                    _currentBitValue = 0;
                }
            }
        }

        public abstract ServerPacket Write();

        /// <summary>
        /// Returns the bytes in the underlying stream between position 0 and the current position of the stream
        /// </summary>
        /// <returns></returns>
        public byte[] GetRawPacket()
        {
            return new ReadOnlySpan<byte>(_buffer, 0, (int)_binaryWriter.BaseStream.Position).ToArray();
        }

        /// <summary>
        /// Resets the underlying stream's position back to 0, allowing to overwrite previous data
        /// </summary>
        public void Clear() { _binaryWriter.BaseStream.Position = 0; }

        public static byte[] BuildHeader(int bodySize, int cmd)
        {
            byte[] headerBuffer = new byte[5];
            byte headerIndex = 0;

            // Large packets get another byte added
            if (bodySize > short.MaxValue)
                headerBuffer[headerIndex++] = (byte)(0x80 | 0xFF & bodySize >> 16);

            headerBuffer[headerIndex++] = (byte)(0xFF & bodySize >> 8);
            headerBuffer[headerIndex++] = (byte)(0xFF & bodySize);

            if (cmd != 0)
            {
                headerBuffer[headerIndex++] = (byte)(0xFF & cmd);
                headerBuffer[headerIndex++] = (byte)(0xFF & cmd >> 8);
            }

            Array.Resize(ref headerBuffer, headerIndex);
            return headerBuffer;
        }
    }
}

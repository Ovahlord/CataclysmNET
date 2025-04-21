using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;

namespace Cryptography
{
    public sealed class ARC4
    {
        private readonly RC4Engine _cypher = new();

        public void Init(byte[] seed)
        {
            KeyParameter key = new(seed);
            _cypher.Init(true, key);
        }
        public void UpdateData(byte[] data)
        {
            _cypher.ProcessBytes(data, 0, data.Length, data, 0);
        }

    }
}

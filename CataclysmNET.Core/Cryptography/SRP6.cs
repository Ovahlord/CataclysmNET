using System.Diagnostics;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace CataclysmNET.Core.Cryptography;

// SHA1 is marked as insecure but because the older versions of the game use it, we have to tolerate its usage here
#pragma warning disable CA5350

public sealed class SRP6
{
    private static int SaltLength { get; set; } = 32;

    private static byte[] g { get; set; }
    private static byte[] N { get; set; }

    private static BigInteger _g { get; set; } // a [g]enerator for the ring of integers mod N, algorithm parameter
    private static BigInteger _N { get; set; }  // the modulus, an algorithm parameter; all operations are mod this

    static SRP6()
    {
        g = [7];
        N = HexStringToByteArray("894B645E89E1535BBDAD5B8B290650530801B18EBFBF5E8FAB3C82872A3E9BB7");
        _N = new(N, true);
        _g = new(g, true);
    }

    public SRP6(string username, byte[] salt, byte[] verifier)
    {
        _I = SHA1.HashData(Encoding.UTF8.GetBytes(username));

        byte[] buffer = new byte[32];
        Span<byte> span = new(buffer, 0, 19);
        RandomNumberGenerator.Fill(span);

        _b = new(buffer, true);
        _v = new(verifier, true);
        s = salt;
        B = _B(_b, _v);
    }

    private bool _used = false;

    // Per instanciation parameters - set on construction
    private readonly byte[] _I;      // H(I) - the username, all uppercase
    private readonly BigInteger _b;  // b - randomly chosen by the server, 19 bytes, never given out
    private readonly BigInteger _v;  // v - the user's password verifier, derived from s + H(USERNAME || ":" || PASSWORD)
    private byte[] s;          // s - the user's password salt, random, used to calculate v on registration
    private byte[] B;  // B = 3v + g^b

    public static Tuple<byte[], byte[]> GenerateRegistrationData(string userName, string password)
    {
        // Initialize Salt
        byte[] salt = new byte[SaltLength];
        RandomNumberGenerator.Fill(salt);

        // Calculate Verifier
        byte[] verifier = CalculateVerifier(userName, password, salt);
        return new(salt, verifier);
    }

    public static byte[] CalculateVerifier(string userName, string password, byte[] salt)
    {
        using SHA1 sha1 = SHA1.Create();
        // Convert username and password to uppercase and concatenate with ':'
        string userPass = userName.ToUpper() + ":" + password.ToUpper();

        // Hash the concatenated string
        byte[] userPassHash = sha1.ComputeHash(Encoding.UTF8.GetBytes(userPass));

        // Concatenate salt and userPassHash
        byte[] saltUserPassHash = new byte[salt.Length + userPassHash.Length];
        Buffer.BlockCopy(salt, 0, saltUserPassHash, 0, salt.Length);
        Buffer.BlockCopy(userPassHash, 0, saltUserPassHash, salt.Length, userPassHash.Length);

        // Hash the result again
        byte[] xHash = sha1.ComputeHash(saltUserPassHash);
        BigInteger x = new(xHash, true);

        // Calculate the verifier: v = g^x % N
        BigInteger v = BigInteger.ModPow(_g, x, _N);

        // Return the verifier as a byte array
        return v.ToByteArray(true);
    }

    public static byte[] _B(BigInteger b, BigInteger v)
    {
        // Calculate B = (g^b % N + v * 3) % N
        BigInteger B = (BigInteger.ModPow(_g, b, _N) + v * 3) % _N;
        return B.ToByteArray(true);
    }

    public static byte[] HexStringToByteArray(string hex)
    {
        int length = hex.Length;
        byte[] bytes = new byte[length / 2];
        for (int i = 0; i < length; i += 2)
        {
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        }

        return bytes;
    }

    public static byte[] SHA1Interleave(byte[] S)
    {
        int halfLength = S.Length / 2;
        byte[] buf0 = new byte[halfLength];
        byte[] buf1 = new byte[halfLength];

        for (int i = 0; i < halfLength; i++)
        {
            buf0[i] = S[2 * i];
            buf1[i] = S[2 * i + 1];
        }

        // Find the position of the first nonzero byte
        int p = 0;
        while (p < S.Length && S[p] == 0)
        {
            p++;
        }

        if (p % 2 != 0)
            p++;

        p /= 2;

        // Hash each half starting from the first nonzero byte
        byte[] hash0 = SHA1.HashData(buf0.Skip(p).ToArray());
        byte[] hash1 = SHA1.HashData(buf1.Skip(p).ToArray());

        // Combine the two hashes
        byte[] K = new byte[hash0.Length + hash1.Length];
        for (int i = 0; i < hash0.Length; i++)
        {
            K[2 * i] = hash0[i];
            K[2 * i + 1] = hash1[i];
        }

        return K;
    }

    public static byte[] GetSessionVerifier(byte[] A, byte[] clientM, byte[] K)
    {
        return SHA1.HashData(A.Concat(clientM).Concat(K).ToArray());
    }

    public bool VerifyChallengeResponse(byte[] A, byte[] clientM, out byte[]? sessionKey)
    {
        sessionKey = null;
        Debug.Assert(!_used, "A single SRP6 instance can only be used to verify once!");
        _used = true;

        BigInteger _A = new(A, true);
        if ((_A % _N).IsZero)
            return false;

        BigInteger u = new(SHA1.HashData(A.Concat(B).ToArray()), true);
        byte[] S = BigInteger.ModPow(_A * BigInteger.ModPow(_v, u, _N), _b, _N).ToByteArray(true);

        sessionKey = SHA1Interleave(S);

        // NgHash = H(N) xor H(g)
        byte[] NHash = SHA1.HashData(N);
        byte[] gHash = SHA1.HashData(g);
        byte[] NgHash = NHash.Zip(gHash, (n, g) => (byte)(n ^ g)).ToArray();

        byte[] ourM = SHA1.HashData(NgHash.Concat(_I).Concat(s).Concat(A).Concat(B).Concat(sessionKey).ToArray());
        if (ourM.SequenceEqual(clientM))
            return true;

        return false;
    }
}

#pragma warning restore CA5350

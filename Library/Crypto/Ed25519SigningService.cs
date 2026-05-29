using NSec.Cryptography;
using System.Numerics;
using System.Text;

namespace Library.Crypto
{
    public interface IEd25519SigningService
    {
        /// <summary>
        /// Generates a new Ed25519 key pair.
        /// </summary>
        /// <returns>
        /// <paramref name="publicKeyMultibase"/> – multicodec-prefixed public key encoded as multibase base58btc (z-prefix).
        /// <paramref name="privateKeyBase64"/> – raw 32-byte private key seed encoded as base64.
        /// </returns>
        (string publicKeyMultibase, string privateKeyBase64) GenerateKeyPair();

        /// <summary>
        /// Signs <paramref name="data"/> with the Ed25519 private key stored as
        /// <paramref name="privateKeyBase64"/> and returns the signature as a multibase
        /// base58btc string (z-prefix).
        /// </summary>
        string Sign(string privateKeyBase64, byte[] data);

        /// <summary>
        /// Derives the multibase public key string from a stored base64 private key seed.
        /// </summary>
        string PublicKeyMultibaseFromPrivate(string privateKeyBase64);

        /// <summary>
        /// Verifies <paramref name="signature"/> (multibase base58btc, z-prefix) over
        /// <paramref name="data"/> using the Ed25519 public key encoded in
        /// <paramref name="publicKeyMultibase"/> (multicodec-prefixed, z-prefix).
        /// Returns <c>true</c> if the signature is valid.
        /// </summary>
        bool Verify(string publicKeyMultibase, byte[] data, string signature);
    }

    /// <summary>
    /// Ed25519 signing service using NSec.Cryptography (libsodium).
    ///
    /// Handles cryptographic operations (key generation, signing, and public-key derivation)
    /// compliant with the eddsa-rdfc-2022 cryptosuite defined in the W3C Data Integrity
    /// EdDSA Cryptosuites spec (https://www.w3.org/TR/vc-di-eddsa/#eddsa-rdfc-2022).
    /// </summary>
    public class Ed25519SigningService : IEd25519SigningService
    {
        private static readonly SignatureAlgorithm _algorithm = SignatureAlgorithm.Ed25519;

        // Base58 Bitcoin alphabet (same as multibase base58btc)
        private const string Base58Alphabet = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

        // Multicodec varint prefix for ed25519-pub (code 0xed)
        // Varint encoding of 0xed: since 237 > 127, split into two bytes: (237 & 0x7f) | 0x80, 237 >> 7 → 0xed, 0x01
        private static readonly byte[] Ed25519PubMulticodecPrefix = [0xed, 0x01];

        public (string publicKeyMultibase, string privateKeyBase64) GenerateKeyPair()
        {
            using var key = Key.Create(_algorithm, new KeyCreationParameters
            {
                ExportPolicy = KeyExportPolicies.AllowPlaintextExport
            });

            var publicKeyBytes = key.PublicKey.Export(KeyBlobFormat.RawPublicKey);
            var privateKeyBytes = key.Export(KeyBlobFormat.RawPrivateKey);

            return (
                BuildPublicKeyMultibase(publicKeyBytes),
                Convert.ToBase64String(privateKeyBytes)
            );
        }

        public string PublicKeyMultibaseFromPrivate(string privateKeyBase64)
        {
            using var key = ImportKey(privateKeyBase64);
            var publicKeyBytes = key.PublicKey.Export(KeyBlobFormat.RawPublicKey);
            return BuildPublicKeyMultibase(publicKeyBytes);
        }

        public string Sign(string privateKeyBase64, byte[] data)
        {
            using var key = ImportKey(privateKeyBase64);
            var signature = _algorithm.Sign(key, data);
            return "z" + Base58Encode(signature);
        }

        public bool Verify(string publicKeyMultibase, byte[] data, string signature)
        {
            // publicKeyMultibase: 'z' + base58btc(0xed01 + 32-byte public key)
            if (!publicKeyMultibase.StartsWith('z')) return false;
            var prefixed = Base58Decode(publicKeyMultibase[1..]);
            // Strip the two-byte multicodec prefix (0xed, 0x01)
            if (prefixed.Length < Ed25519PubMulticodecPrefix.Length + 32) return false;
            var pubKeyBytes = prefixed[Ed25519PubMulticodecPrefix.Length..];
            var publicKey = NSec.Cryptography.PublicKey.Import(
                _algorithm, pubKeyBytes, KeyBlobFormat.RawPublicKey);

            // signature: 'z' + base58btc(64-byte Ed25519 signature)
            if (!signature.StartsWith('z')) return false;
            var sigBytes = Base58Decode(signature[1..]);

            return _algorithm.Verify(publicKey, data, sigBytes);
        }

        // -----------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------

        private static Key ImportKey(string privateKeyBase64)
        {
            var seed = Convert.FromBase64String(privateKeyBase64);
            return Key.Import(_algorithm, seed, KeyBlobFormat.RawPrivateKey,
                new KeyCreationParameters { ExportPolicy = KeyExportPolicies.AllowPlaintextExport });
        }

        private static string BuildPublicKeyMultibase(byte[] publicKeyBytes)
        {
            var prefixed = new byte[Ed25519PubMulticodecPrefix.Length + publicKeyBytes.Length];
            Ed25519PubMulticodecPrefix.CopyTo(prefixed, 0);
            publicKeyBytes.CopyTo(prefixed, Ed25519PubMulticodecPrefix.Length);
            return "z" + Base58Encode(prefixed);
        }

        internal static byte[] Base58Decode(string encoded)
        {
            // Count leading '1's → each becomes a leading zero byte
            int leadingZeros = 0;
            foreach (var c in encoded)
            {
                if (c != '1') break;
                leadingZeros++;
            }

            var intData = BigInteger.Zero;
            foreach (var c in encoded)
            {
                var digit = Base58Alphabet.IndexOf(c);
                if (digit < 0) throw new FormatException($"Invalid Base58 character '{c}'.");
                intData = intData * 58 + digit;
            }

            // Convert to big-endian byte array
            var bytes = intData.ToByteArray(isUnsigned: true, isBigEndian: true);
            // Prepend leading zero bytes
            if (leadingZeros == 0) return bytes;
            var result = new byte[leadingZeros + bytes.Length];
            bytes.CopyTo(result, leadingZeros);
            return result;
        }

        internal static string Base58Encode(byte[] data)
        {
            int leadingZeros = 0;
            foreach (var b in data)
            {
                if (b != 0) break;
                leadingZeros++;
            }

            var intData = new BigInteger(data, isUnsigned: true, isBigEndian: true);
            var sb = new StringBuilder();
            while (intData > BigInteger.Zero)
            {
                intData = BigInteger.DivRem(intData, 58, out var remainder);
                sb.Insert(0, Base58Alphabet[(int)remainder]);
            }

            sb.Insert(0, new string('1', leadingZeros));
            return sb.ToString();
        }
    }
}

using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace ChatGpt.Archive.Api.Services
{
    internal static class AssetSignature
    {
        private const int HashHexLength = 64;
        private static readonly byte[] SigningKeyBytes = GetSigningKeyBytes();

        private static byte[] GetSigningKeyBytes()
        {
            Console.WriteLine("Generating private signature key.");
            return RandomNumberGenerator.GetBytes(32);
        }

        internal static string Create(int rootId, string path)
        {
            var data = BuildPayload(rootId, path);
            var hash = HMACSHA256.HashData(SigningKeyBytes, data);
            return Convert.ToHexString(hash);
        }

        internal static bool IsValid(int rootId, string path, string? signature)
        {
            if (string.IsNullOrEmpty(signature) || signature.Length != HashHexLength)
                return false;

            byte[] provided;
            try
            {
                provided = Convert.FromHexString(signature);
            }
            catch (FormatException)
            {
                return false;
            }

            var data = BuildPayload(rootId, path);
            var expected = HMACSHA256.HashData(SigningKeyBytes, data);
            return CryptographicOperations.FixedTimeEquals(expected, provided);
        }

        private static byte[] BuildPayload(int rootId, string path)
        {
            var payload = string.Concat(rootId.ToString(CultureInfo.InvariantCulture), ":", path);
            return Encoding.UTF8.GetBytes(payload);
        }
    }
}
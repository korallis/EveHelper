using System;
using System.Security.Cryptography;
using System.Text;
using EveHelper.Core.Models.Authentication;

namespace EveHelper.Services.Helpers
{
    /// <summary>
    /// Helper class for generating PKCE (Proof Key for Code Exchange) data
    /// </summary>
    public static class PkceHelper
    {
        private const int CodeVerifierLength = 128;
        private const string Base64UrlChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-._~";

        /// <summary>
        /// Generates PKCE data including code verifier, challenge, and state
        /// </summary>
        /// <returns>Complete PKCE data for OAuth flow</returns>
        public static PkceData GeneratePkceData()
        {
            var codeVerifier = GenerateCodeVerifier();
            var codeChallenge = GenerateCodeChallenge(codeVerifier);
            var state = GenerateState();

            return new PkceData
            {
                CodeVerifier = codeVerifier,
                CodeChallenge = codeChallenge,
                CodeChallengeMethod = "S256",
                State = state
            };
        }

        /// <summary>
        /// Generates a cryptographically random code verifier
        /// </summary>
        /// <returns>Base64url encoded code verifier</returns>
        private static string GenerateCodeVerifier()
        {
            var buffer = new byte[CodeVerifierLength];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(buffer);

            var sb = new StringBuilder(CodeVerifierLength);
            foreach (var b in buffer)
            {
                sb.Append(Base64UrlChars[b % Base64UrlChars.Length]);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Generates a code challenge from the code verifier using SHA256
        /// </summary>
        /// <param name="codeVerifier">The code verifier to hash</param>
        /// <returns>Base64url encoded SHA256 hash of the code verifier</returns>
        private static string GenerateCodeChallenge(string codeVerifier)
        {
            var bytes = Encoding.UTF8.GetBytes(codeVerifier);
            var hash = SHA256.HashData(bytes);
            return Base64UrlEncode(hash);
        }

        /// <summary>
        /// Generates a random state parameter for CSRF protection
        /// </summary>
        /// <returns>Random state string</returns>
        private static string GenerateState()
        {
            var buffer = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(buffer);
            return Base64UrlEncode(buffer);
        }

        /// <summary>
        /// Encodes bytes as base64url (RFC 4648 Section 5)
        /// </summary>
        /// <param name="input">Bytes to encode</param>
        /// <returns>Base64url encoded string</returns>
        private static string Base64UrlEncode(byte[] input)
        {
            var base64 = Convert.ToBase64String(input);
            return base64.TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }
    }
} 
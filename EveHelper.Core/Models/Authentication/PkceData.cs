namespace EveHelper.Core.Models.Authentication
{
    /// <summary>
    /// Represents PKCE (Proof Key for Code Exchange) data for OAuth 2.0 security
    /// </summary>
    public class PkceData
    {
        /// <summary>
        /// Code verifier - a cryptographically random string
        /// </summary>
        public string CodeVerifier { get; set; } = string.Empty;

        /// <summary>
        /// Code challenge - SHA256 hash of the code verifier, base64url encoded
        /// </summary>
        public string CodeChallenge { get; set; } = string.Empty;

        /// <summary>
        /// Code challenge method (always "S256" for SHA256)
        /// </summary>
        public string CodeChallengeMethod { get; set; } = "S256";

        /// <summary>
        /// OAuth state parameter for preventing CSRF attacks
        /// </summary>
        public string State { get; set; } = string.Empty;
    }
} 
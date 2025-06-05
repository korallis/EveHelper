namespace EveHelper.Core.Models.Authentication
{
    /// <summary>
    /// Configuration for EVE Online OAuth 2.0 authentication
    /// </summary>
    public class EveAuthConfiguration
    {
        /// <summary>
        /// EVE Online OAuth 2.0 client ID
        /// </summary>
        public string ClientId { get; set; } = string.Empty;

        /// <summary>
        /// EVE Online OAuth 2.0 client secret
        /// </summary>
        public string ClientSecret { get; set; } = string.Empty;

        /// <summary>
        /// OAuth callback URL for authorization code reception
        /// </summary>
        public string CallbackUrl { get; set; } = string.Empty;

        /// <summary>
        /// EVE Online authentication server base URL
        /// </summary>
        public string AuthorizationEndpoint { get; set; } = "https://login.eveonline.com/v2/oauth/authorize";

        /// <summary>
        /// EVE Online token exchange endpoint
        /// </summary>
        public string TokenEndpoint { get; set; } = "https://login.eveonline.com/v2/oauth/token";

        /// <summary>
        /// EVE ESI API base URL
        /// </summary>
        public string EsiBaseUrl { get; set; } = "https://esi.evetech.net";

        /// <summary>
        /// Required OAuth scopes for the application
        /// </summary>
        public string[] RequiredScopes { get; set; } = new[]
        {
            "publicData",
            "esi-skills.read_skills.v1",
            "esi-skills.read_skillqueue.v1",
            "esi-characters.read_characters.v1",
            "esi-clones.read_clones.v1",
            "esi-clones.read_implants.v1"
        };

        /// <summary>
        /// Default configuration with EVE developer credentials
        /// </summary>
        public static EveAuthConfiguration Default => new()
        {
            ClientId = "3fe0363633d34abcb6bc0d50d9d2c9f8",
            ClientSecret = "Sc5A6JfqljzPiYczdNDcJ5HETqeo01ORzgXHaELQ",
            CallbackUrl = "http://localhost:5000/callback"
        };
    }
} 
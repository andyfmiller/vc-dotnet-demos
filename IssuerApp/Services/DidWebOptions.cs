namespace IssuerApp.Services
{
    /// <summary>
    /// Configuration options for the did:web server hosted by IssuerApp.
    /// </summary>
    public class DidWebOptions
    {
        public const string SectionName = "DidWeb";

        /// <summary>
        /// The default host used when no HTTP request context is available (e.g. during seeding).
        /// Should match the public hostname and port of the IssuerApp (e.g. "issuer.example.com" or "localhost:20001").
        /// </summary>
        public string DefaultHost { get; set; } = "localhost:20001";
    }
}

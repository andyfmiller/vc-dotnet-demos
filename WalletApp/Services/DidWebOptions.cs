namespace WalletApp.Services
{
    /// <summary>
    /// Configuration options for the did:web server hosted by WalletApp.
    /// </summary>
    public class DidWebOptions
    {
        public const string SectionName = "DidWeb";

        /// <summary>
        /// The default host used when no HTTP request context is available (e.g. during seeding).
        /// Should match the public hostname and port of the WalletApp (e.g. "wallet.example.com" or "localhost:22001").
        /// </summary>
        public string DefaultHost { get; set; } = "localhost:22001";
    }
}

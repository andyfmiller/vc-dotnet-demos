namespace WalletApp.Services
{
    /// <summary>
    /// Result returned by <see cref="IHolderVerificationService.GetPresentationRequestAsync"/>
    /// after completing Steps 1-3 of the VCALM exchange (obtaining the QueryByExample VPR).
    /// </summary>
    public class InteractionRequestResult
    {
        public bool Success { get; init; }
        public string? ErrorMessage { get; init; }

        /// <summary>The VC-API exchange URL to use for the VP submission (Round-trip 2).</summary>
        public string? ExchangeUrl { get; init; }

        /// <summary>The challenge from the VPR, required for the VP proof.</summary>
        public string? Challenge { get; init; }

        /// <summary>The domain from the VPR, required for the VP proof.</summary>
        public string? Domain { get; init; }

        /// <summary>
        /// Credential types extracted from the QueryByExample (excluding "VerifiableCredential").
        /// </summary>
        public string[] RequiredCredentialTypes { get; init; } = [];

        /// <summary>
        /// Achievement types extracted from the QueryByExample credentialSubject, if any.
        /// </summary>
        public string[] RequiredAchievementTypes { get; init; } = [];
    }
}

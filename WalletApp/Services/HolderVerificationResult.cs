namespace WalletApp.Services
{
    /// <summary>
    /// Result returned by <see cref="IHolderVerificationService"/> after attempting
    /// to present a credential via a VCALM Interaction URL.
    /// </summary>
    public class HolderVerificationResult
    {
        public bool Success { get; init; }
        public string? ErrorMessage { get; init; }

        /// <summary>
        /// The raw JSON of the ExchangeParticipationServerMessage returned by the verifier.
        /// Contains the verificationResult object.
        /// </summary>
        public string? ServerMessageJson { get; init; }

        /// <summary>Whether the verifier accepted the credential as valid.</summary>
        public bool? Verified { get; init; }

        /// <summary>Error strings returned by the verifier, if any.</summary>
        public string[]? VerifierErrors { get; init; }
    }
}

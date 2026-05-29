namespace WalletApp.Services
{
    /// <summary>
    /// Result returned by the mock holder exchange service after attempting to
    /// receive a credential via a VCALM Interaction URL.
    /// </summary>
    public class HolderExchangeResult
    {
        public bool Success { get; init; }
        public string? ErrorMessage { get; init; }

        /// <summary>
        /// The raw JSON of the ExchangeParticipationServerMessage returned by the issuer.
        /// Contains the VerifiablePresentation with the issued credential.
        /// </summary>
        public string? ServerMessageJson { get; init; }

        /// <summary>
        /// Pretty-printed JSON of the VerifiablePresentation extracted from the
        /// server message, for display in the UI.
        /// </summary>
        public string? VerifiablePresentationJson { get; init; }
    }
}

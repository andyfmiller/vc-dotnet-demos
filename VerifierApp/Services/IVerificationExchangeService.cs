using VerifierApp.Data.Models;

namespace VerifierApp.Services
{
    public interface IVerificationExchangeService
    {
        /// <summary>Creates a new verification exchange for the given requirement and returns the record.</summary>
        VerificationExchangeRecord CreateExchange(CredentialRequirement requirement);

        /// <summary>Returns the record for an existing exchange, or null if not found.</summary>
        VerificationExchangeRecord? GetExchange(string exchangeId);

        /// <summary>Records the wallet's InviteResponse and marks the exchange active.</summary>
        void RecordInviteResponse(string exchangeId, string? referenceId, string? url);

        /// <summary>
        /// Records the one-time challenge and sets Step to AwaitingPresentation.
        /// </summary>
        void StoreChallengeForPresentation(string exchangeId, string challenge);

        /// <summary>Records the verified holder DID on the exchange record.</summary>
        void StoreHolderDid(string exchangeId, string holderDid);

        /// <summary>
        /// Marks the exchange complete and stores the verification outcome.
        /// </summary>
        void CompleteExchange(
            string exchangeId,
            bool passed,
            string? failureReason,
            string? credentialJson,
            bool? proofValid = null,
            bool? statusValid = null,
            string? statusFailureReason = null);
    }
}

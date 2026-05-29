using WalletApp.Data.Models;

namespace WalletApp.Services
{
    /// <summary>
    /// Implements the Holder Coordinator role for a VCALM verification exchange.
    /// The holder's wallet uses this service to present a credential to a Verifier
    /// Coordinator that initiated the exchange via an Interaction URL.
    /// </summary>
    public interface IHolderVerificationService
    {
        /// <summary>
        /// Executes Steps 1-3 of the VCALM exchange: contacts the verifier, obtains
        /// the QueryByExample VPR, and returns the challenge, domain, exchange URL,
        /// and required credential/achievement types so the holder can choose which
        /// credential(s) to present.
        /// </summary>
        Task<InteractionRequestResult> GetPresentationRequestAsync(string interactionUrl);

        /// <summary>
        /// Executes Step 4-5 of the VCALM exchange: builds and signs a VP containing
        /// the selected credentials and posts it to the verifier.
        /// </summary>
        Task<HolderVerificationResult> PresentCredentialsAsync(
            InteractionRequestResult request,
            Holder holder,
            IEnumerable<HolderCredential> credentials);

        /// <summary>
        /// Convenience method that executes the full VCALM verification exchange
        /// (Steps 1-5) in one call.
        /// </summary>
        Task<HolderVerificationResult> PresentCredentialAsync(
            string interactionUrl,
            Holder holder,
            HolderCredential credential);
    }
}

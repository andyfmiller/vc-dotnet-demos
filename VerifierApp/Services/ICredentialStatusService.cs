namespace VerifierApp.Services
{
    /// <summary>
    /// Checks the live status of a credential by dereferencing its
    /// <c>credentialStatus</c> entry against the issuer's Bitstring Status List.
    /// </summary>
    public interface ICredentialStatusService
    {
        /// <summary>
        /// Evaluates all <c>BitstringStatusListEntry</c> entries in the credential JSON.
        /// Returns <c>(true, null)</c> when the credential is active, or
        /// <c>(false, reason)</c> when it is revoked or the status list cannot be fetched.
        /// When the credential contains no <c>credentialStatus</c> the result is
        /// <c>(true, null)</c> — no status to check means not revoked.
        /// </summary>
        Task<(bool Passed, string? FailureReason)> CheckStatusAsync(
            System.Text.Json.JsonElement credentialElement,
            CancellationToken cancellationToken = default);
    }
}

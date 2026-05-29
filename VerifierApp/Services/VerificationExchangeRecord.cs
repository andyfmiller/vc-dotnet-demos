namespace VerifierApp.Services
{
    /// <summary>
    /// An in-memory record tracking a single VCALM verification exchange initiated by a
    /// Holder visiting the HolderPortal.
    /// </summary>
    public class VerificationExchangeRecord
    {
        public required string ExchangeId { get; init; }
        public required string WorkflowId { get; init; }

        /// <summary>"pending" → "active" → "complete"</summary>
        public string State { get; set; } = "pending";

        /// <summary>
        /// Current step in the exchange state machine.
        /// <list type="bullet">
        ///   <item><c>AwaitingPresentation</c> — verifier sent a QueryByExample VPR; waiting for the holder's signed VP.</item>
        /// </list>
        /// Null means the exchange has not started yet (no POST received).
        /// </summary>
        public string? Step { get; set; }

        public int Sequence { get; set; }
        public DateTimeOffset Expires { get; init; }

        /// <summary>referenceId echoed back from the wallet's InviteResponse.</summary>
        public string? InviteResponseReferenceId { get; set; }

        /// <summary>Wallet callback URL from the wallet's InviteResponse (may be null).</summary>
        public string? InviteResponseUrl { get; set; }

        /// <summary>The achievement type required (from the selected CredentialRequirement).</summary>
        public string AchievementType { get; set; } = string.Empty;

        /// <summary>
        /// The credential type required (from the selected CredentialRequirement).
        /// </summary>
        public string CredentialType { get; set; } = string.Empty;

        /// <summary>The reason text shown to the holder (from the selected CredentialRequirement).</summary>
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// One-time challenge issued with the QueryByExample VPR.
        /// Verified against the proof in the wallet's VP response.
        /// </summary>
        public string? Challenge { get; set; }

        /// <summary>
        /// The holder's DID extracted from the VP proof after successful verification.
        /// </summary>
        public string? HolderDid { get; set; }

        /// <summary>
        /// Whether the last presented credential passed all verification checks.
        /// Set after Round-trip 2 completes.
        /// </summary>
        public bool? VerificationPassed { get; set; }

        /// <summary>
        /// Human-readable reason if verification failed.
        /// </summary>
        public string? VerificationFailureReason { get; set; }

        /// <summary>
        /// Whether the credential proof was valid.
        /// </summary>
        public bool? ProofValid { get; set; }

        /// <summary>
        /// Whether the credential passed the live status check.
        /// Null when no <c>credentialStatus</c> was present in the credential.
        /// </summary>
        public bool? StatusValid { get; set; }

        /// <summary>
        /// Human-readable reason for a status check failure, if any.
        /// </summary>
        public string? StatusFailureReason { get; set; }

        /// <summary>
        /// Pretty-printed JSON of the verified VerifiableCredential.
        /// Stored for display on the VerifierPortal result page.
        /// </summary>
        public string? VerifiedCredentialJson { get; set; }
    }
}

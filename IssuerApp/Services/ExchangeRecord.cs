using System;

namespace IssuerApp.Services
{
    /// <summary>
    /// An in-memory record tracking a single VCALM exchange initiated by a teacher
    /// for a specific AchievementCredential.
    /// </summary>
    public class ExchangeRecord
    {
        public required string ExchangeId { get; init; }
        public required string WorkflowId { get; init; }
        public required int AchievementCredentialKey { get; init; }

        /// <summary>"pending" → "active" → "complete"</summary>
        public string State { get; set; } = "pending";

        /// <summary>
        /// Current step in the exchange state machine.
        /// <list type="bullet">
        ///   <item><c>AwaitingDIDAuth</c> — issuer sent a DIDAuthentication VPR; waiting for the holder's signed VP.</item>
        ///   <item><c>AwaitingIssuance</c> — DID proven; credential not yet delivered.</item>
        /// </list>
        /// Null means the exchange has not started yet.
        /// </summary>
        public string? Step { get; set; }

        public int Sequence { get; set; }
        public DateTimeOffset Expires { get; init; }

        /// <summary>referenceId echoed back from the wallet's InviteResponse.</summary>
        public string? InviteResponseReferenceId { get; set; }

        /// <summary>Wallet callback URL from the wallet's InviteResponse (may be null).</summary>
        public string? InviteResponseUrl { get; set; }

        /// <summary>
        /// One-time challenge issued with the DIDAuthentication VPR.
        /// Verified against the proof in the wallet's VP response.
        /// </summary>
        public string? Challenge { get; set; }

        /// <summary>
        /// The holder's DID proven via DIDAuthentication.
        /// Set after the holder's VP proof is successfully verified.
        /// Used as CredentialSubject.Id when the credential is signed.
        /// </summary>
        public string? HolderDid { get; set; }
    }
}

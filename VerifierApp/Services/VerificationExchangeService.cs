using System.Collections.Concurrent;
using VerifierApp.Data.Models;

namespace VerifierApp.Services
{
    /// <summary>
    /// In-memory verification exchange store. Registered as a singleton so exchanges
    /// survive across requests for the lifetime of the process.
    /// </summary>
    public class VerificationExchangeService : IVerificationExchangeService
    {
        /// <summary>
        /// A stable workflow identifier used for all credential verification exchanges.
        /// In a production system this would be configurable per workflow definition.
        /// </summary>
        public const string WorkflowId = "verify-credential";

        private readonly ConcurrentDictionary<string, VerificationExchangeRecord> _exchanges = new();

        public VerificationExchangeRecord CreateExchange(CredentialRequirement requirement)
        {
            var exchangeId = Guid.NewGuid().ToString("N");
            var record = new VerificationExchangeRecord
            {
                ExchangeId = exchangeId,
                WorkflowId = WorkflowId,
                Expires = DateTimeOffset.UtcNow.AddHours(24),
                AchievementType = requirement.AchievementType,
                CredentialType = requirement.CredentialType,
                Reason = requirement.Reason
            };
            _exchanges[exchangeId] = record;
            return record;
        }

        public VerificationExchangeRecord? GetExchange(string exchangeId) =>
            _exchanges.TryGetValue(exchangeId, out var record) ? record : null;

        public void RecordInviteResponse(string exchangeId, string? referenceId, string? url)
        {
            if (_exchanges.TryGetValue(exchangeId, out var record))
            {
                record.InviteResponseReferenceId = referenceId;
                record.InviteResponseUrl = url;
                record.State = "active";
            }
        }

        public void StoreChallengeForPresentation(string exchangeId, string challenge)
        {
            if (_exchanges.TryGetValue(exchangeId, out var record))
            {
                record.Challenge = challenge;
                record.Step = "AwaitingPresentation";
            }
        }

        public void StoreHolderDid(string exchangeId, string holderDid)
        {
            if (_exchanges.TryGetValue(exchangeId, out var record))
            {
                record.HolderDid = holderDid;
            }
        }

        public void CompleteExchange(
            string exchangeId, bool passed, string? failureReason, string? credentialJson,
            bool? proofValid = null, bool? statusValid = null, string? statusFailureReason = null)
        {
            if (_exchanges.TryGetValue(exchangeId, out var record))
            {
                record.VerificationPassed = passed;
                record.VerificationFailureReason = failureReason;
                record.VerifiedCredentialJson = credentialJson;
                record.ProofValid = proofValid;
                record.StatusValid = statusValid;
                record.StatusFailureReason = statusFailureReason;
                record.State = "complete";
                record.Sequence++;
            }
        }
    }
}

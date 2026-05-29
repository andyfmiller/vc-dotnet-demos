using System;
using System.Collections.Concurrent;

namespace IssuerApp.Services
{
    /// <summary>
    /// In-memory exchange store. Registered as a singleton so exchanges survive
    /// across requests for the lifetime of the process.
    /// </summary>
    public class ExchangeService : IExchangeService
    {
        /// <summary>
        /// A stable workflow identifier used for all AchievementCredential issuance exchanges.
        /// In a production system this would be persisted and configurable per workflow definition.
        /// </summary>
        public const string WorkflowId = "sample-school";

        private readonly ConcurrentDictionary<string, ExchangeRecord> _exchanges = new();

        public ExchangeRecord CreateExchange(int achievementCredentialKey)
        {
            var exchangeId = Guid.NewGuid().ToString("N");
            var record = new ExchangeRecord
            {
                ExchangeId = exchangeId,
                WorkflowId = WorkflowId,
                AchievementCredentialKey = achievementCredentialKey,
                Expires = DateTimeOffset.UtcNow.AddHours(24)
            };
            _exchanges[exchangeId] = record;
            return record;
        }

        public ExchangeRecord? GetExchange(string exchangeId) =>
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

        public void CompleteExchange(string exchangeId)
        {
            if (_exchanges.TryGetValue(exchangeId, out var record))
            {
                record.State = "complete";
                record.Sequence++;
            }
        }

        public void StoreChallengeForDIDAuth(string exchangeId, string challenge)
        {
            if (_exchanges.TryGetValue(exchangeId, out var record))
            {
                record.Challenge = challenge;
                record.Step = "AwaitingDIDAuth";
            }
        }

        public void StoreHolderDid(string exchangeId, string holderDid)
        {
            if (_exchanges.TryGetValue(exchangeId, out var record))
            {
                record.HolderDid = holderDid;
                record.Step = "AwaitingIssuance";
            }
        }
    }
}

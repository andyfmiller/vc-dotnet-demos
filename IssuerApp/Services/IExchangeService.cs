namespace IssuerApp.Services
{
    public interface IExchangeService
    {
        /// <summary>Creates a new exchange for the given credential and returns the record.</summary>
        ExchangeRecord CreateExchange(int achievementCredentialKey);

        /// <summary>Returns the record for an existing exchange, or null if not found.</summary>
        ExchangeRecord? GetExchange(string exchangeId);

        /// <summary>Records the wallet's InviteResponse and marks the exchange active.</summary>
        void RecordInviteResponse(string exchangeId, string? referenceId, string? url);

        /// <summary>Marks the exchange complete after the credential has been delivered.</summary>
        void CompleteExchange(string exchangeId);

        /// <summary>
        /// Records the one-time challenge on the exchange and sets Step to AwaitingDIDAuth.
        /// </summary>
        void StoreChallengeForDIDAuth(string exchangeId, string challenge);

        /// <summary>Stores the holder's proven DID on the exchange record.</summary>
        void StoreHolderDid(string exchangeId, string holderDid);
    }
}

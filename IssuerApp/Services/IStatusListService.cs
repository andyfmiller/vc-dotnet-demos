namespace IssuerApp.Services
{
    /// <summary>
    /// Manages a single Bitstring Status List credential per the
    /// <see href="https://www.w3.org/TR/vc-bitstring-status-list/">VC Bitstring Status List</see>
    /// specification.
    ///
    /// Each issued credential gets a unique <c>statusListIndex</c> assigned by
    /// <see cref="AllocateIndex"/>.  The issuer flips bits via <see cref="SetStatus"/>
    /// and consumers can fetch the encoded list via <see cref="GetEncodedStatusList"/>.
    ///
    /// For this demo the list is kept in memory and is scoped to the process lifetime.
    /// </summary>
    public interface IStatusListService
    {
        /// <summary>
        /// Allocates the next available index in the status list.
        /// Returns the zero-based integer index assigned to the credential.
        /// </summary>
        int AllocateIndex();

        /// <summary>
        /// Sets the revocation bit for the credential at <paramref name="index"/>.
        /// When <paramref name="revoked"/> is <c>true</c> the bit is set to 1 (revoked);
        /// when <c>false</c> the bit is cleared (active / not revoked).
        /// </summary>
        void SetStatus(int index, bool revoked);

        /// <summary>
        /// Returns <c>true</c> if the credential at <paramref name="index"/> is currently revoked.
        /// </summary>
        bool IsRevoked(int index);

        /// <summary>
        /// Returns the GZIP-compressed bitstring encoded as base64url multibase.
        /// The string is suitable for the <c>encodedList</c> property of a
        /// <c>BitstringStatusListCredential</c>.
        /// </summary>
        string GetEncodedStatusList();

        /// <summary>
        /// The fixed URL at which the status list credential is served.
        /// Used as the value of <c>credentialStatus.statusListCredential</c> in issued credentials.
        /// </summary>
        string StatusListCredentialUrl { get; }

        /// <summary>
        /// The fixed URL used as the <c>id</c> property of the status list entry embedded in the
        /// issued credential (<c>credentialStatus.id</c>).
        /// Includes a fragment that identifies the purpose.
        /// </summary>
        string StatusListEntryBaseUrl { get; }
    }
}

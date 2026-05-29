# VCALM Issuance Workflow

End-to-end walkthrough of how **IssuerApp** issues a signed `AchievementCredential` (Open Badges 3.0) to **WalletApp** using the [VCALM exchange protocol](https://w3c.github.io/vcalm/).

---

## Actors

| Actor | Role | App |
|---|---|---|
| **Holder** | The person requesting the credential (e.g. a student) | Browser + WalletApp |
| **IssuerApp** | Credential issuer; hosts VCALM server endpoints | IssuerApp |
| **WalletApp** | Holder Coordinator; drives the VCALM protocol on the Holder's behalf | WalletApp |

---

## Flow diagram

```
Holder (browser)          WalletApp                         IssuerApp
     |                        |                                  |
     |-- Request credential -->|                                  |
     |  (IssuerApp HolderPortal)                                  |
     |<-- Interaction URL + QR-|                                  |
     |                        |                                  |
     |-- Paste / scan URL ---->|                                  |
     |                        |-- GET /interactions/{id}?iuv=1 ->|
     |                        |<-- Protocols {vcapi, inviteReq} --|
     |                        |                                  |
     |                        |-- POST /invite-request/response ->|
     |                        |<-- 200 OK ------------------------|
     |                        |                                  |
     |                        |-- POST /exchanges/{id}  {}  ---->|
     |                        |<-- VPR {DIDAuthentication,  -----|
     |                        |         challenge, domain}       |
     |                        |                                  |
     |                        | [sign DIDAuth VP with Holder key]|
     |                        |-- POST /exchanges/{id} {VP}  --->|
     |                        |   [verify sig → extract holderDID|
     |                        |    sign credential w/ Org key]   |
     |                        |<-- serverMsg { VP { VC } }  -----|
     |                        |                                  |
     |<-- Credential stored ---|                                  |
```

---

## Step-by-step

### Step 1 — Holder requests a credential (IssuerApp HolderPortal)

**Code:** `IssuerApp/Pages/HolderPortal/Issue.cshtml.cs` → `OnPostRequestAsync`

1. The authenticated Holder visits the HolderPortal and clicks **Request Credential** for a specific `AchievementCredential`.
2. IssuerApp calls `ExchangeService.CreateExchange(achievementCredentialKey)`, which:
   - Generates a UUID `exchangeId`.
   - Associates the exchange with workflow ID `"sample-school"`.
   - Sets initial state: `State = "pending"`, `Step = null`.
   - Stores the record in an in-memory `ConcurrentDictionary` with a 24-hour expiry.
3. IssuerApp constructs an **Interaction URL** (VCALM §3.7.1):
   ```
   https://<issuerhost>/interactions/{exchangeId}?iuv=1
   ```
4. The Interaction URL and a QR code are displayed to the Holder.

---

### Step 2 — Holder opens WalletApp and submits the Interaction URL

**Code:** `WalletApp/Pages/Exchange/Receive.cshtml.cs`

The Holder navigates to `/Exchange/Receive` in WalletApp, pastes or scans the Interaction URL, and submits the form. WalletApp delegates the entire multi-step protocol to `HolderExchangeService.ReceiveCredentialAsync(interactionUrl, holder)`.

---

### Step 3 — WalletApp GETs the Interaction URL → Protocols object

**Code:** `WalletApp/Services/HolderExchangeService.cs` (Step 1)  
**Handled by:** `IssuerApp/Controllers/VcapiController.cs` → `GetInteraction`

```
GET /interactions/{exchangeId}?iuv=1
```

IssuerApp returns a **Protocols** object:

```json
{
  "vcapi": "https://<issuerhost>/workflows/sample-school/exchanges/{exchangeId}",
  "inviteRequest": "https://<issuerhost>/{exchangeId}/invite-request/response"
}
```

---

### Step 4 — WalletApp POSTs an InviteResponse

**Code:** `WalletApp/Services/HolderExchangeService.cs` (Step 2)  
**Handled by:** `IssuerApp/Controllers/VcapiController.cs` → `ReceiveInviteResponse`

```
POST /{exchangeId}/invite-request/response
Body: { "purpose": "AchievementCredential issuance", "referenceId": "urn:uuid:{newGuid}" }
```

IssuerApp records the InviteResponse and advances `State` to `"active"`. Returns `200 OK`.

---

### Step 5 — Round-trip 1: WalletApp POSTs `{}` → DIDAuthentication VPR

**Code:** `WalletApp/Services/HolderExchangeService.cs` (Step 3)  
**Handled by:** `IssuerApp/Controllers/VcapiController.cs` → `ParticipateInExchange`

```
POST /workflows/sample-school/exchanges/{exchangeId}
Body: {}
```

Because `exchange.Step` is `null` (first POST), IssuerApp:

1. Generates a one-time UUID `challenge`.
2. Captures `domain` from `Request.Host`.
3. Sets `exchange.Step = "AwaitingDIDAuth"`.
4. Returns a **VerifiablePresentationRequest** (VPR):

```json
{
  "referenceId": "urn:uuid:{newGuid}",
  "verifiablePresentationRequest": {
    "query": [{ "type": "DIDAuthentication" }],
    "challenge": "{challenge}",
    "domain": "{issuerHost}"
  }
}
```

---

### Step 6 — Round-trip 2: WalletApp signs a DIDAuth VP and POSTs it back

**Code:** `WalletApp/Services/HolderExchangeService.cs` (Step 4)  
**Handled by:** `IssuerApp/Controllers/VcapiController.cs` → `ParticipateInExchange`

#### WalletApp — building and signing the VP

1. Constructs an unsigned VP:
   ```json
   {
     "@context": ["https://www.w3.org/ns/credentials/v2"],
     "type": ["VerifiablePresentation"],
     "holder": "{holderDid}"
   }
   ```
2. Constructs proof options (`eddsa-rdfc-2022`, `proofPurpose: "authentication"`, `challenge`, `domain`).
3. Canonicalizes both documents (RDF Dataset Normalization).
4. Computes `proofInput = SHA-256(proofOptions) ‖ SHA-256(vp)` and signs with the Holder's **Ed25519** private key.
5. Attaches the proof and POSTs the signed VP.

#### IssuerApp — verifying the DIDAuth VP

Because `exchange.Step == "AwaitingDIDAuth"`, IssuerApp:

1. Extracts `proof.verificationMethod` (format: `{holderDid}#key-1`) and derives `holderDid`.
2. Resolves the Holder's **did:web** DID document over HTTP to obtain their `publicKeyMultibase`.
3. Re-canonicalizes the VP and proof options, reconstructs `proofInput`, and verifies the Ed25519 signature.
4. On success, records `holderDid` on the exchange.

---

### Step 7 — IssuerApp issues and signs the credential

Runs immediately after a successful DIDAuth verification in the same request.

1. Loads the `AchievementCredential` from the database.
2. Sets `credential.CredentialSubject.Id = holderDid` (the cryptographically proven Holder DID).
3. Sets `credential.Issuer.Id` to a **did:key** derived from the Organization's Ed25519 public key.
4. Allocates a **status list index** via `IStatusListService.AllocateIndex()` and persists it on the `AchievementCredential` entity (`StatusListIndex` column).
5. Embeds a `credentialStatus` entry of type `BitstringStatusListEntry`:
   ```json
   {
     "id": "https://<issuerhost>/status-lists/1#revocation",
     "type": "BitstringStatusListEntry",
     "statusPurpose": "revocation",
     "statusListIndex": "42",
     "statusListCredential": "https://<issuerhost>/status-lists/1"
   }
   ```
6. Signs the credential with `eddsa-rdfc-2022` (same `proofInput` construction).
7. Wraps the signed credential in an unsigned VerifiablePresentation and returns it:
   ```json
   {
     "referenceId": "urn:uuid:{newGuid}",
     "verifiablePresentation": {
       "@context": ["https://www.w3.org/ns/credentials/v2"],
       "type": ["VerifiablePresentation"],
       "verifiableCredential": [ { "...signed AchievementCredential with credentialStatus..." } ]
     }
   }
   ```
8. Sets `exchange.State = "complete"`.

---

### Step 8 — WalletApp stores the credential

**Code:** `WalletApp/Services/HolderExchangeService.cs` (Step 5)

WalletApp parses the server message, extracts the VerifiablePresentation JSON, and persists it as a `HolderCredential` record linked to the current Holder. If a credential with the same `credentialId` already exists in the wallet, the stored record is **replaced** rather than skipped — the previous credential JSON is archived in `PreviousCredentialJson` and `ReplacedAt` is set, enabling the wallet to display a diff of what changed. The Holder can view the credential and any replacement history in their wallet.

---

## Bitstring Status List endpoint

**Code:** `IssuerApp/Controllers/VcapiController.cs` → `GetStatusList`

IssuerApp exposes a public endpoint that verifiers use to check live revocation status:

```
GET /status-lists/1
```

Returns a signed **BitstringStatusListCredential** document:

```json
{
  "@context": [
    "https://www.w3.org/ns/credentials/v2",
    "https://w3id.org/vc/status-list/2021/v1"
  ],
  "type": ["VerifiableCredential", "BitstringStatusListCredential"],
  "issuer": { "id": "did:key:..." },
  "credentialSubject": {
    "type": "BitstringStatusList",
    "statusPurpose": "revocation",
    "encodedList": "<base64url-gzip-compressed-bitstring>"
  }
}
```

The `encodedList` is a GZIP-compressed, base64url-encoded bitstring of 131,072 bits (the VCALM/Bitstring Status List minimum). Each bit corresponds to the `statusListIndex` embedded in a credential's `credentialStatus` entry. A `1` bit means the credential at that index is revoked.

---

## Issuer admin: revocation and reactivation

**Code:** `IssuerApp/Pages/Admin/AchievementCredentials/Edit.cshtml.cs`

Admins can revoke or reactivate an issued credential from the Edit page:

- **Revoke** — calls `IStatusListService.SetStatus(index, revoked: true)`, flipping the credential's bit to `1`. Subsequent verifier status checks will return a failure for this credential.
- **Activate** — calls `IStatusListService.SetStatus(index, revoked: false)`, clearing the bit back to `0`.

The Edit page also shows the current `IsRevoked` state based on the live in-memory status list.

> **Note:** The status list is held in memory for the lifetime of the IssuerApp process. In a production system it would be persisted to durable storage.

---

## Key files

| File | Purpose |
|---|---|
| `IssuerApp/Pages/HolderPortal/Issue.cshtml(.cs)` | Holder UI: request credential, view Interaction URL |
| `IssuerApp/Controllers/VcapiController.cs` | VCALM server endpoints; issues credentials with `credentialStatus`; serves `/status-lists/1` |
| `IssuerApp/Services/StatusListService.cs` | In-memory Bitstring Status List (index allocation, revocation, encoded list generation) |
| `IssuerApp/Pages/Admin/AchievementCredentials/Edit.cshtml(.cs)` | Admin UI: revoke/reactivate credentials |
| `IssuerApp/Data/Models/OpenBadges/AchievementCredential.cs` | Issuer credential entity; includes `StatusListIndex` |
| `WalletApp/Pages/Exchange/Receive.cshtml(.cs)` | Holder UI: submit Interaction URL, view received/updated credential |
| `WalletApp/Services/HolderExchangeService.cs` | Holder Coordinator — drives all VCALM issuance steps; replaces duplicate credentials |

## Implementation notes

| Concern | Detail |
|---|---|
| **Exchange store** | In-memory `ConcurrentDictionary` in `ExchangeService` (singleton); not persisted across restarts |
| **Signing algorithm** | `eddsa-rdfc-2022` (Ed25519 + RDF Dataset Canonicalization + SHA-256) for both DIDAuth VP and issued credential |
| **Holder DID method** | `did:web` — resolved over HTTP by IssuerApp |
| **Issuer DID method** | `did:key` — derived on-the-fly; no HTTP resolution needed |
| **VP wrapper** | The outer VP returned to WalletApp is **unsigned**; only the inner VC carries a proof |
| **Exchange expiry** | 24 hours from creation |
| **Workflow ID** | `"sample-school"` (constant in `ExchangeService`) |

## Key files

| File | Purpose |
|---|---|
| `IssuerApp/Pages/HolderPortal/Issue.cshtml(.cs)` | Holder UI: request credential, display Interaction URL |
| `IssuerApp/Controllers/VcapiController.cs` | VCALM server endpoints (Protocols, InviteResponse, exchange participation) |
| `IssuerApp/Services/ExchangeService.cs` | In-memory exchange store (singleton) |
| `WalletApp/Pages/Exchange/Receive.cshtml(.cs)` | Holder UI: paste Interaction URL, trigger issuance |
| `WalletApp/Services/HolderExchangeService.cs` | Holder Coordinator — drives all VCALM steps |

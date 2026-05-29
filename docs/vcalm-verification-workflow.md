# VCALM Verification Workflow

End-to-end walkthrough of how **WalletApp** presents a stored `AchievementCredential` (Open Badges 3.0) to **VerifierApp** using the [VCALM exchange protocol](https://w3c.github.io/vcalm/).

---

## Actors

| Actor | Role | App |
|---|---|---|
| **Verifier** | The organization requesting proof of a credential | Browser + VerifierApp |
| **VerifierApp** | Verifier Coordinator; hosts VCALM server endpoints | VerifierApp |
| **Holder** | The person presenting their credential | Browser + WalletApp |
| **WalletApp** | Holder Coordinator; drives the VCALM protocol on the Holder's behalf | WalletApp |
| **IssuerApp** | Credential issuer; publishes the Bitstring Status List | IssuerApp |

---

## Design decisions

| Concern | Decision |
|---|---|
| **VP proof** | Required — the Holder must sign the Verifiable Presentation |
| **Credential selection** | Holder chooses which stored credential to present |
| **Issuer trust** | "Signature-valid-from-any" — any issuer whose Ed25519 signature verifies is accepted |
| **Credential type required** | `AchievementCredential` with `achievementType: License` (hard-coded in VerifierApp) |
| **Credential status** | Checked live via the issuer's Bitstring Status List; revocation is authoritative regardless of wallet state |

---

## Flow diagram

```
Holder (browser)          WalletApp                  VerifierApp            IssuerApp
      |                       |                           |                      |
      | 1. Visit HolderPortal/Require                     |                      |
      |   Verifier clicks "Start" ──────────────────────>|                      |
      |   Interaction URL returned ◄─────────────────────|                      |
      |                       |                           |                      |
      | 2. Paste Interaction URL                          |                      |
      |──────────────────────>|                           |                      |
      | 3. Select credential  |                           |                      |
      |──────────────────────>|                           |                      |
      |                       |                           |                      |
      |                       |-- GET /interactions/{id}?iuv=1 ───────────────>|  (VerifierApp)
      |                       |<-- Protocols {vcapi, inviteReq} ───────────────|
      |                       |                           |                      |
      |                       |-- POST /invite-request/response ───────────────>|
      |                       |<-- 200 OK ──────────────────────────────────────|
      |                       |                           |                      |
      |                       |-- POST /exchanges/{id}  {}  ──────────────────>|
      |                       |<-- QueryByExample VPR + challenge ──────────────|
      |                       |                           |                      |
      |                       | [build + sign VP with Holder key]               |
      |                       |-- POST /exchanges/{id} {VP}  ─────────────────>|
      |                       |   [Step A: verify VP proof (holderDid)]         |
      |                       |   [Step B: verify VC proof (issuerDid)]         |
      |                       |   [Step C: fetch + check credentialStatus] ─────────────────>|
      |                       |                           |   GET /status-lists/1             |
      |                       |                           |<── BitstringStatusList credential ─|
      |                       |<-- verificationResult  ──|                      |
      |                       |   {verified, checks[proof, credentialStatus]}   |
      |                       |                           |                      |
      |<-- Display result ----|                           |                      |
      |   (proof + status badges, auto-polling)           |                      |
```

---

## Step-by-step

### Step 1 — Verifier generates an Interaction URL (VerifierApp HolderPortal)

**Code:** `VerifierApp/Pages/HolderPortal/Require.cshtml.cs` → `OnPostStartAsync`

1. The authenticated Verifier visits the HolderPortal and clicks **Start Verification**.
2. VerifierApp calls `IVerificationExchangeService.CreateExchange()`, which:
   - Generates a UUID `exchangeId`.
   - Associates the exchange with workflow ID `"verify-credential"`.
   - Sets initial state: `State = "pending"`, `Step = null`.
   - Stores the record in an in-memory `ConcurrentDictionary` with a 24-hour expiry.
3. VerifierApp constructs an **Interaction URL** (VCALM §3.7.1):
   ```
   https://<verifierhost>/interactions/{exchangeId}?iuv=1
   ```
4. The Interaction URL and a QR code are displayed. A JavaScript polling loop begins, checking every 3 seconds for the exchange to complete. When the wallet responds, the page automatically navigates to the result view.

---

### Step 2 — Holder opens WalletApp and submits the Interaction URL

**Code:** `WalletApp/Pages/Exchange/Present.cshtml.cs` → `OnPostAsync`

The Holder navigates to `/Exchange/Present` in WalletApp, pastes or scans the Interaction URL, and clicks **Load My Credentials**. WalletApp loads the Holder's stored credentials for selection.

The Holder selects a credential and clicks **Present Credential**. WalletApp delegates the entire multi-step protocol to `HolderVerificationService.PresentCredentialAsync(interactionUrl, holder, credential)`.

---

### Step 3 — WalletApp GETs the Interaction URL → Protocols object

**Code:** `WalletApp/Services/HolderVerificationService.cs` (Step 1)  
**Handled by:** `VerifierApp/Controllers/VcapiController.cs` → `GetInteraction`

```
GET /interactions/{exchangeId}?iuv=1
```

VerifierApp returns a **Protocols** object:

```json
{
  "vcapi": "https://<verifierhost>/workflows/verify-credential/exchanges/{exchangeId}",
  "inviteRequest": "https://<verifierhost>/{exchangeId}/invite-request/response"
}
```

---

### Step 4 — WalletApp POSTs an InviteResponse

**Code:** `WalletApp/Services/HolderVerificationService.cs` (Step 2)  
**Handled by:** `VerifierApp/Controllers/VcapiController.cs` → `ReceiveInviteResponse`

```
POST /{exchangeId}/invite-request/response
Body: { "purpose": "AchievementCredential verification", "referenceId": "urn:uuid:{newGuid}" }
```

VerifierApp records the InviteResponse and advances `State` to `"active"`. Returns `200 OK`.

---

### Step 5 — Round-trip 1: WalletApp POSTs `{}` → QueryByExample VPR

**Code:** `WalletApp/Services/HolderVerificationService.cs` (Step 3)  
**Handled by:** `VerifierApp/Controllers/VcapiController.cs` → `ParticipateInExchange`

```
POST /workflows/verify-credential/exchanges/{exchangeId}
Body: {}
```

Because `exchange.Step` is `null` (first POST), VerifierApp:

1. Generates a one-time UUID `challenge` and captures `domain` from `Request.Host`.
2. Sets `exchange.Step = "AwaitingPresentation"`.
3. Returns a **VerifiablePresentationRequest** with a `QueryByExample` query:

```json
{
  "referenceId": "urn:uuid:{newGuid}",
  "verifiablePresentationRequest": {
    "query": [{
      "type": "QueryByExample",
      "credentialQuery": {
        "reason": "Please present your License credential to proceed.",
        "example": {
          "@context": [
            "https://www.w3.org/ns/credentials/v2",
            "https://purl.imsglobal.org/spec/ob/v3p0/context-3.0.3.json"
          ],
          "type": ["VerifiableCredential", "AchievementCredential"],
          "credentialSubject": { "achievement": { "achievementType": "License" } }
        },
        "acceptedCryptosuites": [{ "cryptosuite": "eddsa-rdfc-2022" }]
      }
    }],
    "challenge": "{challenge}",
    "domain": "{verifierHost}"
  }
}
```

---

### Step 6 — Round-trip 2: WalletApp builds and signs a VP, POSTs it back

**Code:** `WalletApp/Services/HolderVerificationService.cs` (Step 4)  
**Handled by:** `VerifierApp/Controllers/VcapiController.cs` → `ParticipateInExchange`

#### WalletApp — building and signing the VP

1. Constructs an unsigned VP embedding the selected stored credential:
   ```json
   {
     "@context": ["https://www.w3.org/ns/credentials/v2"],
     "type": ["VerifiablePresentation"],
     "holder": "{holderDid}",
     "verifiableCredential": [ { "...stored credential JSON..." } ]
   }
   ```
2. Constructs proof options (`eddsa-rdfc-2022`, `proofPurpose: "authentication"`, `challenge`, `domain`).
3. Canonicalizes both documents (RDF Dataset Normalization).
4. Computes `proofInput = SHA-256(proofOptions) ‖ SHA-256(vp)` and signs with the Holder's **Ed25519** private key.
5. Attaches the proof and POSTs the signed VP.

---

### Step 7 — VerifierApp verifies the VP proof (Step A)

Because `exchange.Step == "AwaitingPresentation"`, VerifierApp:

1. Extracts `proof.verificationMethod` (format: `{holderDid}#key-1`) and derives `holderDid`.
2. Resolves the Holder's **did:web** DID document over HTTP to obtain their `publicKeyMultibase`.
3. Re-canonicalizes the VP and proof options, reconstructs `proofInput`, and verifies the Ed25519 signature.
4. Checks that `proof.challenge` matches the stored challenge.
5. On success, records `holderDid` on the exchange.

> If VP proof verification fails, VerifierApp returns `400 Bad Request` immediately.

---

### Step 8 — VerifierApp verifies the enclosed credential proof (Step B)

1. Extracts the first element of `verifiablePresentation.verifiableCredential[]`.
2. Resolves the issuer's DID:
   - **did:key** — public key decoded inline from the DID itself (no HTTP needed).
   - **did:web** — DID document fetched from `https://{host}/.well-known/did.json`.
3. Re-canonicalizes the credential and its proof options, reconstructs `proofInput`, and verifies the Ed25519 signature.
4. Records `proofValid` on the exchange record.

> **Issuer trust model:** "signature-valid-from-any" — no issuer allowlist is enforced.

---

### Step 9 — VerifierApp checks live credential status (Step C)

**Code:** `VerifierApp/Services/CredentialStatusService.cs`

After the credential proof is evaluated, VerifierApp independently checks the credential's live revocation status. This check is performed regardless of whether the Holder has the latest version of the credential in their wallet.

1. Reads the `credentialStatus` property from the presented credential. If absent, the status check is skipped (recorded as `StatusValid = null`).
2. For each entry of type `BitstringStatusListEntry`:
   - Extracts `statusListCredential` (the issuer's status list URL) and `statusListIndex` (the credential's bit position).
   - Fetches the **Bitstring Status List credential** from the issuer:
     ```
     GET https://<issuerhost>/status-lists/1
     ```
   - Decodes `credentialSubject.encodedList` (base64url → base64 → gunzip → raw bit array).
   - Checks the bit at `statusListIndex`. A set bit (`1`) means the credential is **revoked**.
3. Records `statusValid` and `statusFailureReason` on the exchange record.

> **Key principle:** The status check is authoritative and independent of the wallet. A credential that has been revoked by the issuer will fail verification even if the wallet holds an older copy that was valid when it was received.

```
credentialStatus entry in the VC:
{
  "id": "https://<issuerhost>/status-lists/1#revocation",
  "type": "BitstringStatusListEntry",
  "statusPurpose": "revocation",
  "statusListIndex": "42",
  "statusListCredential": "https://<issuerhost>/status-lists/1"
}
```

---

### Step 10 — VerifierApp returns the verificationResult

VerifierApp responds to the Round-trip 2 POST with a result that enumerates each check separately:

**All checks passed (proof valid + credential active):**
```json
{
  "referenceId": "urn:uuid:{newGuid}",
  "verificationResult": {
    "verified": true,
    "checks": ["proof", "credentialStatus"],
    "errors": []
  }
}
```

**Proof valid but credential revoked:**
```json
{
  "referenceId": "urn:uuid:{newGuid}",
  "verificationResult": {
    "verified": false,
    "checks": ["proof"],
    "errors": ["Credential has been revoked (statusListIndex=42)."]
  }
}
```

**Proof invalid:**
```json
{
  "referenceId": "urn:uuid:{newGuid}",
  "verificationResult": {
    "verified": false,
    "checks": [],
    "errors": ["Signature verification failed."]
  }
}
```

**No `credentialStatus` present (older credential):**
```json
{
  "referenceId": "urn:uuid:{newGuid}",
  "verificationResult": {
    "verified": true,
    "checks": ["proof"],
    "errors": []
  }
}
```

---

### Step 11 — WalletApp displays the result

**Code:** `WalletApp/Services/HolderVerificationService.cs` (Step 5)  
**Displayed by:** `WalletApp/Pages/Exchange/Present.cshtml`

WalletApp reads `verificationResult.verified` and renders either a green "✓ Credential verified successfully" alert or a red "✗ Credential verification failed" alert with any verifier-reported error messages.

---

### Step 12 — VerifierApp displays the result (polling)

**Displayed by:** `VerifierApp/Pages/HolderPortal/Require.cshtml`

The Verifier's browser has been polling `?handler=Result&exchangeId={id}` every 3 seconds. Once `exchange.State == "complete"`, the page reloads and displays:

| Section | Content |
|---|---|
| **Overall result banner** | Green "Verification Passed" or red "Verification Failed" |
| **Proof** badge | `Valid` (green) or `Invalid` (red) with failure reason |
| **Credential Status** badge | `Active` (green), `Revoked / Invalid` (red), or `Not checked` (grey) with reason |
| **Holder DID** | The cryptographically proven DID of the Holder |
| **Verified Credential** | Pretty-printed JSON of the presented credential |

---

## Exchange state machine (VerifierApp)

| `State` | `Step` | Meaning |
|---|---|---|
| `pending` | `null` | Exchange created; no wallet contact yet |
| `active` | `null` | InviteResponse received; waiting for Round-trip 1 |
| `active` | `AwaitingPresentation` | Round-trip 1 complete; challenge issued; waiting for VP |
| `complete` | `AwaitingPresentation` | Round-trip 2 complete; verification result stored |

---

## Key files

| File | Purpose |
|---|---|
| `VerifierApp/Pages/HolderPortal/Require.cshtml(.cs)` | Verifier/Holder UI: generate Interaction URL, poll for result, display proof + status badges |
| `VerifierApp/Controllers/VcapiController.cs` | VCALM server endpoints; orchestrates Steps A, B, and C |
| `VerifierApp/Services/VerificationExchangeService.cs` | In-memory exchange store (singleton); holds `ProofValid`, `StatusValid`, `StatusFailureReason` |
| `VerifierApp/Services/CredentialStatusService.cs` | Fetches issuer's Bitstring Status List and checks revocation bit (Step C) |
| `WalletApp/Pages/Exchange/Present.cshtml(.cs)` | Holder UI: paste URL, select credential, view result |
| `WalletApp/Services/HolderVerificationService.cs` | Holder Coordinator — drives all VCALM steps |


End-to-end walkthrough of how **WalletApp** presents a stored `AchievementCredential` (Open Badges 3.0) to **VerifierApp** using the [VCALM exchange protocol](https://w3c.github.io/vcalm/).

---

## Actors

| Actor | Role | App |
|---|---|---|
| **Verifier** | The organization requesting proof of a credential | Browser + VerifierApp |
| **VerifierApp** | Verifier Coordinator; hosts VCALM server endpoints | VerifierApp |
| **Holder** | The person presenting their credential | Browser + WalletApp |
| **WalletApp** | Holder Coordinator; drives the VCALM protocol on the Holder's behalf | WalletApp |

---

## Design decisions

| Concern | Decision |
|---|---|
| **VP proof** | Required — the Holder must sign the Verifiable Presentation |
| **Credential selection** | Holder chooses which stored credential to present |
| **Issuer trust** | "Signature-valid-from-any" — any issuer whose Ed25519 signature verifies is accepted |
| **Credential type required** | `AchievementCredential` with `achievementType: License` (hard-coded in VerifierApp) |

---

## Flow diagram

```
Holder (browser)          WalletApp                         VerifierApp
      |                       |                                  |
      | 1. Visit VerifierPortal/Require                          |
      |   Verifier clicks "Start" ──────────────────────────────>|
      |   Interaction URL returned ◄─────────────────────────────|
      |                       |                                  |
      | 2. Paste Interaction URL                                  |
      |──────────────────────>|                                  |
      | 3. Select credential  |                                  |
      |──────────────────────>|                                  |
      |                       |                                  |
      |                       |-- GET /interactions/{id}?iuv=1 ->|
      |                       |<-- Protocols {vcapi, inviteReq} --|
      |                       |                                  |
      |                       |-- POST /invite-request/response ->|
      |                       |<-- 200 OK ------------------------|
      |                       |                                  |
      |                       |-- POST /exchanges/{id}  {}  ---->|
      |                       |<-- QueryByExample VPR + challenge-|
      |                       |                                  |
      |                       | [build + sign VP with Holder key]|
      |                       |-- POST /exchanges/{id} {VP}  --->|
      |                       |   [verify VP proof (holderDid)]  |
      |                       |   [verify VC proof (issuerDid)]  |
      |                       |<-- verificationResult  ----------|
      |                       |                                  |
      |<-- Display result ----|                                  |
```

---

## Step-by-step

### Step 1 — Verifier generates an Interaction URL (VerifierApp VerifierPortal)

**Code:** `VerifierApp/Pages/VerifierPortal/Require.cshtml.cs` → `OnPostStart`

1. The authenticated Verifier visits the VerifierPortal and clicks **Start Verification**.
2. VerifierApp calls `IVerificationExchangeService.CreateExchange()`, which:
   - Generates a UUID `exchangeId`.
   - Associates the exchange with workflow ID `"verify-credential"`.
   - Sets initial state: `State = "pending"`, `Step = null`.
   - Stores the record in an in-memory `ConcurrentDictionary` with a 24-hour expiry.
3. VerifierApp constructs an **Interaction URL** (VCALM §3.7.1):
   ```
   https://<verifierhost>/interactions/{exchangeId}?iuv=1
   ```
4. The Interaction URL and a QR code are displayed to the Verifier, who shares them with the Holder out-of-band (copy-paste or QR scan).

---

### Step 2 — Holder opens WalletApp and submits the Interaction URL

**Code:** `WalletApp/Pages/Exchange/Present.cshtml.cs` → `OnPostAsync`

The Holder navigates to `/Exchange/Present` in WalletApp, pastes or scans the Interaction URL, and clicks **Load My Credentials**. WalletApp loads the Holder's stored credentials for selection.

The Holder selects a credential and clicks **Present Credential**. WalletApp delegates the entire multi-step protocol to `HolderVerificationService.PresentCredentialAsync(interactionUrl, holder, credential)`.

---

### Step 3 — WalletApp GETs the Interaction URL → Protocols object

**Code:** `WalletApp/Services/HolderVerificationService.cs` (Step 1)  
**Handled by:** `VerifierApp/Controllers/VcapiController.cs` → `GetInteraction`

```
GET /interactions/{exchangeId}?iuv=1
```

VerifierApp returns a **Protocols** object:

```json
{
  "vcapi": "https://<verifierhost>/workflows/verify-credential/exchanges/{exchangeId}",
  "inviteRequest": "https://<verifierhost>/{exchangeId}/invite-request/response"
}
```

---

### Step 4 — WalletApp POSTs an InviteResponse

**Code:** `WalletApp/Services/HolderVerificationService.cs` (Step 2)  
**Handled by:** `VerifierApp/Controllers/VcapiController.cs` → `ReceiveInviteResponse`

```
POST /{exchangeId}/invite-request/response
Body: { "purpose": "AchievementCredential verification", "referenceId": "urn:uuid:{newGuid}" }
```

VerifierApp records the InviteResponse and advances `State` to `"active"`. Returns `200 OK`.

---

### Step 5 — Round-trip 1: WalletApp POSTs `{}` → QueryByExample VPR

**Code:** `WalletApp/Services/HolderVerificationService.cs` (Step 3)  
**Handled by:** `VerifierApp/Controllers/VcapiController.cs` → `ParticipateInExchange`

```
POST /workflows/verify-credential/exchanges/{exchangeId}
Body: {}
```

Because `exchange.Step` is `null` (first POST), VerifierApp:

1. Generates a one-time UUID `challenge` and captures `domain` from `Request.Host`.
2. Sets `exchange.Step = "AwaitingPresentation"`.
3. Returns a **VerifiablePresentationRequest** with a `QueryByExample` query:

```json
{
  "referenceId": "urn:uuid:{newGuid}",
  "verifiablePresentationRequest": {
    "query": [{
      "type": "QueryByExample",
      "credentialQuery": {
        "reason": "Please present your License credential to proceed.",
        "example": {
          "@context": [
            "https://www.w3.org/ns/credentials/v2",
            "https://purl.imsglobal.org/spec/ob/v3p0/context-3.0.3.json"
          ],
          "type": ["VerifiableCredential", "AchievementCredential"],
          "credentialSubject": { "achievement": { "achievementType": "License" } }
        },
        "acceptedCryptosuites": [{ "cryptosuite": "eddsa-rdfc-2022" }]
      }
    }],
    "challenge": "{challenge}",
    "domain": "{verifierHost}"
  }
}
```

---

### Step 6 — Round-trip 2: WalletApp builds and signs a VP, POSTs it back

**Code:** `WalletApp/Services/HolderVerificationService.cs` (Step 4)  
**Handled by:** `VerifierApp/Controllers/VcapiController.cs` → `ParticipateInExchange`

#### WalletApp — building and signing the VP

1. Constructs an unsigned VP embedding the selected stored credential:
   ```json
   {
     "@context": ["https://www.w3.org/ns/credentials/v2"],
     "type": ["VerifiablePresentation"],
     "holder": "{holderDid}",
     "verifiableCredential": [ { "...stored credential JSON..." } ]
   }
   ```
2. Constructs proof options (`eddsa-rdfc-2022`, `proofPurpose: "authentication"`, `challenge`, `domain`).
3. Canonicalizes both documents (RDF Dataset Normalization).
4. Computes `proofInput = SHA-256(proofOptions) ‖ SHA-256(vp)` and signs with the Holder's **Ed25519** private key.
5. Attaches the proof and POSTs the signed VP.

---

### Step 7 — VerifierApp verifies the VP proof (Step A)

Because `exchange.Step == "AwaitingPresentation"`, VerifierApp:

1. Extracts `proof.verificationMethod` (format: `{holderDid}#key-1`) and derives `holderDid`.
2. Resolves the Holder's **did:web** DID document over HTTP to obtain their `publicKeyMultibase`.
3. Re-canonicalizes the VP and proof options, reconstructs `proofInput`, and verifies the Ed25519 signature.
4. Checks that `proof.challenge` matches the stored challenge.
5. On success, records `holderDid` on the exchange.

> If VP proof verification fails, VerifierApp returns `400 Bad Request` immediately.

---

### Step 8 — VerifierApp verifies the enclosed credential proof (Step B)

1. Extracts the first element of `verifiablePresentation.verifiableCredential[]`.
2. Resolves the issuer's DID:
   - **did:key** — public key decoded inline from the DID itself (no HTTP needed).
   - **did:web** — DID document fetched from `https://{host}/.well-known/did.json`.
3. Re-canonicalizes the credential and its proof options, reconstructs `proofInput`, and verifies the Ed25519 signature.
4. Calls `IVerificationExchangeService.CompleteExchange(exchangeId, passed, failureReason, credentialJson)`.

> **Issuer trust model:** "signature-valid-from-any" — no issuer allowlist is enforced.

---

### Step 9 — VerifierApp returns the verificationResult

VerifierApp responds to the Round-trip 2 POST with:

```json
{ "verificationResult": { "verified": true } }
```

or on failure:

```json
{ "verificationResult": { "verified": false, "errors": ["Credential proof verification failed: Signature verification failed."] } }
```

---

### Step 10 — WalletApp displays the result

**Code:** `WalletApp/Services/HolderVerificationService.cs` (Step 5)  
**Displayed by:** `WalletApp/Pages/Exchange/Present.cshtml`

WalletApp reads `verificationResult.verified` and renders either a green "✓ Credential verified successfully" alert or a red "✗ Credential verification failed" alert with any verifier-reported error messages.

---

## Exchange state machine (VerifierApp)

| `State` | `Step` | Meaning |
|---|---|---|
| `pending` | `null` | Exchange created; no wallet contact yet |
| `active` | `null` | InviteResponse received; waiting for Round-trip 1 |
| `active` | `AwaitingPresentation` | Round-trip 1 complete; challenge issued; waiting for VP |
| `complete` | `AwaitingPresentation` | Round-trip 2 complete; verification result stored |

---

## Key files

| File | Purpose |
|---|---|
| `VerifierApp/Pages/VerifierPortal/Require.cshtml(.cs)` | Verifier UI: generate Interaction URL |
| `VerifierApp/Controllers/VcapiController.cs` | VCALM server endpoints (Protocols, InviteResponse, exchange participation) |
| `VerifierApp/Services/VerificationExchangeService.cs` | In-memory exchange store (singleton) |
| `WalletApp/Pages/Exchange/Present.cshtml(.cs)` | Holder UI: paste URL, select credential, view result |
| `WalletApp/Services/HolderVerificationService.cs` | Holder Coordinator — drives all VCALM steps |

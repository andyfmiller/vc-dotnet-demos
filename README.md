# Verifiable Credentials Demo Suite

A reference implementation of a complete **Verifiable Credentials ecosystem** built with **.NET 10** and ASP.NET Core Razor Pages.

The suite demonstrates how three independent web applications — an Issuer, a Wallet, and a Verifier — cooperate using open standards to issue, hold, and present digitally-signed credentials.

## Public Apps are available at:
- IssuerApp: https://vcdemos-issuer.azurewebsites.net
- WalletApp: https://vcdemos-wallet.azurewebsites.net
- VerifierApp: https://vcdemos-verifier.azurewebsites.net

It may take a minute or two for the apps to wake up if they haven't been used recently. They all have seeded demo data, so you can jump right in and start issuing and verifying credentials! Use the DEMO TOOLS menu to change your role as appropriate for each workflow.

---

## Standards implemented

| Standard | Role in this demo |
|---|---|
| [1EdTech Open Badges 3.0](https://www.imsglobal.org/spec/ob/v3p0/) | The Verifiable Credential format (`AchievementCredential`) |
| [W3C Verifiable Credentials Data Model 2.0](https://www.w3.org/TR/vc-data-model-2.0/) | Core credential and presentation data structures |
| [W3C VC Credential Exchange (VCALM)](https://w3c.github.io/vcalm/) | Exchange protocol between Wallet ↔ Issuer and Wallet ↔ Verifier |
| [W3C VC Rendering Methods v1.0](https://www.w3.org/TR/vc-render-method/) | `html` render suite — displays credentials inside a sandboxed iframe |
| [Data Integrity Proofs (eddsa-rdfc-2022)](https://www.w3.org/TR/vc-di-eddsa/) | Ed25519 signatures over RDF-canonicalized documents |
| [did:web](https://w3c-ccg.github.io/did-method-web/) | DID method used by Holders (and optionally Issuers) |
| [did:key](https://w3c-ccg.github.io/did-method-key/) | DID method derived inline from an Ed25519 public key (used by Issuers) |

---

## Projects

| Project | Description |
|---|---|
| [`IssuerApp`](IssuerApp/) | ASP.NET Core Razor Pages app. Manages organizations, achievements, and members; issues signed `AchievementCredential`s via VCALM. |
| [`WalletApp`](WalletApp/) | ASP.NET Core Razor Pages app. Stores holders and their credentials; drives VCALM exchanges on the holder's behalf for both issuance and presentation. |
| [`VerifierApp`](VerifierApp/) | ASP.NET Core Razor Pages app. Manages credential requirements; verifies presented credentials via VCALM. |
| [`Library`](Library/) | Shared class library. Contains hand-authored VC/OB3 models, Kiota-generated VCALM and Open Badges API clients, Ed25519 signing, JSON-LD canonicalization, and `did:web` resolution. |
| `*.Tests` | xUnit test projects for each of the above. |

---

## Workflows

Two end-to-end workflows are fully implemented:

1. **[Issuance](docs/vcalm-issuance-workflow.md)** — IssuerApp issues a signed `AchievementCredential` to WalletApp using VCALM with DID Authentication.
2. **[Presentation / Verification](docs/vcalm-verification-workflow.md)** — WalletApp presents a stored credential to VerifierApp using VCALM with a QueryByExample VPR.

---

## Architecture overview

```
┌────────────┐   VCALM exchange   ┌────────────┐   VCALM exchange   ┌──────────────┐
│ IssuerApp  │◄──────────────────►│ WalletApp  │◄──────────────────►│ VerifierApp  │
│            │                    │            │                    │              │
│ did:key    │                    │ did:web    │                    │              │
│ (Issuer)   │                    │ (Holder)   │                    │              │
└────────────┘                    └────────────┘                    └──────────────┘
       ▲                                ▲                                  ▲
       └────────────────────────────────┴──────────────────────────────────┘
                                   Library
                         (models · crypto · VCALM client)
```

All three apps are independent ASP.NET Core Razor Pages applications backed by **SQLite** (via EF Core). They communicate only over HTTPS using the VCALM exchange protocol — there is no shared database or internal API.

---

## Getting started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Visual Studio 2022 17.14+ **or** the `dotnet` CLI

### Running all three apps

The easiest way is to open the solution in Visual Studio and configure **Multiple Startup Projects** (IssuerApp, WalletApp, VerifierApp). Each app runs on its own port and uses its own SQLite database in the project directory.

Alternatively, open three terminals:

```bash
dotnet run --project IssuerApp
dotnet run --project WalletApp
dotnet run --project VerifierApp
```

Each app seeds demo data (organizations, achievements, holders) on first run.

---

## Credential rendering

WalletApp implements the **W3C VC Rendering Methods v1.0** `html` render suite. When a stored credential contains a `renderMethod` with `renderSuite: "html"`, WalletApp fetches the HTML template (from a `data:` URL or an HTTP URL) and displays the credential inside a sandboxed `<iframe>`, injecting the credential JSON as the iframe's source data.

---

## Cryptography

All proofs use the **`eddsa-rdfc-2022`** cryptosuite:

```
proofInput = SHA-256(RDF-canonicalized proof options)
           ‖ SHA-256(RDF-canonicalized document)
signature  = Ed25519.Sign(privateKey, proofInput)   → multibase-encoded proof value
```

- **Issuers** use `did:key` (derived on-the-fly from their Ed25519 public key).
- **Holders** use `did:web` published at `https://{wallethost}/holders/{holderSlug}/.well-known/did.json`.
- Exchange stores are **in-memory** (`ConcurrentDictionary`) and are intentionally not persisted — this is a demo, not a production system.
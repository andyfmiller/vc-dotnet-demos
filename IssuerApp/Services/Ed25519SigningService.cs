// Forwarding aliases - the implementation has moved to Library.Crypto.
// All IssuerApp code that references IEd25519SigningService or Ed25519SigningService
// continues to compile without modification.
global using IEd25519SigningService = Library.Crypto.IEd25519SigningService;
global using Ed25519SigningService = Library.Crypto.Ed25519SigningService;

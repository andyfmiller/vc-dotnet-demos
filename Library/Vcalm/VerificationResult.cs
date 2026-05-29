using Microsoft.Kiota.Abstractions.Serialization;

namespace Library.Vcalm.Client.Models;

// Partial class to supply the factory method omitted by Kiota due to the
// VerificationResult schema name collision between VerifyCredentialResult.yml
// and VerifyPresentationResult.yml.
public partial class VerificationResult
{
    public static VerificationResult CreateFromDiscriminatorValue(IParseNode parseNode)
    {
        ArgumentNullException.ThrowIfNull(parseNode);
        return new VerificationResult();
    }
}

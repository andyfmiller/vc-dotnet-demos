using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Serialization.Json;
using System.Text;

namespace Library.Tests.Vcalm;

internal static class KiotaJsonHelper
{
    internal static T Deserialize<T>(string json, ParsableFactory<T> factory) where T : IParsable
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var parseNodeFactory = new JsonParseNodeFactory();
        var parseNode = parseNodeFactory.GetRootParseNodeAsync("application/json", stream).GetAwaiter().GetResult();
        return parseNode.GetObjectValue<T>(factory)!;
    }

    internal static string Serialize<T>(T model) where T : IParsable
    {
        using var writer = new JsonSerializationWriter();
        writer.WriteObjectValue<T>(null, model);
        using var resultStream = writer.GetSerializedContent();
        return new StreamReader(resultStream).ReadToEnd();
    }
}

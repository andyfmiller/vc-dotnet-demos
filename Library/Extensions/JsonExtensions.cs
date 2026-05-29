using System.Buffers;
using System.Text.Json;

namespace Library.Extensions
{
    public static class JsonExtensions
    {
        public static T ToObject<T>(this JsonElement element, JsonSerializerOptions? options = null)
        {
            var bufferWriter = new ArrayBufferWriter<byte>();
            using (var writer = new Utf8JsonWriter(bufferWriter))
                element.WriteTo(writer);
            var result = JsonSerializer.Deserialize<T>(bufferWriter.WrittenSpan, options);
            if (result is null)
                throw new JsonException($"Deserialization of {typeof(T)} from JsonElement failed.");
            return result;
        }
    }
}

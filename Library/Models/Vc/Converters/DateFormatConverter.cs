namespace Library.Models.Vc.Converters
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    internal class DateFormatConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dateTime = reader.GetString();
            if (dateTime == null)
            {
                throw new JsonException("Unexpected JsonTokenType.Null");
            }

            return DateTime.Parse(dateTime);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("yyyy-MM-dd"));
        }
    }

}
using System;
using Newtonsoft.Json;

namespace nWins.Lib.Settings
{
    // snippet taken from https://www.c-sharpcorner.com/UploadFile/20c06b/deserializing-interface-properties-with-json-net/

    /// <summary>
    /// Generic purpose JSON converter class to fix interface attribute initialization
    /// by specifying explicit implementation types.
    /// </summary>
    /// <typeparam name="T">The explicit JSON-serializable type to be converted.</typeparam>
    public class JsonGenericConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType) => true;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // just use the standard JSON parser for the given explicit type
            return serializer.Deserialize<T>(reader);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // just use the standard JSON serializer for the given explicit type
            serializer.Serialize(writer, value);
        }
    }
}
﻿using Newtonsoft.Json;
using System.Net;

namespace BackendProject.MiscUtils
{
    public class JsonIPConverterUtils : JsonConverter<IPAddress>
    {
        public override void WriteJson(JsonWriter writer, IPAddress? value, JsonSerializer serializer)
        {
            writer.WriteValue(value?.ToString());
        }

        public override IPAddress ReadJson(JsonReader reader, Type objectType, IPAddress? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return IPAddress.Parse((string?)reader.Value ?? "0.0.0.0");
        }
    }
}

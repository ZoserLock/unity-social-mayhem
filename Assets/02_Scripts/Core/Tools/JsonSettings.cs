using System.Collections.Generic;
using Newtonsoft.Json;

namespace StrangeSpace
{
    public class JsonSettings
    {
       public static JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            Converters = new List<JsonConverter>
            {
                new Vector2Converter(),
                new Vector3Converter()
            }
        };
    }
}
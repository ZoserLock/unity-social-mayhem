using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System;

public class Vector2Converter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Vector2);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var jo = JObject.Load(reader);
        
        float x = jo["x"]?.Value<float>() ?? 0f;
        float y = jo["y"]?.Value<float>() ?? 0f;
        
        return new Vector2(x, y);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var vector = (Vector2)value;
        
        writer.WriteStartObject();
        writer.WritePropertyName("x");
        writer.WriteValue(vector.x);
        writer.WritePropertyName("y");
        writer.WriteValue(vector.y);
        writer.WriteEndObject();
    }
}

public class Vector3Converter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(Vector3);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var jo = JObject.Load(reader);
        
        float x = jo["x"]?.Value<float>() ?? 0f;
        float y = jo["y"]?.Value<float>() ?? 0f;
        float z = jo["z"]?.Value<float>() ?? 0f;
        
        return new Vector3(x, y, z);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var vector = (Vector3)value;
        
        writer.WriteStartObject();
        writer.WritePropertyName("x");
        writer.WriteValue(vector.x);
        writer.WritePropertyName("y");
        writer.WriteValue(vector.y);
        writer.WritePropertyName("z");
        writer.WriteValue(vector.z);
        writer.WriteEndObject();
    }
}
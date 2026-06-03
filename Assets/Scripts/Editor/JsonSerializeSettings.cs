using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using UnityEngine;


public class Vector2Serializer : JsonConverter<Vector2>
{
    public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JObject jsonObject = JObject.Load(reader);

        float x = jsonObject["x"] == null ? 0f : (float)jsonObject["x"];
        float y = jsonObject["y"] == null ? 0f : (float)jsonObject["y"];

        return new Vector2(x, y);
    }

    public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer)
    {
        JObject jsonObject = new JObject()
        {
            ["x"] = value.x,
            ["y"] = value.y
        };

        jsonObject.WriteTo(writer);
    }
}
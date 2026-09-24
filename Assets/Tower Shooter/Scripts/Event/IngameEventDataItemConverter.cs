using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class IngameEventDataItemConverter : JsonConverter<IngameEventDataItem>
{
    public override IngameEventDataItem ReadJson(
        JsonReader reader,
        Type objectType,
        IngameEventDataItem existingValue,
        bool hasExistingValue,
        JsonSerializer serializer)
    {
        JObject obj = JObject.Load(reader);

        IngameEventType eventType = obj["eventType"].ToObject<IngameEventType>();

        IngameEventDataItem result = eventType switch
        {
            IngameEventType.NewStarDiscovered =>
                new StarDiscoveryEventDataItem(),

            IngameEventType.ElectionComplete =>
                new ElectionCompleteEventDataItem(),

            _ => throw new JsonSerializationException(
                $"Unknown IngameEventType: {eventType}")
        };

        serializer.Populate(obj.CreateReader(), result);

        return result;
    }

    public override void WriteJson(
        JsonWriter writer,
        IngameEventDataItem value,
        JsonSerializer serializer)
    {
        JObject obj = JObject.FromObject(value, serializer);
        obj.WriteTo(writer);
    }
}

namespace CodyPrototype.Tests;

using System.Text.Json;
using System.Text.Json.Serialization;

public class CycleRawConverter : JsonConverter<CycleRaw>
{
    public override CycleRaw Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Expect: [address, value, "operation"]

        reader.Read();
        int address = reader.GetInt32();

        reader.Read();
        int value = reader.GetInt32();

        reader.Read();
        string op = reader.GetString()!;

        reader.Read(); // EndArray

        return new CycleRaw { Address = address, Value = value, Operation = op };
    }

    public override void Write(Utf8JsonWriter writer, CycleRaw value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.Address);
        writer.WriteNumberValue(value.Value);
        writer.WriteStringValue(value.Operation);
        writer.WriteEndArray();
    }
}

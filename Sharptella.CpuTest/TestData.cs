using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sharptella.CpuTest;

#pragma warning disable CS0649

internal struct TestData
{
    [JsonInclude, JsonPropertyName("name")]
    public string? Name;

    [JsonInclude, JsonPropertyName("initial")]
    public CpuState Initial;

    [JsonInclude, JsonPropertyName("final")]
    public CpuState Final;

    [JsonInclude, JsonPropertyName("cycles")]
    public CpuCycle[]? Cycles;
}

public struct CpuState
{
    [JsonInclude, JsonPropertyName("pc")]
    public ushort PC;

    [JsonInclude, JsonPropertyName("s")]
    public byte S;

    [JsonInclude, JsonPropertyName("a")]
    public byte A;

    [JsonInclude, JsonPropertyName("x")]
    public byte X;

    [JsonInclude, JsonPropertyName("y")]
    public byte Y;

    [JsonInclude, JsonPropertyName("p")]
    public byte P;

    [JsonInclude, JsonPropertyName("ram")]
    public RamData[]? Ram;
}

[JsonConverter(typeof(CpuCycleConverter))]
public struct CpuCycle
{
    public ushort Address;

    public byte Value;

    public CycleType Type;
}

public enum CycleType
{
    Read,
    Write,
}

[JsonConverter(typeof(RamDataConverter))]
public struct RamData
{
    public ushort Address;

    public byte Value;
}

public sealed class CpuCycleConverter : JsonConverter<CpuCycle>
{
    public override CpuCycle Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Expected StartArray token for a CpuCycle.");
        }

        // 1. Read Address
        reader.Read();
        if (reader.TokenType != JsonTokenType.Number)
        {
            throw new JsonException("Expected number for CpuCycle.Address.");
        }
        var address = reader.GetUInt16();

        // 2. Read Value
        reader.Read();
        if (reader.TokenType != JsonTokenType.Number)
        {
            throw new JsonException("Expected number for CpuCycle.Value.");
        }
        var value = reader.GetByte();

        // 3. Read Type ("read" or "write")
        reader.Read();
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("Expected string for CpuCycle.Type.");
        }

        string? typeStr = reader.GetString();
        CycleType type = typeStr switch
        {
            "read" => CycleType.Read,
            "write" => CycleType.Write,
            _ => throw new JsonException($"Unknown CycleType value: '{typeStr}'")
        };

        // 4. Read EndArray
        reader.Read();
        if (reader.TokenType != JsonTokenType.EndArray)
        {
            throw new JsonException("Expected EndArray token for a CpuCycle.");
        }

        return new CpuCycle
        {
            Address = address,
            Value = value,
            Type = type
        };
    }

    public override void Write(Utf8JsonWriter writer, CpuCycle value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.Address);
        writer.WriteNumberValue(value.Value);
        writer.WriteStringValue(value.Type == CycleType.Read ? "read" : "write");
        writer.WriteEndArray();
    }
}

public sealed class RamDataConverter : JsonConverter<RamData>
{
    public override RamData Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Expected StartArray token for a RamData.");
        }

        // 1. Read Address
        reader.Read();
        if (reader.TokenType != JsonTokenType.Number)
        {
            throw new JsonException("Expected number for RamData.Address.");
        }
        var address = reader.GetUInt16();

        // 2. Read Value
        reader.Read();
        if (reader.TokenType != JsonTokenType.Number)
        {
            throw new JsonException("Expected number for RamData.Value.");
        }
        var value = reader.GetByte();

        reader.Read();
        if (reader.TokenType != JsonTokenType.EndArray)
        {
            throw new JsonException("Expected EndArray token for a RamData.");
        }

        return new RamData
        {
            Address = address,
            Value = value,
        };
    }

    public override void Write(Utf8JsonWriter writer, RamData value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        writer.WriteNumberValue(value.Address);
        writer.WriteNumberValue(value.Value);
        writer.WriteEndArray();
    }
}
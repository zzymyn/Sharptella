using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Sharptari.CpuTest;

#pragma warning disable CS0649

internal sealed class TestData
{
    [JsonInclude, JsonPropertyName("name")]
    public string? Name;

    [JsonInclude, JsonPropertyName("initial")]
    public CpuState? Initial;

    [JsonInclude, JsonPropertyName("final")]
    public CpuState? Final;

    // Cycles contain heterogeneous data: [address (int), value (int), type (string)]
    // We use JsonElement[] to read this safely without crashing
    [JsonInclude, JsonPropertyName("cycles")]
    public JsonElement[]?[]? Cycles;
}

public sealed class CpuState
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
    public int[]?[]? Ram;
}
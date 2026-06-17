using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Sharptari.CpuTest;

internal sealed class TestData
{
    [JsonPropertyName("name")]
    public string? Name;

    [JsonPropertyName("initial")]
    public CpuState? Initial;

    [JsonPropertyName("final")]
    public CpuState? Final;

    // Cycles contain heterogeneous data: [address (int), value (int), type (string)]
    // We use JsonElement[] to read this safely without crashing
    [JsonPropertyName("cycles")]
    public JsonElement[]?[]? Cycles;
}

public sealed class CpuState
{
    [JsonPropertyName("pc")]
    public ushort PC;

    [JsonPropertyName("s")]
    public byte S;

    [JsonPropertyName("a")]
    public byte A;

    [JsonPropertyName("x")]
    public byte X;

    [JsonPropertyName("y")]
    public byte Y;

    [JsonPropertyName("p")]
    public byte P;

    [JsonPropertyName("ram")]
    public int[]?[]? Ram;
}
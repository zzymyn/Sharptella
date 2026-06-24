using System.Collections.Generic;

namespace Sharptari.CpuTest;

internal struct TestData
{
    public string? Name;
    public CpuState Initial;
    public CpuState Final;
    public List<CpuCycle>? Cycles;
}

internal struct CpuState
{
    public ushort PC;
    public byte S;
    public byte A;
    public byte X;
    public byte Y;
    public byte P;
    public List<RamEntry>? Ram;
}

internal struct CpuCycle
{
    public ushort Address;
    public byte Value;
    public CycleType Type;
}

internal enum CycleType
{
    Read,
    Write,
}

internal struct RamEntry
{
    public ushort Address;
    public byte Value;
}
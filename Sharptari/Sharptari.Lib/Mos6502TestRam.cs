using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharptari.Lib;

/// <summary>
/// A simple RAM implementation that covers the entire address space.
/// </summary>
public sealed class Mos6502TestRam
    : IMos6502BusItem
{
    private readonly byte[] m_Ram = new byte[ushort.MaxValue + 1];

    public int TryRead(ushort address) => m_Ram[address];

    public void TryWrite(ushort address, byte value) => m_Ram[address] = value;
}

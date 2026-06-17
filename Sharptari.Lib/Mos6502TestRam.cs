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
    private readonly List<LogItem> m_Log = [];

    public List<LogItem> Log => m_Log;

    public int TryRead(ushort address)
    {
        var value = m_Ram[address];
        m_Log.Add(new LogItem { IsRead = true, Address = address, Value = value });
        return value;
    }

    public void TryWrite(ushort address, byte value)
    {
        m_Ram[address] = value;
        m_Log.Add(new LogItem { IsRead = false, Address = address, Value = value });
    }

    public struct LogItem
    {
        public bool IsRead;
        public ushort Address;
        public byte Value;
    }
}

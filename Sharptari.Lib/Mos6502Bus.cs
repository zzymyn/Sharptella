using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharptari.Lib;

public interface IMos6502BusItem
{
    /// <summary>
    /// Try to read from the bus item at the given address.
    /// </summary>
    /// <param name="address">The address to read from.</param>
    /// <returns>negative if the device does not respond to the address, otherwise the value read from the bus.</returns>
    int TryRead(ushort address);

    /// <summary>
    /// Try to write to the bus item at the given address.
    /// </summary>
    /// <param name="address">The address to write to.</param>
    /// <param name="value">The value to write.</param>
    void TryWrite(ushort address, byte value);
}

public sealed class Mos6502Bus
{
    public List<IMos6502BusItem> Items { get; } = new List<IMos6502BusItem>();

    private byte m_BusValue;

    public byte Read(ushort address)
    {
        // we need to know if multiple bus items respond to the same address:
        int readValue = -1;

        foreach (var item in Items)
        {
            var value = item.TryRead(address);
            if (value >= 0)
            {
                if (readValue >= 0)
                {
                    // if multiple items respond to the same address, pulling low wins:
                    readValue &= value;
                }
                else
                {
                    readValue = value;
                }
            }
        }

        // if nothing responds to the address, the bus value is unchanged:
        if (readValue >= 0)
        {
            m_BusValue = (byte)readValue;
        }

        return m_BusValue;
    }

    public void Write(ushort address, byte value)
    {
        // writing always updates the bus value:

        m_BusValue = value;

        foreach (var item in Items)
        {
            item.TryWrite(address, value);
        }
    }
}

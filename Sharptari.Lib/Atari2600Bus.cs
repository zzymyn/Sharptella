using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharptari.Lib;

public sealed class Atari2600Bus
    : IMos6502Bus
{
    private readonly Atari2600Rom m_Rom;
    private byte m_BusValue;
#if DEBUG
    private bool m_HasDoneSomethingThisStep;
#endif

    public Atari2600Bus(Atari2600Rom rom)
    {
        m_Rom = rom;
    }

    public void Step()
    {
#if DEBUG
        if (!m_HasDoneSomethingThisStep)
        {
            throw new InvalidOperationException("The bus must do something each step, but nothing has done anything this step.");
        }
        m_HasDoneSomethingThisStep = false;
#endif
    }

    public byte Read(ushort address)
    {
#if DEBUG
        if (m_HasDoneSomethingThisStep)
        {
            throw new InvalidOperationException("The bus can only do one thing per step, but something has already done something this step.");
        }
        m_HasDoneSomethingThisStep = true;
#endif
        address &= 0x1FFF;

        if ((address & 0x1000) != 0)
        {
            // we're in the ROM area:
            address &= 0x0FFF;
            m_BusValue = m_Rom.TryRead(address);
        }

        return m_BusValue;
    }

    public void Write(ushort address, byte value)
    {
#if DEBUG
        if (m_HasDoneSomethingThisStep)
        {
            throw new InvalidOperationException("The bus can only do one thing per step, but something has already done something this step.");
        }
        m_HasDoneSomethingThisStep = true;
#endif

        // writing always updates the bus value:

        m_BusValue = value;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharptari.Lib;

public sealed class Atari2600Bus
    : IMos6502Bus
{
    private const ushort RomSelectM = 0b0001_0000_0000_0000;
    private const ushort RomSelectV = 0b0001_0000_0000_0000;

    private const ushort RiotSelectM = 0b0001_0000_1000_0000;
    private const ushort RiotSelectV = 0b0000_0000_1000_0000;

    private readonly Atari2600Rom m_Rom;
    private readonly Mos6532Riot m_Riot;
    private byte m_BusValue;
#if DEBUG
    private bool m_HasDoneSomethingThisStep;
#endif

    public Atari2600Bus(Atari2600Rom rom, Mos6532Riot riot)
    {
        m_Rom = rom;
        m_Riot = riot;
    }

    public void Reboot()
    {
        m_Rom.Reboot();
        m_Riot.Reboot();
        m_BusValue = 0;
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

        m_Rom.Step();
        m_Riot.Step();
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

        int readValue = -1;

        if ((address & RomSelectM) == RomSelectV)
        {
            readValue = m_Rom.TryRead(address);
        }
        else if ((address & RiotSelectM) == RiotSelectV)
        {
            readValue = m_Riot.TryRead(address);
        }

        if (readValue >= 0)
        {
            m_BusValue = (byte)readValue;
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

        if ((address & RomSelectM) == RomSelectV)
        {
            m_Rom.TryWrite(address, value);
        }
        else if ((address & RiotSelectM) == RiotSelectV)
        {
            m_Riot.TryWrite(address, value);
        }
    }
}

using System;

namespace Sharptella.Lib;

public sealed class Atari2600Bus
    : IMos6502Bus
{
    private const ushort RomSelectM = 0b0001_0000_0000_0000;
    private const ushort RomSelectV = 0b0001_0000_0000_0000;

    private const ushort RiotSelectM = 0b0001_0000_1000_0000;
    private const ushort RiotSelectV = 0b0000_0000_1000_0000;

    private const ushort TiaSelectM = 0b0001_0000_1000_0000;
    private const ushort TiaSelectV = 0b0000_0000_0000_0000;

    private readonly Atari2600Rom m_Rom;
    private readonly Mos6532Riot m_Riot;
    private readonly Atari2600Tia m_Tia;
    private byte m_BusValue;
#if DEBUG
    private bool m_HasDoneSomethingThisStep;
#endif

    public Atari2600Bus(Atari2600Rom rom, Mos6532Riot riot, Atari2600Tia tia)
    {
        m_Rom = rom;
        m_Riot = riot;
        m_Tia = tia;
    }

    public void Reboot()
    {
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
        else if ((address & TiaSelectM) == TiaSelectV)
        {
            readValue = m_Tia.TryRead(address);
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
        else if ((address & TiaSelectM) == TiaSelectV)
        {
            m_Tia.TryWrite(address, value);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharptari.Lib;

public sealed class Mos6532Riot
{
    private const ushort RegisterSelectM = 0b0001_0010_1000_0000;
    private const ushort RegisterSelectV = 0b0000_0010_1000_0000;
    private const ushort RegisterMask = 0b0000_0000_0000_0111;

    private const ushort RamSelectM = 0b0001_0010_1000_0000;
    private const ushort RamSelectV = 0b0000_0000_1000_0000;
    private const ushort RamMask = 0b0000_0000_0111_1111;

    private const byte FlagTimerMask = 0b1000_0000;

    private readonly byte[] m_Ram = new byte[128];

    private byte m_Timer;
    private ushort m_Prescaler;
    private ushort m_PrescalerMask;
    private bool m_FlagTimer;

    public void Reboot()
    {
        m_Timer = 255;
        m_Prescaler = 0;
        m_PrescalerMask = 1023;
        m_FlagTimer = false;
    }

    public void Step()
    {
        m_Prescaler = (ushort)((m_Prescaler - 1) & m_PrescalerMask);

        if (m_FlagTimer || m_Prescaler == m_PrescalerMask)
        {
            --m_Timer;
            if (m_Timer == byte.MaxValue)
            {
                m_FlagTimer = true;
            }
        }
    }

    public int TryRead(ushort address)
    {
        if ((address & RegisterSelectM) == RegisterSelectV)
        {
            address &= RegisterMask;

            switch (address)
            {
                case 0x00:
                    return 0x00; // SWCHA
                case 0x01:
                    return 0x00; // SWACNT
                case 0x02:
                    return 0x00; // SWCHB
                case 0x03:
                    return 0x00; // SWBCNT
                case 0x04:
                case 0x06:
                    // INTIM
                    // bug/quirk: if the flag timer will underflow this step, it doesn't get reset on read here:
                    m_FlagTimer = TimerWillUnderflowThisStep();
                    return m_Timer;
                case 0x05:
                case 0x07:
                    // TIMINT
                    {
                        byte result = 0;
                        if (m_FlagTimer)
                        {
                            result |= FlagTimerMask;
                        }
                        return result;
                    }
            }
        }
        else if ((address & RamSelectM) == RamSelectV)
        {
            address &= RamMask;
            return m_Ram[address];
        }

        return -1;
    }

    public void TryWrite(ushort address, byte value)
    {
        if ((address & RegisterSelectM) == RegisterSelectV)
        {
            address &= RegisterMask;

            switch (address)
            {
                case 0x00:
                    break; // SWCHA
                case 0x01:
                    break; // SWACNT
                case 0x02:
                    break; // SWCHB
                case 0x03:
                    break; // SWBCNT
                case 0x04:
                    // 0
                    StartTimer(value, 0);
                    break;
                case 0x05:
                    // 7
                    StartTimer(value, 7);
                    break;
                case 0x06:
                    // 63
                    StartTimer(value, 63);
                    break;
                case 0x07:
                    // 1023
                    StartTimer(value, 1023);
                    break;
            }
        }
        else if ((address & RamSelectM) == RamSelectV)
        {
            address &= RamMask;
            m_Ram[address] = value;
        }
    }

    private void StartTimer(byte value, ushort prescalerMask)
    {
        m_Timer = value;
        m_Prescaler = 0;
        m_PrescalerMask = prescalerMask;

        // bug/quirk: if the flag timer will underflow this step, it doesn't get reset on read here:
        m_FlagTimer = TimerWillUnderflowThisStep();
    }

    private bool TimerWillUnderflowThisStep()
    {
        return m_Timer == 0 && (m_FlagTimer || m_Prescaler == 0);
    }
}

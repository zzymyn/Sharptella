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

    private readonly IAtariInput m_Input;

    // TODO: read controller inputs for SWCHA:
    private byte m_SWCHA = 0b1111_1111;
    private byte m_ORA;
    private byte m_SWACNT;

    // TODO: read switch inputs for SWCHB:
    private byte m_SWCHB = 0b0011_1111;
    private byte m_ORB;
    private byte m_SWBCNT;

    private byte m_Timer;
    private ushort m_Prescaler;
    private ushort m_PrescalerMask;
    private bool m_FlagTimer;

    public IReadOnlyList<byte> DebugRam => m_Ram;

    public Mos6532Riot(IAtariInput input)
    {
        m_Input = input;
    }

    public void Reboot()
    {
        Array.Clear(m_Ram, 0, m_Ram.Length);

        m_Timer = 255;
        m_Prescaler = 0;
        m_PrescalerMask = 1023;
        m_FlagTimer = false;

        m_ORA = 0;
        m_SWACNT = 0;
        m_ORB = 0;
        m_SWBCNT = 0;
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
                    // SWCHA
                    // m_SWCHA being zero will always pull to 0, even if SWACNT is 1:
                    // simplify: (m_SWCHA & ~m_SWACNT) | (m_SWCHA & m_ORA & m_SWACNT)
                    m_SWCHA = 0b1111_1111;
                    if (m_Input.Player1Up)
                    {
                        m_SWCHA &= 0b1111_1110;
                    }
                    if (m_Input.Player1Down)
                    {
                        m_SWCHA &= 0b1111_1101;
                    }
                    if (m_Input.Player1Left)
                    {
                        m_SWCHA &= 0b1111_1011;
                    }
                    if (m_Input.Player1Right)
                    {
                        m_SWCHA &= 0b1111_0111;
                    }
                    if (m_Input.Player0Up)
                    {
                        m_SWCHA &= 0b1110_1111;
                    }
                    if (m_Input.Player0Down)
                    {
                        m_SWCHA &= 0b1101_1111;
                    }
                    if (m_Input.Player0Left)
                    {
                        m_SWCHA &= 0b1011_1111;
                    }
                    if (m_Input.Player0Right)
                    {
                        m_SWCHA &= 0b0111_1111;
                    }
                    return 0xFF & m_SWCHA & (~m_SWACNT | m_ORA);
                case 0x01:
                    // SWACNT
                    return m_SWACNT;
                case 0x02:
                    // SWCHB
                    // Simple mux between SWCHB and ORB based on SWBCNT:
                    m_SWCHB = 0b1111_1111;
                    if (m_Input.GameResetSwitch)
                    {
                        m_SWCHB &= 0b1111_1110;
                    }
                    if (m_Input.GameSelectSwitch)
                    {
                        m_SWCHB &= 0b1111_1101;
                    }
                    if (m_Input.TvTypeSwitch)
                    {
                        m_SWCHB &= 0b1111_0111;
                    }
                    if (m_Input.PlayerDifficultySwitchA)
                    {
                        m_SWCHB &= 0b1011_1111;
                    }
                    if (m_Input.PlayerDifficultySwitchB)
                    {
                        m_SWCHB &= 0b0111_1111;
                    }
                    return 0xFF & ((m_SWBCNT & m_ORB) | (~m_SWBCNT & m_SWCHB));
                case 0x03:
                    // SWBCNT
                    return m_SWBCNT;
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
                    // SWCHA
                    m_ORA = value;
                    break;
                case 0x01:
                    // SWACNT
                    m_SWACNT = value;
                    break;
                case 0x02:
                    // SWCHB
                    m_ORB = value;
                    break;
                case 0x03:
                    // SWBCNT
                    m_SWBCNT = value;
                    break;
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
        bool timerWillUnderflowThisStep = TimerWillUnderflowThisStep();

        m_Timer = value;
        m_Prescaler = 0;
        m_PrescalerMask = prescalerMask;

        // bug/quirk: if the flag timer would have underflowed this step, it doesn't get reset on read here:
        m_FlagTimer = timerWillUnderflowThisStep;
    }

    private bool TimerWillUnderflowThisStep()
    {
        return m_Timer == 0 && (m_FlagTimer || m_Prescaler == 0);
    }
}

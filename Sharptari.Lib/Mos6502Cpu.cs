using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharptari.Lib;

public sealed class Mos6502Cpu
{
    private readonly Mos6502Bus m_Bus;
    private Mos6502Registers m_Registers;

    private byte m_CurrentOpCode;
    private int m_CurrentOpCodeCycle;

    private byte m_SavedValue0;
    private byte m_SavedValue1;

    public Mos6502Bus Bus => m_Bus;
    public Mos6502Registers Registers => m_Registers;

    public bool IsAtOpCodeStart => m_CurrentOpCodeCycle == 0;

    public Mos6502Cpu(Mos6502Bus bus, Mos6502Registers initialRegisters = default)
    {
        m_Bus = bus;
        m_Registers = initialRegisters;
    }

    public void Step()
    {
        if (m_CurrentOpCodeCycle == 0)
        {
            m_CurrentOpCode = m_Bus.Read(m_Registers.PC);
            ++m_Registers.PC;

            m_CurrentOpCodeCycle = 1;
        }
        else
        {
            switch (m_CurrentOpCode)
            {
                case 0xea:
                case 0x1a:
                case 0x3a:
                case 0x5a:
                case 0x7a:
                case 0xda:
                case 0xfa:
                    NOP_impl();
                    break;
                case 0x80:
                case 0x82:
                case 0x89:
                case 0xc2:
                case 0xe2:
                    NOP_immediate();
                    break;
                case 0x04:
                case 0x44:
                case 0x64:
                    NOP_zeropage();
                    break;
                case 0x14:
                case 0x34:
                case 0x54:
                case 0x74:
                case 0xd4:
                case 0xf4:
                    NOP_zeropage_xindexed();
                    break;
                case 0x0c:
                    NOP_absolute();
                    break;
                case 0x1c:
                case 0x3c:
                case 0x5c:
                case 0x7c:
                case 0xdc:
                case 0xfc:
                    NOP_absolute_xindexed();
                    break;
                default:
                    _ = m_Bus.Read(m_Registers.PC);
                    m_CurrentOpCodeCycle = 0;
                    break;
            }
        }

        m_Bus.Step();
    }

    private void NOP_impl()
    {
        _ = m_Bus.Read(m_Registers.PC);
        m_CurrentOpCodeCycle = 0;
    }

    private void NOP_immediate()
    {
        _ = m_Bus.Read(m_Registers.PC);
        ++m_Registers.PC;
        m_CurrentOpCodeCycle = 0;
    }

    private void NOP_zeropage()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                m_CurrentOpCodeCycle = 2;
                break;
            default:
                _ = m_Bus.Read(m_SavedValue0);
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }

    private void NOP_zeropage_xindexed()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                m_CurrentOpCodeCycle = 2;
                break;
            case 2:
                _ = m_Bus.Read(m_SavedValue0);
                m_CurrentOpCodeCycle = 3;
                break;
            default:
                _ = m_Bus.Read(GetZeropageXIndexedNoCarry(m_SavedValue0, m_Registers.X));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }

    private void NOP_absolute()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                m_CurrentOpCodeCycle = 2;
                break;
            case 2:
                m_SavedValue1 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                m_CurrentOpCodeCycle = 3;
                break;
            default:
                _ = m_Bus.Read(GetAbsolute(m_SavedValue0, m_SavedValue1));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }

    private void NOP_absolute_xindexed()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                m_CurrentOpCodeCycle = 2;
                break;
            case 2:
                m_SavedValue1 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                m_CurrentOpCodeCycle = 3;
                break;
            case 3:
                _ = m_Bus.Read(GetAbsoluteXIndexedNoCarry(m_SavedValue0, m_SavedValue1, m_Registers.X));
                if (m_SavedValue0 + m_Registers.X > 0xFF)
                {
                    m_CurrentOpCodeCycle = 4;
                }
                else
                {
                    m_CurrentOpCodeCycle = 0;
                }
                break;
            default:
                _ = m_Bus.Read(GetAbsoluteXIndexed(m_SavedValue0, m_SavedValue1, m_Registers.X));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }

    private static ushort GetZeropageXIndexedNoCarry(byte value, byte x)
    {
        return (ushort)((value + x) & 0xFF);
    }

    private static ushort GetAbsolute(byte low, byte high)
    {
        return (ushort)(low | high << 8);
    }

    private static ushort GetAbsoluteXIndexedNoCarry(byte low, byte high, byte x)
    {
        return (ushort)((low + x) & 0xFF | high << 8);
    }

    private static ushort GetAbsoluteXIndexed(byte low, byte high, byte x)
    {
        return (ushort)((low | high << 8) + x);
    }
}

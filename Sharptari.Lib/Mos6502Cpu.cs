using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharptari.Lib;

public sealed partial class Mos6502Cpu
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
                case 0x69:
                    ADC_immediate();
                    break;
                case 0x65:
                    ADC_zeropage();
                    break;
                case 0x75:
                    ADC_zeropage_xindexed();
                    break;
                case 0x6d:
                    ADC_absolute();
                    break;
                case 0x7d:
                    ADC_absolute_xindexed();
                    break;
                case 0x79:
                    ADC_absolute_yindexed();
                    break;
                case 0x61:
                    ADC_indirect_xindexed();
                    break;
                case 0x71:
                    ADC_indirect_yindexed();
                    break;

                case 0x29:
                    AND_immediate();
                    break;
                case 0x25:
                    AND_zeropage();
                    break;
                case 0x35:
                    AND_zeropage_xindexed();
                    break;
                case 0x2d:
                    AND_absolute();
                    break;
                case 0x3d:
                    AND_absolute_xindexed();
                    break;
                case 0x39:
                    AND_absolute_yindexed();
                    break;
                case 0x21:
                    AND_indirect_xindexed();
                    break;
                case 0x31:
                    AND_indirect_yindexed();
                    break;

                case 0xa9:
                    LDA_immediate();
                    break;
                case 0xa5:
                    LDA_zeropage();
                    break;
                case 0xb5:
                    LDA_zeropage_xindexed();
                    break;
                case 0xad:
                    LDA_absolute();
                    break;
                case 0xbd:
                    LDA_absolute_xindexed();
                    break;
                case 0xb9:
                    LDA_absolute_yindexed();
                    break;
                case 0xa1:
                    LDA_indirect_xindexed();
                    break;
                case 0xb1:
                    LDA_indirect_yindexed();
                    break;

                case 0xa2:
                    LDX_immediate();
                    break;
                case 0xa6:
                    LDX_zeropage();
                    break;
                case 0xb6:
                    LDX_zeropage_yindexed();
                    break;
                case 0xae:
                    LDX_absolute();
                    break;
                case 0xbe:
                    LDX_absolute_yindexed();
                    break;

                case 0xa0:
                    LDY_immediate();
                    break;
                case 0xa4:
                    LDY_zeropage();
                    break;
                case 0xb4:
                    LDY_zeropage_xindexed();
                    break;
                case 0xac:
                    LDY_absolute();
                    break;
                case 0xbc:
                    LDY_absolute_xindexed();
                    break;

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

    private void ADC()
    {
        if (m_Registers.PDecimal)
        {
            int carryIn = m_Registers.PCarry ? 1 : 0;

            int resultForZero = m_Registers.A + m_SavedValue0 + carryIn;

            int resultBcd = (0x0F & m_Registers.A) + (0x0F & m_SavedValue0) + carryIn;

            if (resultBcd >= 10)
                resultBcd = ((resultBcd + 6) & 0x0F) | 0x10;

            resultBcd = (0xF0 & m_Registers.A) + (0xF0 & m_SavedValue0) + resultBcd;

            bool negative = (resultBcd & 0x80) != 0;
            bool overflow = ((m_Registers.A ^ m_SavedValue0) & 0x80) == 0 && ((m_Registers.A ^ resultBcd) & 0x80) != 0;

            if (resultBcd >= 0xA0)
                resultBcd += 0x60;

            bool carryOut = resultBcd > 0xFF;

            m_Registers.A = (byte)(resultBcd & 0xFF);
            m_Registers.PCarry = carryOut;
            m_Registers.PZero = (resultForZero & 0xFF) == 0;
            m_Registers.PNegative = negative;
            m_Registers.POverflow = overflow;
        }
        else
        {
            int carryIn = m_Registers.PCarry ? 1 : 0;
            int result = m_Registers.A + m_SavedValue0 + carryIn;
            bool carryOut = result > 0xFF;
            bool overflow = ((m_Registers.A ^ m_SavedValue0) & 0x80) == 0 && ((m_Registers.A ^ result) & 0x80) != 0;
            m_Registers.A = (byte)(result & 0xFF);
            m_Registers.PCarry = carryOut;
            m_Registers.PZero = (result & 0xFF) == 0;
            m_Registers.PNegative = (result & 0x80) != 0;
            m_Registers.POverflow = overflow;
        }
    }

    private void AND()
    {
        int result = m_Registers.A & m_SavedValue0;
        m_Registers.A = (byte)result;
        m_Registers.PZero = result == 0;
        m_Registers.PNegative = (result & 0x80) != 0;
    }

    private void LDA()
    {
        m_Registers.A = m_SavedValue0;
        m_Registers.PZero = m_Registers.A == 0;
        m_Registers.PNegative = (m_Registers.A & 0x80) != 0;
    }

    private void LDX()
    {
        m_Registers.X = m_SavedValue0;
        m_Registers.PZero = m_Registers.X == 0;
        m_Registers.PNegative = (m_Registers.X & 0x80) != 0;
    }

    private void LDY()
    {
        m_Registers.Y = m_SavedValue0;
        m_Registers.PZero = m_Registers.Y == 0;
        m_Registers.PNegative = (m_Registers.Y & 0x80) != 0;
    }

    private void NOP()
    {
        // nothing
    }

    private static byte FromBcd(byte value)
    {
        var low = value & 0x0F;
        var high = (value >> 4) & 0x0F;
        return (byte)(low + high * 10);
    }

    private static byte ToBcd(byte value)
    {
        if (value > 99)
            value = 99;
        var low = value % 10;
        var high = value / 10;
        return (byte)(low + (high << 4));
    }

    private static ushort GetAdd1NoCarry(byte value)
    {
        return (ushort)((value + 1) & 0xFF);
    }

    private static ushort GetZeropageIndexedNoCarry(byte value, byte x)
    {
        return (ushort)((value + x) & 0xFF);
    }

    private static ushort GetZeropageIndexedAdd1NoCarry(byte value, byte x)
    {
        return (ushort)((value + x + 1) & 0xFF);
    }

    private static ushort GetAbsolute(byte low, byte high)
    {
        return (ushort)(low | high << 8);
    }

    private static ushort GetAbsoluteIndexedNoCarry(byte low, byte high, byte x)
    {
        return (ushort)((low + x) & 0xFF | high << 8);
    }

    private static ushort GetAbsoluteIndexed(byte low, byte high, byte x)
    {
        return (ushort)((low | high << 8) + x);
    }
}

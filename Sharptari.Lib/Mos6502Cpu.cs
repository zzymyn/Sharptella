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

                case 0xe9:
                    SBC_immediate();
                    break;
                case 0xe5:
                    SBC_zeropage();
                    break;
                case 0xf5:
                    SBC_zeropage_xindexed();
                    break;
                case 0xed:
                    SBC_absolute();
                    break;
                case 0xfd:
                    SBC_absolute_xindexed();
                    break;
                case 0xf9:
                    SBC_absolute_yindexed();
                    break;
                case 0xe1:
                    SBC_indirect_xindexed();
                    break;
                case 0xf1:
                    SBC_indirect_yindexed();
                    break;

                case 0x85:
                    STA_zeropage();
                    break;
                case 0x95:
                    STA_zeropage_xindexed();
                    break;
                case 0x8d:
                    STA_absolute();
                    break;
                case 0x9d:
                    STA_absolute_xindexed();
                    break;
                case 0x99:
                    STA_absolute_yindexed();
                    break;
                case 0x81:
                    STA_indirect_xindexed();
                    break;
                case 0x91:
                    STA_indirect_yindexed();
                    break;

                case 0x86:
                    STX_zeropage();
                    break;
                case 0x96:
                    STX_zeropage_yindexed();
                    break;
                case 0x8e:
                    STX_absolute();
                    break;

                case 0x84:
                    STY_zeropage();
                    break;
                case 0x94:
                    STY_zeropage_xindexed();
                    break;
                case 0x8c:
                    STY_absolute();
                    break;

                default:
                    _ = m_Bus.Read(m_Registers.PC);
                    m_CurrentOpCodeCycle = 0;
                    break;
            }
        }

        m_Bus.Step();
    }

    private void ADC(byte arg)
    {
        if (m_Registers.PDecimal)
        {
            int carryIn = m_Registers.PCarry ? 1 : 0;

            int resultFull = (0x0F & m_Registers.A) + (0x0F & arg) + carryIn;

            if (resultFull >= 10)
                resultFull = ((resultFull + 6) & 0x0F) | 0x10;

            resultFull = (0xF0 & m_Registers.A) + (0xF0 & arg) + resultFull;
            byte result = (byte)resultFull;

            // 6502 sets the negative flag based on the result before adjusting for BCD, for some reason:
            bool negative = CheckNegative(result);
            bool overflow = CheckOverflow(m_Registers.A, arg, result);

            if (resultFull >= 0xA0)
                resultFull += 0x60;

            result = (byte)resultFull;

            bool carryOut = resultFull > 0xFF;

            // the 6502 calcs the zero flag based on the non-bcd result, for some reason:
            int resultForZero = m_Registers.A + arg + carryIn;
            bool zero = CheckZero((byte)resultForZero);

            m_Registers.A = result;
            m_Registers.PCarry = carryOut;
            m_Registers.PZero = zero;
            m_Registers.PNegative = negative;
            m_Registers.POverflow = overflow;
        }
        else
        {
            int carryIn = m_Registers.PCarry ? 1 : 0;

            int resultFull = m_Registers.A + arg + carryIn;
            byte result = (byte)resultFull;

            bool carryOut = resultFull > 0xFF;
            bool overflow = CheckOverflow(m_Registers.A, arg, result);

            m_Registers.A = result;
            m_Registers.PCarry = carryOut;
            m_Registers.PZero = CheckZero(result);
            m_Registers.PNegative = CheckNegative(result);
            m_Registers.POverflow = overflow;
        }
    }

    private void AND(byte arg)
    {
        byte result = (byte)(m_Registers.A & arg);

        m_Registers.A = result;
        m_Registers.PZero = CheckZero(result);
        m_Registers.PNegative = CheckNegative(result);
    }

    private void LDA(byte arg)
    {
        m_Registers.A = arg;
        m_Registers.PZero = CheckZero(arg);
        m_Registers.PNegative = CheckNegative(arg);
    }

    private void LDX(byte arg)
    {
        m_Registers.X = arg;
        m_Registers.PZero = CheckZero(arg);
        m_Registers.PNegative = CheckNegative(arg);
    }

    private void LDY(byte arg)
    {
        m_Registers.Y = arg;
        m_Registers.PZero = CheckZero(arg);
        m_Registers.PNegative = CheckNegative(arg);
    }

    private static void NOP()
    {
        // nothing
    }

    private static void NOP(byte _)
    {
        // nothing
    }

    private void SBC(byte arg)
    {
        byte inv0 = (byte)~arg;

        if (m_Registers.PDecimal)
        {
            int carryIn = m_Registers.PCarry ? 1 : 0;

            int resultFull = (0x0F & m_Registers.A) - (0x0F & arg) + carryIn - 1;

            if (resultFull < 0)
                resultFull = ((resultFull - 6) & 0x0F) - 0x10;

            resultFull = (0xF0 & m_Registers.A) - (0xF0 & arg) + resultFull;
            byte result = (byte)resultFull;

            // 6502 sets the negative flag based on the result before adjusting for BCD, for some reason:
            bool negative = CheckNegative(result);
            bool overflow = CheckOverflow(m_Registers.A, inv0, result);

            if (resultFull < 0)
                resultFull -= 0x60;

            result = (byte)resultFull;

            // the 6502 calcs the zero and carry flags based on the non-bcd result, for some reason:
            int resultForZero = m_Registers.A + inv0 + carryIn;
            bool carryOut = resultForZero > 0xFF;
            bool zero = CheckZero((byte)resultForZero);

            m_Registers.A = result;
            m_Registers.PCarry = carryOut;
            m_Registers.PZero = zero;
            m_Registers.PNegative = negative;
            m_Registers.POverflow = overflow;
        }
        else
        {
            int carryIn = m_Registers.PCarry ? 1 : 0;

            int resultFull = m_Registers.A + inv0 + carryIn;
            byte result = (byte)resultFull;

            bool carryOut = resultFull > 0xFF;
            bool overflow = CheckOverflow(m_Registers.A, inv0, result);

            m_Registers.A = result;
            m_Registers.PCarry = carryOut;
            m_Registers.PZero = CheckZero(result);
            m_Registers.PNegative = CheckNegative(result);
            m_Registers.POverflow = overflow;
        }
    }

    private byte STA()
    {
        return m_Registers.A;
    }

    private byte STX()
    {
        return m_Registers.X;
    }

    private byte STY()
    {
        return m_Registers.Y;
    }

    private static bool CheckOverflow(byte a, byte b, byte result)
    {
        var hiA = (a & 0x80) != 0;
        var hiB = (b & 0x80) != 0;
        var hiResult = (result & 0x80) != 0;
        return (hiA == hiB) && (hiA != hiResult);
    }

    private static bool CheckNegative(byte value)
    {
        return (value & 0x80) != 0;
    }

    private static bool CheckZero(byte value)
    {
        return value == 0;
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

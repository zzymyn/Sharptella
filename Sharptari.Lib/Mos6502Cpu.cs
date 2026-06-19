using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharptari.Lib;

public sealed partial class Mos6502Cpu<BusT>
    where BusT : IMos6502Bus
{
    private readonly BusT m_Bus;
    private Mos6502Registers m_Registers;

    private int m_CurrentOpCode;
    private int m_CurrentOpCodeCycle;

    private byte m_SavedValue0;
    private byte m_SavedValue1;
    private byte m_SavedValue2;

    public BusT Bus => m_Bus;
    public Mos6502Registers Registers => m_Registers;

    public bool IsAtOpCodeStart => m_CurrentOpCodeCycle == 0;

    public Mos6502Cpu(BusT bus, Mos6502Registers initialRegisters = default)
    {
        m_Bus = bus;
        m_Registers = initialRegisters;
    }

    public void Reboot()
    {
        m_CurrentOpCode = 0x100;
        m_CurrentOpCodeCycle = 1;
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
                case 0x100:
                    BOOT();
                    break;

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

                case 0x4b:
                    ALR_immediate();
                    break;

                case 0x0b:
                case 0x2b:
                    ANC_immediate();
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

                case 0x6b:
                    ARR_immediate();
                    break;

                case 0x0a:
                    ASL_impl();
                    break;
                case 0x06:
                    ASL_zeropage();
                    break;
                case 0x16:
                    ASL_zeropage_xindexed();
                    break;
                case 0x0e:
                    ASL_absolute();
                    break;
                case 0x1e:
                    ASL_absolute_xindexed();
                    break;

                case 0x90:
                    BCC_relative();
                    break;

                case 0xb0:
                    BCS_relative();
                    break;

                case 0xf0:
                    BEQ_relative();
                    break;

                case 0x24:
                    BIT_zeropage();
                    break;
                case 0x2c:
                    BIT_absolute();
                    break;

                case 0x30:
                    BMI_relative();
                    break;

                case 0xd0:
                    BNE_relative();
                    break;

                case 0x10:
                    BPL_relative();
                    break;

                case 0x00:
                    BRK_impl();
                    break;

                case 0x50:
                    BVC_relative();
                    break;

                case 0x70:
                    BVS_relative();
                    break;

                case 0x18:
                    CLC_impl();
                    break;

                case 0xd8:
                    CLD_impl();
                    break;

                case 0x58:
                    CLI_impl();
                    break;

                case 0xb8:
                    CLV_impl();
                    break;

                case 0xc9:
                    CMP_immediate();
                    break;
                case 0xc5:
                    CMP_zeropage();
                    break;
                case 0xd5:
                    CMP_zeropage_xindexed();
                    break;
                case 0xcd:
                    CMP_absolute();
                    break;
                case 0xdd:
                    CMP_absolute_xindexed();
                    break;
                case 0xd9:
                    CMP_absolute_yindexed();
                    break;
                case 0xc1:
                    CMP_indirect_xindexed();
                    break;
                case 0xd1:
                    CMP_indirect_yindexed();
                    break;

                case 0xe0:
                    CPX_immediate();
                    break;
                case 0xe4:
                    CPX_zeropage();
                    break;
                case 0xec:
                    CPX_absolute();
                    break;

                case 0xc0:
                    CPY_immediate();
                    break;
                case 0xc4:
                    CPY_zeropage();
                    break;
                case 0xcc:
                    CPY_absolute();
                    break;

                case 0xc7:
                    DCP_zeropage();
                    break;
                case 0xd7:
                    DCP_zeropage_xindexed();
                    break;
                case 0xcf:
                    DCP_absolute();
                    break;
                case 0xdf:
                    DCP_absolute_xindexed();
                    break;
                case 0xdb:
                    DCP_absolute_yindexed();
                    break;
                case 0xc3:
                    DCP_indirect_xindexed();
                    break;
                case 0xd3:
                    DCP_indirect_yindexed();
                    break;

                case 0xc6:
                    DEC_zeropage();
                    break;
                case 0xd6:
                    DEC_zeropage_xindexed();
                    break;
                case 0xce:
                    DEC_absolute();
                    break;
                case 0xde:
                    DEC_absolute_xindexed();
                    break;

                case 0xca:
                    DEX_impl();
                    break;

                case 0x88:
                    DEY_impl();
                    break;

                case 0x49:
                    EOR_immediate();
                    break;
                case 0x45:
                    EOR_zeropage();
                    break;
                case 0x55:
                    EOR_zeropage_xindexed();
                    break;
                case 0x4d:
                    EOR_absolute();
                    break;
                case 0x5d:
                    EOR_absolute_xindexed();
                    break;
                case 0x59:
                    EOR_absolute_yindexed();
                    break;
                case 0x41:
                    EOR_indirect_xindexed();
                    break;
                case 0x51:
                    EOR_indirect_yindexed();
                    break;

                case 0xe6:
                    INC_zeropage();
                    break;
                case 0xf6:
                    INC_zeropage_xindexed();
                    break;
                case 0xee:
                    INC_absolute();
                    break;
                case 0xfe:
                    INC_absolute_xindexed();
                    break;

                case 0xe8:
                    INX_impl();
                    break;

                case 0xc8:
                    INY_impl();
                    break;

                case 0xe7:
                    ISC_zeropage();
                    break;
                case 0xf7:
                    ISC_zeropage_xindexed();
                    break;
                case 0xef:
                    ISC_absolute();
                    break;
                case 0xff:
                    ISC_absolute_xindexed();
                    break;
                case 0xfb:
                    ISC_absolute_yindexed();
                    break;
                case 0xe3:
                    ISC_indirect_xindexed();
                    break;
                case 0xf3:
                    ISC_indirect_yindexed();
                    break;

                case 0x02:
                case 0x12:
                case 0x22:
                case 0x32:
                case 0x42:
                case 0x52:
                case 0x62:
                case 0x72:
                case 0x92:
                case 0xb2:
                case 0xd2:
                case 0xf2:
                    JAM_impl();
                    break;

                case 0x4c:
                    JMP_absolute();
                    break;

                case 0x6c:
                    JMP_indirect();
                    break;

                case 0x20:
                    JSR_absolute();
                    break;

                case 0xbb:
                    LAS_absolute_yindexed();
                    break;

                case 0xa7:
                    LAX_zeropage();
                    break;
                case 0xb7:
                    LAX_zeropage_yindexed();
                    break;
                case 0xaf:
                    LAX_absolute();
                    break;
                case 0xbf:
                    LAX_absolute_yindexed();
                    break;
                case 0xa3:
                    LAX_indirect_xindexed();
                    break;
                case 0xb3:
                    LAX_indirect_yindexed();
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

                case 0x4a:
                    LSR_impl();
                    break;
                case 0x46:
                    LSR_zeropage();
                    break;
                case 0x56:
                    LSR_zeropage_xindexed();
                    break;
                case 0x4e:
                    LSR_absolute();
                    break;
                case 0x5e:
                    LSR_absolute_xindexed();
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

                case 0x09:
                    ORA_immediate();
                    break;
                case 0x05:
                    ORA_zeropage();
                    break;
                case 0x15:
                    ORA_zeropage_xindexed();
                    break;
                case 0x0d:
                    ORA_absolute();
                    break;
                case 0x1d:
                    ORA_absolute_xindexed();
                    break;
                case 0x19:
                    ORA_absolute_yindexed();
                    break;
                case 0x01:
                    ORA_indirect_xindexed();
                    break;
                case 0x11:
                    ORA_indirect_yindexed();
                    break;

                case 0x48:
                    PHA_impl();
                    break;

                case 0x08:
                    PHP_impl();
                    break;

                case 0x68:
                    PLA_impl();
                    break;

                case 0x28:
                    PLP_impl();
                    break;

                case 0x27:
                    RLA_zeropage();
                    break;
                case 0x37:
                    RLA_zeropage_xindexed();
                    break;
                case 0x2f:
                    RLA_absolute();
                    break;
                case 0x3f:
                    RLA_absolute_xindexed();
                    break;
                case 0x3b:
                    RLA_absolute_yindexed();
                    break;
                case 0x23:
                    RLA_indirect_xindexed();
                    break;
                case 0x33:
                    RLA_indirect_yindexed();
                    break;

                case 0x2a:
                    ROL_impl();
                    break;
                case 0x26:
                    ROL_zeropage();
                    break;
                case 0x36:
                    ROL_zeropage_xindexed();
                    break;
                case 0x2e:
                    ROL_absolute();
                    break;
                case 0x3e:
                    ROL_absolute_xindexed();
                    break;

                case 0x6a:
                    ROR_impl();
                    break;
                case 0x66:
                    ROR_zeropage();
                    break;
                case 0x76:
                    ROR_zeropage_xindexed();
                    break;
                case 0x6e:
                    ROR_absolute();
                    break;
                case 0x7e:
                    ROR_absolute_xindexed();
                    break;

                case 0x67:
                    RRA_zeropage();
                    break;
                case 0x77:
                    RRA_zeropage_xindexed();
                    break;
                case 0x6f:
                    RRA_absolute();
                    break;
                case 0x7f:
                    RRA_absolute_xindexed();
                    break;
                case 0x7b:
                    RRA_absolute_yindexed();
                    break;
                case 0x63:
                    RRA_indirect_xindexed();
                    break;
                case 0x73:
                    RRA_indirect_yindexed();
                    break;

                case 0x40:
                    RTI_impl();
                    break;

                case 0x60:
                    RTS_impl();
                    break;

                case 0x87:
                    SAX_zeropage();
                    break;
                case 0x97:
                    SAX_zeropage_yindexed();
                    break;
                case 0x8f:
                    SAX_absolute();
                    break;
                case 0x83:
                    SAX_indirect_xindexed();
                    break;

                case 0xcb:
                    SBX_immediate();
                    break;

                case 0xe9:
                case 0xeb:
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

                case 0x38:
                    SEC_impl();
                    break;

                case 0xf8:
                    SED_impl();
                    break;

                case 0x78:
                    SEI_impl();
                    break;

                case 0x07:
                    SLO_zeropage();
                    break;
                case 0x17:
                    SLO_zeropage_xindexed();
                    break;
                case 0x0f:
                    SLO_absolute();
                    break;
                case 0x1f:
                    SLO_absolute_xindexed();
                    break;
                case 0x1b:
                    SLO_absolute_yindexed();
                    break;
                case 0x03:
                    SLO_indirect_xindexed();
                    break;
                case 0x13:
                    SLO_indirect_yindexed();
                    break;

                case 0x47:
                    SRE_zeropage();
                    break;
                case 0x57:
                    SRE_zeropage_xindexed();
                    break;
                case 0x4f:
                    SRE_absolute();
                    break;
                case 0x5f:
                    SRE_absolute_xindexed();
                    break;
                case 0x5b:
                    SRE_absolute_yindexed();
                    break;
                case 0x43:
                    SRE_indirect_xindexed();
                    break;
                case 0x53:
                    SRE_indirect_yindexed();
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

                case 0xaa:
                    TAX_impl();
                    break;
                case 0xa8:
                    TAY_impl();
                    break;
                case 0xba:
                    TSX_impl();
                    break;
                case 0x8a:
                    TXA_impl();
                    break;
                case 0x9a:
                    TXS_impl();
                    break;
                case 0x98:
                    TYA_impl();
                    break;

                default:
                    _ = m_Bus.Read(m_Registers.PC);
                    m_CurrentOpCodeCycle = 0;
                    break;
            }
        }

        m_Bus.Step();
    }

    private void BOOT()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_Registers.PInterruptDisable = true;
                m_CurrentOpCodeCycle = 2;
                break;
            case 2:
                m_CurrentOpCodeCycle = 3;
                break;
            case 3:
            case 4:
            case 5:
                _ = m_Bus.Read(GetStackAddress(m_Registers.S));
                ++m_CurrentOpCodeCycle;
                break;
            case 6:
                m_SavedValue0 = m_Bus.Read(0xFFFC);
                m_CurrentOpCodeCycle = 7;
                break;
            case 7:
                m_SavedValue1 = m_Bus.Read(0xFFFD);
                m_Registers.PC = GetAbsolute(m_SavedValue0, m_SavedValue1);
                m_Registers.PInterruptDisable = false;
                m_CurrentOpCodeCycle = 0;
                break;
        }
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

    private void ALR(byte arg)
    {
        AND(arg);
        LSR();
    }

    private void ANC(byte arg)
    {
        AND(arg);
        m_Registers.PCarry = m_Registers.PNegative;
    }

    private void AND(byte arg)
    {
        byte result = (byte)(m_Registers.A & arg);

        m_Registers.A = result;
        m_Registers.PZero = CheckZero(result);
        m_Registers.PNegative = CheckNegative(result);
    }

    private void ARR(byte arg)
    {
        int carryIn = m_Registers.PCarry ? 1 : 0;

        int andResult = m_Registers.A & arg;
        int rorResult = (andResult >> 1) | (carryIn << 7);
        byte result = (byte)rorResult;

        bool bit6 = (result & 0x40) != 0;
        bool bit5 = (result & 0x20) != 0;

        m_Registers.A = result;
        m_Registers.PZero = CheckZero(result);
        m_Registers.PNegative = CheckNegative(result);

        // Carry is set to Bit 6 of the rotated result
        m_Registers.PCarry = bit6;

        // Overflow is set to Bit 6 XOR Bit 5 of the rotated result
        m_Registers.POverflow = bit6 != bit5;

        if (m_Registers.PDecimal)
        {
            int bcdResult = 0;

            // Low Nibble BCD Correction
            if ((andResult & 0x0F) + (andResult & 0x01) >= 6)
            {
                bcdResult |= (rorResult + 6) & 0x0F;
            }
            else
            {
                bcdResult |= rorResult & 0x0F;
            }

            // High Nibble BCD Correction
            if ((andResult & 0xF0) + (andResult & 0x10) >= 0x60)
            {
                bcdResult |= (rorResult + 0x60) & 0xF0;
                m_Registers.PCarry = true;
            }
            else
            {
                bcdResult |= rorResult & 0xF0;
                m_Registers.PCarry = false;
            }

            m_Registers.A = (byte)bcdResult;
        }
    }

    private void ASL()
    {
        byte arg = m_Registers.A;
        byte result = (byte)(arg << 1);
        bool carryOut = (arg & 0x80) != 0;
        m_Registers.A = result;
        m_Registers.PCarry = carryOut;
        m_Registers.PZero = CheckZero(result);
        m_Registers.PNegative = CheckNegative(result);
    }

    private byte ASL(byte arg)
    {
        byte result = (byte)(arg << 1);
        bool carryOut = (arg & 0x80) != 0;
        m_Registers.PCarry = carryOut;
        m_Registers.PZero = CheckZero(result);
        m_Registers.PNegative = CheckNegative(result);
        return result;
    }

    private bool BCC()
    {
        return m_Registers.PCarry;
    }

    private bool BCS()
    {
        return !m_Registers.PCarry;
    }

    private bool BEQ()
    {
        return !m_Registers.PZero;
    }

    private void BIT(byte arg)
    {
        byte result = (byte)(m_Registers.A & arg);
        m_Registers.PZero = CheckZero(result);
        m_Registers.PNegative = (arg & 0x80) != 0;
        m_Registers.POverflow = (arg & 0x40) != 0;
    }

    private bool BMI()
    {
        return !m_Registers.PNegative;
    }

    private bool BNE()
    {
        return m_Registers.PZero;
    }

    private bool BPL()
    {
        return m_Registers.PNegative;
    }

    private void BRK_impl()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                _ = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                m_CurrentOpCodeCycle = 2;
                break;
            case 2:
                m_Bus.Write(GetStackAddress(m_Registers.S), GetHi(m_Registers.PC));
                --m_Registers.S;
                m_CurrentOpCodeCycle = 3;
                break;
            case 3:
                m_Bus.Write(GetStackAddress(m_Registers.S), GetLo(m_Registers.PC));
                --m_Registers.S;
                m_CurrentOpCodeCycle = 4;
                break;
            case 4:
                m_Bus.Write(GetStackAddress(m_Registers.S), m_Registers.ReadP(true));
                --m_Registers.S;
                m_CurrentOpCodeCycle = 5;
                break;
            case 5:
                m_SavedValue0 = m_Bus.Read(0xFFFE);
                m_CurrentOpCodeCycle = 6;
                break;
            case 6:
                m_SavedValue1 = m_Bus.Read(0xFFFF);
                m_Registers.PC = GetAbsolute(m_SavedValue0, m_SavedValue1);
                m_Registers.PInterruptDisable = true;
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }

    private bool BVC()
    {
        return m_Registers.POverflow;
    }

    private bool BVS()
    {
        return !m_Registers.POverflow;
    }

    private void CLC()
    {
        m_Registers.PCarry = false;
    }

    private void CLD()
    {
        m_Registers.PDecimal = false;
    }

    private void CLI()
    {
        m_Registers.PInterruptDisable = false;
    }

    private void CLV()
    {
        m_Registers.POverflow = false;
    }

    private void CMP(byte arg)
    {
        byte invArg = (byte)~arg;

        int resultFull = m_Registers.A + invArg + 1;
        byte result = (byte)resultFull;

        bool carryOut = resultFull > 0xFF;
        m_Registers.PCarry = carryOut;
        m_Registers.PZero = CheckZero(result);
        m_Registers.PNegative = CheckNegative(result);
    }

    private void CPX(byte arg)
    {
        byte invArg = (byte)~arg;

        int resultFull = m_Registers.X + invArg + 1;
        byte result = (byte)resultFull;

        bool carryOut = resultFull > 0xFF;
        m_Registers.PCarry = carryOut;
        m_Registers.PZero = CheckZero(result);
        m_Registers.PNegative = CheckNegative(result);
    }

    private void CPY(byte arg)
    {
        byte invArg = (byte)~arg;

        int resultFull = m_Registers.Y + invArg + 1;
        byte result = (byte)resultFull;

        bool carryOut = resultFull > 0xFF;
        m_Registers.PCarry = carryOut;
        m_Registers.PZero = CheckZero(result);
        m_Registers.PNegative = CheckNegative(result);
    }

    private byte DCP(byte arg)
    {
        byte decResult = DEC(arg);
        CMP(decResult);
        return decResult;
    }

    private byte DEC(byte arg)
    {
        byte result = (byte)(arg - 1);
        m_Registers.PZero = CheckZero(result);
        m_Registers.PNegative = CheckNegative(result);
        return result;
    }

    private void DEX()
    {
        byte result = (byte)(m_Registers.X - 1);
        m_Registers.X = result;
        m_Registers.PZero = CheckZero(result);
        m_Registers.PNegative = CheckNegative(result);
    }

    private void DEY()
    {
        byte result = (byte)(m_Registers.Y - 1);
        m_Registers.Y = result;
        m_Registers.PZero = CheckZero(result);
        m_Registers.PNegative = CheckNegative(result);
    }

    private void EOR(byte arg)
    {
        byte result = (byte)(m_Registers.A ^ arg);
        m_Registers.A = result;
        m_Registers.PZero = CheckZero(result);
        m_Registers.PNegative = CheckNegative(result);
    }

    private byte INC(byte arg)
    {
        byte result = (byte)(arg + 1);
        m_Registers.PZero = CheckZero(result);
        m_Registers.PNegative = CheckNegative(result);
        return result;
    }

    private void INX()
    {
        m_Registers.X = (byte)(m_Registers.X + 1);
        m_Registers.PZero = CheckZero(m_Registers.X);
        m_Registers.PNegative = CheckNegative(m_Registers.X);
    }

    private void INY()
    {
        m_Registers.Y = (byte)(m_Registers.Y + 1);
        m_Registers.PZero = CheckZero(m_Registers.Y);
        m_Registers.PNegative = CheckNegative(m_Registers.Y);
    }

    private byte ISC(byte arg)
    {
        byte incResult = INC(arg);
        SBC(incResult);
        return incResult;
    }

    private void JAM_impl()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                _ = m_Bus.Read(m_Registers.PC);
                m_CurrentOpCodeCycle = 2;
                break;
            case 2:
            default:
                _ = m_Bus.Read(0xFFFF);
                ++m_CurrentOpCodeCycle;
                break;
            case 3:
            case 4:
                _ = m_Bus.Read(0xFFFE);
                ++m_CurrentOpCodeCycle;
                break;
        }
    }

    private void JMP_absolute()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                m_CurrentOpCodeCycle = 2;
                break;
            default:
                m_SavedValue1 = m_Bus.Read(m_Registers.PC);
                m_Registers.PC = GetAbsolute(m_SavedValue0, m_SavedValue1);
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }

    private void JMP_indirect()
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
                m_SavedValue2 = m_Bus.Read(GetAbsolute(m_SavedValue0, m_SavedValue1));
                ++m_SavedValue0;
                m_CurrentOpCodeCycle = 4;
                break;
            case 4:
                m_SavedValue1 = m_Bus.Read(GetAbsolute(m_SavedValue0, m_SavedValue1));
                m_Registers.PC = GetAbsolute(m_SavedValue2, m_SavedValue1);
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }

    private void JSR_absolute()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                m_CurrentOpCodeCycle = 2;
                break;
            case 2:
                _ = m_Bus.Read(GetStackAddress(m_Registers.S));
                m_CurrentOpCodeCycle = 3;
                break;
            case 3:
                m_Bus.Write(GetStackAddress(m_Registers.S), GetHi(m_Registers.PC));
                --m_Registers.S;
                m_CurrentOpCodeCycle = 4;
                break;
            case 4:
                m_Bus.Write(GetStackAddress(m_Registers.S), GetLo(m_Registers.PC));
                --m_Registers.S;
                m_CurrentOpCodeCycle = 5;
                break;
            default:
                m_SavedValue1 = m_Bus.Read(m_Registers.PC);
                m_Registers.PC = GetAbsolute(m_SavedValue0, m_SavedValue1);
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }

    private void LAS(byte arg)
    {
        byte result = (byte)(m_Registers.S & arg);
        m_Registers.A = result;
        m_Registers.X = result;
        m_Registers.S = result;
        m_Registers.PZero = CheckZero(result);
        m_Registers.PNegative = CheckNegative(result);
    }

    private void LAX(byte arg)
    {
        m_Registers.A = arg;
        m_Registers.X = arg;
        m_Registers.PZero = CheckZero(arg);
        m_Registers.PNegative = CheckNegative(arg);
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

    private void LSR()
    {
        byte arg = m_Registers.A;
        byte result = (byte)(arg >> 1);
        bool carryOut = (arg & 0x01) != 0;
        m_Registers.A = result;
        m_Registers.PCarry = carryOut;
        m_Registers.PZero = CheckZero(result);
        m_Registers.PNegative = false;
    }

    private byte LSR(byte arg)
    {
        byte result = (byte)(arg >> 1);
        bool carryOut = (arg & 0x01) != 0;
        m_Registers.PCarry = carryOut;
        m_Registers.PZero = CheckZero(result);
        m_Registers.PNegative = false;
        return result;
    }

    private static void NOP()
    {
        // nothing
    }

    private static void NOP(byte _)
    {
        // nothing
    }

    private void ORA(byte arg)
    {
        byte result = (byte)(m_Registers.A | arg);
        m_Registers.A = result;
        m_Registers.PZero = CheckZero(result);
        m_Registers.PNegative = CheckNegative(result);
    }

    private void PHA_impl()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                _ = m_Bus.Read(m_Registers.PC);
                m_CurrentOpCodeCycle = 2;
                break;
            default:
                m_Bus.Write(GetStackAddress(m_Registers.S), m_Registers.A);
                --m_Registers.S;
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }

    private void PHP_impl()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                _ = m_Bus.Read(m_Registers.PC);
                m_CurrentOpCodeCycle = 2;
                break;
            default:
                m_Bus.Write(GetStackAddress(m_Registers.S), m_Registers.ReadP(true));
                --m_Registers.S;
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }

    private void PLA_impl()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                _ = m_Bus.Read(m_Registers.PC);
                m_CurrentOpCodeCycle = 2;
                break;
            case 2:
                _ = m_Bus.Read(GetStackAddress(m_Registers.S));
                m_Registers.S++;
                m_CurrentOpCodeCycle = 3;
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetStackAddress(m_Registers.S));
                m_CurrentOpCodeCycle = 0;
                m_Registers.A = m_SavedValue2;
                m_Registers.PZero = CheckZero(m_SavedValue2);
                m_Registers.PNegative = CheckNegative(m_SavedValue2);
                break;
        }
    }

    private void PLP_impl()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                _ = m_Bus.Read(m_Registers.PC);
                m_CurrentOpCodeCycle = 2;
                break;
            case 2:
                _ = m_Bus.Read(GetStackAddress(m_Registers.S));
                m_Registers.S++;
                m_CurrentOpCodeCycle = 3;
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetStackAddress(m_Registers.S));
                m_CurrentOpCodeCycle = 0;
                m_Registers.WriteP(m_SavedValue2);
                break;
        }
    }

    private byte RLA(byte arg)
    {
        byte rol = ROL(arg);
        AND(rol);
        return rol;
    }

    private void ROL()
    {
        byte arg = m_Registers.A;
        byte result = (byte)((arg << 1) | (m_Registers.PCarry ? 1 : 0));
        bool carryOut = (arg & 0x80) != 0;
        m_Registers.A = result;
        m_Registers.PCarry = carryOut;
        m_Registers.PZero = CheckZero(result);
        m_Registers.PNegative = CheckNegative(result);
    }

    private byte ROL(byte arg)
    {
        byte result = (byte)((arg << 1) | (m_Registers.PCarry ? 1 : 0));
        bool carryOut = (arg & 0x80) != 0;
        m_Registers.PCarry = carryOut;
        m_Registers.PZero = CheckZero(result);
        m_Registers.PNegative = CheckNegative(result);
        return result;
    }

    private void ROR()
    {
        byte arg = m_Registers.A;
        byte result = (byte)((arg >> 1) | (m_Registers.PCarry ? 0x80 : 0));
        bool carryOut = (arg & 0x01) != 0;
        m_Registers.A = result;
        m_Registers.PCarry = carryOut;
        m_Registers.PZero = CheckZero(result);
        m_Registers.PNegative = CheckNegative(result);
    }

    private byte ROR(byte arg)
    {
        byte result = (byte)((arg >> 1) | (m_Registers.PCarry ? 0x80 : 0));
        bool carryOut = (arg & 0x01) != 0;
        m_Registers.PCarry = carryOut;
        m_Registers.PZero = CheckZero(result);
        m_Registers.PNegative = CheckNegative(result);
        return result;
    }

    private byte RRA(byte arg)
    {
        byte ror = ROR(arg);
        ADC(ror);
        return ror;
    }

    private void RTI_impl()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                _ = m_Bus.Read(m_Registers.PC);
                m_CurrentOpCodeCycle = 2;
                break;
            case 2:
                _ = m_Bus.Read(GetStackAddress(m_Registers.S));
                m_Registers.S++;
                m_CurrentOpCodeCycle = 3;
                break;
            case 3:
                m_Registers.WriteP(m_Bus.Read(GetStackAddress(m_Registers.S)));
                m_Registers.S++;
                m_CurrentOpCodeCycle = 4;
                break;
            case 4:
                m_SavedValue0 = m_Bus.Read(GetStackAddress(m_Registers.S));
                m_Registers.S++;
                m_CurrentOpCodeCycle = 5;
                break;
            case 5:
                m_SavedValue1 = m_Bus.Read(GetStackAddress(m_Registers.S));
                m_Registers.PC = GetAbsolute(m_SavedValue0, m_SavedValue1);
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }

    private void RTS_impl()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                _ = m_Bus.Read(m_Registers.PC);
                m_CurrentOpCodeCycle = 2;
                break;
            case 2:
                _ = m_Bus.Read(GetStackAddress(m_Registers.S));
                m_Registers.S++;
                m_CurrentOpCodeCycle = 3;
                break;
            case 3:
                m_SavedValue0 = m_Bus.Read(GetStackAddress(m_Registers.S));
                m_Registers.S++;
                m_CurrentOpCodeCycle = 4;
                break;
            case 4:
                m_SavedValue1 = m_Bus.Read(GetStackAddress(m_Registers.S));
                m_Registers.PC = GetAbsolute(m_SavedValue0, m_SavedValue1);
                m_CurrentOpCodeCycle = 5;
                break;
            case 5:
                _ = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }

    private byte SAX()
    {
        return (byte)(m_Registers.A & m_Registers.X);
    }

    private void SBX(byte arg)
    {
        int resultFull = (m_Registers.X & m_Registers.A) - arg;
        byte result = (byte)resultFull;
        bool carryOut = resultFull >= 0;
        m_Registers.X = result;
        m_Registers.PZero = CheckZero(result);
        m_Registers.PNegative = CheckNegative(result);
        m_Registers.PCarry = carryOut;
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

    private void SEC()
    {
        m_Registers.PCarry = true;
    }

    private void SED()
    {
        m_Registers.PDecimal = true;
    }

    private void SEI()
    {
        m_Registers.PInterruptDisable = true;
    }

    private byte SLO(byte arg)
    {
        arg = ASL(arg);
        ORA(arg);
        return arg;
    }

    private byte SRE(byte arg)
    {
        arg = LSR(arg);
        EOR(arg);
        return arg;
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

    private void TAX()
    {
        byte result = m_Registers.A;
        m_Registers.X = result;
        m_Registers.PZero = CheckZero(result);
        m_Registers.PNegative = CheckNegative(result);
    }

    private void TAY()
    {
        byte result = m_Registers.A;
        m_Registers.Y = result;
        m_Registers.PZero = CheckZero(result);
        m_Registers.PNegative = CheckNegative(result);
    }

    private void TSX()
    {
        byte result = m_Registers.S;
        m_Registers.X = result;
        m_Registers.PZero = CheckZero(result);
        m_Registers.PNegative = CheckNegative(result);
    }

    private void TXA()
    {
        byte result = m_Registers.X;
        m_Registers.A = result;
        m_Registers.PZero = CheckZero(result);
        m_Registers.PNegative = CheckNegative(result);
    }

    private void TXS()
    {
        m_Registers.S = m_Registers.X;
    }

    private void TYA()
    {
        byte result = m_Registers.Y;
        m_Registers.A = result;
        m_Registers.PZero = CheckZero(result);
        m_Registers.PNegative = CheckNegative(result);
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

    private static byte GetLo(ushort value)
    {
        return (byte)value;
    }

    private static byte GetHi(ushort value)
    {
        return (byte)(value >> 8);
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

    private static ushort GetRelativeSigned(ushort pc, byte offset)
    {
        return (ushort)(pc + (sbyte)offset);
    }

    private static ushort GetRelativeSignedNoCarry(ushort pc, byte offset)
    {
        return (ushort)(pc & 0xFF00 | (byte)(pc + offset));
    }

    private static ushort GetStackAddress(byte offset)
    {
        return (ushort)(0x100 | offset);
    }
}

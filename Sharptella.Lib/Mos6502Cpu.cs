using System.Diagnostics;
using Sharptella.Gen;

namespace Sharptella.Lib;

public sealed partial class Mos6502Cpu<BusT>
    where BusT : IMos6502Bus
{
    // Not a real 6502 opcode, just a pseudo-opcode to represent the CPU booting up and reading the reset vector:
    private const int OpCodeBOOT = 0x100;

    private readonly BusT m_Bus;
    private Mos6502Registers m_Registers;

    private ushort m_CurrentOpCodeAddress;
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
        m_CurrentOpCode = OpCodeBOOT;
        m_CurrentOpCodeCycle = 1;
        m_Bus.Reboot();
    }

    public void StepHalted()
    {
        // CPU has to hold something on the bus, even when halted, so just read the current PC repeatedly,
        // should be ok because CPU is usually only halted after writing WSYNC to the TIA, and the next
        // cycle will be reading the next instruction.
        // Possible accuracy improvement: figure out exactly what the CPU would hold on the bus when halted,
        // and use that instead of just reading the PC.
        _ = m_Bus.Read(m_Registers.PC);
    }

    public void Step()
    {
        if (m_CurrentOpCodeCycle == 0)
        {
            // The first cycle of an opcode is always reading the opcode byte from the current PC:
            m_CurrentOpCodeAddress = m_Registers.PC;
            m_CurrentOpCode = m_Bus.Read(m_Registers.PC);
            ++m_Registers.PC;

            m_CurrentOpCodeCycle = 1;
        }
        else
        {
            Dispatch(m_CurrentOpCode);
        }
    }

    private void UNKNOWN()
    {
        // We're explicitly not handling the unstable opcodes, just NOPing instead.
        // Possible accuracy improvement: at least run for the same number of cycles
        // as the unstable opcode would have, to improve timing accuracy?
        _ = m_Bus.Read(m_Registers.PC);
        Trace($"UNKNOWN ${m_CurrentOpCode:X2}");
        m_CurrentOpCodeCycle = 0;
    }

    [CpuInstruction(OpCodeBOOT, InstructionType.Custom)]
    private void BOOT()
    {
        // TODO: This isn't exactly correct yet, need to verify the exact sequence of reads and writes that the CPU does on boot
        // and make sure this matches that.
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                // how many of these do we actually need to do?
                m_Registers.A = 0;
                m_Registers.X = 0;
                m_Registers.Y = 0;
                m_Registers.S = 0;
                m_Registers.PNegative = false;
                m_Registers.POverflow = false;
                m_Registers.PZero = false;
                m_Registers.PCarry = false;
                m_Registers.PDecimal = false;
                m_Registers.PInterruptDisable = true;
                _ = m_Bus.Read(m_Registers.PC);
                Trace("BOOT");
                m_CurrentOpCodeCycle = 2;
                break;
            case 2:
                _ = m_Bus.Read(m_Registers.PC);
                m_CurrentOpCodeCycle = 3;
                break;
            case 3:
            case 4:
            case 5:
                _ = m_Bus.Read(GetStackAddress(m_Registers.S));
                --m_Registers.S;
                ++m_CurrentOpCodeCycle;
                break;
            case 6:
                m_SavedValue0 = m_Bus.Read(0xFFFC);
                m_CurrentOpCodeCycle = 7;
                break;
            case 7:
                m_SavedValue1 = m_Bus.Read(0xFFFD);
                m_Registers.PC = GetAbsolute(m_SavedValue0, m_SavedValue1);
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }

    [CpuInstruction(0x69, InstructionType.ReadImmediate)]
    [CpuInstruction(0x65, InstructionType.ReadZeropage)]
    [CpuInstruction(0x75, InstructionType.ReadZeropageXIndexed)]
    [CpuInstruction(0x6d, InstructionType.ReadAbsolute)]
    [CpuInstruction(0x7d, InstructionType.ReadAbsoluteXIndexed)]
    [CpuInstruction(0x79, InstructionType.ReadAbsoluteYIndexed)]
    [CpuInstruction(0x61, InstructionType.ReadIndirectXIndexed)]
    [CpuInstruction(0x71, InstructionType.ReadIndirectYIndexed)]
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

    [CpuInstruction(0x4b, InstructionType.ReadImmediate)]
    private void ALR(byte arg)
    {
        AND(arg);
        LSR();
    }

    [CpuInstruction(0x0b, InstructionType.ReadImmediate)]
    [CpuInstruction(0x2b, InstructionType.ReadImmediate)]
    private void ANC(byte arg)
    {
        AND(arg);
        m_Registers.PCarry = m_Registers.PNegative;
    }

    [CpuInstruction(0x29, InstructionType.ReadImmediate)]
    [CpuInstruction(0x25, InstructionType.ReadZeropage)]
    [CpuInstruction(0x35, InstructionType.ReadZeropageXIndexed)]
    [CpuInstruction(0x2d, InstructionType.ReadAbsolute)]
    [CpuInstruction(0x3d, InstructionType.ReadAbsoluteXIndexed)]
    [CpuInstruction(0x39, InstructionType.ReadAbsoluteYIndexed)]
    [CpuInstruction(0x21, InstructionType.ReadIndirectXIndexed)]
    [CpuInstruction(0x31, InstructionType.ReadIndirectYIndexed)]
    private void AND(byte arg)
    {
        byte result = (byte)(m_Registers.A & arg);

        m_Registers.A = result;
        m_Registers.PZero = CheckZero(result);
        m_Registers.PNegative = CheckNegative(result);
    }

    [CpuInstruction(0x6b, InstructionType.ReadImmediate)]
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

    [CpuInstruction(0x0a, InstructionType.Accumulator)]
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

    [CpuInstruction(0x06, InstructionType.ReadWriteZeropage)]
    [CpuInstruction(0x16, InstructionType.ReadWriteZeropageXIndexed)]
    [CpuInstruction(0x0e, InstructionType.ReadWriteAbsolute)]
    [CpuInstruction(0x1e, InstructionType.ReadWriteAbsoluteXIndexed)]
    private byte ASL(byte arg)
    {
        byte result = (byte)(arg << 1);
        bool carryOut = (arg & 0x80) != 0;
        m_Registers.PCarry = carryOut;
        m_Registers.PZero = CheckZero(result);
        m_Registers.PNegative = CheckNegative(result);
        return result;
    }

    [CpuInstruction(0x90, InstructionType.BranchConditionalRelative)]
    private bool BCC()
    {
        return m_Registers.PCarry;
    }

    [CpuInstruction(0xb0, InstructionType.BranchConditionalRelative)]
    private bool BCS()
    {
        return !m_Registers.PCarry;
    }

    [CpuInstruction(0xf0, InstructionType.BranchConditionalRelative)]
    private bool BEQ()
    {
        return !m_Registers.PZero;
    }

    [CpuInstruction(0x24, InstructionType.ReadZeropage)]
    [CpuInstruction(0x2c, InstructionType.ReadAbsolute)]
    private void BIT(byte arg)
    {
        byte result = (byte)(m_Registers.A & arg);
        m_Registers.PZero = CheckZero(result);
        m_Registers.PNegative = (arg & 0x80) != 0;
        m_Registers.POverflow = (arg & 0x40) != 0;
    }

    [CpuInstruction(0x30, InstructionType.BranchConditionalRelative)]
    private bool BMI()
    {
        return !m_Registers.PNegative;
    }

    [CpuInstruction(0xd0, InstructionType.BranchConditionalRelative)]
    private bool BNE()
    {
        return m_Registers.PZero;
    }

    [CpuInstruction(0x10, InstructionType.BranchConditionalRelative)]
    private bool BPL()
    {
        return m_Registers.PNegative;
    }

    [CpuInstruction(0x00, InstructionType.Custom)]
    private void BRK_impl()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                _ = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                Trace("BRK");
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

    [CpuInstruction(0x50, InstructionType.BranchConditionalRelative)]
    private bool BVC()
    {
        return m_Registers.POverflow;
    }

    [CpuInstruction(0x70, InstructionType.BranchConditionalRelative)]
    private bool BVS()
    {
        return !m_Registers.POverflow;
    }

    [CpuInstruction(0x18, InstructionType.Implied)]
    private void CLC()
    {
        m_Registers.PCarry = false;
    }

    [CpuInstruction(0xd8, InstructionType.Implied)]
    private void CLD()
    {
        m_Registers.PDecimal = false;
    }

    [CpuInstruction(0x58, InstructionType.Implied)]
    private void CLI()
    {
        m_Registers.PInterruptDisable = false;
    }

    [CpuInstruction(0xb8, InstructionType.Implied)]
    private void CLV()
    {
        m_Registers.POverflow = false;
    }

    [CpuInstruction(0xc9, InstructionType.ReadImmediate)]
    [CpuInstruction(0xc5, InstructionType.ReadZeropage)]
    [CpuInstruction(0xd5, InstructionType.ReadZeropageXIndexed)]
    [CpuInstruction(0xcd, InstructionType.ReadAbsolute)]
    [CpuInstruction(0xdd, InstructionType.ReadAbsoluteXIndexed)]
    [CpuInstruction(0xd9, InstructionType.ReadAbsoluteYIndexed)]
    [CpuInstruction(0xc1, InstructionType.ReadIndirectXIndexed)]
    [CpuInstruction(0xd1, InstructionType.ReadIndirectYIndexed)]
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

    [CpuInstruction(0xe0, InstructionType.ReadImmediate)]
    [CpuInstruction(0xe4, InstructionType.ReadZeropage)]
    [CpuInstruction(0xec, InstructionType.ReadAbsolute)]
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

    [CpuInstruction(0xc0, InstructionType.ReadImmediate)]
    [CpuInstruction(0xc4, InstructionType.ReadZeropage)]
    [CpuInstruction(0xcc, InstructionType.ReadAbsolute)]
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

    [CpuInstruction(0xc7, InstructionType.ReadWriteZeropage)]
    [CpuInstruction(0xd7, InstructionType.ReadWriteZeropageXIndexed)]
    [CpuInstruction(0xcf, InstructionType.ReadWriteAbsolute)]
    [CpuInstruction(0xdf, InstructionType.ReadWriteAbsoluteXIndexed)]
    [CpuInstruction(0xdb, InstructionType.ReadWriteAbsoluteYIndexed)]
    [CpuInstruction(0xc3, InstructionType.ReadWriteIndirectXIndexed)]
    [CpuInstruction(0xd3, InstructionType.ReadWriteIndirectYIndexed)]
    private byte DCP(byte arg)
    {
        byte decResult = DEC(arg);
        CMP(decResult);
        return decResult;
    }

    [CpuInstruction(0xc6, InstructionType.ReadWriteZeropage)]
    [CpuInstruction(0xd6, InstructionType.ReadWriteZeropageXIndexed)]
    [CpuInstruction(0xce, InstructionType.ReadWriteAbsolute)]
    [CpuInstruction(0xde, InstructionType.ReadWriteAbsoluteXIndexed)]
    private byte DEC(byte arg)
    {
        byte result = (byte)(arg - 1);
        m_Registers.PZero = CheckZero(result);
        m_Registers.PNegative = CheckNegative(result);
        return result;
    }

    [CpuInstruction(0xca, InstructionType.Implied)]
    private void DEX()
    {
        byte result = (byte)(m_Registers.X - 1);
        m_Registers.X = result;
        m_Registers.PZero = CheckZero(result);
        m_Registers.PNegative = CheckNegative(result);
    }

    [CpuInstruction(0x88, InstructionType.Implied)]
    private void DEY()
    {
        byte result = (byte)(m_Registers.Y - 1);
        m_Registers.Y = result;
        m_Registers.PZero = CheckZero(result);
        m_Registers.PNegative = CheckNegative(result);
    }

    [CpuInstruction(0x49, InstructionType.ReadImmediate)]
    [CpuInstruction(0x45, InstructionType.ReadZeropage)]
    [CpuInstruction(0x55, InstructionType.ReadZeropageXIndexed)]
    [CpuInstruction(0x4d, InstructionType.ReadAbsolute)]
    [CpuInstruction(0x5d, InstructionType.ReadAbsoluteXIndexed)]
    [CpuInstruction(0x59, InstructionType.ReadAbsoluteYIndexed)]
    [CpuInstruction(0x41, InstructionType.ReadIndirectXIndexed)]
    [CpuInstruction(0x51, InstructionType.ReadIndirectYIndexed)]
    private void EOR(byte arg)
    {
        byte result = (byte)(m_Registers.A ^ arg);
        m_Registers.A = result;
        m_Registers.PZero = CheckZero(result);
        m_Registers.PNegative = CheckNegative(result);
    }

    [CpuInstruction(0xe6, InstructionType.ReadWriteZeropage)]
    [CpuInstruction(0xf6, InstructionType.ReadWriteZeropageXIndexed)]
    [CpuInstruction(0xee, InstructionType.ReadWriteAbsolute)]
    [CpuInstruction(0xfe, InstructionType.ReadWriteAbsoluteXIndexed)]
    private byte INC(byte arg)
    {
        byte result = (byte)(arg + 1);
        m_Registers.PZero = CheckZero(result);
        m_Registers.PNegative = CheckNegative(result);
        return result;
    }

    [CpuInstruction(0xe8, InstructionType.Implied)]
    private void INX()
    {
        m_Registers.X = (byte)(m_Registers.X + 1);
        m_Registers.PZero = CheckZero(m_Registers.X);
        m_Registers.PNegative = CheckNegative(m_Registers.X);
    }

    [CpuInstruction(0xc8, InstructionType.Implied)]
    private void INY()
    {
        m_Registers.Y = (byte)(m_Registers.Y + 1);
        m_Registers.PZero = CheckZero(m_Registers.Y);
        m_Registers.PNegative = CheckNegative(m_Registers.Y);
    }

    [CpuInstruction(0xe7, InstructionType.ReadWriteZeropage)]
    [CpuInstruction(0xf7, InstructionType.ReadWriteZeropageXIndexed)]
    [CpuInstruction(0xef, InstructionType.ReadWriteAbsolute)]
    [CpuInstruction(0xff, InstructionType.ReadWriteAbsoluteXIndexed)]
    [CpuInstruction(0xfb, InstructionType.ReadWriteAbsoluteYIndexed)]
    [CpuInstruction(0xe3, InstructionType.ReadWriteIndirectXIndexed)]
    [CpuInstruction(0xf3, InstructionType.ReadWriteIndirectYIndexed)]
    private byte ISC(byte arg)
    {
        byte incResult = INC(arg);
        SBC(incResult);
        return incResult;
    }

    [CpuInstruction(0x02, InstructionType.Custom)]
    [CpuInstruction(0x12, InstructionType.Custom)]
    [CpuInstruction(0x22, InstructionType.Custom)]
    [CpuInstruction(0x32, InstructionType.Custom)]
    [CpuInstruction(0x42, InstructionType.Custom)]
    [CpuInstruction(0x52, InstructionType.Custom)]
    [CpuInstruction(0x62, InstructionType.Custom)]
    [CpuInstruction(0x72, InstructionType.Custom)]
    [CpuInstruction(0x92, InstructionType.Custom)]
    [CpuInstruction(0xb2, InstructionType.Custom)]
    [CpuInstruction(0xd2, InstructionType.Custom)]
    [CpuInstruction(0xf2, InstructionType.Custom)]
    private void JAM_impl()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                _ = m_Bus.Read(m_Registers.PC);
                Trace("JAM");
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

    [CpuInstruction(0x4c, InstructionType.Custom)]
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
                Trace($"JMP ${m_SavedValue0:X2}{m_SavedValue1:X2}");
                m_Registers.PC = GetAbsolute(m_SavedValue0, m_SavedValue1);
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }

    [CpuInstruction(0x6c, InstructionType.Custom)]
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
                Trace($"JMP (${m_SavedValue0:X2}{m_SavedValue1:X2})");
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

    [CpuInstruction(0x20, InstructionType.Custom)]
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
                Trace($"JSR ${m_SavedValue0:X2}{m_SavedValue1:X2}");
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }

    [CpuInstruction(0xbb, InstructionType.ReadAbsoluteYIndexed)]
    private void LAS(byte arg)
    {
        byte result = (byte)(m_Registers.S & arg);
        m_Registers.A = result;
        m_Registers.X = result;
        m_Registers.S = result;
        m_Registers.PZero = CheckZero(result);
        m_Registers.PNegative = CheckNegative(result);
    }

    [CpuInstruction(0xa7, InstructionType.ReadZeropage)]
    [CpuInstruction(0xb7, InstructionType.ReadZeropageYIndexed)]
    [CpuInstruction(0xaf, InstructionType.ReadAbsolute)]
    [CpuInstruction(0xbf, InstructionType.ReadAbsoluteYIndexed)]
    [CpuInstruction(0xa3, InstructionType.ReadIndirectXIndexed)]
    [CpuInstruction(0xb3, InstructionType.ReadIndirectYIndexed)]
    private void LAX(byte arg)
    {
        m_Registers.A = arg;
        m_Registers.X = arg;
        m_Registers.PZero = CheckZero(arg);
        m_Registers.PNegative = CheckNegative(arg);
    }

    [CpuInstruction(0xa9, InstructionType.ReadImmediate)]
    [CpuInstruction(0xa5, InstructionType.ReadZeropage)]
    [CpuInstruction(0xb5, InstructionType.ReadZeropageXIndexed)]
    [CpuInstruction(0xad, InstructionType.ReadAbsolute)]
    [CpuInstruction(0xbd, InstructionType.ReadAbsoluteXIndexed)]
    [CpuInstruction(0xb9, InstructionType.ReadAbsoluteYIndexed)]
    [CpuInstruction(0xa1, InstructionType.ReadIndirectXIndexed)]
    [CpuInstruction(0xb1, InstructionType.ReadIndirectYIndexed)]
    private void LDA(byte arg)
    {
        m_Registers.A = arg;
        m_Registers.PZero = CheckZero(arg);
        m_Registers.PNegative = CheckNegative(arg);
    }

    [CpuInstruction(0xa2, InstructionType.ReadImmediate)]
    [CpuInstruction(0xa6, InstructionType.ReadZeropage)]
    [CpuInstruction(0xb6, InstructionType.ReadZeropageYIndexed)]
    [CpuInstruction(0xae, InstructionType.ReadAbsolute)]
    [CpuInstruction(0xbe, InstructionType.ReadAbsoluteYIndexed)]
    private void LDX(byte arg)
    {
        m_Registers.X = arg;
        m_Registers.PZero = CheckZero(arg);
        m_Registers.PNegative = CheckNegative(arg);
    }

    [CpuInstruction(0xa0, InstructionType.ReadImmediate)]
    [CpuInstruction(0xa4, InstructionType.ReadZeropage)]
    [CpuInstruction(0xb4, InstructionType.ReadZeropageXIndexed)]
    [CpuInstruction(0xac, InstructionType.ReadAbsolute)]
    [CpuInstruction(0xbc, InstructionType.ReadAbsoluteXIndexed)]
    private void LDY(byte arg)
    {
        m_Registers.Y = arg;
        m_Registers.PZero = CheckZero(arg);
        m_Registers.PNegative = CheckNegative(arg);
    }

    [CpuInstruction(0x4a, InstructionType.Accumulator)]
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

    [CpuInstruction(0x46, InstructionType.ReadWriteZeropage)]
    [CpuInstruction(0x56, InstructionType.ReadWriteZeropageXIndexed)]
    [CpuInstruction(0x4e, InstructionType.ReadWriteAbsolute)]
    [CpuInstruction(0x5e, InstructionType.ReadWriteAbsoluteXIndexed)]
    private byte LSR(byte arg)
    {
        byte result = (byte)(arg >> 1);
        bool carryOut = (arg & 0x01) != 0;
        m_Registers.PCarry = carryOut;
        m_Registers.PZero = CheckZero(result);
        m_Registers.PNegative = false;
        return result;
    }

    [CpuInstruction(0xea, InstructionType.Implied)]
    [CpuInstruction(0x1a, InstructionType.Implied)]
    [CpuInstruction(0x3a, InstructionType.Implied)]
    [CpuInstruction(0x5a, InstructionType.Implied)]
    [CpuInstruction(0x7a, InstructionType.Implied)]
    [CpuInstruction(0xda, InstructionType.Implied)]
    [CpuInstruction(0xfa, InstructionType.Implied)]
    private static void NOP()
    {
        // nothing
    }

    [CpuInstruction(0x80, InstructionType.ReadImmediate)]
    [CpuInstruction(0x82, InstructionType.ReadImmediate)]
    [CpuInstruction(0x89, InstructionType.ReadImmediate)]
    [CpuInstruction(0xc2, InstructionType.ReadImmediate)]
    [CpuInstruction(0xe2, InstructionType.ReadImmediate)]
    [CpuInstruction(0x04, InstructionType.ReadZeropage)]
    [CpuInstruction(0x44, InstructionType.ReadZeropage)]
    [CpuInstruction(0x64, InstructionType.ReadZeropage)]
    [CpuInstruction(0x14, InstructionType.ReadZeropageXIndexed)]
    [CpuInstruction(0x34, InstructionType.ReadZeropageXIndexed)]
    [CpuInstruction(0x54, InstructionType.ReadZeropageXIndexed)]
    [CpuInstruction(0x74, InstructionType.ReadZeropageXIndexed)]
    [CpuInstruction(0xd4, InstructionType.ReadZeropageXIndexed)]
    [CpuInstruction(0xf4, InstructionType.ReadZeropageXIndexed)]
    [CpuInstruction(0x0c, InstructionType.ReadAbsolute)]
    [CpuInstruction(0x1c, InstructionType.ReadAbsoluteXIndexed)]
    [CpuInstruction(0x3c, InstructionType.ReadAbsoluteXIndexed)]
    [CpuInstruction(0x5c, InstructionType.ReadAbsoluteXIndexed)]
    [CpuInstruction(0x7c, InstructionType.ReadAbsoluteXIndexed)]
    [CpuInstruction(0xdc, InstructionType.ReadAbsoluteXIndexed)]
    [CpuInstruction(0xfc, InstructionType.ReadAbsoluteXIndexed)]
    private static void NOP(byte _)
    {
        // nothing
    }

    [CpuInstruction(0x09, InstructionType.ReadImmediate)]
    [CpuInstruction(0x05, InstructionType.ReadZeropage)]
    [CpuInstruction(0x15, InstructionType.ReadZeropageXIndexed)]
    [CpuInstruction(0x0d, InstructionType.ReadAbsolute)]
    [CpuInstruction(0x1d, InstructionType.ReadAbsoluteXIndexed)]
    [CpuInstruction(0x19, InstructionType.ReadAbsoluteYIndexed)]
    [CpuInstruction(0x01, InstructionType.ReadIndirectXIndexed)]
    [CpuInstruction(0x11, InstructionType.ReadIndirectYIndexed)]
    private void ORA(byte arg)
    {
        byte result = (byte)(m_Registers.A | arg);
        m_Registers.A = result;
        m_Registers.PZero = CheckZero(result);
        m_Registers.PNegative = CheckNegative(result);
    }

    [CpuInstruction(0x48, InstructionType.Custom)]
    private void PHA_impl()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                _ = m_Bus.Read(m_Registers.PC);
                Trace("PHA");
                m_CurrentOpCodeCycle = 2;
                break;
            default:
                m_Bus.Write(GetStackAddress(m_Registers.S), m_Registers.A);
                --m_Registers.S;
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }

    [CpuInstruction(0x08, InstructionType.Custom)]
    private void PHP_impl()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                _ = m_Bus.Read(m_Registers.PC);
                Trace("PHP");
                m_CurrentOpCodeCycle = 2;
                break;
            default:
                m_Bus.Write(GetStackAddress(m_Registers.S), m_Registers.ReadP(true));
                --m_Registers.S;
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }

    [CpuInstruction(0x68, InstructionType.Custom)]
    private void PLA_impl()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                _ = m_Bus.Read(m_Registers.PC);
                Trace("PLA");
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

    [CpuInstruction(0x28, InstructionType.Custom)]
    private void PLP_impl()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                _ = m_Bus.Read(m_Registers.PC);
                Trace("PLP");
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

    [CpuInstruction(0x27, InstructionType.ReadWriteZeropage)]
    [CpuInstruction(0x37, InstructionType.ReadWriteZeropageXIndexed)]
    [CpuInstruction(0x2f, InstructionType.ReadWriteAbsolute)]
    [CpuInstruction(0x3f, InstructionType.ReadWriteAbsoluteXIndexed)]
    [CpuInstruction(0x3b, InstructionType.ReadWriteAbsoluteYIndexed)]
    [CpuInstruction(0x23, InstructionType.ReadWriteIndirectXIndexed)]
    [CpuInstruction(0x33, InstructionType.ReadWriteIndirectYIndexed)]
    private byte RLA(byte arg)
    {
        byte rol = ROL(arg);
        AND(rol);
        return rol;
    }

    [CpuInstruction(0x2a, InstructionType.Accumulator)]
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

    [CpuInstruction(0x26, InstructionType.ReadWriteZeropage)]
    [CpuInstruction(0x36, InstructionType.ReadWriteZeropageXIndexed)]
    [CpuInstruction(0x2e, InstructionType.ReadWriteAbsolute)]
    [CpuInstruction(0x3e, InstructionType.ReadWriteAbsoluteXIndexed)]
    private byte ROL(byte arg)
    {
        byte result = (byte)((arg << 1) | (m_Registers.PCarry ? 1 : 0));
        bool carryOut = (arg & 0x80) != 0;
        m_Registers.PCarry = carryOut;
        m_Registers.PZero = CheckZero(result);
        m_Registers.PNegative = CheckNegative(result);
        return result;
    }

    [CpuInstruction(0x6a, InstructionType.Accumulator)]
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

    [CpuInstruction(0x66, InstructionType.ReadWriteZeropage)]
    [CpuInstruction(0x76, InstructionType.ReadWriteZeropageXIndexed)]
    [CpuInstruction(0x6e, InstructionType.ReadWriteAbsolute)]
    [CpuInstruction(0x7e, InstructionType.ReadWriteAbsoluteXIndexed)]
    private byte ROR(byte arg)
    {
        byte result = (byte)((arg >> 1) | (m_Registers.PCarry ? 0x80 : 0));
        bool carryOut = (arg & 0x01) != 0;
        m_Registers.PCarry = carryOut;
        m_Registers.PZero = CheckZero(result);
        m_Registers.PNegative = CheckNegative(result);
        return result;
    }

    [CpuInstruction(0x67, InstructionType.ReadWriteZeropage)]
    [CpuInstruction(0x77, InstructionType.ReadWriteZeropageXIndexed)]
    [CpuInstruction(0x6f, InstructionType.ReadWriteAbsolute)]
    [CpuInstruction(0x7f, InstructionType.ReadWriteAbsoluteXIndexed)]
    [CpuInstruction(0x7b, InstructionType.ReadWriteAbsoluteYIndexed)]
    [CpuInstruction(0x63, InstructionType.ReadWriteIndirectXIndexed)]
    [CpuInstruction(0x73, InstructionType.ReadWriteIndirectYIndexed)]
    private byte RRA(byte arg)
    {
        byte ror = ROR(arg);
        ADC(ror);
        return ror;
    }

    [CpuInstruction(0x40, InstructionType.Custom)]
    private void RTI_impl()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                _ = m_Bus.Read(m_Registers.PC);
                Trace("RTI");
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

    [CpuInstruction(0x60, InstructionType.Custom)]
    private void RTS_impl()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                _ = m_Bus.Read(m_Registers.PC);
                Trace("RTS");
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

    [CpuInstruction(0x87, InstructionType.WriteZeropage)]
    [CpuInstruction(0x97, InstructionType.WriteZeropageYIndexed)]
    [CpuInstruction(0x8f, InstructionType.WriteAbsolute)]
    [CpuInstruction(0x83, InstructionType.WriteIndirectXIndexed)]
    private byte SAX()
    {
        return (byte)(m_Registers.A & m_Registers.X);
    }

    [CpuInstruction(0xcb, InstructionType.ReadImmediate)]
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

    [CpuInstruction(0xe9, InstructionType.ReadImmediate)]
    [CpuInstruction(0xeb, InstructionType.ReadImmediate)]
    [CpuInstruction(0xe5, InstructionType.ReadZeropage)]
    [CpuInstruction(0xf5, InstructionType.ReadZeropageXIndexed)]
    [CpuInstruction(0xed, InstructionType.ReadAbsolute)]
    [CpuInstruction(0xfd, InstructionType.ReadAbsoluteXIndexed)]
    [CpuInstruction(0xf9, InstructionType.ReadAbsoluteYIndexed)]
    [CpuInstruction(0xe1, InstructionType.ReadIndirectXIndexed)]
    [CpuInstruction(0xf1, InstructionType.ReadIndirectYIndexed)]
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

    [CpuInstruction(0x38, InstructionType.Implied)]
    private void SEC()
    {
        m_Registers.PCarry = true;
    }

    [CpuInstruction(0xf8, InstructionType.Implied)]
    private void SED()
    {
        m_Registers.PDecimal = true;
    }

    [CpuInstruction(0x78, InstructionType.Implied)]
    private void SEI()
    {
        m_Registers.PInterruptDisable = true;
    }

    [CpuInstruction(0x07, InstructionType.ReadWriteZeropage)]
    [CpuInstruction(0x17, InstructionType.ReadWriteZeropageXIndexed)]
    [CpuInstruction(0x0f, InstructionType.ReadWriteAbsolute)]
    [CpuInstruction(0x1f, InstructionType.ReadWriteAbsoluteXIndexed)]
    [CpuInstruction(0x1b, InstructionType.ReadWriteAbsoluteYIndexed)]
    [CpuInstruction(0x03, InstructionType.ReadWriteIndirectXIndexed)]
    [CpuInstruction(0x13, InstructionType.ReadWriteIndirectYIndexed)]
    private byte SLO(byte arg)
    {
        arg = ASL(arg);
        ORA(arg);
        return arg;
    }

    [CpuInstruction(0x47, InstructionType.ReadWriteZeropage)]
    [CpuInstruction(0x57, InstructionType.ReadWriteZeropageXIndexed)]
    [CpuInstruction(0x4f, InstructionType.ReadWriteAbsolute)]
    [CpuInstruction(0x5f, InstructionType.ReadWriteAbsoluteXIndexed)]
    [CpuInstruction(0x5b, InstructionType.ReadWriteAbsoluteYIndexed)]
    [CpuInstruction(0x43, InstructionType.ReadWriteIndirectXIndexed)]
    [CpuInstruction(0x53, InstructionType.ReadWriteIndirectYIndexed)]
    private byte SRE(byte arg)
    {
        arg = LSR(arg);
        EOR(arg);
        return arg;
    }

    [CpuInstruction(0x85, InstructionType.WriteZeropage)]
    [CpuInstruction(0x95, InstructionType.WriteZeropageXIndexed)]
    [CpuInstruction(0x8d, InstructionType.WriteAbsolute)]
    [CpuInstruction(0x9d, InstructionType.WriteAbsoluteXIndexed)]
    [CpuInstruction(0x99, InstructionType.WriteAbsoluteYIndexed)]
    [CpuInstruction(0x81, InstructionType.WriteIndirectXIndexed)]
    [CpuInstruction(0x91, InstructionType.WriteIndirectYIndexed)]
    private byte STA()
    {
        return m_Registers.A;
    }

    [CpuInstruction(0x86, InstructionType.WriteZeropage)]
    [CpuInstruction(0x96, InstructionType.WriteZeropageYIndexed)]
    [CpuInstruction(0x8e, InstructionType.WriteAbsolute)]
    private byte STX()
    {
        return m_Registers.X;
    }

    [CpuInstruction(0x84, InstructionType.WriteZeropage)]
    [CpuInstruction(0x94, InstructionType.WriteZeropageXIndexed)]
    [CpuInstruction(0x8c, InstructionType.WriteAbsolute)]
    private byte STY()
    {
        return m_Registers.Y;
    }

    [CpuInstruction(0xaa, InstructionType.Implied)]
    private void TAX()
    {
        byte result = m_Registers.A;
        m_Registers.X = result;
        m_Registers.PZero = CheckZero(result);
        m_Registers.PNegative = CheckNegative(result);
    }

    [CpuInstruction(0xa8, InstructionType.Implied)]
    private void TAY()
    {
        byte result = m_Registers.A;
        m_Registers.Y = result;
        m_Registers.PZero = CheckZero(result);
        m_Registers.PNegative = CheckNegative(result);
    }

    [CpuInstruction(0xba, InstructionType.Implied)]
    private void TSX()
    {
        byte result = m_Registers.S;
        m_Registers.X = result;
        m_Registers.PZero = CheckZero(result);
        m_Registers.PNegative = CheckNegative(result);
    }

    [CpuInstruction(0x8a, InstructionType.Implied)]
    private void TXA()
    {
        byte result = m_Registers.X;
        m_Registers.A = result;
        m_Registers.PZero = CheckZero(result);
        m_Registers.PNegative = CheckNegative(result);
    }

    [CpuInstruction(0x9a, InstructionType.Implied)]
    private void TXS()
    {
        m_Registers.S = m_Registers.X;
    }

    [CpuInstruction(0x98, InstructionType.Implied)]
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

    [Conditional("DEBUG")]
    private void Trace(string message)
    {
#if DEBUG
        //Console.WriteLine($"{m_CurrentOpCodeAddress:X4}: {message}");
#endif
    }
}

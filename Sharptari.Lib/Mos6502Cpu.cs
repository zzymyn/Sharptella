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
    private uint m_CurrentOpCodeA;
    private uint m_CurrentOpCodeB;
    private uint m_CurrentOpCodeC;
    private int m_CurrentOpCodeCycle;

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
            m_CurrentOpCodeA = Mos6502OpCode.GetOpCodeA(m_CurrentOpCode);
            m_CurrentOpCodeB = Mos6502OpCode.GetOpCodeB(m_CurrentOpCode);
            m_CurrentOpCodeC = Mos6502OpCode.GetOpCodeC(m_CurrentOpCode);
            ++m_Registers.PC;
            m_CurrentOpCodeCycle = 1;
        }
        else
        {
            // TODO: implement the actual op codes:
            var nextValue = m_Bus.Read(m_Registers.PC);
            m_CurrentOpCodeCycle = 0;
        }

        m_Bus.Step();
    }
}

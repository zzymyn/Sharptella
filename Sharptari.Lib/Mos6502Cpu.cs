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

    private Mos6502OpCode m_CurrentOpCode;
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
            m_CurrentOpCode.Data = m_Bus.Read(m_Registers.PC);
            ++m_Registers.PC;
            m_CurrentOpCodeCycle = 1;
        }
        else
        {
            // TODO: implement the actual op codes:
            m_CurrentOpCodeCycle = 0;
        }
    }
}

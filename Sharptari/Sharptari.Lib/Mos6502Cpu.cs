using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharptari.Lib;

public sealed class Mos6502Cpu
{
    private readonly Mos6502Bus Bus;
    private Mos6502Registers Registers;

    private Mos6502OpCode m_CurrentOpCode;
    private int m_CurrentOpCodeCycle;

    public bool IsAtOpCodeStart => m_CurrentOpCodeCycle == 0;

    public Mos6502Cpu(Mos6502Bus bus)
    {
        Bus = bus;
    }

    public void Step()
    {
        if (m_CurrentOpCodeCycle == 0)
        {
            m_CurrentOpCode.Data = Bus.Read(Registers.PC);
            ++Registers.PC;
            m_CurrentOpCodeCycle = 1;
        }
        else
        {
            // TODO: implement the actual op codes:
            m_CurrentOpCodeCycle = 0;
        }
    }
}

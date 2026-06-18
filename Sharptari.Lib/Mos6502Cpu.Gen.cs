using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharptari.Lib;

public sealed partial class Mos6502Cpu
{
    private void AND_immediate()
    {
        m_SavedValue0 = m_Bus.Read(m_Registers.PC);
        ++m_Registers.PC;
        m_CurrentOpCodeCycle = 0;
        AND();
    }
    private void AND_zeropage()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                m_CurrentOpCodeCycle = 2;
                break;
            default:
                m_SavedValue0 = m_Bus.Read(m_SavedValue0);
                m_CurrentOpCodeCycle = 0;
                AND();
                break;
        }
    }
    private void AND_zeropage_xindexed()
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
                m_SavedValue0 = m_Bus.Read(GetZeropageIndexedNoCarry(m_SavedValue0, m_Registers.X));
                m_CurrentOpCodeCycle = 0;
                AND();
                break;
        }
    }
    private void AND_absolute()
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
                m_SavedValue0 = m_Bus.Read(GetAbsolute(m_SavedValue0, m_SavedValue1));
                m_CurrentOpCodeCycle = 0;
                AND();
                break;
        }
    }
    private void AND_absolute_xindexed()
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
                if (m_SavedValue0 + m_Registers.X > 0xFF)
                {
                    _ = m_Bus.Read(GetAbsoluteIndexedNoCarry(m_SavedValue0, m_SavedValue1, m_Registers.X));
                    m_CurrentOpCodeCycle = 4;
                }
                else
                {
                    m_SavedValue0 = m_Bus.Read(GetAbsoluteIndexedNoCarry(m_SavedValue0, m_SavedValue1, m_Registers.X));
                    m_CurrentOpCodeCycle = 0;
                    AND();
                }
                break;
            default:
                m_SavedValue0 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.X));
                m_CurrentOpCodeCycle = 0;
                AND();
                break;
        }
    }
    private void AND_absolute_yindexed()
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
                if (m_SavedValue0 + m_Registers.Y > 0xFF)
                {
                    _ = m_Bus.Read(GetAbsoluteIndexedNoCarry(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                    m_CurrentOpCodeCycle = 4;
                }
                else
                {
                    m_SavedValue0 = m_Bus.Read(GetAbsoluteIndexedNoCarry(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                    m_CurrentOpCodeCycle = 0;
                    AND();
                }
                break;
            default:
                m_SavedValue0 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                m_CurrentOpCodeCycle = 0;
                AND();
                break;
        }
    }
    private void AND_indirect_xindexed()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue1 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                m_CurrentOpCodeCycle = 2;
                break;
            case 2:
                _ = m_Bus.Read(m_SavedValue1);
                m_CurrentOpCodeCycle = 3;
                break;
            case 3:
                m_SavedValue0 = m_Bus.Read(GetZeropageIndexedNoCarry(m_SavedValue1, m_Registers.X));
                m_CurrentOpCodeCycle = 4;
                break;
            case 4:
                m_SavedValue1 = m_Bus.Read(GetZeropageIndexedAdd1NoCarry(m_SavedValue1, m_Registers.X));
                m_CurrentOpCodeCycle = 5;
                break;
            case 5:
                m_SavedValue0 = m_Bus.Read(GetAbsolute(m_SavedValue0, m_SavedValue1));
                m_CurrentOpCodeCycle = 0;
                AND();
                break;
        }
    }
    private void AND_indirect_yindexed()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue1 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                m_CurrentOpCodeCycle = 2;
                break;
            case 2:
                m_SavedValue0 = m_Bus.Read(m_SavedValue1);
                m_CurrentOpCodeCycle = 3;
                break;
            case 3:
                m_SavedValue1 = m_Bus.Read(GetAdd1NoCarry(m_SavedValue1));
                m_CurrentOpCodeCycle = 4;
                break;
            case 4:
                if (m_SavedValue0 + m_Registers.Y > 0xFF)
                {
                    _ = m_Bus.Read(GetAbsoluteIndexedNoCarry(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                    m_CurrentOpCodeCycle = 5;
                }
                else
                {
                    m_SavedValue0 = m_Bus.Read(GetAbsoluteIndexedNoCarry(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                    m_CurrentOpCodeCycle = 0;
                    AND();
                }
                break;
            default:
                m_SavedValue0 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                m_CurrentOpCodeCycle = 0;
                AND();
                break;
        }
    }

    private void LDA_immediate()
    {
        m_SavedValue0 = m_Bus.Read(m_Registers.PC);
        ++m_Registers.PC;
        m_CurrentOpCodeCycle = 0;
        LDA();
    }
    private void LDA_zeropage()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                m_CurrentOpCodeCycle = 2;
                break;
            default:
                m_SavedValue0 = m_Bus.Read(m_SavedValue0);
                m_CurrentOpCodeCycle = 0;
                LDA();
                break;
        }
    }
    private void LDA_zeropage_xindexed()
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
                m_SavedValue0 = m_Bus.Read(GetZeropageIndexedNoCarry(m_SavedValue0, m_Registers.X));
                m_CurrentOpCodeCycle = 0;
                LDA();
                break;
        }
    }
    private void LDA_absolute()
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
                m_SavedValue0 = m_Bus.Read(GetAbsolute(m_SavedValue0, m_SavedValue1));
                m_CurrentOpCodeCycle = 0;
                LDA();
                break;
        }
    }
    private void LDA_absolute_xindexed()
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
                if (m_SavedValue0 + m_Registers.X > 0xFF)
                {
                    _ = m_Bus.Read(GetAbsoluteIndexedNoCarry(m_SavedValue0, m_SavedValue1, m_Registers.X));
                    m_CurrentOpCodeCycle = 4;
                }
                else
                {
                    m_SavedValue0 = m_Bus.Read(GetAbsoluteIndexedNoCarry(m_SavedValue0, m_SavedValue1, m_Registers.X));
                    m_CurrentOpCodeCycle = 0;
                    LDA();
                }
                break;
            default:
                m_SavedValue0 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.X));
                m_CurrentOpCodeCycle = 0;
                LDA();
                break;
        }
    }
    private void LDA_absolute_yindexed()
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
                if (m_SavedValue0 + m_Registers.Y > 0xFF)
                {
                    _ = m_Bus.Read(GetAbsoluteIndexedNoCarry(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                    m_CurrentOpCodeCycle = 4;
                }
                else
                {
                    m_SavedValue0 = m_Bus.Read(GetAbsoluteIndexedNoCarry(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                    m_CurrentOpCodeCycle = 0;
                    LDA();
                }
                break;
            default:
                m_SavedValue0 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                m_CurrentOpCodeCycle = 0;
                LDA();
                break;
        }
    }
    private void LDA_indirect_xindexed()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue1 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                m_CurrentOpCodeCycle = 2;
                break;
            case 2:
                _ = m_Bus.Read(m_SavedValue1);
                m_CurrentOpCodeCycle = 3;
                break;
            case 3:
                m_SavedValue0 = m_Bus.Read(GetZeropageIndexedNoCarry(m_SavedValue1, m_Registers.X));
                m_CurrentOpCodeCycle = 4;
                break;
            case 4:
                m_SavedValue1 = m_Bus.Read(GetZeropageIndexedAdd1NoCarry(m_SavedValue1, m_Registers.X));
                m_CurrentOpCodeCycle = 5;
                break;
            case 5:
                m_SavedValue0 = m_Bus.Read(GetAbsolute(m_SavedValue0, m_SavedValue1));
                m_CurrentOpCodeCycle = 0;
                LDA();
                break;
        }
    }
    private void LDA_indirect_yindexed()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue1 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                m_CurrentOpCodeCycle = 2;
                break;
            case 2:
                m_SavedValue0 = m_Bus.Read(m_SavedValue1);
                m_CurrentOpCodeCycle = 3;
                break;
            case 3:
                m_SavedValue1 = m_Bus.Read(GetAdd1NoCarry(m_SavedValue1));
                m_CurrentOpCodeCycle = 4;
                break;
            case 4:
                if (m_SavedValue0 + m_Registers.Y > 0xFF)
                {
                    _ = m_Bus.Read(GetAbsoluteIndexedNoCarry(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                    m_CurrentOpCodeCycle = 5;
                }
                else
                {
                    m_SavedValue0 = m_Bus.Read(GetAbsoluteIndexedNoCarry(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                    m_CurrentOpCodeCycle = 0;
                    LDA();
                }
                break;
            default:
                m_SavedValue0 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                m_CurrentOpCodeCycle = 0;
                LDA();
                break;
        }
    }

    private void LDX_immediate()
    {
        m_SavedValue0 = m_Bus.Read(m_Registers.PC);
        ++m_Registers.PC;
        m_CurrentOpCodeCycle = 0;
        LDX();
    }
    private void LDX_zeropage()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                m_CurrentOpCodeCycle = 2;
                break;
            default:
                m_SavedValue0 = m_Bus.Read(m_SavedValue0);
                m_CurrentOpCodeCycle = 0;
                LDX();
                break;
        }
    }
    private void LDX_zeropage_yindexed()
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
                m_SavedValue0 = m_Bus.Read(GetZeropageIndexedNoCarry(m_SavedValue0, m_Registers.Y));
                m_CurrentOpCodeCycle = 0;
                LDX();
                break;
        }
    }
    private void LDX_absolute()
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
                m_SavedValue0 = m_Bus.Read(GetAbsolute(m_SavedValue0, m_SavedValue1));
                m_CurrentOpCodeCycle = 0;
                LDX();
                break;
        }
    }
    private void LDX_absolute_yindexed()
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
                if (m_SavedValue0 + m_Registers.Y > 0xFF)
                {
                    _ = m_Bus.Read(GetAbsoluteIndexedNoCarry(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                    m_CurrentOpCodeCycle = 4;
                }
                else
                {
                    m_SavedValue0 = m_Bus.Read(GetAbsoluteIndexedNoCarry(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                    m_CurrentOpCodeCycle = 0;
                    LDX();
                }
                break;
            default:
                m_SavedValue0 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                m_CurrentOpCodeCycle = 0;
                LDX();
                break;
        }
    }

    private void LDY_immediate()
    {
        m_SavedValue0 = m_Bus.Read(m_Registers.PC);
        ++m_Registers.PC;
        m_CurrentOpCodeCycle = 0;
        LDY();
    }
    private void LDY_zeropage()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                m_CurrentOpCodeCycle = 2;
                break;
            default:
                m_SavedValue0 = m_Bus.Read(m_SavedValue0);
                m_CurrentOpCodeCycle = 0;
                LDY();
                break;
        }
    }
    private void LDY_zeropage_xindexed()
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
                m_SavedValue0 = m_Bus.Read(GetZeropageIndexedNoCarry(m_SavedValue0, m_Registers.X));
                m_CurrentOpCodeCycle = 0;
                LDY();
                break;
        }
    }
    private void LDY_absolute()
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
                m_SavedValue0 = m_Bus.Read(GetAbsolute(m_SavedValue0, m_SavedValue1));
                m_CurrentOpCodeCycle = 0;
                LDY();
                break;
        }
    }
    private void LDY_absolute_xindexed()
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
                if (m_SavedValue0 + m_Registers.X > 0xFF)
                {
                    _ = m_Bus.Read(GetAbsoluteIndexedNoCarry(m_SavedValue0, m_SavedValue1, m_Registers.X));
                    m_CurrentOpCodeCycle = 4;
                }
                else
                {
                    m_SavedValue0 = m_Bus.Read(GetAbsoluteIndexedNoCarry(m_SavedValue0, m_SavedValue1, m_Registers.X));
                    m_CurrentOpCodeCycle = 0;
                    LDY();
                }
                break;
            default:
                m_SavedValue0 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.X));
                m_CurrentOpCodeCycle = 0;
                LDY();
                break;
        }
    }

    private void NOP_impl()
    {
        _ = m_Bus.Read(m_Registers.PC);
        m_CurrentOpCodeCycle = 0;
        NOP();
    }
    private void NOP_immediate()
    {
        m_SavedValue0 = m_Bus.Read(m_Registers.PC);
        ++m_Registers.PC;
        m_CurrentOpCodeCycle = 0;
        NOP();
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
                m_SavedValue0 = m_Bus.Read(m_SavedValue0);
                m_CurrentOpCodeCycle = 0;
                NOP();
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
                m_SavedValue0 = m_Bus.Read(GetZeropageIndexedNoCarry(m_SavedValue0, m_Registers.X));
                m_CurrentOpCodeCycle = 0;
                NOP();
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
                m_SavedValue0 = m_Bus.Read(GetAbsolute(m_SavedValue0, m_SavedValue1));
                m_CurrentOpCodeCycle = 0;
                NOP();
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
                if (m_SavedValue0 + m_Registers.X > 0xFF)
                {
                    _ = m_Bus.Read(GetAbsoluteIndexedNoCarry(m_SavedValue0, m_SavedValue1, m_Registers.X));
                    m_CurrentOpCodeCycle = 4;
                }
                else
                {
                    m_SavedValue0 = m_Bus.Read(GetAbsoluteIndexedNoCarry(m_SavedValue0, m_SavedValue1, m_Registers.X));
                    m_CurrentOpCodeCycle = 0;
                    NOP();
                }
                break;
            default:
                m_SavedValue0 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.X));
                m_CurrentOpCodeCycle = 0;
                NOP();
                break;
        }
    }
}

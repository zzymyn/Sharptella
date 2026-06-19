using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharptari.Lib;

public sealed partial class Mos6502Cpu
{
    private void ADC_immediate()
    {
        m_SavedValue2 = m_Bus.Read(m_Registers.PC);
        ++m_Registers.PC;
        m_CurrentOpCodeCycle = 0;
        ADC(m_SavedValue2);
    }
    private void ADC_zeropage()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                m_CurrentOpCodeCycle = 2;
                break;
            default:
                m_SavedValue2 = m_Bus.Read(m_SavedValue0);
                m_CurrentOpCodeCycle = 0;
                ADC(m_SavedValue2);
                break;
        }
    }
    private void ADC_zeropage_xindexed()
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
                m_SavedValue2 = m_Bus.Read(GetZeropageIndexedNoCarry(m_SavedValue0, m_Registers.X));
                m_CurrentOpCodeCycle = 0;
                ADC(m_SavedValue2);
                break;
        }
    }
    private void ADC_absolute()
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
                m_SavedValue2 = m_Bus.Read(GetAbsolute(m_SavedValue0, m_SavedValue1));
                m_CurrentOpCodeCycle = 0;
                ADC(m_SavedValue2);
                break;
        }
    }
    private void ADC_absolute_xindexed()
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
                    m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexedNoCarry(m_SavedValue0, m_SavedValue1, m_Registers.X));
                    m_CurrentOpCodeCycle = 0;
                    ADC(m_SavedValue2);
                }
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.X));
                m_CurrentOpCodeCycle = 0;
                ADC(m_SavedValue2);
                break;
        }
    }
    private void ADC_absolute_yindexed()
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
                    m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexedNoCarry(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                    m_CurrentOpCodeCycle = 0;
                    ADC(m_SavedValue2);
                }
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                m_CurrentOpCodeCycle = 0;
                ADC(m_SavedValue2);
                break;
        }
    }
    private void ADC_indirect_xindexed()
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
                m_SavedValue2 = m_Bus.Read(GetAbsolute(m_SavedValue0, m_SavedValue1));
                m_CurrentOpCodeCycle = 0;
                ADC(m_SavedValue2);
                break;
        }
    }
    private void ADC_indirect_yindexed()
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
                    m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexedNoCarry(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                    m_CurrentOpCodeCycle = 0;
                    ADC(m_SavedValue2);
                }
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                m_CurrentOpCodeCycle = 0;
                ADC(m_SavedValue2);
                break;
        }
    }

    private void AND_immediate()
    {
        m_SavedValue2 = m_Bus.Read(m_Registers.PC);
        ++m_Registers.PC;
        m_CurrentOpCodeCycle = 0;
        AND(m_SavedValue2);
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
                m_SavedValue2 = m_Bus.Read(m_SavedValue0);
                m_CurrentOpCodeCycle = 0;
                AND(m_SavedValue2);
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
                m_SavedValue2 = m_Bus.Read(GetZeropageIndexedNoCarry(m_SavedValue0, m_Registers.X));
                m_CurrentOpCodeCycle = 0;
                AND(m_SavedValue2);
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
                m_SavedValue2 = m_Bus.Read(GetAbsolute(m_SavedValue0, m_SavedValue1));
                m_CurrentOpCodeCycle = 0;
                AND(m_SavedValue2);
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
                    m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexedNoCarry(m_SavedValue0, m_SavedValue1, m_Registers.X));
                    m_CurrentOpCodeCycle = 0;
                    AND(m_SavedValue2);
                }
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.X));
                m_CurrentOpCodeCycle = 0;
                AND(m_SavedValue2);
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
                    m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexedNoCarry(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                    m_CurrentOpCodeCycle = 0;
                    AND(m_SavedValue2);
                }
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                m_CurrentOpCodeCycle = 0;
                AND(m_SavedValue2);
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
                m_SavedValue2 = m_Bus.Read(GetAbsolute(m_SavedValue0, m_SavedValue1));
                m_CurrentOpCodeCycle = 0;
                AND(m_SavedValue2);
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
                    m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexedNoCarry(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                    m_CurrentOpCodeCycle = 0;
                    AND(m_SavedValue2);
                }
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                m_CurrentOpCodeCycle = 0;
                AND(m_SavedValue2);
                break;
        }
    }

    private void BIT_zeropage()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                m_CurrentOpCodeCycle = 2;
                break;
            default:
                m_SavedValue2 = m_Bus.Read(m_SavedValue0);
                m_CurrentOpCodeCycle = 0;
                BIT(m_SavedValue2);
                break;
        }
    }
    private void BIT_absolute()
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
                m_SavedValue2 = m_Bus.Read(GetAbsolute(m_SavedValue0, m_SavedValue1));
                m_CurrentOpCodeCycle = 0;
                BIT(m_SavedValue2);
                break;
        }
    }

    private void CLC_impl()
    {
        _ = m_Bus.Read(m_Registers.PC);
        m_CurrentOpCodeCycle = 0;
        CLC();
    }

    private void CLD_impl()
    {
        _ = m_Bus.Read(m_Registers.PC);
        m_CurrentOpCodeCycle = 0;
        CLD();
    }

    private void CLI_impl()
    {
        _ = m_Bus.Read(m_Registers.PC);
        m_CurrentOpCodeCycle = 0;
        CLI();
    }

    private void CLV_impl()
    {
        _ = m_Bus.Read(m_Registers.PC);
        m_CurrentOpCodeCycle = 0;
        CLV();
    }

    private void CMP_immediate()
    {
        m_SavedValue2 = m_Bus.Read(m_Registers.PC);
        ++m_Registers.PC;
        m_CurrentOpCodeCycle = 0;
        CMP(m_SavedValue2);
    }
    private void CMP_zeropage()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                m_CurrentOpCodeCycle = 2;
                break;
            default:
                m_SavedValue2 = m_Bus.Read(m_SavedValue0);
                m_CurrentOpCodeCycle = 0;
                CMP(m_SavedValue2);
                break;
        }
    }
    private void CMP_zeropage_xindexed()
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
                m_SavedValue2 = m_Bus.Read(GetZeropageIndexedNoCarry(m_SavedValue0, m_Registers.X));
                m_CurrentOpCodeCycle = 0;
                CMP(m_SavedValue2);
                break;
        }
    }
    private void CMP_absolute()
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
                m_SavedValue2 = m_Bus.Read(GetAbsolute(m_SavedValue0, m_SavedValue1));
                m_CurrentOpCodeCycle = 0;
                CMP(m_SavedValue2);
                break;
        }
    }
    private void CMP_absolute_xindexed()
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
                    m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexedNoCarry(m_SavedValue0, m_SavedValue1, m_Registers.X));
                    m_CurrentOpCodeCycle = 0;
                    CMP(m_SavedValue2);
                }
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.X));
                m_CurrentOpCodeCycle = 0;
                CMP(m_SavedValue2);
                break;
        }
    }
    private void CMP_absolute_yindexed()
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
                    m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexedNoCarry(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                    m_CurrentOpCodeCycle = 0;
                    CMP(m_SavedValue2);
                }
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                m_CurrentOpCodeCycle = 0;
                CMP(m_SavedValue2);
                break;
        }
    }
    private void CMP_indirect_xindexed()
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
                m_SavedValue2 = m_Bus.Read(GetAbsolute(m_SavedValue0, m_SavedValue1));
                m_CurrentOpCodeCycle = 0;
                CMP(m_SavedValue2);
                break;
        }
    }
    private void CMP_indirect_yindexed()
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
                    m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexedNoCarry(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                    m_CurrentOpCodeCycle = 0;
                    CMP(m_SavedValue2);
                }
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                m_CurrentOpCodeCycle = 0;
                CMP(m_SavedValue2);
                break;
        }
    }

    private void ASL_impl()
    {
        _ = m_Bus.Read(m_Registers.PC);
        m_CurrentOpCodeCycle = 0;
        ASL();
    }
    private void ASL_zeropage()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                m_CurrentOpCodeCycle = 2;
                break;
            case 2:
                m_SavedValue2 = m_Bus.Read(m_SavedValue0);
                m_CurrentOpCodeCycle = 3;
                break;
            case 3:
                m_Bus.Write(m_SavedValue0, m_SavedValue2);
                m_CurrentOpCodeCycle = 4;
                break;
            default:
                m_Bus.Write(m_SavedValue0, ASL(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void ASL_zeropage_xindexed()
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
            case 3:
                m_SavedValue2 = m_Bus.Read(GetZeropageIndexedNoCarry(m_SavedValue0, m_Registers.X));
                m_CurrentOpCodeCycle = 4;
                break;
            case 4:
                m_Bus.Write(GetZeropageIndexedNoCarry(m_SavedValue0, m_Registers.X), m_SavedValue2);
                m_CurrentOpCodeCycle = 5;
                break;
            default:
                m_Bus.Write(GetZeropageIndexedNoCarry(m_SavedValue0, m_Registers.X), ASL(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void ASL_absolute()
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
                m_CurrentOpCodeCycle = 4;
                break;
            case 4:
                m_Bus.Write(GetAbsolute(m_SavedValue0, m_SavedValue1), m_SavedValue2);
                m_CurrentOpCodeCycle = 5;
                break;
            default:
                m_Bus.Write(GetAbsolute(m_SavedValue0, m_SavedValue1), ASL(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void ASL_absolute_xindexed()
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
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexedNoCarry(m_SavedValue0, m_SavedValue1, m_Registers.X));
                m_CurrentOpCodeCycle = 4;
                break;
            case 4:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.X));
                m_CurrentOpCodeCycle = 5;
                break;
            case 5:
                m_Bus.Write(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.X), m_SavedValue2);
                m_CurrentOpCodeCycle = 6;
                break;
            default:
                m_Bus.Write(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.X), ASL(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }

    private void LDA_immediate()
    {
        m_SavedValue2 = m_Bus.Read(m_Registers.PC);
        ++m_Registers.PC;
        m_CurrentOpCodeCycle = 0;
        LDA(m_SavedValue2);
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
                m_SavedValue2 = m_Bus.Read(m_SavedValue0);
                m_CurrentOpCodeCycle = 0;
                LDA(m_SavedValue2);
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
                m_SavedValue2 = m_Bus.Read(GetZeropageIndexedNoCarry(m_SavedValue0, m_Registers.X));
                m_CurrentOpCodeCycle = 0;
                LDA(m_SavedValue2);
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
                m_SavedValue2 = m_Bus.Read(GetAbsolute(m_SavedValue0, m_SavedValue1));
                m_CurrentOpCodeCycle = 0;
                LDA(m_SavedValue2);
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
                    m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexedNoCarry(m_SavedValue0, m_SavedValue1, m_Registers.X));
                    m_CurrentOpCodeCycle = 0;
                    LDA(m_SavedValue2);
                }
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.X));
                m_CurrentOpCodeCycle = 0;
                LDA(m_SavedValue2);
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
                    m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexedNoCarry(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                    m_CurrentOpCodeCycle = 0;
                    LDA(m_SavedValue2);
                }
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                m_CurrentOpCodeCycle = 0;
                LDA(m_SavedValue2);
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
                m_SavedValue2 = m_Bus.Read(GetAbsolute(m_SavedValue0, m_SavedValue1));
                m_CurrentOpCodeCycle = 0;
                LDA(m_SavedValue2);
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
                    m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexedNoCarry(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                    m_CurrentOpCodeCycle = 0;
                    LDA(m_SavedValue2);
                }
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                m_CurrentOpCodeCycle = 0;
                LDA(m_SavedValue2);
                break;
        }
    }

    private void LDX_immediate()
    {
        m_SavedValue2 = m_Bus.Read(m_Registers.PC);
        ++m_Registers.PC;
        m_CurrentOpCodeCycle = 0;
        LDX(m_SavedValue2);
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
                m_SavedValue2 = m_Bus.Read(m_SavedValue0);
                m_CurrentOpCodeCycle = 0;
                LDX(m_SavedValue2);
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
                m_SavedValue2 = m_Bus.Read(GetZeropageIndexedNoCarry(m_SavedValue0, m_Registers.Y));
                m_CurrentOpCodeCycle = 0;
                LDX(m_SavedValue2);
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
                m_SavedValue2 = m_Bus.Read(GetAbsolute(m_SavedValue0, m_SavedValue1));
                m_CurrentOpCodeCycle = 0;
                LDX(m_SavedValue2);
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
                    m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexedNoCarry(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                    m_CurrentOpCodeCycle = 0;
                    LDX(m_SavedValue2);
                }
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                m_CurrentOpCodeCycle = 0;
                LDX(m_SavedValue2);
                break;
        }
    }

    private void LDY_immediate()
    {
        m_SavedValue2 = m_Bus.Read(m_Registers.PC);
        ++m_Registers.PC;
        m_CurrentOpCodeCycle = 0;
        LDY(m_SavedValue2);
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
                m_SavedValue2 = m_Bus.Read(m_SavedValue0);
                m_CurrentOpCodeCycle = 0;
                LDY(m_SavedValue2);
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
                m_SavedValue2 = m_Bus.Read(GetZeropageIndexedNoCarry(m_SavedValue0, m_Registers.X));
                m_CurrentOpCodeCycle = 0;
                LDY(m_SavedValue2);
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
                m_SavedValue2 = m_Bus.Read(GetAbsolute(m_SavedValue0, m_SavedValue1));
                m_CurrentOpCodeCycle = 0;
                LDY(m_SavedValue2);
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
                    m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexedNoCarry(m_SavedValue0, m_SavedValue1, m_Registers.X));
                    m_CurrentOpCodeCycle = 0;
                    LDY(m_SavedValue2);
                }
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.X));
                m_CurrentOpCodeCycle = 0;
                LDY(m_SavedValue2);
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
        m_SavedValue2 = m_Bus.Read(m_Registers.PC);
        ++m_Registers.PC;
        m_CurrentOpCodeCycle = 0;
        NOP(m_SavedValue2);
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
                m_SavedValue2 = m_Bus.Read(m_SavedValue0);
                m_CurrentOpCodeCycle = 0;
                NOP(m_SavedValue2);
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
                m_SavedValue2 = m_Bus.Read(GetZeropageIndexedNoCarry(m_SavedValue0, m_Registers.X));
                m_CurrentOpCodeCycle = 0;
                NOP(m_SavedValue2);
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
                m_SavedValue2 = m_Bus.Read(GetAbsolute(m_SavedValue0, m_SavedValue1));
                m_CurrentOpCodeCycle = 0;
                NOP(m_SavedValue2);
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
                    m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexedNoCarry(m_SavedValue0, m_SavedValue1, m_Registers.X));
                    m_CurrentOpCodeCycle = 0;
                    NOP(m_SavedValue2);
                }
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.X));
                m_CurrentOpCodeCycle = 0;
                NOP(m_SavedValue2);
                break;
        }
    }

    private void SBC_immediate()
    {
        m_SavedValue2 = m_Bus.Read(m_Registers.PC);
        ++m_Registers.PC;
        m_CurrentOpCodeCycle = 0;
        SBC(m_SavedValue2);
    }
    private void SBC_zeropage()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                m_CurrentOpCodeCycle = 2;
                break;
            default:
                m_SavedValue2 = m_Bus.Read(m_SavedValue0);
                m_CurrentOpCodeCycle = 0;
                SBC(m_SavedValue2);
                break;
        }
    }
    private void SBC_zeropage_xindexed()
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
                m_SavedValue2 = m_Bus.Read(GetZeropageIndexedNoCarry(m_SavedValue0, m_Registers.X));
                m_CurrentOpCodeCycle = 0;
                SBC(m_SavedValue2);
                break;
        }
    }
    private void SBC_absolute()
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
                m_SavedValue2 = m_Bus.Read(GetAbsolute(m_SavedValue0, m_SavedValue1));
                m_CurrentOpCodeCycle = 0;
                SBC(m_SavedValue2);
                break;
        }
    }
    private void SBC_absolute_xindexed()
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
                    m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexedNoCarry(m_SavedValue0, m_SavedValue1, m_Registers.X));
                    m_CurrentOpCodeCycle = 0;
                    SBC(m_SavedValue2);
                }
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.X));
                m_CurrentOpCodeCycle = 0;
                SBC(m_SavedValue2);
                break;
        }
    }
    private void SBC_absolute_yindexed()
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
                    m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexedNoCarry(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                    m_CurrentOpCodeCycle = 0;
                    SBC(m_SavedValue2);
                }
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                m_CurrentOpCodeCycle = 0;
                SBC(m_SavedValue2);
                break;
        }
    }
    private void SBC_indirect_xindexed()
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
                m_SavedValue2 = m_Bus.Read(GetAbsolute(m_SavedValue0, m_SavedValue1));
                m_CurrentOpCodeCycle = 0;
                SBC(m_SavedValue2);
                break;
        }
    }
    private void SBC_indirect_yindexed()
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
                    m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexedNoCarry(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                    m_CurrentOpCodeCycle = 0;
                    SBC(m_SavedValue2);
                }
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                m_CurrentOpCodeCycle = 0;
                SBC(m_SavedValue2);
                break;
        }
    }

    private void STA_zeropage()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                m_CurrentOpCodeCycle = 2;
                break;
            default:
                m_Bus.Write(m_SavedValue0, STA());
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void STA_zeropage_xindexed()
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
                m_Bus.Write(GetZeropageIndexedNoCarry(m_SavedValue0, m_Registers.X), STA());
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void STA_absolute()
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
                m_Bus.Write(GetAbsolute(m_SavedValue0, m_SavedValue1), STA());
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void STA_absolute_xindexed()
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
                _ = m_Bus.Read(GetAbsoluteIndexedNoCarry(m_SavedValue0, m_SavedValue1, m_Registers.X));
                m_CurrentOpCodeCycle = 4;
                break;
            default:
                m_Bus.Write(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.X), STA());
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void STA_absolute_yindexed()
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
                _ = m_Bus.Read(GetAbsoluteIndexedNoCarry(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                m_CurrentOpCodeCycle = 4;
                break;
            default:
                m_Bus.Write(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y), STA());
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void STA_indirect_xindexed()
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
                m_Bus.Write(GetAbsolute(m_SavedValue0, m_SavedValue1), STA());
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void STA_indirect_yindexed()
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
                _ = m_Bus.Read(GetAbsoluteIndexedNoCarry(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                m_CurrentOpCodeCycle = 5;
                break;
            default:
                m_Bus.Write(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y), STA());
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }

    private void STX_zeropage()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                m_CurrentOpCodeCycle = 2;
                break;
            default:
                m_Bus.Write(m_SavedValue0, STX());
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void STX_zeropage_yindexed()
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
                m_Bus.Write(GetZeropageIndexedNoCarry(m_SavedValue0, m_Registers.Y), STX());
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void STX_absolute()
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
                m_Bus.Write(GetAbsolute(m_SavedValue0, m_SavedValue1), STX());
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }

    private void STY_zeropage()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                m_CurrentOpCodeCycle = 2;
                break;
            default:
                m_Bus.Write(m_SavedValue0, STY());
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void STY_zeropage_xindexed()
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
                m_Bus.Write(GetZeropageIndexedNoCarry(m_SavedValue0, m_Registers.X), STY());
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void STY_absolute()
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
                m_Bus.Write(GetAbsolute(m_SavedValue0, m_SavedValue1), STY());
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }

}

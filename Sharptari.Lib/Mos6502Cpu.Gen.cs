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
            default:
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

    private void ALR_immediate()
    {
        m_SavedValue2 = m_Bus.Read(m_Registers.PC);
        ++m_Registers.PC;
        m_CurrentOpCodeCycle = 0;
        ALR(m_SavedValue2);
    }

    private void ANC_immediate()
    {
        m_SavedValue2 = m_Bus.Read(m_Registers.PC);
        ++m_Registers.PC;
        m_CurrentOpCodeCycle = 0;
        ANC(m_SavedValue2);
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
            default:
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

    private void BCC_relative()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;

                if (BCC())
                {
                    m_CurrentOpCodeCycle = 0;
                }
                else
                {
                    m_CurrentOpCodeCycle = 2;
                }
                break;
            case 2:
                _ = m_Bus.Read(m_Registers.PC);

                ushort newPc = GetRelativeSigned(m_Registers.PC, m_SavedValue0);
                ushort pcNoCarry = GetRelativeSignedNoCarry(m_Registers.PC, m_SavedValue0);

                if (newPc == pcNoCarry)
                {
                    m_Registers.PC = newPc;
                    m_CurrentOpCodeCycle = 0;
                }
                else
                {
                    m_CurrentOpCodeCycle = 3;
                }
                break;
            default:
                _ = m_Bus.Read(GetRelativeSignedNoCarry(m_Registers.PC, m_SavedValue0));
                m_Registers.PC = GetRelativeSigned(m_Registers.PC, m_SavedValue0);
                m_CurrentOpCodeCycle = 0;
                break;
        }

    }

    private void BCS_relative()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;

                if (BCS())
                {
                    m_CurrentOpCodeCycle = 0;
                }
                else
                {
                    m_CurrentOpCodeCycle = 2;
                }
                break;
            case 2:
                _ = m_Bus.Read(m_Registers.PC);

                ushort newPc = GetRelativeSigned(m_Registers.PC, m_SavedValue0);
                ushort pcNoCarry = GetRelativeSignedNoCarry(m_Registers.PC, m_SavedValue0);

                if (newPc == pcNoCarry)
                {
                    m_Registers.PC = newPc;
                    m_CurrentOpCodeCycle = 0;
                }
                else
                {
                    m_CurrentOpCodeCycle = 3;
                }
                break;
            default:
                _ = m_Bus.Read(GetRelativeSignedNoCarry(m_Registers.PC, m_SavedValue0));
                m_Registers.PC = GetRelativeSigned(m_Registers.PC, m_SavedValue0);
                m_CurrentOpCodeCycle = 0;
                break;
        }

    }

    private void BEQ_relative()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;

                if (BEQ())
                {
                    m_CurrentOpCodeCycle = 0;
                }
                else
                {
                    m_CurrentOpCodeCycle = 2;
                }
                break;
            case 2:
                _ = m_Bus.Read(m_Registers.PC);

                ushort newPc = GetRelativeSigned(m_Registers.PC, m_SavedValue0);
                ushort pcNoCarry = GetRelativeSignedNoCarry(m_Registers.PC, m_SavedValue0);

                if (newPc == pcNoCarry)
                {
                    m_Registers.PC = newPc;
                    m_CurrentOpCodeCycle = 0;
                }
                else
                {
                    m_CurrentOpCodeCycle = 3;
                }
                break;
            default:
                _ = m_Bus.Read(GetRelativeSignedNoCarry(m_Registers.PC, m_SavedValue0));
                m_Registers.PC = GetRelativeSigned(m_Registers.PC, m_SavedValue0);
                m_CurrentOpCodeCycle = 0;
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

    private void BMI_relative()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;

                if (BMI())
                {
                    m_CurrentOpCodeCycle = 0;
                }
                else
                {
                    m_CurrentOpCodeCycle = 2;
                }
                break;
            case 2:
                _ = m_Bus.Read(m_Registers.PC);

                ushort newPc = GetRelativeSigned(m_Registers.PC, m_SavedValue0);
                ushort pcNoCarry = GetRelativeSignedNoCarry(m_Registers.PC, m_SavedValue0);

                if (newPc == pcNoCarry)
                {
                    m_Registers.PC = newPc;
                    m_CurrentOpCodeCycle = 0;
                }
                else
                {
                    m_CurrentOpCodeCycle = 3;
                }
                break;
            default:
                _ = m_Bus.Read(GetRelativeSignedNoCarry(m_Registers.PC, m_SavedValue0));
                m_Registers.PC = GetRelativeSigned(m_Registers.PC, m_SavedValue0);
                m_CurrentOpCodeCycle = 0;
                break;
        }

    }

    private void BNE_relative()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;

                if (BNE())
                {
                    m_CurrentOpCodeCycle = 0;
                }
                else
                {
                    m_CurrentOpCodeCycle = 2;
                }
                break;
            case 2:
                _ = m_Bus.Read(m_Registers.PC);

                ushort newPc = GetRelativeSigned(m_Registers.PC, m_SavedValue0);
                ushort pcNoCarry = GetRelativeSignedNoCarry(m_Registers.PC, m_SavedValue0);

                if (newPc == pcNoCarry)
                {
                    m_Registers.PC = newPc;
                    m_CurrentOpCodeCycle = 0;
                }
                else
                {
                    m_CurrentOpCodeCycle = 3;
                }
                break;
            default:
                _ = m_Bus.Read(GetRelativeSignedNoCarry(m_Registers.PC, m_SavedValue0));
                m_Registers.PC = GetRelativeSigned(m_Registers.PC, m_SavedValue0);
                m_CurrentOpCodeCycle = 0;
                break;
        }

    }

    private void BPL_relative()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;

                if (BPL())
                {
                    m_CurrentOpCodeCycle = 0;
                }
                else
                {
                    m_CurrentOpCodeCycle = 2;
                }
                break;
            case 2:
                _ = m_Bus.Read(m_Registers.PC);

                ushort newPc = GetRelativeSigned(m_Registers.PC, m_SavedValue0);
                ushort pcNoCarry = GetRelativeSignedNoCarry(m_Registers.PC, m_SavedValue0);

                if (newPc == pcNoCarry)
                {
                    m_Registers.PC = newPc;
                    m_CurrentOpCodeCycle = 0;
                }
                else
                {
                    m_CurrentOpCodeCycle = 3;
                }
                break;
            default:
                _ = m_Bus.Read(GetRelativeSignedNoCarry(m_Registers.PC, m_SavedValue0));
                m_Registers.PC = GetRelativeSigned(m_Registers.PC, m_SavedValue0);
                m_CurrentOpCodeCycle = 0;
                break;
        }

    }

    private void BVC_relative()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;

                if (BVC())
                {
                    m_CurrentOpCodeCycle = 0;
                }
                else
                {
                    m_CurrentOpCodeCycle = 2;
                }
                break;
            case 2:
                _ = m_Bus.Read(m_Registers.PC);

                ushort newPc = GetRelativeSigned(m_Registers.PC, m_SavedValue0);
                ushort pcNoCarry = GetRelativeSignedNoCarry(m_Registers.PC, m_SavedValue0);

                if (newPc == pcNoCarry)
                {
                    m_Registers.PC = newPc;
                    m_CurrentOpCodeCycle = 0;
                }
                else
                {
                    m_CurrentOpCodeCycle = 3;
                }
                break;
            default:
                _ = m_Bus.Read(GetRelativeSignedNoCarry(m_Registers.PC, m_SavedValue0));
                m_Registers.PC = GetRelativeSigned(m_Registers.PC, m_SavedValue0);
                m_CurrentOpCodeCycle = 0;
                break;
        }

    }

    private void BVS_relative()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;

                if (BVS())
                {
                    m_CurrentOpCodeCycle = 0;
                }
                else
                {
                    m_CurrentOpCodeCycle = 2;
                }
                break;
            case 2:
                _ = m_Bus.Read(m_Registers.PC);

                ushort newPc = GetRelativeSigned(m_Registers.PC, m_SavedValue0);
                ushort pcNoCarry = GetRelativeSignedNoCarry(m_Registers.PC, m_SavedValue0);

                if (newPc == pcNoCarry)
                {
                    m_Registers.PC = newPc;
                    m_CurrentOpCodeCycle = 0;
                }
                else
                {
                    m_CurrentOpCodeCycle = 3;
                }
                break;
            default:
                _ = m_Bus.Read(GetRelativeSignedNoCarry(m_Registers.PC, m_SavedValue0));
                m_Registers.PC = GetRelativeSigned(m_Registers.PC, m_SavedValue0);
                m_CurrentOpCodeCycle = 0;
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
            default:
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

    private void CPX_immediate()
    {
        m_SavedValue2 = m_Bus.Read(m_Registers.PC);
        ++m_Registers.PC;
        m_CurrentOpCodeCycle = 0;
        CPX(m_SavedValue2);
    }
    private void CPX_zeropage()
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
                CPX(m_SavedValue2);
                break;
        }
    }
    private void CPX_absolute()
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
                CPX(m_SavedValue2);
                break;
        }
    }

    private void CPY_immediate()
    {
        m_SavedValue2 = m_Bus.Read(m_Registers.PC);
        ++m_Registers.PC;
        m_CurrentOpCodeCycle = 0;
        CPY(m_SavedValue2);
    }
    private void CPY_zeropage()
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
                CPY(m_SavedValue2);
                break;
        }
    }
    private void CPY_absolute()
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
                CPY(m_SavedValue2);
                break;
        }
    }

    private void DEC_zeropage()
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
                m_Bus.Write(m_SavedValue0, DEC(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void DEC_zeropage_xindexed()
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
                m_Bus.Write(GetZeropageIndexedNoCarry(m_SavedValue0, m_Registers.X), DEC(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void DEC_absolute()
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
                m_Bus.Write(GetAbsolute(m_SavedValue0, m_SavedValue1), DEC(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void DEC_absolute_xindexed()
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
                m_Bus.Write(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.X), DEC(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }

    private void DEX_impl()
    {
        _ = m_Bus.Read(m_Registers.PC);
        m_CurrentOpCodeCycle = 0;
        DEX();
    }

    private void DEY_impl()
    {
        _ = m_Bus.Read(m_Registers.PC);
        m_CurrentOpCodeCycle = 0;
        DEY();
    }

    private void EOR_immediate()
    {
        m_SavedValue2 = m_Bus.Read(m_Registers.PC);
        ++m_Registers.PC;
        m_CurrentOpCodeCycle = 0;
        EOR(m_SavedValue2);
    }
    private void EOR_zeropage()
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
                EOR(m_SavedValue2);
                break;
        }
    }
    private void EOR_zeropage_xindexed()
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
                EOR(m_SavedValue2);
                break;
        }
    }
    private void EOR_absolute()
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
                EOR(m_SavedValue2);
                break;
        }
    }
    private void EOR_absolute_xindexed()
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
                    EOR(m_SavedValue2);
                }
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.X));
                m_CurrentOpCodeCycle = 0;
                EOR(m_SavedValue2);
                break;
        }
    }
    private void EOR_absolute_yindexed()
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
                    EOR(m_SavedValue2);
                }
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                m_CurrentOpCodeCycle = 0;
                EOR(m_SavedValue2);
                break;
        }
    }
    private void EOR_indirect_xindexed()
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
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsolute(m_SavedValue0, m_SavedValue1));
                m_CurrentOpCodeCycle = 0;
                EOR(m_SavedValue2);
                break;
        }
    }
    private void EOR_indirect_yindexed()
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
                    EOR(m_SavedValue2);
                }
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                m_CurrentOpCodeCycle = 0;
                EOR(m_SavedValue2);
                break;
        }
    }

    private void INC_zeropage()
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
                m_Bus.Write(m_SavedValue0, INC(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void INC_zeropage_xindexed()
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
                m_Bus.Write(GetZeropageIndexedNoCarry(m_SavedValue0, m_Registers.X), INC(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void INC_absolute()
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
                m_Bus.Write(GetAbsolute(m_SavedValue0, m_SavedValue1), INC(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void INC_absolute_xindexed()
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
                m_Bus.Write(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.X), INC(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }

    private void INX_impl()
    {
        _ = m_Bus.Read(m_Registers.PC);
        m_CurrentOpCodeCycle = 0;
        INX();
    }

    private void INY_impl()
    {
        _ = m_Bus.Read(m_Registers.PC);
        m_CurrentOpCodeCycle = 0;
        INY();
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
            default:
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

    private void LSR_impl()
    {
        _ = m_Bus.Read(m_Registers.PC);
        m_CurrentOpCodeCycle = 0;
        LSR();
    }
    private void LSR_zeropage()
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
                m_Bus.Write(m_SavedValue0, LSR(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void LSR_zeropage_xindexed()
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
                m_Bus.Write(GetZeropageIndexedNoCarry(m_SavedValue0, m_Registers.X), LSR(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void LSR_absolute()
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
                m_Bus.Write(GetAbsolute(m_SavedValue0, m_SavedValue1), LSR(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void LSR_absolute_xindexed()
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
                m_Bus.Write(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.X), LSR(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
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

    private void ORA_immediate()
    {
        m_SavedValue2 = m_Bus.Read(m_Registers.PC);
        ++m_Registers.PC;
        m_CurrentOpCodeCycle = 0;
        ORA(m_SavedValue2);
    }
    private void ORA_zeropage()
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
                ORA(m_SavedValue2);
                break;
        }
    }
    private void ORA_zeropage_xindexed()
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
                ORA(m_SavedValue2);
                break;
        }
    }
    private void ORA_absolute()
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
                ORA(m_SavedValue2);
                break;
        }
    }
    private void ORA_absolute_xindexed()
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
                    ORA(m_SavedValue2);
                }
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.X));
                m_CurrentOpCodeCycle = 0;
                ORA(m_SavedValue2);
                break;
        }
    }
    private void ORA_absolute_yindexed()
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
                    ORA(m_SavedValue2);
                }
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                m_CurrentOpCodeCycle = 0;
                ORA(m_SavedValue2);
                break;
        }
    }
    private void ORA_indirect_xindexed()
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
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsolute(m_SavedValue0, m_SavedValue1));
                m_CurrentOpCodeCycle = 0;
                ORA(m_SavedValue2);
                break;
        }
    }
    private void ORA_indirect_yindexed()
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
                    ORA(m_SavedValue2);
                }
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                m_CurrentOpCodeCycle = 0;
                ORA(m_SavedValue2);
                break;
        }
    }

    private void ROL_impl()
    {
        _ = m_Bus.Read(m_Registers.PC);
        m_CurrentOpCodeCycle = 0;
        ROL();
    }
    private void ROL_zeropage()
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
                m_Bus.Write(m_SavedValue0, ROL(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void ROL_zeropage_xindexed()
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
                m_Bus.Write(GetZeropageIndexedNoCarry(m_SavedValue0, m_Registers.X), ROL(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void ROL_absolute()
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
                m_Bus.Write(GetAbsolute(m_SavedValue0, m_SavedValue1), ROL(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void ROL_absolute_xindexed()
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
                m_Bus.Write(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.X), ROL(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }

    private void ROR_impl()
    {
        _ = m_Bus.Read(m_Registers.PC);
        m_CurrentOpCodeCycle = 0;
        ROR();
    }
    private void ROR_zeropage()
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
                m_Bus.Write(m_SavedValue0, ROR(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void ROR_zeropage_xindexed()
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
                m_Bus.Write(GetZeropageIndexedNoCarry(m_SavedValue0, m_Registers.X), ROR(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void ROR_absolute()
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
                m_Bus.Write(GetAbsolute(m_SavedValue0, m_SavedValue1), ROR(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void ROR_absolute_xindexed()
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
                m_Bus.Write(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.X), ROR(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
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
            default:
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

    private void SEC_impl()
    {
        _ = m_Bus.Read(m_Registers.PC);
        m_CurrentOpCodeCycle = 0;
        SEC();
    }

    private void SED_impl()
    {
        _ = m_Bus.Read(m_Registers.PC);
        m_CurrentOpCodeCycle = 0;
        SED();
    }

    private void SEI_impl()
    {
        _ = m_Bus.Read(m_Registers.PC);
        m_CurrentOpCodeCycle = 0;
        SEI();
    }

    private void SLO_zeropage()
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
                m_Bus.Write(m_SavedValue0, SLO(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void SLO_zeropage_xindexed()
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
                m_Bus.Write(GetZeropageIndexedNoCarry(m_SavedValue0, m_Registers.X), SLO(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void SLO_absolute()
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
                m_Bus.Write(GetAbsolute(m_SavedValue0, m_SavedValue1), SLO(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void SLO_absolute_xindexed()
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
                m_Bus.Write(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.X), SLO(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void SLO_absolute_yindexed()
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
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexedNoCarry(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                m_CurrentOpCodeCycle = 4;
                break;
            case 4:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                m_CurrentOpCodeCycle = 5;
                break;
            case 5:
                m_Bus.Write(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y), m_SavedValue2);
                m_CurrentOpCodeCycle = 6;
                break;
            default:
                m_Bus.Write(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y), SLO(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void SLO_indirect_xindexed()
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
                m_CurrentOpCodeCycle = 6;
                break;
            case 6:
                m_Bus.Write(GetAbsolute(m_SavedValue0, m_SavedValue1), m_SavedValue2);
                m_CurrentOpCodeCycle = 7;
                break;
            default:
                m_Bus.Write(GetAbsolute(m_SavedValue0, m_SavedValue1), SLO(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void SLO_indirect_yindexed()
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
            case 5:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                m_CurrentOpCodeCycle = 6;
                break;
            case 6:
                m_Bus.Write(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y), m_SavedValue2);
                m_CurrentOpCodeCycle = 7;
                break;
            default:
                m_Bus.Write(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y), SLO(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
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
            default:
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

    private void TAX_impl()
    {
        _ = m_Bus.Read(m_Registers.PC);
        m_CurrentOpCodeCycle = 0;
        TAX();
    }

    private void TAY_impl()
    {
        _ = m_Bus.Read(m_Registers.PC);
        m_CurrentOpCodeCycle = 0;
        TAY();
    }

    private void TSX_impl()
    {
        _ = m_Bus.Read(m_Registers.PC);
        m_CurrentOpCodeCycle = 0;
        TSX();
    }

    private void TXA_impl()
    {
        _ = m_Bus.Read(m_Registers.PC);
        m_CurrentOpCodeCycle = 0;
        TXA();
    }

    private void TXS_impl()
    {
        _ = m_Bus.Read(m_Registers.PC);
        m_CurrentOpCodeCycle = 0;
        TXS();
    }

    private void TYA_impl()
    {
        _ = m_Bus.Read(m_Registers.PC);
        m_CurrentOpCodeCycle = 0;
        TYA();
    }

}

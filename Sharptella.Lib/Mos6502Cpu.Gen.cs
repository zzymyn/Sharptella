namespace Sharptella.Lib;

public sealed partial class Mos6502Cpu<BusT>
{
    private void ADC_immediate()
    {
        m_SavedValue2 = m_Bus.Read(m_Registers.PC);
        ++m_Registers.PC;
        Trace($"ADC #${m_SavedValue2:X2}");
        ADC(m_SavedValue2);
        m_CurrentOpCodeCycle = 0;
    }
    private void ADC_zeropage()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                Trace($"ADC ${m_SavedValue0:X2}");
                m_CurrentOpCodeCycle = 2;
                break;
            default:
                m_SavedValue2 = m_Bus.Read(m_SavedValue0);
                ADC(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
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
                Trace($"ADC ${m_SavedValue0:X2},X");
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
                Trace($"ADC ${m_SavedValue0:X2}{m_SavedValue1:X2}");
                m_CurrentOpCodeCycle = 3;
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsolute(m_SavedValue0, m_SavedValue1));
                ADC(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
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
                Trace($"ADC ${m_SavedValue0:X2}{m_SavedValue1:X2},X");
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
                    ADC(m_SavedValue2);
                    m_CurrentOpCodeCycle = 0;
                }
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.X));
                ADC(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
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
                Trace($"ADC ${m_SavedValue0:X2}{m_SavedValue1:X2},Y");
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
                    ADC(m_SavedValue2);
                    m_CurrentOpCodeCycle = 0;
                }
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                ADC(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
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
                Trace($"ADC (${m_SavedValue1:X2},X)");
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
                ADC(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
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
                Trace($"ADC (${m_SavedValue1:X2}),Y");
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
                    ADC(m_SavedValue2);
                    m_CurrentOpCodeCycle = 0;
                }
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                ADC(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }

    private void ALR_immediate()
    {
        m_SavedValue2 = m_Bus.Read(m_Registers.PC);
        ++m_Registers.PC;
        Trace($"ALR #${m_SavedValue2:X2}");
        ALR(m_SavedValue2);
        m_CurrentOpCodeCycle = 0;
    }

    private void ANC_immediate()
    {
        m_SavedValue2 = m_Bus.Read(m_Registers.PC);
        ++m_Registers.PC;
        Trace($"ANC #${m_SavedValue2:X2}");
        ANC(m_SavedValue2);
        m_CurrentOpCodeCycle = 0;
    }

    private void AND_immediate()
    {
        m_SavedValue2 = m_Bus.Read(m_Registers.PC);
        ++m_Registers.PC;
        Trace($"AND #${m_SavedValue2:X2}");
        AND(m_SavedValue2);
        m_CurrentOpCodeCycle = 0;
    }
    private void AND_zeropage()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                Trace($"AND ${m_SavedValue0:X2}");
                m_CurrentOpCodeCycle = 2;
                break;
            default:
                m_SavedValue2 = m_Bus.Read(m_SavedValue0);
                AND(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
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
                Trace($"AND ${m_SavedValue0:X2},X");
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
                Trace($"AND ${m_SavedValue0:X2}{m_SavedValue1:X2}");
                m_CurrentOpCodeCycle = 3;
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsolute(m_SavedValue0, m_SavedValue1));
                AND(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
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
                Trace($"AND ${m_SavedValue0:X2}{m_SavedValue1:X2},X");
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
                    AND(m_SavedValue2);
                    m_CurrentOpCodeCycle = 0;
                }
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.X));
                AND(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
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
                Trace($"AND ${m_SavedValue0:X2}{m_SavedValue1:X2},Y");
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
                    AND(m_SavedValue2);
                    m_CurrentOpCodeCycle = 0;
                }
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                AND(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
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
                Trace($"AND (${m_SavedValue1:X2},X)");
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
                AND(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
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
                Trace($"AND (${m_SavedValue1:X2}),Y");
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
                    AND(m_SavedValue2);
                    m_CurrentOpCodeCycle = 0;
                }
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                AND(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }

    private void ASL_impl()
    {
        _ = m_Bus.Read(m_Registers.PC);
        Trace($"ASL A");
        ASL();
        m_CurrentOpCodeCycle = 0;
    }
    private void ASL_zeropage()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                Trace($"ASL ${m_SavedValue0:X2}");
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
                Trace($"ASL ${m_SavedValue0:X2},X");
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
                Trace($"ASL ${m_SavedValue0:X2}{m_SavedValue1:X2}");
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
                Trace($"ASL ${m_SavedValue0:X2}{m_SavedValue1:X2},X");
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

    private void ARR_immediate()
    {
        m_SavedValue2 = m_Bus.Read(m_Registers.PC);
        ++m_Registers.PC;
        Trace($"ARR #${m_SavedValue2:X2}");
        ARR(m_SavedValue2);
        m_CurrentOpCodeCycle = 0;
    }

    private void BCC_relative()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                Trace($"BCC ${m_SavedValue0:X2}");

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
                Trace($"BCS ${m_SavedValue0:X2}");

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
                Trace($"BEQ ${m_SavedValue0:X2}");

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
                Trace($"BIT ${m_SavedValue0:X2}");
                m_CurrentOpCodeCycle = 2;
                break;
            default:
                m_SavedValue2 = m_Bus.Read(m_SavedValue0);
                BIT(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
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
                Trace($"BIT ${m_SavedValue0:X2}{m_SavedValue1:X2}");
                m_CurrentOpCodeCycle = 3;
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsolute(m_SavedValue0, m_SavedValue1));
                BIT(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
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
                Trace($"BMI ${m_SavedValue0:X2}");

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
                Trace($"BNE ${m_SavedValue0:X2}");

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
                Trace($"BPL ${m_SavedValue0:X2}");

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
                Trace($"BVC ${m_SavedValue0:X2}");

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
                Trace($"BVS ${m_SavedValue0:X2}");

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
        Trace($"CLC");
        CLC();
        m_CurrentOpCodeCycle = 0;
    }

    private void CLD_impl()
    {
        _ = m_Bus.Read(m_Registers.PC);
        Trace($"CLD");
        CLD();
        m_CurrentOpCodeCycle = 0;
    }

    private void CLI_impl()
    {
        _ = m_Bus.Read(m_Registers.PC);
        Trace($"CLI");
        CLI();
        m_CurrentOpCodeCycle = 0;
    }

    private void CLV_impl()
    {
        _ = m_Bus.Read(m_Registers.PC);
        Trace($"CLV");
        CLV();
        m_CurrentOpCodeCycle = 0;
    }

    private void CMP_immediate()
    {
        m_SavedValue2 = m_Bus.Read(m_Registers.PC);
        ++m_Registers.PC;
        Trace($"CMP #${m_SavedValue2:X2}");
        CMP(m_SavedValue2);
        m_CurrentOpCodeCycle = 0;
    }
    private void CMP_zeropage()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                Trace($"CMP ${m_SavedValue0:X2}");
                m_CurrentOpCodeCycle = 2;
                break;
            default:
                m_SavedValue2 = m_Bus.Read(m_SavedValue0);
                CMP(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
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
                Trace($"CMP ${m_SavedValue0:X2},X");
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
                Trace($"CMP ${m_SavedValue0:X2}{m_SavedValue1:X2}");
                m_CurrentOpCodeCycle = 3;
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsolute(m_SavedValue0, m_SavedValue1));
                CMP(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
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
                Trace($"CMP ${m_SavedValue0:X2}{m_SavedValue1:X2},X");
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
                    CMP(m_SavedValue2);
                    m_CurrentOpCodeCycle = 0;
                }
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.X));
                CMP(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
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
                Trace($"CMP ${m_SavedValue0:X2}{m_SavedValue1:X2},Y");
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
                    CMP(m_SavedValue2);
                    m_CurrentOpCodeCycle = 0;
                }
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                CMP(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
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
                Trace($"CMP (${m_SavedValue1:X2},X)");
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
                CMP(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
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
                Trace($"CMP (${m_SavedValue1:X2}),Y");
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
                    CMP(m_SavedValue2);
                    m_CurrentOpCodeCycle = 0;
                }
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                CMP(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }

    private void CPX_immediate()
    {
        m_SavedValue2 = m_Bus.Read(m_Registers.PC);
        ++m_Registers.PC;
        Trace($"CPX #${m_SavedValue2:X2}");
        CPX(m_SavedValue2);
        m_CurrentOpCodeCycle = 0;
    }
    private void CPX_zeropage()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                Trace($"CPX ${m_SavedValue0:X2}");
                m_CurrentOpCodeCycle = 2;
                break;
            default:
                m_SavedValue2 = m_Bus.Read(m_SavedValue0);
                CPX(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
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
                Trace($"CPX ${m_SavedValue0:X2}{m_SavedValue1:X2}");
                m_CurrentOpCodeCycle = 3;
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsolute(m_SavedValue0, m_SavedValue1));
                CPX(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }

    private void CPY_immediate()
    {
        m_SavedValue2 = m_Bus.Read(m_Registers.PC);
        ++m_Registers.PC;
        Trace($"CPY #${m_SavedValue2:X2}");
        CPY(m_SavedValue2);
        m_CurrentOpCodeCycle = 0;
    }
    private void CPY_zeropage()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                Trace($"CPY ${m_SavedValue0:X2}");
                m_CurrentOpCodeCycle = 2;
                break;
            default:
                m_SavedValue2 = m_Bus.Read(m_SavedValue0);
                CPY(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
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
                Trace($"CPY ${m_SavedValue0:X2}{m_SavedValue1:X2}");
                m_CurrentOpCodeCycle = 3;
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsolute(m_SavedValue0, m_SavedValue1));
                CPY(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }

    private void DCP_zeropage()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                Trace($"DCP ${m_SavedValue0:X2}");
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
                m_Bus.Write(m_SavedValue0, DCP(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void DCP_zeropage_xindexed()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                Trace($"DCP ${m_SavedValue0:X2},X");
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
                m_Bus.Write(GetZeropageIndexedNoCarry(m_SavedValue0, m_Registers.X), DCP(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void DCP_absolute()
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
                Trace($"DCP ${m_SavedValue0:X2}{m_SavedValue1:X2}");
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
                m_Bus.Write(GetAbsolute(m_SavedValue0, m_SavedValue1), DCP(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void DCP_absolute_xindexed()
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
                Trace($"DCP ${m_SavedValue0:X2}{m_SavedValue1:X2},X");
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
                m_Bus.Write(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.X), DCP(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void DCP_absolute_yindexed()
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
                Trace($"DCP ${m_SavedValue0:X2}{m_SavedValue1:X2},Y");
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
                m_Bus.Write(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y), DCP(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void DCP_indirect_xindexed()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue1 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                Trace($"DCP (${m_SavedValue1:X2},X)");
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
                m_Bus.Write(GetAbsolute(m_SavedValue0, m_SavedValue1), DCP(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void DCP_indirect_yindexed()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue1 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                Trace($"DCP (${m_SavedValue1:X2}),Y");
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
                m_Bus.Write(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y), DCP(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
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
                Trace($"DEC ${m_SavedValue0:X2}");
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
                Trace($"DEC ${m_SavedValue0:X2},X");
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
                Trace($"DEC ${m_SavedValue0:X2}{m_SavedValue1:X2}");
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
                Trace($"DEC ${m_SavedValue0:X2}{m_SavedValue1:X2},X");
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
        Trace($"DEX");
        DEX();
        m_CurrentOpCodeCycle = 0;
    }

    private void DEY_impl()
    {
        _ = m_Bus.Read(m_Registers.PC);
        Trace($"DEY");
        DEY();
        m_CurrentOpCodeCycle = 0;
    }

    private void EOR_immediate()
    {
        m_SavedValue2 = m_Bus.Read(m_Registers.PC);
        ++m_Registers.PC;
        Trace($"EOR #${m_SavedValue2:X2}");
        EOR(m_SavedValue2);
        m_CurrentOpCodeCycle = 0;
    }
    private void EOR_zeropage()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                Trace($"EOR ${m_SavedValue0:X2}");
                m_CurrentOpCodeCycle = 2;
                break;
            default:
                m_SavedValue2 = m_Bus.Read(m_SavedValue0);
                EOR(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
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
                Trace($"EOR ${m_SavedValue0:X2},X");
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
                Trace($"EOR ${m_SavedValue0:X2}{m_SavedValue1:X2}");
                m_CurrentOpCodeCycle = 3;
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsolute(m_SavedValue0, m_SavedValue1));
                EOR(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
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
                Trace($"EOR ${m_SavedValue0:X2}{m_SavedValue1:X2},X");
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
                    EOR(m_SavedValue2);
                    m_CurrentOpCodeCycle = 0;
                }
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.X));
                EOR(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
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
                Trace($"EOR ${m_SavedValue0:X2}{m_SavedValue1:X2},Y");
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
                    EOR(m_SavedValue2);
                    m_CurrentOpCodeCycle = 0;
                }
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                EOR(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
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
                Trace($"EOR (${m_SavedValue1:X2},X)");
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
                EOR(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
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
                Trace($"EOR (${m_SavedValue1:X2}),Y");
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
                    EOR(m_SavedValue2);
                    m_CurrentOpCodeCycle = 0;
                }
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                EOR(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
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
                Trace($"INC ${m_SavedValue0:X2}");
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
                Trace($"INC ${m_SavedValue0:X2},X");
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
                Trace($"INC ${m_SavedValue0:X2}{m_SavedValue1:X2}");
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
                Trace($"INC ${m_SavedValue0:X2}{m_SavedValue1:X2},X");
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
        Trace($"INX");
        INX();
        m_CurrentOpCodeCycle = 0;
    }

    private void INY_impl()
    {
        _ = m_Bus.Read(m_Registers.PC);
        Trace($"INY");
        INY();
        m_CurrentOpCodeCycle = 0;
    }

    private void ISC_zeropage()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                Trace($"ISC ${m_SavedValue0:X2}");
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
                m_Bus.Write(m_SavedValue0, ISC(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void ISC_zeropage_xindexed()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                Trace($"ISC ${m_SavedValue0:X2},X");
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
                m_Bus.Write(GetZeropageIndexedNoCarry(m_SavedValue0, m_Registers.X), ISC(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void ISC_absolute()
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
                Trace($"ISC ${m_SavedValue0:X2}{m_SavedValue1:X2}");
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
                m_Bus.Write(GetAbsolute(m_SavedValue0, m_SavedValue1), ISC(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void ISC_absolute_xindexed()
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
                Trace($"ISC ${m_SavedValue0:X2}{m_SavedValue1:X2},X");
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
                m_Bus.Write(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.X), ISC(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void ISC_absolute_yindexed()
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
                Trace($"ISC ${m_SavedValue0:X2}{m_SavedValue1:X2},Y");
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
                m_Bus.Write(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y), ISC(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void ISC_indirect_xindexed()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue1 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                Trace($"ISC (${m_SavedValue1:X2},X)");
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
                m_Bus.Write(GetAbsolute(m_SavedValue0, m_SavedValue1), ISC(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void ISC_indirect_yindexed()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue1 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                Trace($"ISC (${m_SavedValue1:X2}),Y");
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
                m_Bus.Write(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y), ISC(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }

    private void LAS_absolute_yindexed()
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
                Trace($"LAS ${m_SavedValue0:X2}{m_SavedValue1:X2},Y");
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
                    LAS(m_SavedValue2);
                    m_CurrentOpCodeCycle = 0;
                }
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                LAS(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }

    private void LAX_zeropage()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                Trace($"LAX ${m_SavedValue0:X2}");
                m_CurrentOpCodeCycle = 2;
                break;
            default:
                m_SavedValue2 = m_Bus.Read(m_SavedValue0);
                LAX(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void LAX_zeropage_yindexed()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                Trace($"LAX ${m_SavedValue0:X2},Y");
                m_CurrentOpCodeCycle = 2;
                break;
            case 2:
                _ = m_Bus.Read(m_SavedValue0);
                m_CurrentOpCodeCycle = 3;
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetZeropageIndexedNoCarry(m_SavedValue0, m_Registers.Y));
                LAX(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void LAX_absolute()
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
                Trace($"LAX ${m_SavedValue0:X2}{m_SavedValue1:X2}");
                m_CurrentOpCodeCycle = 3;
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsolute(m_SavedValue0, m_SavedValue1));
                LAX(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void LAX_absolute_yindexed()
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
                Trace($"LAX ${m_SavedValue0:X2}{m_SavedValue1:X2},Y");
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
                    LAX(m_SavedValue2);
                    m_CurrentOpCodeCycle = 0;
                }
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                LAX(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void LAX_indirect_xindexed()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue1 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                Trace($"LAX (${m_SavedValue1:X2},X)");
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
                LAX(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void LAX_indirect_yindexed()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue1 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                Trace($"LAX (${m_SavedValue1:X2}),Y");
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
                    LAX(m_SavedValue2);
                    m_CurrentOpCodeCycle = 0;
                }
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                LAX(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }

    private void LDA_immediate()
    {
        m_SavedValue2 = m_Bus.Read(m_Registers.PC);
        ++m_Registers.PC;
        Trace($"LDA #${m_SavedValue2:X2}");
        LDA(m_SavedValue2);
        m_CurrentOpCodeCycle = 0;
    }
    private void LDA_zeropage()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                Trace($"LDA ${m_SavedValue0:X2}");
                m_CurrentOpCodeCycle = 2;
                break;
            default:
                m_SavedValue2 = m_Bus.Read(m_SavedValue0);
                LDA(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
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
                Trace($"LDA ${m_SavedValue0:X2},X");
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
                Trace($"LDA ${m_SavedValue0:X2}{m_SavedValue1:X2}");
                m_CurrentOpCodeCycle = 3;
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsolute(m_SavedValue0, m_SavedValue1));
                LDA(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
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
                Trace($"LDA ${m_SavedValue0:X2}{m_SavedValue1:X2},X");
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
                    LDA(m_SavedValue2);
                    m_CurrentOpCodeCycle = 0;
                }
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.X));
                LDA(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
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
                Trace($"LDA ${m_SavedValue0:X2}{m_SavedValue1:X2},Y");
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
                    LDA(m_SavedValue2);
                    m_CurrentOpCodeCycle = 0;
                }
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                LDA(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
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
                Trace($"LDA (${m_SavedValue1:X2},X)");
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
                LDA(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
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
                Trace($"LDA (${m_SavedValue1:X2}),Y");
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
                    LDA(m_SavedValue2);
                    m_CurrentOpCodeCycle = 0;
                }
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                LDA(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }

    private void LDX_immediate()
    {
        m_SavedValue2 = m_Bus.Read(m_Registers.PC);
        ++m_Registers.PC;
        Trace($"LDX #${m_SavedValue2:X2}");
        LDX(m_SavedValue2);
        m_CurrentOpCodeCycle = 0;
    }
    private void LDX_zeropage()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                Trace($"LDX ${m_SavedValue0:X2}");
                m_CurrentOpCodeCycle = 2;
                break;
            default:
                m_SavedValue2 = m_Bus.Read(m_SavedValue0);
                LDX(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
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
                Trace($"LDX ${m_SavedValue0:X2},Y");
                m_CurrentOpCodeCycle = 2;
                break;
            case 2:
                _ = m_Bus.Read(m_SavedValue0);
                m_CurrentOpCodeCycle = 3;
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetZeropageIndexedNoCarry(m_SavedValue0, m_Registers.Y));
                LDX(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
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
                Trace($"LDX ${m_SavedValue0:X2}{m_SavedValue1:X2}");
                m_CurrentOpCodeCycle = 3;
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsolute(m_SavedValue0, m_SavedValue1));
                LDX(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
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
                Trace($"LDX ${m_SavedValue0:X2}{m_SavedValue1:X2},Y");
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
                    LDX(m_SavedValue2);
                    m_CurrentOpCodeCycle = 0;
                }
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                LDX(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }

    private void LDY_immediate()
    {
        m_SavedValue2 = m_Bus.Read(m_Registers.PC);
        ++m_Registers.PC;
        Trace($"LDY #${m_SavedValue2:X2}");
        LDY(m_SavedValue2);
        m_CurrentOpCodeCycle = 0;
    }
    private void LDY_zeropage()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                Trace($"LDY ${m_SavedValue0:X2}");
                m_CurrentOpCodeCycle = 2;
                break;
            default:
                m_SavedValue2 = m_Bus.Read(m_SavedValue0);
                LDY(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
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
                Trace($"LDY ${m_SavedValue0:X2},X");
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
                Trace($"LDY ${m_SavedValue0:X2}{m_SavedValue1:X2}");
                m_CurrentOpCodeCycle = 3;
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsolute(m_SavedValue0, m_SavedValue1));
                LDY(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
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
                Trace($"LDY ${m_SavedValue0:X2}{m_SavedValue1:X2},X");
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
                    LDY(m_SavedValue2);
                    m_CurrentOpCodeCycle = 0;
                }
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.X));
                LDY(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }

    private void LSR_impl()
    {
        _ = m_Bus.Read(m_Registers.PC);
        Trace($"LSR A");
        LSR();
        m_CurrentOpCodeCycle = 0;
    }
    private void LSR_zeropage()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                Trace($"LSR ${m_SavedValue0:X2}");
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
                Trace($"LSR ${m_SavedValue0:X2},X");
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
                Trace($"LSR ${m_SavedValue0:X2}{m_SavedValue1:X2}");
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
                Trace($"LSR ${m_SavedValue0:X2}{m_SavedValue1:X2},X");
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
        Trace($"NOP");
        NOP();
        m_CurrentOpCodeCycle = 0;
    }
    private void NOP_immediate()
    {
        m_SavedValue2 = m_Bus.Read(m_Registers.PC);
        ++m_Registers.PC;
        Trace($"NOP #${m_SavedValue2:X2}");
        NOP(m_SavedValue2);
        m_CurrentOpCodeCycle = 0;
    }
    private void NOP_zeropage()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                Trace($"NOP ${m_SavedValue0:X2}");
                m_CurrentOpCodeCycle = 2;
                break;
            default:
                m_SavedValue2 = m_Bus.Read(m_SavedValue0);
                NOP(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
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
                Trace($"NOP ${m_SavedValue0:X2},X");
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
                Trace($"NOP ${m_SavedValue0:X2}{m_SavedValue1:X2}");
                m_CurrentOpCodeCycle = 3;
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsolute(m_SavedValue0, m_SavedValue1));
                NOP(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
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
                Trace($"NOP ${m_SavedValue0:X2}{m_SavedValue1:X2},X");
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
                    NOP(m_SavedValue2);
                    m_CurrentOpCodeCycle = 0;
                }
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.X));
                NOP(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }

    private void ORA_immediate()
    {
        m_SavedValue2 = m_Bus.Read(m_Registers.PC);
        ++m_Registers.PC;
        Trace($"ORA #${m_SavedValue2:X2}");
        ORA(m_SavedValue2);
        m_CurrentOpCodeCycle = 0;
    }
    private void ORA_zeropage()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                Trace($"ORA ${m_SavedValue0:X2}");
                m_CurrentOpCodeCycle = 2;
                break;
            default:
                m_SavedValue2 = m_Bus.Read(m_SavedValue0);
                ORA(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
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
                Trace($"ORA ${m_SavedValue0:X2},X");
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
                Trace($"ORA ${m_SavedValue0:X2}{m_SavedValue1:X2}");
                m_CurrentOpCodeCycle = 3;
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsolute(m_SavedValue0, m_SavedValue1));
                ORA(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
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
                Trace($"ORA ${m_SavedValue0:X2}{m_SavedValue1:X2},X");
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
                    ORA(m_SavedValue2);
                    m_CurrentOpCodeCycle = 0;
                }
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.X));
                ORA(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
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
                Trace($"ORA ${m_SavedValue0:X2}{m_SavedValue1:X2},Y");
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
                    ORA(m_SavedValue2);
                    m_CurrentOpCodeCycle = 0;
                }
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                ORA(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
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
                Trace($"ORA (${m_SavedValue1:X2},X)");
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
                ORA(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
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
                Trace($"ORA (${m_SavedValue1:X2}),Y");
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
                    ORA(m_SavedValue2);
                    m_CurrentOpCodeCycle = 0;
                }
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                ORA(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }

    private void RLA_zeropage()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                Trace($"RLA ${m_SavedValue0:X2}");
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
                m_Bus.Write(m_SavedValue0, RLA(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void RLA_zeropage_xindexed()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                Trace($"RLA ${m_SavedValue0:X2},X");
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
                m_Bus.Write(GetZeropageIndexedNoCarry(m_SavedValue0, m_Registers.X), RLA(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void RLA_absolute()
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
                Trace($"RLA ${m_SavedValue0:X2}{m_SavedValue1:X2}");
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
                m_Bus.Write(GetAbsolute(m_SavedValue0, m_SavedValue1), RLA(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void RLA_absolute_xindexed()
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
                Trace($"RLA ${m_SavedValue0:X2}{m_SavedValue1:X2},X");
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
                m_Bus.Write(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.X), RLA(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void RLA_absolute_yindexed()
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
                Trace($"RLA ${m_SavedValue0:X2}{m_SavedValue1:X2},Y");
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
                m_Bus.Write(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y), RLA(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void RLA_indirect_xindexed()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue1 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                Trace($"RLA (${m_SavedValue1:X2},X)");
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
                m_Bus.Write(GetAbsolute(m_SavedValue0, m_SavedValue1), RLA(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void RLA_indirect_yindexed()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue1 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                Trace($"RLA (${m_SavedValue1:X2}),Y");
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
                m_Bus.Write(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y), RLA(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }

    private void ROL_impl()
    {
        _ = m_Bus.Read(m_Registers.PC);
        Trace($"ROL A");
        ROL();
        m_CurrentOpCodeCycle = 0;
    }
    private void ROL_zeropage()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                Trace($"ROL ${m_SavedValue0:X2}");
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
                Trace($"ROL ${m_SavedValue0:X2},X");
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
                Trace($"ROL ${m_SavedValue0:X2}{m_SavedValue1:X2}");
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
                Trace($"ROL ${m_SavedValue0:X2}{m_SavedValue1:X2},X");
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
        Trace($"ROR A");
        ROR();
        m_CurrentOpCodeCycle = 0;
    }
    private void ROR_zeropage()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                Trace($"ROR ${m_SavedValue0:X2}");
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
                Trace($"ROR ${m_SavedValue0:X2},X");
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
                Trace($"ROR ${m_SavedValue0:X2}{m_SavedValue1:X2}");
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
                Trace($"ROR ${m_SavedValue0:X2}{m_SavedValue1:X2},X");
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

    private void RRA_zeropage()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                Trace($"RRA ${m_SavedValue0:X2}");
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
                m_Bus.Write(m_SavedValue0, RRA(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void RRA_zeropage_xindexed()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                Trace($"RRA ${m_SavedValue0:X2},X");
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
                m_Bus.Write(GetZeropageIndexedNoCarry(m_SavedValue0, m_Registers.X), RRA(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void RRA_absolute()
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
                Trace($"RRA ${m_SavedValue0:X2}{m_SavedValue1:X2}");
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
                m_Bus.Write(GetAbsolute(m_SavedValue0, m_SavedValue1), RRA(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void RRA_absolute_xindexed()
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
                Trace($"RRA ${m_SavedValue0:X2}{m_SavedValue1:X2},X");
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
                m_Bus.Write(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.X), RRA(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void RRA_absolute_yindexed()
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
                Trace($"RRA ${m_SavedValue0:X2}{m_SavedValue1:X2},Y");
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
                m_Bus.Write(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y), RRA(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void RRA_indirect_xindexed()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue1 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                Trace($"RRA (${m_SavedValue1:X2},X)");
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
                m_Bus.Write(GetAbsolute(m_SavedValue0, m_SavedValue1), RRA(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void RRA_indirect_yindexed()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue1 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                Trace($"RRA (${m_SavedValue1:X2}),Y");
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
                m_Bus.Write(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y), RRA(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }

    private void SAX_zeropage()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                Trace($"SAX ${m_SavedValue0:X2}");
                m_CurrentOpCodeCycle = 2;
                break;
            default:
                m_Bus.Write(m_SavedValue0, SAX());
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void SAX_zeropage_yindexed()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                Trace($"SAX ${m_SavedValue0:X2},Y");
                m_CurrentOpCodeCycle = 2;
                break;
            case 2:
                _ = m_Bus.Read(m_SavedValue0);
                m_CurrentOpCodeCycle = 3;
                break;
            default:
                m_Bus.Write(GetZeropageIndexedNoCarry(m_SavedValue0, m_Registers.Y), SAX());
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void SAX_absolute()
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
                Trace($"SAX ${m_SavedValue0:X2}{m_SavedValue1:X2}");
                m_CurrentOpCodeCycle = 3;
                break;
            default:
                m_Bus.Write(GetAbsolute(m_SavedValue0, m_SavedValue1), SAX());
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void SAX_indirect_xindexed()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue1 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                Trace($"SAX (${m_SavedValue1:X2},X)");
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
                m_Bus.Write(GetAbsolute(m_SavedValue0, m_SavedValue1), SAX());
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }

    private void SBX_immediate()
    {
        m_SavedValue2 = m_Bus.Read(m_Registers.PC);
        ++m_Registers.PC;
        Trace($"SBX #${m_SavedValue2:X2}");
        SBX(m_SavedValue2);
        m_CurrentOpCodeCycle = 0;
    }

    private void SBC_immediate()
    {
        m_SavedValue2 = m_Bus.Read(m_Registers.PC);
        ++m_Registers.PC;
        Trace($"SBC #${m_SavedValue2:X2}");
        SBC(m_SavedValue2);
        m_CurrentOpCodeCycle = 0;
    }
    private void SBC_zeropage()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                Trace($"SBC ${m_SavedValue0:X2}");
                m_CurrentOpCodeCycle = 2;
                break;
            default:
                m_SavedValue2 = m_Bus.Read(m_SavedValue0);
                SBC(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
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
                Trace($"SBC ${m_SavedValue0:X2},X");
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
                Trace($"SBC ${m_SavedValue0:X2}{m_SavedValue1:X2}");
                m_CurrentOpCodeCycle = 3;
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsolute(m_SavedValue0, m_SavedValue1));
                SBC(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
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
                Trace($"SBC ${m_SavedValue0:X2}{m_SavedValue1:X2},X");
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
                    SBC(m_SavedValue2);
                    m_CurrentOpCodeCycle = 0;
                }
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.X));
                SBC(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
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
                Trace($"SBC ${m_SavedValue0:X2}{m_SavedValue1:X2},Y");
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
                    SBC(m_SavedValue2);
                    m_CurrentOpCodeCycle = 0;
                }
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                SBC(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
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
                Trace($"SBC (${m_SavedValue1:X2},X)");
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
                SBC(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
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
                Trace($"SBC (${m_SavedValue1:X2}),Y");
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
                    SBC(m_SavedValue2);
                    m_CurrentOpCodeCycle = 0;
                }
                break;
            default:
                m_SavedValue2 = m_Bus.Read(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y));
                SBC(m_SavedValue2);
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }

    private void SEC_impl()
    {
        _ = m_Bus.Read(m_Registers.PC);
        Trace($"SEC");
        SEC();
        m_CurrentOpCodeCycle = 0;
    }

    private void SED_impl()
    {
        _ = m_Bus.Read(m_Registers.PC);
        Trace($"SED");
        SED();
        m_CurrentOpCodeCycle = 0;
    }

    private void SEI_impl()
    {
        _ = m_Bus.Read(m_Registers.PC);
        Trace($"SEI");
        SEI();
        m_CurrentOpCodeCycle = 0;
    }

    private void SLO_zeropage()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                Trace($"SLO ${m_SavedValue0:X2}");
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
                Trace($"SLO ${m_SavedValue0:X2},X");
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
                Trace($"SLO ${m_SavedValue0:X2}{m_SavedValue1:X2}");
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
                Trace($"SLO ${m_SavedValue0:X2}{m_SavedValue1:X2},X");
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
                Trace($"SLO ${m_SavedValue0:X2}{m_SavedValue1:X2},Y");
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
                Trace($"SLO (${m_SavedValue1:X2},X)");
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
                Trace($"SLO (${m_SavedValue1:X2}),Y");
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

    private void SRE_zeropage()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                Trace($"SRE ${m_SavedValue0:X2}");
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
                m_Bus.Write(m_SavedValue0, SRE(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void SRE_zeropage_xindexed()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue0 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                Trace($"SRE ${m_SavedValue0:X2},X");
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
                m_Bus.Write(GetZeropageIndexedNoCarry(m_SavedValue0, m_Registers.X), SRE(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void SRE_absolute()
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
                Trace($"SRE ${m_SavedValue0:X2}{m_SavedValue1:X2}");
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
                m_Bus.Write(GetAbsolute(m_SavedValue0, m_SavedValue1), SRE(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void SRE_absolute_xindexed()
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
                Trace($"SRE ${m_SavedValue0:X2}{m_SavedValue1:X2},X");
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
                m_Bus.Write(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.X), SRE(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void SRE_absolute_yindexed()
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
                Trace($"SRE ${m_SavedValue0:X2}{m_SavedValue1:X2},Y");
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
                m_Bus.Write(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y), SRE(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void SRE_indirect_xindexed()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue1 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                Trace($"SRE (${m_SavedValue1:X2},X)");
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
                m_Bus.Write(GetAbsolute(m_SavedValue0, m_SavedValue1), SRE(m_SavedValue2));
                m_CurrentOpCodeCycle = 0;
                break;
        }
    }
    private void SRE_indirect_yindexed()
    {
        switch (m_CurrentOpCodeCycle)
        {
            case 1:
                m_SavedValue1 = m_Bus.Read(m_Registers.PC);
                ++m_Registers.PC;
                Trace($"SRE (${m_SavedValue1:X2}),Y");
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
                m_Bus.Write(GetAbsoluteIndexed(m_SavedValue0, m_SavedValue1, m_Registers.Y), SRE(m_SavedValue2));
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
                Trace($"STA ${m_SavedValue0:X2}");
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
                Trace($"STA ${m_SavedValue0:X2},X");
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
                Trace($"STA ${m_SavedValue0:X2}{m_SavedValue1:X2}");
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
                Trace($"STA ${m_SavedValue0:X2}{m_SavedValue1:X2},X");
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
                Trace($"STA ${m_SavedValue0:X2}{m_SavedValue1:X2},Y");
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
                Trace($"STA (${m_SavedValue1:X2},X)");
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
                Trace($"STA (${m_SavedValue1:X2}),Y");
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
                Trace($"STX ${m_SavedValue0:X2}");
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
                Trace($"STX ${m_SavedValue0:X2},Y");
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
                Trace($"STX ${m_SavedValue0:X2}{m_SavedValue1:X2}");
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
                Trace($"STY ${m_SavedValue0:X2}");
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
                Trace($"STY ${m_SavedValue0:X2},X");
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
                Trace($"STY ${m_SavedValue0:X2}{m_SavedValue1:X2}");
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
        Trace($"TAX");
        TAX();
        m_CurrentOpCodeCycle = 0;
    }

    private void TAY_impl()
    {
        _ = m_Bus.Read(m_Registers.PC);
        Trace($"TAY");
        TAY();
        m_CurrentOpCodeCycle = 0;
    }

    private void TSX_impl()
    {
        _ = m_Bus.Read(m_Registers.PC);
        Trace($"TSX");
        TSX();
        m_CurrentOpCodeCycle = 0;
    }

    private void TXA_impl()
    {
        _ = m_Bus.Read(m_Registers.PC);
        Trace($"TXA");
        TXA();
        m_CurrentOpCodeCycle = 0;
    }

    private void TXS_impl()
    {
        _ = m_Bus.Read(m_Registers.PC);
        Trace($"TXS");
        TXS();
        m_CurrentOpCodeCycle = 0;
    }

    private void TYA_impl()
    {
        _ = m_Bus.Read(m_Registers.PC);
        Trace($"TYA");
        TYA();
        m_CurrentOpCodeCycle = 0;
    }

}

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharptari.Lib;

public sealed class Atari2600
{
    private readonly Atari2600Rom m_Rom;
    private readonly IAtariInput m_Input;
    private readonly Mos6532Riot m_Riot;
    private readonly Atari2600Tia m_Tia;
    private readonly Atari2600Bus m_Bus;
    private readonly Mos6502Cpu<Atari2600Bus> m_Cpu;

    public Atari2600(byte[] romBytes, IAtariInput input)
    {
        m_Rom = new Atari2600Rom(romBytes);
        m_Input = input;
        m_Riot = new Mos6532Riot(m_Input);
        m_Tia = new Atari2600Tia(m_Input);
        m_Bus = new Atari2600Bus(m_Rom, m_Riot, m_Tia);
        m_Cpu = new Mos6502Cpu<Atari2600Bus>(m_Bus);
    }

    public void Reboot()
    {
        m_Cpu.Reboot();
        m_Bus.Reboot();
        m_Rom.Reboot();
        m_Riot.Reboot();
        m_Tia.Reboot();
    }

    public void Step()
    {
        if (m_Tia.WSync)
        {
            m_Cpu.StepHalted();
        }
        else
        {
            m_Cpu.Step();
        }

        m_Bus.Step();
        m_Rom.Step();
        m_Riot.Step();

        // The TIA is stepped three times per CPU step:
        m_Tia.Step();
        m_Tia.Step();
        m_Tia.Step();
    }

    public void StepFrame(ref ColorAbgr8888[] buffer, out int bufferLength)
    {
        while (!m_Tia.HasFrameReady)
        {
            Step();
        }

        var pixels = m_Tia.FramePixels;
        m_Tia.ClearFrameReady();

        if (buffer == null || pixels.Count > buffer.Length)
        {
            buffer = new ColorAbgr8888[pixels.Count];
        }

        // Copy the pixels to the screen bytes:
        for (int i = 0; i < pixels.Count; i++)
        {
            buffer[i] = pixels[i];
        }

        bufferLength = pixels.Count;
    }
}
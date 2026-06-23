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
    private const long CpuSpeed = 1193181;

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

    public TimeSpan StepFrame(ColorAbgr8888[] buffer)
    {
        int steps = 0;

        // 50000 is a reasonable upper bound on how many steps it should take to get a frame ready
        // if we hit that, just give up and leave the buffer alone

        while (!m_Tia.HasFrameReady && steps < 50000)
        {
            Step();
            ++steps;
        }

        if (m_Tia.HasFrameReady)
        {
            var pixels = m_Tia.FramePixels;
            m_Tia.ClearFrameReady();

            int i = 0;
            int iMax = Math.Min(buffer.Length, pixels.Count);

            // Copy the pixels to the screen bytes:
            for (i = 0; i < iMax; i++)
            {
                buffer[i] = pixels[i];
            }
            Array.Clear(buffer, i, buffer.Length - i);
        }

        return TimeSpan.FromSeconds((double)steps / CpuSpeed);
    }
}
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
    private const long CpuSpeed = 3579543;

    private readonly Atari2600Rom m_Rom;
    private readonly IAtariInput m_Input;
    private readonly Mos6532Riot m_Riot;
    private readonly Atari2600Tia m_Tia;
    private readonly Atari2600Bus m_Bus;
    private readonly Mos6502Cpu<Atari2600Bus> m_Cpu;
    private int m_SubClock;

    public Mos6502Registers DebugCpuRegisters => m_Cpu.Registers;
    public IReadOnlyList<byte> DebugRam => m_Riot.DebugRam;

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
        if (m_SubClock == 0)
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
        }

        // The TIA is stepped every sub clock:
        m_Tia.Step();

        ++m_SubClock;
        if (m_SubClock == 3)
        {
            m_SubClock = 0;
        }
    }

    public TimeSpan DebugStep(out bool didStepCpu)
    {
        didStepCpu = false;

        if (m_SubClock == 0)
        {
            if (m_Tia.WSync)
            {
                m_Cpu.StepHalted();
            }
            else
            {
                m_Cpu.Step();
                didStepCpu = true;
            }

            m_Bus.Step();
            m_Rom.Step();
            m_Riot.Step();
        }

        // The TIA is stepped every sub clock:
        m_Tia.Step();

        ++m_SubClock;
        if (m_SubClock == 3)
        {
            m_SubClock = 0;
        }

        return TimeSpan.FromSeconds(1.0 / CpuSpeed);
    }

    public TimeSpan DebugStepCpu()
    {
        int steps = 0;
        bool didStepCpu;

        do
        {
            DebugStep(out didStepCpu);
            ++steps;
        } while (!didStepCpu);

        return TimeSpan.FromSeconds((double)steps / CpuSpeed);
    }

    public TimeSpan DebugStepInstruction()
    {
        int steps = 0;
        bool didStepCpu;

        do
        {
            DebugStep(out didStepCpu);
            ++steps;
        }
        while (!didStepCpu || !m_Cpu.IsAtOpCodeStart);

        return TimeSpan.FromSeconds((double)steps / CpuSpeed);
    }

    public TimeSpan DebugStepScanline()
    {
        int steps = 0;

        do
        {
            DebugStep(out _);
            ++steps;
        }
        while (!m_Tia.IsAtStartOfScanline);

        return TimeSpan.FromSeconds((double)steps / CpuSpeed);
    }

    public TimeSpan StepFrame()
    {
        int steps = 0;

        // 150000 is a reasonable upper bound on how many steps it should take to get a frame ready
        // if we hit that, just give up and leave the buffer alone

        while (!m_Tia.HasFrameReady && steps < 150000)
        {
            Step();
            ++steps;
        }

        m_Tia.ClearFrameReady();

        return TimeSpan.FromSeconds((double)steps / CpuSpeed);
    }

    public TimeSpan DebugStepFrame()
    {
        int steps = 0;

        // 150000 is a reasonable upper bound on how many steps it should take to get a frame ready
        // if we hit that, just give up and leave the buffer alone

        while (!m_Tia.HasFrameReady && steps < 150000)
        {
            DebugStep(out _);
            ++steps;
        }

        m_Tia.ClearFrameReady();

        return TimeSpan.FromSeconds((double)steps / CpuSpeed);
    }

    public void ReadFrame(ColorAbgr8888[] buffer)
    {
        var pixelCount = m_Tia.CopyPixels(buffer);
        Array.Clear(buffer, pixelCount, buffer.Length - pixelCount);
    }

    public int ReadAudio0(byte[] buffer)
    {
        var sampleCount = m_Tia.ReadAudioSamples0(buffer);
        return sampleCount;
    }

    public int ReadAudio1(byte[] buffer)
    {
        var sampleCount = m_Tia.ReadAudioSamples1(buffer);
        return sampleCount;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharptari.Lib;

public sealed class Atari2600Tia
{
    private const ushort RegisterReadMask = 0b0000_0000_0000_1111;
    private const ushort RegisterWriteMask = 0b0000_0000_0011_1111;

    private const int ScanlineLength = 228;
    private const int HBlankLength = 68;

    private bool m_HasFrameReady = false;
    private List<byte> m_GeneratedPixels = [];
    private List<byte> m_PrevGeneratedPixels = [];
    private readonly byte[] m_CurrentScalinePixels = new byte[4 * ScanlineLength];
    private int m_CurrentScanline = 0;
    private int m_CurrentScanlineCycle = 0;

    private bool m_VBlank = false;
    private bool m_VSync = false;

    public bool HasFrameReady => m_HasFrameReady;
    public IReadOnlyList<byte> FramePixels => m_PrevGeneratedPixels;

    public void ClearFrameReady()
    {
        m_HasFrameReady = false;
    }

    public void Reboot()
    {
        m_GeneratedPixels.Clear();
        m_CurrentScanline = 0;
        m_CurrentScanlineCycle = 0;
        m_VBlank = true;
        m_VSync = false;
    }

    public void Step()
    {
        int i = 4 * m_CurrentScanlineCycle;

        // TODO: write a colour:
        if (m_VBlank || m_CurrentScanlineCycle < HBlankLength)
        {
            m_CurrentScalinePixels[i + 0] = 0;
            m_CurrentScalinePixels[i + 1] = 0;
            m_CurrentScalinePixels[i + 2] = 0;
            m_CurrentScalinePixels[i + 3] = 255;
        }
        else
        {
            // Random Magenta for now, to show that we're generating pixels:
            m_CurrentScalinePixels[i + 0] = 255;
            m_CurrentScalinePixels[i + 1] = 0;
            m_CurrentScalinePixels[i + 2] = 255;
            m_CurrentScalinePixels[i + 3] = 255;
        }

        ++m_CurrentScanlineCycle;
        if (m_CurrentScanlineCycle >= ScanlineLength)
        {
            m_GeneratedPixels.AddRange(m_CurrentScalinePixels);
            m_CurrentScanlineCycle = 0;
            ++m_CurrentScanline;
        }
    }

    public int TryRead(ushort address)
    {
        address &= RegisterReadMask;

        switch (address)
        {
            case 0x00:
                // CXM0P
                break;
            case 0x01:
                // CXM1P
                break;
            case 0x02:
                // CXP0FB
                break;
            case 0x03:
                // CXP1FB
                break;
            case 0x04:
                // CXM0FB
                break;
            case 0x05:
                // CXM1FB
                break;
            case 0x06:
                // CXBLPF
                break;
            case 0x07:
                // CXPPMM
                break;
            case 0x08:
                // INPT0
                break;
            case 0x09:
                // INPT1
                break;
            case 0x0a:
                // INPT2
                break;
            case 0x0b:
                // INPT3
                break;
            case 0x0c:
                // INPT4
                break;
            case 0x0d:
                // INPT5
                break;
        }

        return -1;
    }

    public void TryWrite(ushort address, byte value)
    {
        address &= RegisterWriteMask;

        switch (address)
        {
            case 0x00:
                // VSYNC
                SetVsync((value & 0b0000_0010) != 0);
                break;
            case 0x01:
                // VBLANK
                SetVBlank((value & 0b0000_0010) != 0);
                SetTriggerButtonLatches((value & 0b0100_0000) != 0);
                SetPaddleCapacitorDumpToGround((value & 0b1000_0000) != 0);
                break;
            case 0x02:
                // WSYNC
                break;
            case 0x03:
                // RSYNC
                break;
            case 0x04:
                // NUSIZ0
                break;
            case 0x05:
                // NUSIZ1
                break;
            case 0x06:
                // COLUP0
                break;
            case 0x07:
                // COLUP1
                break;
            case 0x08:
                // COLUPF
                break;
            case 0x09:
                // COLUBK
                break;
            case 0x0a:
                // CTRLPF
                break;
            case 0x0b:
                // REFP0
                break;
            case 0x0c:
                // REFP1
                break;
            case 0x0d:
                // PF0
                break;
            case 0x0e:
                // PF1
                break;
            case 0x0f:
                // PF2
                break;
            case 0x10:
                // RESP0
                break;
            case 0x11:
                // RESP1
                break;
            case 0x12:
                // RESM0
                break;
            case 0x13:
                // RESM1
                break;
            case 0x14:
                // RESBL
                break;
            case 0x15:
                // AUDC0
                break;
            case 0x16:
                // AUDC1
                break;
            case 0x17:
                // AUDF0
                break;
            case 0x18:
                // AUDF1
                break;
            case 0x19:
                // AUDV0
                break;
            case 0x1a:
                // AUDV1
                break;
            case 0x1b:
                // GRP0
                break;
            case 0x1c:
                // GRP1
                break;
            case 0x1d:
                // ENAM0
                break;
            case 0x1e:
                // ENAM1
                break;
            case 0x1f:
                // ENABL
                break;
            case 0x20:
                // HMP0
                break;
            case 0x21:
                // HMP1
                break;
            case 0x22:
                // HMM0
                break;
            case 0x23:
                // HMM1
                break;
            case 0x24:
                // HMBL
                break;
            case 0x25:
                // VDELP0
                break;
            case 0x26:
                // VDELP1
                break;
            case 0x27:
                // VDELBL
                break;
            case 0x28:
                // RESMP0
                break;
            case 0x29:
                // RESMP1
                break;
            case 0x2a:
                // HMOVE
                break;
            case 0x2b:
                // HMCLR
                break;
            case 0x2c:
                // CXCLR
                break;
        }
    }

    private void SetVsync(bool value)
    {
        var oldVSync = m_VSync;
        m_VSync = value;

        if (oldVSync != m_VSync)
        {
            Console.WriteLine($"VSYNC: {m_VSync} @ scanline {m_CurrentScanline}");
        }

        if (m_VSync && !oldVSync)
        {        
            m_HasFrameReady = true;
            (m_GeneratedPixels, m_PrevGeneratedPixels) = (m_PrevGeneratedPixels, m_GeneratedPixels);
            m_GeneratedPixels.Clear();
            m_CurrentScanline = 0;
        }
    }

    private void SetVBlank(bool value)
    {
        var oldVBlank = m_VBlank;
        m_VBlank = value;

        if (oldVBlank != m_VBlank)
        {
            Console.WriteLine($"VBLANK: {m_VBlank} @ scanline {m_CurrentScanline}");
        }
    }

    private void SetTriggerButtonLatches(bool value)
    {
        // TODO
    }

    private void SetPaddleCapacitorDumpToGround(bool value)
    {
        // TODO
    }
}
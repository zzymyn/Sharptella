using System;
using System.Collections.Generic;
using System.Drawing;
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
    private const int MaxScanlineCount = 512; // sensible upper bound to prevent unbounded memory usage in case of a bug

    private bool m_HasFrameReady = false;
    private List<ColorAbgr8888> m_GeneratedPixels = [];
    private List<ColorAbgr8888> m_PrevGeneratedPixels = [];
    private readonly ColorAbgr8888[] m_CurrentScalinePixels = new ColorAbgr8888[ScanlineLength];
    private int m_CurrentScanline = 0;
    private int m_CurrentScanlineCycle = 0;

    private bool m_WSYNC = false;
    private bool m_VBLANK = false;
    private bool m_VSYNC = false;
    private ColorAbgr8888 m_COLUBK = ColorAbgr8888.Black;

    public bool WSync => m_WSYNC;
    public bool HasFrameReady => m_HasFrameReady;
    public IReadOnlyList<ColorAbgr8888> FramePixels => m_PrevGeneratedPixels;

    public void ClearFrameReady()
    {
        m_HasFrameReady = false;
    }

    public void Reboot()
    {
        m_GeneratedPixels.Clear();
        m_CurrentScanline = 0;
        m_CurrentScanlineCycle = 0;
        m_VBLANK = true;
        m_VSYNC = false;
    }

    public void Step()
    {
        int i = m_CurrentScanlineCycle;

        // TODO: write a colour:
        if (m_VBLANK || m_CurrentScanlineCycle < HBlankLength)
        {
            m_CurrentScalinePixels[i] = ColorAbgr8888.Black;
        }
        else
        {
            // Random Magenta for now, to show that we're generating pixels:
            m_CurrentScalinePixels[i] = m_COLUBK;
        }

        ++m_CurrentScanlineCycle;
        if (m_CurrentScanlineCycle >= ScanlineLength)
        {
            if (m_CurrentScanline < MaxScanlineCount)
            {
                m_GeneratedPixels.AddRange(m_CurrentScalinePixels);
            }
            m_CurrentScanlineCycle = 0;
            ++m_CurrentScanline;
            m_WSYNC = false;
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
                m_WSYNC = true;
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
                m_COLUBK = DecodeYiq(value);
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
        var oldVSync = m_VSYNC;
        m_VSYNC = value;

        if (oldVSync != m_VSYNC)
        {
            Console.WriteLine($"VSYNC: {m_VSYNC} @ scanline {m_CurrentScanline}");
        }

        if (m_VSYNC && !oldVSync)
        {
            Console.WriteLine($"Frame ready with {m_CurrentScanline} scanlines");
            m_HasFrameReady = true;
            (m_GeneratedPixels, m_PrevGeneratedPixels) = (m_PrevGeneratedPixels, m_GeneratedPixels);
            m_GeneratedPixels.Clear();
            m_CurrentScanline = 0;
        }
    }

    private void SetVBlank(bool value)
    {
        var oldVBlank = m_VBLANK;
        m_VBLANK = value;

        if (oldVBlank != m_VBLANK)
        {
            Console.WriteLine($"VBLANK: {m_VBLANK} @ scanline {m_CurrentScanline}");
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

    private ColorAbgr8888 DecodeYiq(byte yiq)
    {
        int hue = (yiq & 0b1111_0000) >> 4;
        int lum = (yiq & 0b0000_1110) >> 1;

        float y = lum / 7.0f;

        byte r, g, b;

        if (hue == 0)
        {
            // Grayscale mode (I and Q are 0)
            byte grayscale = (byte)Math.Clamp(y * 255.0f, 0.0f, 255.0f);
            r = grayscale;
            g = grayscale;
            b = grayscale;
        }
        else
        {
            // Invert the hue direction to match the NTSC color wheel sweep
            float h = 14.0f - hue;

            // Base angle per hue step (360 degrees / 15 steps = 24 degrees)
            float phiHue = h * (360.0f / 15.0f);

            // Color burst reference angle (180 degrees)
            const float phiBurst = 180.0f;

            float phi = phiHue + phiBurst;
            float radians = phi * (MathF.PI / 180.0f);

            // Saturation factor (chroma gain)
            const float chromaGain = 0.5f;

            float i = y * chromaGain * MathF.Sin(radians);
            float q = y * chromaGain * MathF.Cos(radians);

            // Convert YIQ to RGB using 1953 NTSC matrix coefficients
            float rFloat = y + 0.956f * i + 0.621f * q;
            float gFloat = y - 0.272f * i - 0.647f * q;
            float bFloat = y - 1.106f * i + 1.703f * q;

            // Clamp to byte range (0-255)
            r = (byte)Math.Clamp(rFloat * 255f, 0f, 255f);
            g = (byte)Math.Clamp(gFloat * 255f, 0f, 255f);
            b = (byte)Math.Clamp(bFloat * 255f, 0f, 255f);
        }

        return new (r, g, b, 255);
    }
}
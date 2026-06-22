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
    private const int HBlankLengthHMoved = 76;
    private const int MaxScanlineCount = 512; // sensible upper bound to prevent unbounded memory usage in case of a bug

    private static readonly ColorAbgr8888[] YiqToColorLut = new ColorAbgr8888[256];

    private bool m_HasFrameReady = false;
    private List<ColorAbgr8888> m_GeneratedPixels = [];
    private List<ColorAbgr8888> m_PrevGeneratedPixels = [];
    private readonly ColorAbgr8888[] m_CurrentScalinePixels = new ColorAbgr8888[ScanlineLength];
    private int m_CurrentScanline = 0;
    private int m_CurrentScanlineCycle = 0;

    private int m_CurrentBallX = 0;

    private bool m_WSync = false;
    private bool m_VBlank = false;
    private bool m_VSync = false;
    private bool m_HMoved = false;
    private ColorAbgr8888 m_Player0Color = ColorAbgr8888.Black;
    private ColorAbgr8888 m_Player1Color = ColorAbgr8888.Black;
    private ColorAbgr8888 m_PlayfieldColor = ColorAbgr8888.Black;
    private ColorAbgr8888 m_BackgroundColor = ColorAbgr8888.Black;
    private bool m_PlayfieldReflect = false;
    private bool m_PlayfieldScoreMode = false;
    private bool m_PlayfieldPriority = false;
    private byte m_BallSize = 0;
    private bool[] m_Playfield = new bool[20];
    private bool m_EnableBall;
    private byte m_HorzontalMotionBall;
    private bool m_VerticalDelayBall;

    public bool WSync => m_WSync;
    public bool HasFrameReady => m_HasFrameReady;
    public IReadOnlyList<ColorAbgr8888> FramePixels => m_PrevGeneratedPixels;

    static Atari2600Tia()
    {
        for (var i = 0; i < 256; ++i)
        {
            YiqToColorLut[i] = DecodeYiq((byte)i);
        }
    }

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
        int i = m_CurrentScanlineCycle;

        if (m_VBlank || m_CurrentScanlineCycle < HBlankLength)
        {
            m_CurrentScalinePixels[i] = ColorAbgr8888.Black;
        }
        else
        {
            if (m_HMoved && m_CurrentScanlineCycle < HBlankLengthHMoved)
            {
                m_CurrentScalinePixels[i] = ColorAbgr8888.Black;
            }
            else
            {
                m_CurrentScalinePixels[i] = m_BackgroundColor;

                var playfieldIndex = (i - HBlankLength) / 4;
                var playfieldColor = m_PlayfieldColor;
                if (m_PlayfieldScoreMode)
                {
                    playfieldColor = m_Player0Color;
                }

                if (playfieldIndex >= 20)
                {
                    playfieldIndex -= 20;
                    if (m_PlayfieldReflect)
                    {
                        playfieldIndex = 19 - playfieldIndex;
                    }

                    if (m_PlayfieldScoreMode)
                    {
                        playfieldColor = m_Player1Color;
                    }
                }


                if (m_Playfield[playfieldIndex])
                {
                    m_CurrentScalinePixels[i] = playfieldColor;
                }

                if (m_EnableBall)
                {
                    int ballWidth = (1 << m_BallSize);
                    if (m_CurrentBallX < ballWidth)
                    {
                        m_CurrentScalinePixels[i] = m_PlayfieldColor;
                    }
                }
            }

            m_CurrentBallX = (m_CurrentBallX + 1) % 160;
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
            m_WSync = false;
            m_HMoved = false;
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
                m_WSync = true;
                break;
            case 0x03:
                // RSYNC
                m_CurrentScanlineCycle = ScanlineLength - 1;
                break;
            case 0x04:
                // NUSIZ0
                break;
            case 0x05:
                // NUSIZ1
                break;
            case 0x06:
                // COLUP0
                m_Player0Color = YiqToColorLut[value];
                break;
            case 0x07:
                // COLUP1
                m_Player1Color = YiqToColorLut[value];
                break;
            case 0x08:
                // COLUPF
                m_PlayfieldColor = YiqToColorLut[value];
                break;
            case 0x09:
                // COLUBK
                m_BackgroundColor = YiqToColorLut[value];
                break;
            case 0x0a:
                // CTRLPF
                m_PlayfieldReflect = (value & 0b0000_0001) != 0;
                m_PlayfieldScoreMode = (value & 0b0000_0010) != 0;
                m_PlayfieldPriority = (value & 0b0000_0100) != 0;
                m_BallSize = (byte)((value & 0b0011_0000) >> 4);
                break;
            case 0x0b:
                // REFP0
                break;
            case 0x0c:
                // REFP1
                break;
            case 0x0d:
                // PF0
                m_Playfield[0] = (value & 0b0001_0000) != 0;
                m_Playfield[1] = (value & 0b0010_0000) != 0;
                m_Playfield[2] = (value & 0b0100_0000) != 0;
                m_Playfield[3] = (value & 0b1000_0000) != 0;
                break;
            case 0x0e:
                // PF1
                m_Playfield[4] = (value & 0b1000_0000) != 0;
                m_Playfield[5] = (value & 0b0100_0000) != 0;
                m_Playfield[6] = (value & 0b0010_0000) != 0;
                m_Playfield[7] = (value & 0b0001_0000) != 0;
                m_Playfield[8] = (value & 0b0000_1000) != 0;
                m_Playfield[9] = (value & 0b0000_0100) != 0;
                m_Playfield[10] = (value & 0b0000_0010) != 0;
                m_Playfield[11] = (value & 0b0000_0001) != 0;
                break;
            case 0x0f:
                // PF2
                m_Playfield[12] = (value & 0b0000_0001) != 0;
                m_Playfield[13] = (value & 0b0000_0010) != 0;
                m_Playfield[14] = (value & 0b0000_0100) != 0;
                m_Playfield[15] = (value & 0b0000_1000) != 0;
                m_Playfield[16] = (value & 0b0001_0000) != 0;
                m_Playfield[17] = (value & 0b0010_0000) != 0;
                m_Playfield[18] = (value & 0b0100_0000) != 0;
                m_Playfield[19] = (value & 0b1000_0000) != 0;
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
                m_CurrentBallX = 155;
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
                m_EnableBall = (value & 0b0000_0010) != 0;
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
                m_HorzontalMotionBall = (byte)((value & 0b1111_0000) >> 4);
                break;
            case 0x25:
                // VDELP0
                break;
            case 0x26:
                // VDELP1
                break;
            case 0x27:
                // VDELBL
                m_VerticalDelayBall = (value & 0b0000_0001) != 0;
                break;
            case 0x28:
                // RESMP0
                break;
            case 0x29:
                // RESMP1
                break;
            case 0x2a:
                // HMOVE
                m_CurrentBallX = ApplyHMove(m_CurrentBallX, m_HorzontalMotionBall);
                m_HMoved = true;
                break;
            case 0x2b:
                // HMCLR
                m_HorzontalMotionBall = 0;
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
            Console.WriteLine($"Frame ready with {m_CurrentScanline} scanlines");
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

    private static int ApplyHMove(int x, byte hmove)
    {
        if (hmove >= 8)
        {
            return (x + 144 + hmove) % 160;
        }
        else
        {
            return (x + hmove) % 160;
        }
    }

    private static ColorAbgr8888 DecodeYiq(byte yiq)
    {
        int hue = (yiq & 0b1111_0000) >> 4;
        int lum = (yiq & 0b0000_1110) >> 1;

        if (hue == 0)
        {
            float y = lum / 7.0f;
            byte grayscale = (byte)Math.Clamp(y * 255.0f, 0.0f, 255.0f);
            return new(grayscale, grayscale, grayscale, 255);
        }
        else
        {
            float y = Lerp(0.2f, 0.85f, lum / 7.0f);

            float hueDegs = 156.0f - 24.0f * hue;
            float hueRads = hueDegs * (MathF.PI / 180.0f);

            const float saturation = 0.5f;

            float i = y * saturation * MathF.Sin(hueRads);
            float q = y * saturation * MathF.Cos(hueRads);

            float rFloat = y + 0.956f * i + 0.621f * q;
            float gFloat = y - 0.272f * i - 0.647f * q;
            float bFloat = y - 1.106f * i + 1.703f * q;

            // Clamp to byte range (0-255)
            byte r = (byte)Math.Clamp(rFloat * 255.0f, 0.0f, 255.0f);
            byte g = (byte)Math.Clamp(gFloat * 255.0f, 0.0f, 255.0f);
            byte b = (byte)Math.Clamp(bFloat * 255.0f, 0.0f, 255.0f);

            return new(r, g, b, 255);
        }
    }

    private static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * Math.Clamp(t, 0.0f, 1.0f);
    }
}
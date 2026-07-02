using System;

namespace Sharptella.Lib;

public sealed class Atari2600Tia
{
    private const ushort RegisterReadMask = 0b0000_0000_0000_1111;
    private const ushort RegisterWriteMask = 0b0000_0000_0011_1111;

    private const int ScanlineLength = 228;
    private const int HBlankLength = 68;
    private const int HBlankLengthHMoved = 76;
    private const int MaxScanlineCount = 512; // sensible upper bound to prevent unbounded memory usage in case of a bug

    private const double CapacitorDischargeRate = 125.0;
    private const double CapacitorChargeRateMin = 58000.0;
    private const double CapacitorChargeRateMax = 3.0;

    private static readonly ColorAbgr8888[] YiqToColorLut = new ColorAbgr8888[256];

    private readonly IAtariInput m_Input;

    private bool m_HasFrameReady = false;
    private readonly ColorAbgr8888[] m_GeneratedPixels = new ColorAbgr8888[ScanlineLength * MaxScanlineCount];
    private int m_CurrentScanline = 0;
    private int m_CurrentScanlineCycle = 0;

    private bool m_WSync = false;
    private bool m_VBlank = false;
    private bool m_VSync = false;
    private bool m_HMoved = false;
    private ColorAbgr8888 m_PlayfieldColor = ColorAbgr8888.Black;
    private ColorAbgr8888 m_BackgroundColor = ColorAbgr8888.Black;

    private bool m_PlayfieldReflect = false;
    private bool m_PlayfieldScoreMode = false;
    private bool m_PlayfieldPriority = false;
    private bool[] m_Playfield = new bool[20];

    private bool m_EnableBallCurr;
    private bool m_EnableBallPrev;
    private int m_BallSize;
    private bool m_VerticalDelayBall;
    private PositionCounter m_BallPositionCounter;

    private PlayerMissile m_PlayerMissile0;
    private PlayerMissile m_PlayerMissile1;

    private bool m_CxM0P0;
    private bool m_CxM0P1;
    private bool m_CxM1P0;
    private bool m_CxM1P1;
    private bool m_CxP0PF;
    private bool m_CxP0BL;
    private bool m_CxP1PF;
    private bool m_CxP1BL;
    private bool m_CxM0PF;
    private bool m_CxM0BL;
    private bool m_CxM1PF;
    private bool m_CxM1BL;
    private bool m_CxBLPF;
    private bool m_CxP0P1;
    private bool m_CxM0M1;

    private bool m_LatchesEnabled;
    private bool m_P0Latch;
    private bool m_P1Latch;

    private bool m_DumpPaddlesToGround;
    private double m_CapacitorCharge0;
    private double m_CapacitorCharge1;
    private double m_CapacitorCharge2;
    private double m_CapacitorCharge3;

    private AudioChannel m_AudioChannel0 = new();
    private AudioChannel m_AudioChannel1 = new();

    public bool WSync => m_WSync;
    public bool HasFrameReady => m_HasFrameReady;
    public bool IsAtStartOfScanline => m_CurrentScanlineCycle == 0;

    static Atari2600Tia()
    {
        for (var i = 0; i < 256; ++i)
        {
            YiqToColorLut[i] = DecodeYiq((byte)i);
        }
    }

    public Atari2600Tia(IAtariInput atariInput)
    {
        m_Input = atariInput;
    }

    public void ClearFrameReady()
    {
        m_HasFrameReady = false;
    }

    public int CopyPixels(Span<ColorAbgr8888> destination)
    {
        int count = Math.Min(destination.Length, m_GeneratedPixels.Length);
        m_GeneratedPixels.AsSpan(0, count).CopyTo(destination);
        return count;
    }

    public int ReadAudioSamples0(Span<byte> destination)
    {
        return m_AudioChannel0.ReadAudioSamples(destination);
    }

    public int ReadAudioSamples1(Span<byte> destination)
    {
        return m_AudioChannel1.ReadAudioSamples(destination);
    }

    public void Reboot()
    {
        Array.Clear(m_GeneratedPixels, 0, m_GeneratedPixels.Length);
        m_CurrentScanline = 0;
        m_CurrentScanlineCycle = 0;
        m_VBlank = true;
        m_VSync = false;
        m_AudioChannel0.Reboot();
        m_AudioChannel1.Reboot();
    }

    public void Step()
    {
        int i = m_CurrentScanlineCycle;
        var sl0 = Math.Min(m_CurrentScanline, MaxScanlineCount - 1) * ScanlineLength;
        var currentScanline = m_GeneratedPixels.AsSpan(sl0, ScanlineLength);

        var hBlank = m_CurrentScanlineCycle < HBlankLength;

        if (m_VBlank
            || hBlank
            || m_HMoved && m_CurrentScanlineCycle < HBlankLengthHMoved)
        {
            currentScanline[i] = ColorAbgr8888.Zero;
        }
        else
        {
            currentScanline[i] = m_BackgroundColor;

            bool drewPF = false;
            bool drewBL = false;
            bool drewP0 = false;
            bool drewP1 = false;
            bool drewM0 = false;
            bool drewM1 = false;

            if (m_PlayfieldPriority)
            {
                drewM1 = DrawMissile(in m_PlayerMissile1, currentScanline, i);
                drewP1 = DrawPlayer(in m_PlayerMissile1, currentScanline, i);
                drewM0 = DrawMissile(in m_PlayerMissile0, currentScanline, i);
                drewP0 = DrawPlayer(in m_PlayerMissile0, currentScanline, i);
                drewPF = DrawPlayfield(currentScanline, i);
                drewBL = DrawBall(currentScanline, i);
            }
            else
            {
                drewPF = DrawPlayfield(currentScanline, i);
                drewBL = DrawBall(currentScanline, i);
                drewM1 = DrawMissile(in m_PlayerMissile1, currentScanline, i);
                drewP1 = DrawPlayer(in m_PlayerMissile1, currentScanline, i);
                drewM0 = DrawMissile(in m_PlayerMissile0, currentScanline, i);
                drewP0 = DrawPlayer(in m_PlayerMissile0, currentScanline, i);
            }

            m_CxM0P0 |= drewM0 && drewP0;
            m_CxM0P1 |= drewM0 && drewP1;
            m_CxM1P0 |= drewM1 && drewP0;
            m_CxM1P1 |= drewM1 && drewP1;
            m_CxP0PF |= drewP0 && drewPF;
            m_CxP0BL |= drewP0 && drewBL;
            m_CxP1PF |= drewP1 && drewPF;
            m_CxP1BL |= drewP1 && drewBL;
            m_CxM0PF |= drewM0 && drewPF;
            m_CxM0BL |= drewM0 && drewBL;
            m_CxM1PF |= drewM1 && drewPF;
            m_CxM1BL |= drewM1 && drewBL;
            m_CxBLPF |= drewBL && drewPF;
            m_CxP0P1 |= drewP0 && drewP1;
            m_CxM0M1 |= drewM0 && drewM1;
        }

        m_BallPositionCounter.StepX(hBlank);
        m_PlayerMissile0.StepX(hBlank);
        m_PlayerMissile1.StepX(hBlank);

        if (m_CurrentScanlineCycle == 0 || m_CurrentScanlineCycle == 114)
        {
            m_AudioChannel0.Step();
            m_AudioChannel1.Step();
        }

        ++m_CurrentScanlineCycle;
        if (m_CurrentScanlineCycle >= ScanlineLength)
        {
            m_CurrentScanlineCycle = 0;
            ++m_CurrentScanline;
            m_WSync = false;
            m_HMoved = false;
        }

        if (m_LatchesEnabled)
        {
            m_P0Latch |= m_Input.Player0Button;
            m_P1Latch |= m_Input.Player1Button;
        }
        else
        {
            m_P0Latch = false;
            m_P1Latch = false;
        }

        if (m_DumpPaddlesToGround)
        {
            m_CapacitorCharge0 = Math.Max(0.0f, m_CapacitorCharge0 - 1.0 / CapacitorDischargeRate);
            m_CapacitorCharge1 = Math.Max(0.0f, m_CapacitorCharge1 - 1.0 / CapacitorDischargeRate);
            m_CapacitorCharge2 = Math.Max(0.0f, m_CapacitorCharge2 - 1.0 / CapacitorDischargeRate);
            m_CapacitorCharge3 = Math.Max(0.0f, m_CapacitorCharge3 - 1.0 / CapacitorDischargeRate);
        }
        else
        {
            m_CapacitorCharge0 = Math.Min(1.0f, m_CapacitorCharge0 + 1.0 / MathEx.Lerp(CapacitorChargeRateMin, CapacitorChargeRateMax, m_Input.Player0Paddle0));
            m_CapacitorCharge1 = Math.Min(1.0f, m_CapacitorCharge1 + 1.0 / MathEx.Lerp(CapacitorChargeRateMin, CapacitorChargeRateMax, m_Input.Player0Paddle1));
            m_CapacitorCharge2 = Math.Min(1.0f, m_CapacitorCharge2 + 1.0 / MathEx.Lerp(CapacitorChargeRateMin, CapacitorChargeRateMax, m_Input.Player1Paddle0));
            m_CapacitorCharge3 = Math.Min(1.0f, m_CapacitorCharge3 + 1.0 / MathEx.Lerp(CapacitorChargeRateMin, CapacitorChargeRateMax, m_Input.Player1Paddle1));
        }
    }

    private bool DrawPlayfield(Span<ColorAbgr8888> currentScanline, int i)
    {
        var playfieldIndex = (i - HBlankLength) / 4;
        var playfieldColor = m_PlayfieldColor;
        if (m_PlayfieldScoreMode)
        {
            playfieldColor = m_PlayerMissile0.Color;
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
                playfieldColor = m_PlayerMissile1.Color;
            }
        }

        if (m_Playfield[playfieldIndex])
        {
            currentScanline[i] = playfieldColor;
            return true;
        }

        return false;
    }

    private bool DrawPlayer(in PlayerMissile pm, Span<ColorAbgr8888> currentScanline, int i)
    {
        byte bitmap = pm.VerticalDelay ? pm.BitmapPrev : pm.BitmapCurr;

        if (bitmap != 0)
        {
            int copy = pm.PositionCounter.CurrentX / pm.Spacing;
            int pixel = (pm.PositionCounter.CurrentX / pm.Size) % pm.Spacing;

            if (pixel < 8 && copy < pm.Copies)
            {
                if (pm.Reflect)
                {
                    if ((bitmap & (0b0000_0001 << pixel)) != 0)
                    {
                        currentScanline[i] = pm.Color;
                        return true;
                    }
                }
                else
                {
                    if ((bitmap & (0b1000_0000 >> pixel)) != 0)
                    {
                        currentScanline[i] = pm.Color;
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private bool DrawMissile(in PlayerMissile pm, Span<ColorAbgr8888> currentScanline, int i)
    {
        if (pm.EnableMissile && !pm.MissileLocked)
        {
            int copy = pm.MissilePositionCounter.CurrentX / pm.Spacing;
            int pixel = pm.MissilePositionCounter.CurrentX % pm.Spacing;

            if (pixel < pm.MissileSize && copy < pm.Copies)
            {
                currentScanline[i] = pm.Color;
                return true;
            }
        }

        return false;
    }

    private bool DrawBall(Span<ColorAbgr8888> currentScanline, int i)
    {
        var drawBall = m_VerticalDelayBall ? m_EnableBallPrev : m_EnableBallCurr;
        if (drawBall)
        {
            if (m_BallPositionCounter.CurrentX < m_BallSize)
            {
                currentScanline[i] = m_PlayfieldColor;
                return true;
            }
        }

        return false;
    }

    public int TryRead(ushort address)
    {
        address &= RegisterReadMask;

        switch (address)
        {
            case 0x00:
                // CXM0P
                return GetCx(m_CxM0P1, m_CxM0P0);
            case 0x01:
                // CXM1P
                return GetCx(m_CxM1P0, m_CxM1P1);
            case 0x02:
                // CXP0FB
                return GetCx(m_CxP0PF, m_CxP0BL);
            case 0x03:
                // CXP1FB
                return GetCx(m_CxP1PF, m_CxP1BL);
            case 0x04:
                // CXM0FB
                return GetCx(m_CxM0PF, m_CxM0BL);
            case 0x05:
                // CXM1FB
                return GetCx(m_CxM1PF, m_CxM1BL);
            case 0x06:
                // CXBLPF
                return GetCx(m_CxBLPF, false);
            case 0x07:
                // CXPPMM
                return GetCx(m_CxP0P1, m_CxM0M1);
            case 0x08:
                // INPT0
                return (m_CapacitorCharge0 >= 1.0) ? 0x80 : 0x00;
            case 0x09:
                // INPT1
                return (m_CapacitorCharge1 >= 1.0) ? 0x80 : 0x00;
            case 0x0a:
                // INPT2
                return (m_CapacitorCharge2 >= 1.0) ? 0x80 : 0x00;
            case 0x0b:
                // INPT3
                return (m_CapacitorCharge3 >= 1.0) ? 0x80 : 0x00;
            case 0x0c:
                // INPT4
                if (m_LatchesEnabled)
                {
                    return m_P0Latch ? 0b0000_0000 : 0b1000_0000;
                }
                else
                {
                    return m_Input.Player0Button ? 0b0000_0000 : 0b1000_0000;
                }
            case 0x0d:
                // INPT5
                if (m_LatchesEnabled)
                {
                    return m_P1Latch ? 0b0000_0000 : 0b1000_0000;
                }
                else
                {
                    return m_Input.Player1Button ? 0b0000_0000 : 0b1000_0000;
                }
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
                m_LatchesEnabled = (value & 0b0100_0000) != 0;
                m_DumpPaddlesToGround = (value & 0b1000_0000) != 0;
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
                m_PlayerMissile0.SetNusiz(value);
                break;
            case 0x05:
                // NUSIZ1
                m_PlayerMissile1.SetNusiz(value);
                break;
            case 0x06:
                // COLUP0
                m_PlayerMissile0.Color = YiqToColorLut[value];
                break;
            case 0x07:
                // COLUP1
                m_PlayerMissile1.Color = YiqToColorLut[value];
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
                m_BallSize = 1 << ((value & 0b0011_0000) >> 4);
                break;
            case 0x0b:
                // REFP0
                m_PlayerMissile0.Reflect = (value & 0b0000_1000) != 0;
                break;
            case 0x0c:
                // REFP1
                m_PlayerMissile1.Reflect = (value & 0b0000_1000) != 0;
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
                m_PlayerMissile0.PositionCounter.Reset(3, 157);
                break;
            case 0x11:
                // RESP1
                m_PlayerMissile1.PositionCounter.Reset(3, 157);
                break;
            case 0x12:
                // RESM0
                m_PlayerMissile0.MissilePositionCounter.Reset(3, 158);
                break;
            case 0x13:
                // RESM1
                m_PlayerMissile1.MissilePositionCounter.Reset(3, 158);
                break;
            case 0x14:
                // RESBL
                m_BallPositionCounter.Reset(3, 158);
                break;
            case 0x15:
                // AUDC0
                m_AudioChannel0.Control = value & 0b0000_1111;
                break;
            case 0x16:
                // AUDC1
                m_AudioChannel1.Control = value & 0b0000_1111;
                break;
            case 0x17:
                // AUDF0
                m_AudioChannel0.Frequency = value & 0b0001_1111;
                break;
            case 0x18:
                // AUDF1
                m_AudioChannel1.Frequency = value & 0b0001_1111;
                break;
            case 0x19:
                // AUDV0
                m_AudioChannel0.Volume = value & 0b0000_1111;
                break;
            case 0x1a:
                // AUDV1
                m_AudioChannel1.Volume = value & 0b0000_1111;
                break;
            case 0x1b:
                // GRP0
                m_PlayerMissile0.BitmapCurr = value;
                m_PlayerMissile1.BitmapPrev = m_PlayerMissile1.BitmapCurr;
                break;
            case 0x1c:
                // GRP1
                m_PlayerMissile1.BitmapCurr = value;
                m_PlayerMissile0.BitmapPrev = m_PlayerMissile0.BitmapCurr;
                m_EnableBallPrev = m_EnableBallCurr;
                break;
            case 0x1d:
                // ENAM0
                m_PlayerMissile0.EnableMissile = (value & 0b0000_0010) != 0;
                break;
            case 0x1e:
                // ENAM1
                m_PlayerMissile1.EnableMissile = (value & 0b0000_0010) != 0;
                break;
            case 0x1f:
                // ENABL
                m_EnableBallCurr = (value & 0b0000_0010) != 0;
                break;
            case 0x20:
                // HMP0
                m_PlayerMissile0.PositionCounter.HorzontalMotion = (value & 0b1111_0000) >> 4;
                break;
            case 0x21:
                // HMP1
                m_PlayerMissile1.PositionCounter.HorzontalMotion = (value & 0b1111_0000) >> 4;
                break;
            case 0x22:
                // HMM0
                m_PlayerMissile0.MissilePositionCounter.HorzontalMotion = (value & 0b1111_0000) >> 4;
                break;
            case 0x23:
                // HMM1
                m_PlayerMissile1.MissilePositionCounter.HorzontalMotion = (value & 0b1111_0000) >> 4;
                break;
            case 0x24:
                // HMBL
                m_BallPositionCounter.HorzontalMotion = (value & 0b1111_0000) >> 4;
                break;
            case 0x25:
                // VDELP0
                m_PlayerMissile0.VerticalDelay = (value & 0b0000_0001) != 0;
                break;
            case 0x26:
                // VDELP1
                m_PlayerMissile1.VerticalDelay = (value & 0b0000_0001) != 0;
                break;
            case 0x27:
                // VDELBL
                m_VerticalDelayBall = (value & 0b0000_0001) != 0;
                break;
            case 0x28:
                // RESMP0
                m_PlayerMissile0.MissileLocked = (value & 0b0000_0010) != 0;
                break;
            case 0x29:
                // RESMP1
                m_PlayerMissile1.MissileLocked = (value & 0b0000_0010) != 0;
                break;
            case 0x2a:
                // HMOVE
                m_BallPositionCounter.ApplyHMove();
                m_PlayerMissile0.PositionCounter.ApplyHMove();
                m_PlayerMissile1.PositionCounter.ApplyHMove();
                m_PlayerMissile0.MissilePositionCounter.ApplyHMove();
                m_PlayerMissile1.MissilePositionCounter.ApplyHMove();
                m_HMoved = true;
                break;
            case 0x2b:
                // HMCLR
                m_BallPositionCounter.HorzontalMotion = 0;
                m_PlayerMissile0.PositionCounter.HorzontalMotion = 0;
                m_PlayerMissile1.PositionCounter.HorzontalMotion = 0;
                m_PlayerMissile0.MissilePositionCounter.HorzontalMotion = 0;
                m_PlayerMissile1.MissilePositionCounter.HorzontalMotion = 0;
                break;
            case 0x2c:
                // CXCLR
                m_CxM0P0 = false;
                m_CxM0P1 = false;
                m_CxM1P0 = false;
                m_CxM1P1 = false;
                m_CxP0PF = false;
                m_CxP0BL = false;
                m_CxP1PF = false;
                m_CxP1BL = false;
                m_CxM0PF = false;
                m_CxM0BL = false;
                m_CxM1PF = false;
                m_CxM1BL = false;
                m_CxBLPF = false;
                m_CxP0P1 = false;
                m_CxM0M1 = false;
                break;
        }
    }

    private void SetVsync(bool value)
    {
        var oldVSync = m_VSync;
        m_VSync = value;

        if (oldVSync != m_VSync)
        {
            //Console.WriteLine($"VSYNC: {m_VSync} @ scanline {m_CurrentScanline}");
        }

        if (m_VSync && !oldVSync)
        {
            // zero out the rest of the frame buffer:
            var slBegin = Math.Min(m_CurrentScanline, MaxScanlineCount - 1) * ScanlineLength;
            Array.Clear(m_GeneratedPixels, slBegin, m_GeneratedPixels.Length - slBegin);

            //Console.WriteLine($"Frame ready with {m_CurrentScanline} scanlines");

            m_HasFrameReady = true;
            m_CurrentScanline = 0;
        }
    }

    private void SetVBlank(bool value)
    {
        var oldVBlank = m_VBlank;
        m_VBlank = value;

        if (oldVBlank != m_VBlank)
        {
            //Console.WriteLine($"VBLANK: {m_VBlank} @ scanline {m_CurrentScanline}");
        }
    }

    private static byte GetCx(bool b7, bool b6)
    {
        byte result = 0;
        if (b7)
        {
            result |= 0b1000_0000;
        }
        if (b6)
        {
            result |= 0b0100_0000;
        }
        return result;
    }

    private struct PositionCounter
    {
        public int ResetDelay;
        public int ResetValue;
        public int CurrentX;
        public int HorzontalMotion;

        public void Reset(int delay, int value)
        {
            ResetDelay = delay;
            ResetValue = value;
        }

        public void ApplyHMove()
        {
            if (HorzontalMotion >= 8)
            {
                CurrentX = (CurrentX + 144 + HorzontalMotion) % 160;
            }
            else
            {
                CurrentX = (CurrentX + HorzontalMotion) % 160;
            }
        }

        public void StepX(bool hBlank)
        {
            if (!hBlank)
            {
                CurrentX = (CurrentX + 1) % 160;
            }

            if (ResetDelay > 0)
            {
                --ResetDelay;
                if (ResetDelay == 0)
                {
                    CurrentX = ResetValue;
                }
            }
        }

        public void LockX(int value)
        {
            CurrentX = value % 160;
        }
    }

    private struct PlayerMissile
    {
        public ColorAbgr8888 Color;
        public byte BitmapCurr;
        public byte BitmapPrev;
        public bool Reflect;
        public int Copies;
        public int Spacing;
        public int Size;
        public bool VerticalDelay;
        public PositionCounter PositionCounter;

        public bool EnableMissile;
        public int MissileSize;
        public bool MissileLocked;
        public PositionCounter MissilePositionCounter;

        public void SetNusiz(byte value)
        {
            switch (value & 0b0000_0111)
            {
                case 0:
                    Copies = 1;
                    Spacing = 64;
                    Size = 1;
                    break;
                case 1:
                    Copies = 2;
                    Spacing = 16;
                    Size = 1;
                    break;
                case 2:
                    Copies = 2;
                    Spacing = 32;
                    Size = 1;
                    break;
                case 3:
                    Copies = 3;
                    Spacing = 16;
                    Size = 1;
                    break;
                case 4:
                    Copies = 2;
                    Spacing = 64;
                    Size = 1;
                    break;
                case 5:
                    Copies = 1;
                    Spacing = 64;
                    Size = 2;
                    break;
                case 6:
                    Copies = 3;
                    Spacing = 32;
                    Size = 1;
                    break;
                case 7:
                    Copies = 1;
                    Spacing = 64;
                    Size = 4;
                    break;
            }

            MissileSize = 1 << ((value & 0b0011_0000) >> 4);
        }

        public void StepX(bool hBlank)
        {
            PositionCounter.StepX(hBlank);
            if (MissileLocked)
            {
                MissilePositionCounter.LockX(PositionCounter.CurrentX + 4);
            }
            else
            {
                MissilePositionCounter.StepX(hBlank);
            }
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
            float y = MathEx.Lerp(0.2f, 0.85f, lum / 7.0f);

            float hueDegs = 156.0f - 24.0f * hue;
            float hueRads = hueDegs * (MathF.PI / 180.0f);

            const float saturation = 0.5f;

            float i = y * saturation * MathF.Sin(hueRads);
            float q = y * saturation * MathF.Cos(hueRads);

            float rFloat = y + 0.956f * i + 0.621f * q;
            float gFloat = y - 0.272f * i - 0.647f * q;
            float bFloat = y - 1.106f * i + 1.703f * q;

            byte r = (byte)Math.Clamp(rFloat * 255.0f, 0.0f, 255.0f);
            byte g = (byte)Math.Clamp(gFloat * 255.0f, 0.0f, 255.0f);
            byte b = (byte)Math.Clamp(bFloat * 255.0f, 0.0f, 255.0f);

            return new(r, g, b, 255);
        }
    }

    private struct AudioChannel
    {
        private const double Alpha = 0.997;

        public int Control;
        public int Frequency;
        public int Volume;

        public readonly byte[] GeneratedAudio = new byte[2 * MaxScanlineCount];
        public int AudioIndex;

        public int FrequencyCounter;
        public int ShiftRegister4 = 0b1111;
        public int ShiftRegister5 = 0b11111;
        public int DivideBy3Counter;
        public bool Alternator = false;

        private double m_PrevInput;
        private double m_PrevOutput;

        public AudioChannel()
        {
        }

        public void Reboot()
        {
            Array.Clear(GeneratedAudio, 0, GeneratedAudio.Length);
            AudioIndex = 0;
            m_PrevInput = 0.0;
            m_PrevOutput = 0.0;
        }

        public int ReadAudioSamples(Span<byte> destination)
        {
            int count = Math.Min(destination.Length, AudioIndex);
            GeneratedAudio.AsSpan(0, count).CopyTo(destination);
            AudioIndex = 0;
            return count;
        }

        public void Step()
        {
            --FrequencyCounter;
            if (FrequencyCounter <= 0)
            {
                FrequencyCounter = 1 + Frequency;
                StepOutput();
            }

            if (AudioIndex < GeneratedAudio.Length)
            {
                double rawInput = GetOutput() ? Volume * 17 : 0;

                double filteredOutput = Alpha * (m_PrevOutput + rawInput - m_PrevInput);

                m_PrevInput = rawInput;
                m_PrevOutput = filteredOutput;

                GeneratedAudio[AudioIndex] = (byte)Math.Clamp(filteredOutput + 128, 0, 255);
                ++AudioIndex;
            }

            // Prevent lock-up states even though the real hardware doesn't do this
            if (Control == 8)
            {
                if (ShiftRegister4 == 0 && ShiftRegister5 == 0)
                {
                    ShiftRegister4 = 0b0001;
                    ShiftRegister5 = 0b00001;
                }
            }
            else
            {
                if (ShiftRegister4 == 0)
                {
                    ShiftRegister4 = 0b0001;
                }
                if (ShiftRegister5 == 0)
                {
                    ShiftRegister5 = 0b00001;
                }
            }
        }

        private void StepOutput()
        {
            switch (Control)
            {
                case 1:
                    // 4 bit poly
                    ClockPoly4();
                    break;

                case 2:
                    // div 15 -> 4 bit poly
                    ClockPoly5();
                    if (ShiftRegister5 == 0b01111 || ShiftRegister5 == 0b10100)
                    {
                        ClockPoly4();
                    }
                    break;

                case 3:
                    // 5 bit poly -> 4 bit poly
                    ClockPoly5();
                    if ((ShiftRegister5 & 0b00001) != 0)
                    {
                        ClockPoly4();
                    }
                    break;

                case 4:
                case 5:
                    // div 2 : pure tone
                    Alternator = !Alternator;
                    break;

                case 6:
                case 10:
                    // div 31 : pure tone
                    ClockPoly5();
                    if (ShiftRegister5 == 0b01111 || ShiftRegister5 == 0b10100)
                    {
                        Alternator = !Alternator;
                    }
                    break;

                case 7:
                    // 5 bit poly -> div 2
                    ClockPoly5();
                    if ((ShiftRegister5 & 0b00001) != 0)
                    {
                        Alternator = !Alternator;
                    }
                    break;

                case 8:
                    // 9 bit poly (white noise)
                    ClockPoly9();
                    break;

                case 9:
                    // 5 bit poly
                    ClockPoly5();
                    break;

                case 12:
                case 13:
                    // div 6 : pure tone
                    ++DivideBy3Counter;
                    if (DivideBy3Counter >= 3)
                    {
                        DivideBy3Counter = 0;
                        Alternator = !Alternator;
                    }
                    break;

                case 14:
                    // div 93 : pure tone
                    ++DivideBy3Counter;
                    if (DivideBy3Counter >= 3)
                    {
                        DivideBy3Counter = 0;
                        ClockPoly5();
                        if (ShiftRegister5 == 0b01111 || ShiftRegister5 == 0b10100)
                        {
                            Alternator = !Alternator;
                        }
                    }
                    break;

                case 15:
                    // 5 bit poly div 6
                    ++DivideBy3Counter;
                    if (DivideBy3Counter >= 3)
                    {
                        DivideBy3Counter = 0;
                        ClockPoly5();
                        if ((ShiftRegister5 & 0b00001) != 0)
                        {
                            Alternator = !Alternator;
                        }
                    }
                    break;
            }
        }

        private readonly bool GetOutput()
        {
            switch (Control)
            {
                case 0:
                case 11:
                    return true;
                case 1:
                case 2:
                case 3:
                case 8:
                    return (ShiftRegister4 & 0b0001) != 0;
                case 9:
                    return (ShiftRegister5 & 0b00001) != 0;
                case 4:
                case 5:
                case 6:
                case 10:
                case 7:
                case 12:
                case 13:
                case 14:
                case 15:
                    return Alternator;
            }

            return false;
        }


        private void ClockPoly4()
        {
            bool bit0 = (ShiftRegister4 & 0b0001) != 0;
            bool bit1 = (ShiftRegister4 & 0b0010) != 0;
            int carryIn = (bit0 ^ bit1) ? 0b1000 : 0b0000;
            ShiftRegister4 = carryIn | (ShiftRegister4 >> 1);
        }

        private void ClockPoly5()
        {
            bool bit0 = (ShiftRegister5 & 0b00001) != 0;
            bool bit2 = (ShiftRegister5 & 0b00100) != 0;
            int carryIn = (bit0 ^ bit2) ? 0b10000 : 0b00000;
            ShiftRegister5 = carryIn | (ShiftRegister5 >> 1);
        }

        private void ClockPoly9()
        {
            bool bit40 = (ShiftRegister4 & 0b0001) != 0;
            bool bit50 = (ShiftRegister5 & 0b00001) != 0;

            int carryIn5 = (bit40 ^ bit50) ? 0b10000 : 0b00000;
            int carryIn4 = bit50 ? 0b1000 : 0b0000;

            ShiftRegister5 = carryIn5 | (ShiftRegister5 >> 1);
            ShiftRegister4 = carryIn4 | (ShiftRegister4 >> 1);
        }
    }
}
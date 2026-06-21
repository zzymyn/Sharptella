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

    public void Reboot()
    {
    }

    public void Step()
    {
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
                break;
            case 0x01:
                // VBLANK
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
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharptari.Lib;

public sealed class Mos6532Riot
{
    private const ushort RegisterSelectM = 0b0001_0010_1000_0000;
    private const ushort RegisterSelectV = 0b0000_0010_1000_0000;
    private const ushort RegisterMask = 0b0000_0000_0000_0111;

    private const ushort RamSelectM = 0b0001_0010_1000_0000;
    private const ushort RamSelectV = 0b0000_0000_1000_0000;
    private const ushort RamMask = 0b0000_0000_0111_1111;

    private readonly byte[] m_Ram = new byte[128];

    public int TryRead(ushort address)
    {
        if ((address & RegisterSelectM) == RegisterSelectV)
        {
            address &= RegisterMask;

            switch (address)
            {
                case 0x00:
                    return 0x00; // SWCHA
                case 0x01:
                    return 0x00; // SWACNT
                case 0x02:
                    return 0x00; // SWCHB
                case 0x03:
                    return 0x00; // SWBCNT
                case 0x04:
                case 0x06:
                    return 0x00; // INTIM
                case 0x05:
                case 0x07:
                    return 0x00; // INSTAT
            }
        }
        else if ((address & RamSelectM) == RamSelectV)
        {
            address &= RamMask;
            return m_Ram[address];
        }

        return -1;
    }

    public void TryWrite(ushort address, byte value)
    {
        if ((address & RegisterSelectM) == RegisterSelectV)
        {
            address &= RegisterMask;

            switch (address)
            {
                case 0x00:
                    break; // SWCHA
                case 0x01:
                    break; // SWACNT
                case 0x02:
                    break; // SWCHB
                case 0x03:
                    break; // SWBCNT
                case 0x04:
                    break; // TIM1T
                case 0x05:
                    break; // TIM8T
                case 0x06:
                    break; // TIM64T
                case 0x07:
                    break; // TIM1024T
            }
        }
        else if ((address & RamSelectM) == RamSelectV)
        {
            address &= RamMask;
            m_Ram[address] = value;
        }
    }
}

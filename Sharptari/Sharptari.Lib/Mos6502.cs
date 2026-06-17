using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharptari.Lib;

public struct Mos6502OpCode
{
    public const uint OpCodeHiMask = 0b1110_0000;
    public const int OpCodeHiShift = 5;
    public const uint OpCodeLoMask = 0b0000_0011;
    public const int OpCodeLoShift = 0;
    public const uint OpCodeModeMask = 0b0001_1100;
    public const int OpCodeModeShift = 2;

    public byte Data;

    public readonly uint OpCode => (Data & OpCodeHiMask) >> OpCodeHiShift | (Data & OpCodeLoMask) >> OpCodeLoShift;
    public readonly uint Mode => (Data & OpCodeModeMask) >> OpCodeModeShift;
}

public sealed class Mos6502
{
}

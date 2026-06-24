namespace Sharptella.Lib;

public struct Mos6502Registers
{
    public const uint FlagNegativeMask = 0b1000_0000;
    public const uint FlagOverflowMask = 0b0100_0000;
    public const uint FlagUnusedMask = 0b0010_0000;
    public const uint FlagBreakMask = 0b0001_0000;
    public const uint FlagDecimalMask = 0b0000_1000;
    public const uint FlagInterruptDisableMask = 0b0000_0100;
    public const uint FlagZeroMask = 0b0000_0010;
    public const uint FlagCarryMask = 0b0000_0001;

    public byte A;
    public bool PNegative;
    public bool POverflow;
    public bool PDecimal;
    public bool PInterruptDisable;
    public bool PZero;
    public bool PCarry;
    public ushort PC;
    public byte S;
    public byte X;
    public byte Y;

    public byte TestP
    {
        readonly get => ReadP(false);
        set => WriteP(value);
    }

    public readonly byte ReadP(bool fromBrk)
    {
        uint p = 0;
        if (PNegative) p |= FlagNegativeMask;
        if (POverflow) p |= FlagOverflowMask;
        p |= FlagUnusedMask;
        if (fromBrk) p |= FlagBreakMask;
        if (PDecimal) p |= FlagDecimalMask;
        if (PInterruptDisable) p |= FlagInterruptDisableMask;
        if (PZero) p |= FlagZeroMask;
        if (PCarry) p |= FlagCarryMask;
        return (byte)p;
    }

    public void WriteP(byte value)
    {
        PNegative = (value & FlagNegativeMask) != 0;
        POverflow = (value & FlagOverflowMask) != 0;
        PDecimal = (value & FlagDecimalMask) != 0;
        PInterruptDisable = (value & FlagInterruptDisableMask) != 0;
        PZero = (value & FlagZeroMask) != 0;
        PCarry = (value & FlagCarryMask) != 0;
    }
}

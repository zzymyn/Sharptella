namespace Sharptari.Lib;

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
    public byte P;
    public ushort PC;
    public byte S;
    public byte X;
    public byte Y;

    public bool FlagNegative
    {
        readonly get => ReadFlag(FlagNegativeMask);
        set => WriteFlag(FlagNegativeMask, value);
    }

    public bool FlagOverflow
    {
        readonly get => ReadFlag(FlagOverflowMask);
        set => WriteFlag(FlagOverflowMask, value);
    }

    public bool FlagUnused
    {
        readonly get => ReadFlag(FlagUnusedMask);
        set => WriteFlag(FlagUnusedMask, value);
    }

    public bool FlagBreak
    {
        readonly get => ReadFlag(FlagBreakMask);
        set => WriteFlag(FlagBreakMask, value);
    }

    public bool FlagDecimal
    {
        readonly get => ReadFlag(FlagDecimalMask);
        set => WriteFlag(FlagDecimalMask, value);
    }

    public bool FlagInterruptDisable
    {
        readonly get => ReadFlag(FlagInterruptDisableMask);
        set => WriteFlag(FlagInterruptDisableMask, value);
    }

    public bool FlagZero
    {
        readonly get => ReadFlag(FlagZeroMask);
        set => WriteFlag(FlagZeroMask, value);
    }

    public bool FlagCarry
    {
        readonly get => ReadFlag(FlagCarryMask);
        set => WriteFlag(FlagCarryMask, value);
    }

    private readonly bool ReadFlag(uint mask) => (P & mask) != 0;

    private void WriteFlag(uint mask, bool value)
    {
        if (value)
            P = (byte)(P | mask);
        else
            P = (byte)(P & ~mask);
    }
}

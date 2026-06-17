namespace Sharptari.Lib;

public enum Mos6502AddrMode
{
    Accumulator,
    Absolute,
    AbsoluteXIndexed,
    AbsoluteYIndexed,
    Immediate,
    Implied,
    Indirect,
    XIndexedIndirect,
    IndirectYIndexed,
    Relative,
    ZeroPage,
    ZeroPageXIndexed,
    ZeroPageYIndexed,
}
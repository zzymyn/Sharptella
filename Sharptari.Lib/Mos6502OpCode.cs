namespace Sharptari.Lib;

public static class Mos6502OpCode
{
    private const uint OpCodeAMask = 0b1110_0000;
    private const uint OpCodeBMask = 0b0001_1100;
    private const uint OpCodeCMask = 0b0000_0011;

    public static uint GetOpCodeA(byte opCode) => opCode & OpCodeAMask;
    public static uint GetOpCodeB(byte opCode) => opCode & OpCodeBMask;
    public static uint GetOpCodeC(byte opCode) => opCode & OpCodeCMask;
}

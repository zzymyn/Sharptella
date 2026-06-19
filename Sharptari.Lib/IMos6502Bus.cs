namespace Sharptari.Lib;

public interface IMos6502Bus
{
    void Step();
    byte Read(ushort address);
    void Write(ushort address, byte value);
}

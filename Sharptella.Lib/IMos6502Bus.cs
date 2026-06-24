namespace Sharptella.Lib;

public interface IMos6502Bus
{
    void Reboot();
    void Step();
    byte Read(ushort address);
    void Write(ushort address, byte value);
}

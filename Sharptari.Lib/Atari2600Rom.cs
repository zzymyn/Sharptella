namespace Sharptari.Lib;

public sealed class Atari2600Rom
{
    private readonly byte[] m_Data;

    public Atari2600Rom(byte[] data)
    {
        m_Data = data;
    }

    public byte TryRead(ushort address)
    {
        if (address < m_Data.Length)
        {
            return m_Data[address];
        }
        else
        {
            return 0;
        }
    }
}
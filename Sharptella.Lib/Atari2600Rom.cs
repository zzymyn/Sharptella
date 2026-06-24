using System;
using System.Numerics;

namespace Sharptella.Lib;

public sealed class Atari2600Rom
{
    private readonly byte[] m_Data;
    private readonly ushort m_DataMask;

    public Atari2600Rom(byte[] data)
    {
        if (!BitOperations.IsPow2(data.Length))
        {
            throw new ArgumentException("Invalid ROM: size must be a power of two.", nameof(data));
        }

        m_Data = data;
        m_DataMask = (ushort)(data.Length - 1);
    }

    public void Reboot()
    {
        // ROM doesn't have any internal state, so do nothing.
    }

    public void Step()
    {
        // ROM doesn't have any internal state, so do nothing.
    }

    public int TryRead(ushort address)
    {
        address &= m_DataMask;
        return m_Data[address];
    }

    public void TryWrite(ushort address, byte value)
    {
        // ROM is read-only, so do nothing.
    }
}
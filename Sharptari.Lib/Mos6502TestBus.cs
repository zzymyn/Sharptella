using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sharptari.Lib;

public sealed class Mos6502TestBus
    : IMos6502Bus
{
    private readonly byte[] m_Ram = new byte[ushort.MaxValue + 1];
    private readonly List<LogItem> m_Log = [];
    private bool m_HasDoneSomethingThisStep;

    public List<LogItem> Log => m_Log;

    public void Reboot()
    {
        m_HasDoneSomethingThisStep = false;
        Array.Clear(m_Ram);
        m_Log.Clear();
    }

    public void Step()
    {
        if (!m_HasDoneSomethingThisStep)
        {
            throw new InvalidOperationException("The bus must do something each step, but nothing has done anything this step.");
        }
        m_HasDoneSomethingThisStep = false;
    }

    public byte Read(ushort address)
    {
        if (m_HasDoneSomethingThisStep)
        {
            throw new InvalidOperationException("The bus can only do one thing per step, but something has already done something this step.");
        }
        m_HasDoneSomethingThisStep = true;
        var value = m_Ram[address];
        m_Log.Add(new LogItem { IsRead = true, Address = address, Value = value });
        return value;
    }

    public void Write(ushort address, byte value)
    {
        if (m_HasDoneSomethingThisStep)
        {
            throw new InvalidOperationException("The bus can only do one thing per step, but something has already done something this step.");
        }
        m_HasDoneSomethingThisStep = true;
        m_Ram[address] = value;
        m_Log.Add(new LogItem { IsRead = false, Address = address, Value = value });
    }

    public byte ReadDirect(ushort address)
    {
        return m_Ram[address];
    }

    public void WriteDirect(ushort address, byte value)
    {
        m_Ram[address] = value;
    }

    public struct LogItem
    {
        public bool IsRead;
        public ushort Address;
        public byte Value;
    }
}

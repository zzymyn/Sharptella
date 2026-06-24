using System;
using System.Collections.Generic;
using System.Text;

namespace Sharptella.CpuTest;

internal static class FastCpuTestParser
{
    public static List<TestData> Parse(byte[] fileData)
    {
        var reader = new FastSpanReader(fileData);
        reader.Expect((byte)'[');

        var list = new List<TestData>(10000);

        while (!reader.IsEnd)
        {
            reader.SkipWhitespace();
            if (reader.TryExpect((byte)']'))
            {
                break;
            }

            list.Add(ParseTestData(ref reader));

            reader.SkipWhitespace();
            if (reader.TryExpect((byte)','))
            {
                continue;
            }
        }

        return list;
    }

    private static TestData ParseTestData(ref FastSpanReader reader)
    {
        reader.Expect((byte)'{');
        var test = new TestData();

        while (true)
        {
            var key = reader.ReadStringAsSpan();
            reader.Expect((byte)':');

            if (key.SequenceEqual("name"u8))
            {
                var nameSpan = reader.ReadStringAsSpan();
                test.Name = Encoding.UTF8.GetString(nameSpan);
            }
            else if (key.SequenceEqual("initial"u8))
            {
                test.Initial = ParseState(ref reader);
            }
            else if (key.SequenceEqual("final"u8))
            {
                test.Final = ParseState(ref reader);
            }
            else if (key.SequenceEqual("cycles"u8))
            {
                test.Cycles = ParseCycles(ref reader);
            }
            else
            {
                reader.SkipValue();
            }

            reader.SkipWhitespace();
            if (reader.TryExpect((byte)',')) continue;
            if (reader.TryExpect((byte)'}')) break;
        }

        return test;
    }

    private static CpuState ParseState(ref FastSpanReader reader)
    {
        reader.Expect((byte)'{');
        var state = new CpuState();

        while (true)
        {
            var key = reader.ReadStringAsSpan();
            reader.Expect((byte)':');

            if (key.SequenceEqual("pc"u8)) state.PC = (ushort)reader.ReadInt();
            else if (key.SequenceEqual("s"u8)) state.S = (byte)reader.ReadInt();
            else if (key.SequenceEqual("a"u8)) state.A = (byte)reader.ReadInt();
            else if (key.SequenceEqual("x"u8)) state.X = (byte)reader.ReadInt();
            else if (key.SequenceEqual("y"u8)) state.Y = (byte)reader.ReadInt();
            else if (key.SequenceEqual("p"u8)) state.P = (byte)reader.ReadInt();
            else if (key.SequenceEqual("ram"u8)) state.Ram = ParseRam(ref reader);
            else reader.SkipValue();

            reader.SkipWhitespace();
            if (reader.TryExpect((byte)',')) continue;
            if (reader.TryExpect((byte)'}')) break;
        }

        return state;
    }

    private static List<RamEntry>? ParseRam(ref FastSpanReader reader)
    {
        reader.SkipWhitespace();
        if (reader.TryExpect("null"u8)) return null;

        reader.Expect((byte)'[');
        var ramList = new List<RamEntry>(4);

        while (true)
        {
            reader.SkipWhitespace();
            if (reader.TryExpect((byte)']')) break;

            reader.Expect((byte)'[');
            ushort addr = (ushort)reader.ReadInt();
            reader.Expect((byte)',');
            byte val = (byte)reader.ReadInt();
            reader.Expect((byte)']');

            ramList.Add(new RamEntry { Address = addr, Value = val });

            reader.SkipWhitespace();
            if (reader.TryExpect((byte)',')) continue;
            if (reader.TryExpect((byte)']')) break;
        }

        return ramList;
    }

    private static List<CpuCycle>? ParseCycles(ref FastSpanReader reader)
    {
        reader.SkipWhitespace();
        if (reader.TryExpect("null"u8)) return null;

        reader.Expect((byte)'[');
        var cycleList = new List<CpuCycle>(8);

        while (true)
        {
            reader.SkipWhitespace();
            if (reader.TryExpect((byte)']')) break;

            reader.Expect((byte)'[');
            ushort addr = (ushort)reader.ReadInt();
            reader.Expect((byte)',');
            byte val = (byte)reader.ReadInt();
            reader.Expect((byte)',');

            var typeSpan = reader.ReadStringAsSpan();
            CycleType type;
            if (typeSpan.SequenceEqual("read"u8)) type = CycleType.Read;
            else if (typeSpan.SequenceEqual("write"u8)) type = CycleType.Write;
            else throw new Exception("Unknown CycleType");

            reader.Expect((byte)']');

            cycleList.Add(new CpuCycle { Address = addr, Value = val, Type = type });

            reader.SkipWhitespace();
            if (reader.TryExpect((byte)',')) continue;
            if (reader.TryExpect((byte)']')) break;
        }

        return cycleList;
    }
}

public ref struct FastSpanReader
{
    private readonly ReadOnlySpan<byte> _span;
    private int _pos;

    public FastSpanReader(ReadOnlySpan<byte> span)
    {
        _span = span;
        _pos = 0;
    }

    public readonly bool IsEnd => _pos >= _span.Length;

    public readonly byte PeekByte() => _pos < _span.Length ? _span[_pos] : (byte)0;
    public byte ReadByte() => _pos < _span.Length ? _span[_pos++] : (byte)0;

    public void SkipWhitespace()
    {
        while (_pos < _span.Length)
        {
            byte b = _span[_pos];
            if (b == ' ' || b == '\t' || b == '\r' || b == '\n')
            {
                _pos++;
            }
            else
            {
                break;
            }
        }
    }

    public void Expect(byte expected)
    {
        SkipWhitespace();
        if (_pos >= _span.Length || _span[_pos] != expected)
            throw new Exception($"Expected {(char)expected} at position {_pos}");
        _pos++;
    }

    public bool TryExpect(byte expected)
    {
        SkipWhitespace();
        if (_pos < _span.Length && _span[_pos] == expected)
        {
            _pos++;
            return true;
        }
        return false;
    }

    public bool TryExpect(ReadOnlySpan<byte> sequence)
    {
        SkipWhitespace();
        if (_pos + sequence.Length <= _span.Length && _span[_pos..(_pos + sequence.Length)].SequenceEqual(sequence))
        {
            _pos += sequence.Length;
            return true;
        }
        return false;
    }

    public ReadOnlySpan<byte> ReadStringAsSpan()
    {
        Expect((byte)'"');
        int start = _pos;
        while (_pos < _span.Length && _span[_pos] != '"')
        {
            if (_span[_pos] == '\\')
            {
                _pos += 2; // skip escape sequence securely
            }
            else
            {
                _pos++;
            }
        }
        int end = _pos;
        Expect((byte)'"');
        return _span[start..end];
    }

    public int ReadInt()
    {
        SkipWhitespace();
        if (_pos >= _span.Length) throw new Exception("Unexpected EOF parsing integer.");

        bool negative = false;
        if (_span[_pos] == '-')
        {
            negative = true;
            _pos++;
        }

        int val = 0;
        int start = _pos;
        while (_pos < _span.Length)
        {
            byte b = _span[_pos];
            if (b >= '0' && b <= '9')
            {
                val = val * 10 + (b - '0');
                _pos++;
            }
            else
            {
                break;
            }
        }

        if (_pos == start) throw new Exception("Expected digits at position " + start);
        return negative ? -val : val;
    }

    public void SkipValue()
    {
        SkipWhitespace();
        if (TryExpect((byte)'"'))
        {
            while (!IsEnd)
            {
                var b = ReadByte();
                if (b == '"') break;
                if (b == '\\') ReadByte();
            }
            return;
        }

        byte bStart = PeekByte();
        if (bStart == '{' || bStart == '[')
        {
            byte bEnd = bStart == '{' ? (byte)'}' : (byte)']';
            ReadByte(); // consume start
            int depth = 1;
            while (depth > 0 && !IsEnd)
            {
                byte b = ReadByte();
                if (b == bStart) depth++;
                else if (b == bEnd) depth--;
                else if (b == '"')
                {
                    while (!IsEnd)
                    {
                        var sByte = ReadByte();
                        if (sByte == '"') break;
                        if (sByte == '\\') ReadByte();
                    }
                }
            }
            return;
        }

        while (!IsEnd)
        {
            byte b = PeekByte();
            if (b == ',' || b == '}' || b == ']' || b == ' ' || b == '\t' || b == '\r' || b == '\n')
                break;
            ReadByte();
        }
    }
}
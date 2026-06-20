using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Sharptari.Lib;

namespace Sharptari.Cli;

internal class Program
{
    private static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("Sharptari - An Atari 2600 Emulator written in C#");
            var romPath = args[0];
            var romBytes = await File.ReadAllBytesAsync(romPath);

            var rom = new Atari2600Rom(romBytes);
            var riot = new Mos6532Riot();
            var bus = new Atari2600Bus(rom, riot);
            var cpu = new Mos6502Cpu<Atari2600Bus>(bus);

            cpu.Reboot();
            while (true)
            {
                cpu.Step();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
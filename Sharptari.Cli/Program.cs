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

            var atari2600 = new Atari2600(romBytes);

            atari2600.Reboot();
            while (true)
            {
                atari2600.Step();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
using System;
using System.IO;
using System.Threading.Tasks;
using Sharptari.Lib;

namespace Sharptari.Gui;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        try
        {
            var romPath = args[0];
            var romBytes = await File.ReadAllBytesAsync(romPath);

            var atari2600 = new Atari2600(romBytes);

            using var app = new App(atari2600);
            app.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}
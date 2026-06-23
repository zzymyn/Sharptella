using System.Drawing;
using Silk.NET.Windowing;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using System.Threading.Tasks;
using System.IO;
using System;

namespace Sharptari.Gui;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        try
        {
            using var app = new App();

            if (args.Length == 1)
            {
                app.LoadRom(args[0]);
            }

            app.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}\n{ex.StackTrace}");
        }
    }
}
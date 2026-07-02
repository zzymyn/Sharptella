using System;
using System.Threading.Tasks;
using Silk.NET.Input.Glfw;
using Silk.NET.Windowing.Glfw;

namespace Sharptella.Gui;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        try
        {
            GlfwWindowing.RegisterPlatform();
            GlfwInput.RegisterPlatform();

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
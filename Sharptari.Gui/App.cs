using System;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Sharptari.Gui;

internal sealed class App
{
    private readonly IWindow m_Window;
    private IInputContext? m_Input;

    public App()
    {
        var options = WindowOptions.Default with
        {
            Size = new Vector2D<int>(800, 600),
            Title = "Sharptari"
        };

        m_Window = Window.Create(options);
        m_Window.Load += OnLoad;
        m_Window.Closing += OnClosing;
        m_Window.Update += OnUpdate;
        m_Window.Render += OnRender;
    }

    public void Run()
    {
        m_Window.Run();
    }

    private void OnLoad()
    {
        m_Input = m_Window.CreateInput();
        Console.WriteLine($"Window loaded. Keyboards: {m_Input.Keyboards.Count}, Mice: {m_Input.Mice.Count}");

        if (m_Input.Keyboards.Count > 0)
        {
            m_Input.Keyboards[0].KeyDown += OnKeyDown;
        }
    }

    private void OnClosing()
    {
        m_Input?.Dispose();
    }

    private void OnUpdate(double deltaTime)
    {
    }

    private void OnRender(double deltaTime)
    {
    }

    private void OnKeyDown(IKeyboard keyboard, Key key, int keyCode)
    {
        if (key == Key.Escape)
        {
            m_Window.Close();
        }
    }
}
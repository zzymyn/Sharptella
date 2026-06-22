using System;
using Silk.NET.SDL;

namespace Sharptari.Gui;

internal sealed unsafe class SdlWindow
    : IDisposable
{
    private readonly Sdl m_Sdl;
    private Window* m_Window;

    public Window* Ptr => m_Window;

    public SdlWindow(Sdl sdl, string title, int x, int y, int w, int h, WindowFlags flags)
    {
        m_Sdl = sdl;
        m_Window = m_Sdl.CreateWindow(
            title,
            x,
            y,
            w,
            h,
            (uint)flags
        );
        if (m_Window == null)
        {
            throw new Exception($"Could not create window: {m_Sdl.GetErrorS()}");
        }
    }

    public void Dispose()
    {
        if (m_Window != null)
        {
            m_Sdl.DestroyWindow(m_Window);
            m_Window = null;
        }
    }
}
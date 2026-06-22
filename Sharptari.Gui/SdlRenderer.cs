using System;
using Silk.NET.SDL;

namespace Sharptari.Gui;

internal sealed unsafe class SdlRenderer
    : IDisposable
{
    private readonly Sdl m_Sdl;
    private Renderer* m_Renderer;

    public Renderer* Ptr => m_Renderer;

    public SdlRenderer(Sdl sdl, Window* window, int index, RendererFlags flags)
    {
        m_Sdl = sdl;
        m_Renderer = m_Sdl.CreateRenderer(
            window,
            index,
            (uint)flags
        );
        if (m_Renderer == null)
        {
            throw new Exception($"Could not create renderer: {m_Sdl.GetErrorS()}");
        }
    }

    public void Dispose()
    {
        if (m_Renderer != null)
        {
            m_Sdl.DestroyRenderer(m_Renderer);
            m_Renderer = null;
        }
    }
}
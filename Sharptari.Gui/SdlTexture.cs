using System;
using Silk.NET.SDL;

namespace Sharptari.Gui;

internal sealed unsafe class SdlTexture
    : IDisposable
{
    private readonly Sdl m_Sdl;
    private Texture* m_Texture;
    private readonly PixelFormatEnum m_PixelFormat;
    private readonly TextureAccess m_TextureAccess;
    private readonly int m_Width;
    private readonly int m_Height;

    public Texture* Ptr => m_Texture;
    public PixelFormatEnum PixelFormat => m_PixelFormat;
    public TextureAccess TextureAccess => m_TextureAccess;
    public int Width => m_Width;
    public int Height => m_Height;

    public SdlTexture(Sdl sdl, Renderer* renderer, PixelFormatEnum pixelFormat, TextureAccess textureAccess, int width, int height)
    {
        m_Sdl = sdl;
        m_Texture = m_Sdl.CreateTexture(
            renderer,
            (uint)pixelFormat,
            (int)TextureAccess.Streaming,
            width,
            height
        );
        if (m_Texture == null)
        {
            throw new Exception($"Could not create texture: {m_Sdl.GetErrorS()}");
        }
        m_PixelFormat = pixelFormat;
        m_TextureAccess = textureAccess;
        m_Width = width;
        m_Height = height;
    }

    public void Dispose()
    {
        if (m_Texture != null)
        {
            m_Sdl.DestroyTexture(m_Texture);
            m_Texture = null;
        }
    }
}
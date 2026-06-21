using System;
using Silk.NET.Maths;
using Silk.NET.SDL;

namespace Sharptari.Gui;

internal unsafe sealed class App
    : IDisposable
{
    private readonly Sdl m_Sdl;
    private readonly Window* m_Window;
    private readonly Renderer* m_Renderer;
    private readonly Texture* m_Texture;
    private readonly byte[] m_PixelData = new byte[160 * 192 * 4];

    public App()
    {
        m_Sdl = Sdl.GetApi();

        if (m_Sdl.Init(Sdl.InitVideo) != 0)
        {
            throw new Exception($"Could not initialize SDL: {m_Sdl.GetErrorS()}");
        }

        m_Window = m_Sdl.CreateWindow(
            "Sharptari",
            Sdl.WindowposCentered,
            Sdl.WindowposCentered,
            800,
            600,
            (uint)(WindowFlags.Shown | WindowFlags.Resizable | WindowFlags.AllowHighdpi)
        );

        if (m_Window == null)
        {
            throw new Exception($"Could not create window: {m_Sdl.GetErrorS()}");
        }

        m_Renderer = m_Sdl.CreateRenderer(
            m_Window,
            -1,
            (uint)(RendererFlags.Accelerated | RendererFlags.Presentvsync)
        );
        if (m_Renderer == null)
        {
            throw new Exception($"Could not create renderer: {m_Sdl.GetErrorS()}");
        }

        m_Texture = m_Sdl.CreateTexture(
            m_Renderer,
            (uint)PixelFormatEnum.Abgr8888,
            (int)TextureAccess.Streaming,
            160,
            192
        );

        if (m_Texture == null)
        {
            throw new Exception($"Could not create texture: {m_Sdl.GetErrorS()}");
        }
    }

    public void Dispose()
    {
        if (m_Texture != null)
        {
            m_Sdl.DestroyTexture(m_Texture);
        }
        if (m_Renderer != null)
        {
            m_Sdl.DestroyRenderer(m_Renderer);
        }
        if (m_Window != null)
        {
            m_Sdl.DestroyWindow(m_Window);
        }
        m_Sdl.Quit();
    }

    public void Run()
    {
        bool running = true;
        Event ev = new();

        while (running)
        {
            while (m_Sdl.PollEvent(&ev) != 0)
            {
                if (ev.Type == (uint)EventType.Quit)
                {
                    running = false;
                }
            }

            // Clear the screen (e.g., to dark blue)
            m_Sdl.SetRenderDrawColor(m_Renderer, 20, 40, 80, 255);
            m_Sdl.RenderClear(m_Renderer);

            for (int i = 0; i < m_PixelData.Length; i += 4)
            {
                var x = i / 4 % 160;
                var y = i / 4 / 160;
                m_PixelData[i + 0] = (byte)(x * 255 / 159); // R
                m_PixelData[i + 1] = (byte)(y * 255 / 191); // G
                m_PixelData[i + 2] = 0; // B
                m_PixelData[i + 3] = 255; // A
            }

            fixed (byte* pixelDataPtr = m_PixelData)
            {
                m_Sdl.UpdateTexture(m_Texture, null, pixelDataPtr, 160 * 4);
            }

            // Get the size of the renderer output:
            int rWidth = 0;
            int rHeight = 0;
            m_Sdl.GetRendererOutputSize(m_Renderer, ref rWidth, ref rHeight);
            float rAspect = (float)rWidth / rHeight;

            // Render your texture (srcrect and dstrect set to null fills the window)
            Rectangle<int> srcRect = new(0, 0, 160, 192);
            Rectangle<int> dstRect = default;

            // fit 4:3 into dstRect while maintaining aspect ratio
            float targetAspect = 4.0f / 3.0f;
            if (rAspect > targetAspect)
            {
                var w = (int)(rHeight * targetAspect);
                dstRect.Size.Y = rHeight;
                dstRect.Size.X = w;
                dstRect.Origin.X = (rWidth - w) / 2;
                dstRect.Origin.Y = 0;
            }
            else
            {
                var h = (int)(rWidth / targetAspect);
                dstRect.Size.X = rWidth;
                dstRect.Size.Y = h;
                dstRect.Origin.X = 0;
                dstRect.Origin.Y = (rHeight - h) / 2;
            }

            m_Sdl.RenderCopy(m_Renderer, m_Texture, in srcRect, in dstRect);

            // Present to the screen
            m_Sdl.RenderPresent(m_Renderer);
        }
    }
}
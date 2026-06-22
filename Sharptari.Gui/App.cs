using System;
using Sharptari.Lib;
using Silk.NET.Maths;
using Silk.NET.SDL;

namespace Sharptari.Gui;

internal unsafe sealed class App
    : IDisposable
{
    private const int ScanlineLength = 228;
    private const int HBlankLength = 68;
    private const int InitialScanlineCount = 262;
    private const float PixelAspectRatio = 12.0f / 7.0f;

    private readonly Atari2600 m_Atari2600;
    private readonly Sdl m_Sdl;
    private readonly Window* m_Window;
    private readonly Renderer* m_Renderer;
    private SdlTexture m_Texture;

    public App(Atari2600 atari2600)
    {
        m_Atari2600 = atari2600;
        m_Atari2600.Reboot();

        m_Sdl = Sdl.GetApi();

        if (m_Sdl.Init(Sdl.InitVideo) != 0)
        {
            throw new Exception($"Could not initialize SDL: {m_Sdl.GetErrorS()}");
        }

        m_Window = m_Sdl.CreateWindow(
            "Sharptari",
            Sdl.WindowposCentered,
            Sdl.WindowposCentered,
            1920,
            1080,
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

        m_Texture = new(m_Sdl, m_Renderer, PixelFormatEnum.Xbgr8888, TextureAccess.Streaming, ScanlineLength, InitialScanlineCount);
    }

    public void Dispose()
    {
        m_Texture.Dispose();
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
        ColorAbgr8888[] frameBuffer = new ColorAbgr8888[ScanlineLength * InitialScanlineCount];

        bool running = true;
        Event ev = new();

        while (running)
        {
            // Process window events:
            while (m_Sdl.PollEvent(&ev) != 0)
            {
                if (ev.Type == (uint)EventType.Quit)
                {
                    running = false;
                }
            }

            // Process one frame of emulation:
            m_Atari2600.StepFrame(ref frameBuffer, out int frameBufferLength);

            // copy the frame buffer to texture:
            int frameBufferHeight = frameBufferLength / ScanlineLength;

            if (frameBufferHeight > m_Texture.Height)
            {
                m_Texture.Dispose();
                m_Texture = new(m_Sdl, m_Renderer, PixelFormatEnum.Xbgr8888, TextureAccess.Streaming, ScanlineLength, frameBufferHeight);
            }

            fixed (ColorAbgr8888* pixelDataPtr = frameBuffer)
            {
                Rectangle<int> rect = new(0, 0, ScanlineLength, frameBufferHeight);
                m_Sdl.UpdateTexture(m_Texture.Ptr, ref rect, pixelDataPtr, 4 * ScanlineLength);
            }

            // Clear the screen (e.g., to dark blue)
            m_Sdl.SetRenderDrawColor(m_Renderer, 20, 40, 80, 255);
            m_Sdl.RenderClear(m_Renderer);

            // Get the size of the renderer output:
            int rWidth = 0;
            int rHeight = 0;
            m_Sdl.GetRendererOutputSize(m_Renderer, ref rWidth, ref rHeight);
            float rAspect = (float)rWidth / rHeight;

            // Render your texture (srcrect and dstrect set to null fills the window)
            Rectangle<int> srcRect = new(0, 0, ScanlineLength, frameBufferHeight);
            Rectangle<int> dstRect = default;

            // fit 4:3 into dstRect while maintaining aspect ratio
            float targetAspect = PixelAspectRatio * ScanlineLength / frameBufferHeight;
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

            m_Sdl.RenderCopy(m_Renderer, m_Texture.Ptr, in srcRect, in dstRect);

            // Present to the screen
            m_Sdl.RenderPresent(m_Renderer);
        }
    }
}

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
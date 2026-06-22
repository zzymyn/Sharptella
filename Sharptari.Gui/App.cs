using System;
using System.Diagnostics;
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
    private const long CpuSpeed = 1193181;

    private readonly SdlAtariInput m_AtariInput;
    private readonly Atari2600 m_Atari2600;
    private readonly Sdl m_Sdl;
    private SdlWindow m_Window;
    private SdlRenderer m_Renderer;
    private SdlTexture m_Texture;

    public App(byte[] romBytes)
    {
        m_AtariInput = new SdlAtariInput();
        m_Atari2600 = new Atari2600(romBytes, m_AtariInput);
        m_Atari2600.Reboot();

        m_Sdl = Sdl.GetApi();

        if (m_Sdl.Init(Sdl.InitVideo | Sdl.InitTimer) != 0)
        {
            throw new Exception($"Could not initialize SDL: {m_Sdl.GetErrorS()}");
        }

        m_Window = new(
            m_Sdl,
            "Sharptari",
            Sdl.WindowposCentered,
            Sdl.WindowposCentered,
            1920,
            1080,
            WindowFlags.Shown | WindowFlags.Resizable | WindowFlags.AllowHighdpi
        );

        m_Renderer = new(m_Sdl,
            m_Window.Ptr,
            -1,
            RendererFlags.Accelerated);

        m_Texture = new(m_Sdl, m_Renderer.Ptr, PixelFormatEnum.Xbgr8888, TextureAccess.Streaming, ScanlineLength, InitialScanlineCount);
    }

    public void Dispose()
    {
        m_Texture.Dispose();
        m_Renderer.Dispose();
        m_Window.Dispose();
        m_Sdl.Quit();
    }

    public void Run()
    {
        ColorAbgr8888[] frameBuffer = new ColorAbgr8888[ScanlineLength * InitialScanlineCount];

        Event ev = new();

        double realElapsedTime = 0.0;
        double cpuElapsedTime = 0.0;

        var perfFreq = 1.0 / m_Sdl.GetPerformanceFrequency();
        var prevTime = m_Sdl.GetPerformanceCounter();

        while (true)
        {
            do
            {
                // Process window events:
                while (m_Sdl.PollEvent(&ev) != 0)
                {
                    switch ((EventType)ev.Type)
                    {
                        case EventType.Quit:
                            return;
                        case EventType.Keydown:
                            m_AtariInput.ProcessKeyboardEvent(ev);
                            if (ev.Key.Keysym.Scancode == Scancode.ScancodeEscape)
                            {
                                return;
                            }
                            break;
                        case EventType.Keyup:
                            m_AtariInput.ProcessKeyboardEvent(ev);
                            break;
                    }
                }

                var nowTime = m_Sdl.GetPerformanceCounter();
                realElapsedTime += (nowTime - prevTime) * perfFreq;
                prevTime = nowTime;

                double timeRemaining = cpuElapsedTime - realElapsedTime;
                if (timeRemaining > 0.2)
                {
                    // If we're more than 200ms ahead, something weird has happened (no frame rendered for a long time)
                    // just reset the timers to avoid a long sleep:
                    realElapsedTime = cpuElapsedTime;
                    Console.WriteLine($"Warning: Emulation is more than 200ms ahead of real time. Resetting timers.");
                }
                else if (timeRemaining > 0.005)
                {
                    uint sleepTime = (uint)(timeRemaining * 1000) - 2;
                    m_Sdl.Delay(sleepTime);
                }
                else if (timeRemaining <= -0.1)
                {
                    // If we're more than 100ms behind, (user dragged the window around perhaps?)
                    // just reset the timers to avoid a fast-forward:
                    realElapsedTime = cpuElapsedTime;
                    Console.WriteLine($"Warning: Emulation is more than 100ms behind real time. Resetting timers.");
                }
            }
            while (realElapsedTime < cpuElapsedTime);

            // Process one frame of emulation:
            int cpuSteps = m_Atari2600.StepFrame(ref frameBuffer, out int frameBufferLength);
            cpuElapsedTime += (double)cpuSteps / CpuSpeed;

            int minX = ScanlineLength - 1;
            int maxX = 0;
            int minY = frameBufferLength / ScanlineLength - 1;
            int maxY = 0;
            for (int i = 0; i < frameBufferLength; ++i)
            {
                var pixel = frameBuffer[i];
                if (pixel.A != 0)
                {
                    int x = i % ScanlineLength;
                    int y = i / ScanlineLength;
                    minX = Math.Min(minX, x);
                    maxX = Math.Max(maxX, x);
                    minY = Math.Min(minY, y);
                    maxY = Math.Max(maxY, y);
                }
            }

            // copy the frame buffer to texture:
            int frameBufferHeight = frameBufferLength / ScanlineLength;

            if (frameBufferHeight > m_Texture.Height)
            {
                m_Texture.Dispose();
                m_Texture = new(m_Sdl, m_Renderer.Ptr, PixelFormatEnum.Xbgr8888, TextureAccess.Streaming, ScanlineLength, frameBufferHeight);
            }

            fixed (ColorAbgr8888* pixelDataPtr = frameBuffer)
            {
                Rectangle<int> rect = new(0, 0, ScanlineLength, frameBufferHeight);
                m_Sdl.UpdateTexture(m_Texture.Ptr, ref rect, pixelDataPtr, 4 * ScanlineLength);
            }

            // Clear the screen (e.g., to dark blue)
            m_Sdl.SetRenderDrawColor(m_Renderer.Ptr, 20, 40, 80, 255);
            m_Sdl.RenderClear(m_Renderer.Ptr);

            // Get the size of the renderer output:
            int rWidth = 0;
            int rHeight = 0;
            m_Sdl.GetRendererOutputSize(m_Renderer.Ptr, ref rWidth, ref rHeight);
            float rAspect = (float)rWidth / rHeight;

            // Render your texture (srcrect and dstrect set to null fills the window)
            Rectangle<int> srcRect = new(HBlankLength, minY, ScanlineLength - HBlankLength, maxY - minY + 1);
            Rectangle<int> dstRect = default;

            // fit 4:3 into dstRect while maintaining aspect ratio
            float targetAspect = PixelAspectRatio * (ScanlineLength - HBlankLength) / (maxY - minY + 1);
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

            m_Sdl.RenderCopy(m_Renderer.Ptr, m_Texture.Ptr, in srcRect, in dstRect);

            // Present to the screen
            m_Sdl.RenderPresent(m_Renderer.Ptr);
        }
    }
}

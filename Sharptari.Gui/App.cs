using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Threading;
using ImGuiNET;
using Sharptari.Lib;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;

namespace Sharptari.Gui;

internal unsafe sealed class App
    : IDisposable
{
    private const int ScanlineLength = 228;
    private const int HBlankLength = 68;
    private const int MaxScanlineCount = 512;
    private const float PixelAspectRatio = 12.0f / 7.0f;
    private const long CpuSpeed = 1193181;

    private IWindow? m_Window;
    private bool m_QuitRequested;
    private GL? m_Gl;
    private IInputContext? m_InputContext;
    private ImGuiController? m_ImguiController;

    private byte[]? m_RomBytes;
    private DummyAtariInput? m_AtariInput;
    private Atari2600? m_Atari2600;

    private uint m_MainTex;
    private readonly ColorAbgr8888[] m_FrameBuffer = new ColorAbgr8888[ScanlineLength * MaxScanlineCount];
    private Vector2 m_FrameBufferPos0;
    private Vector2 m_FrameBufferPos1;
    private Vector2 m_FrameBufferUV0;
    private Vector2 m_FrameBufferUV1;
    private TimeSpan m_CpuElapsedTime;
    private TimeSpan m_RealElapsedTime;

    public App()
    {
        m_Window = Window.Create(WindowOptions.Default with
        {
            Title = "Sharptari",
            Size = new Vector2D<int>(1920, 1080),
            WindowState = WindowState.Normal,
            WindowBorder = WindowBorder.Resizable,
            IsVisible = true,
            IsEventDriven = false,
        });
        m_Window.Load += OnWindowLoad;
        m_Window.Closing += OnWindowUnload;
        m_Window.FileDrop += OnFileDrop;
        m_Window.Update += OnWindowUpdate;
        m_Window.Render += OnWindowRender;
        m_Window.FramebufferResize += OnWindowFramebufferResize;

        m_Window.Initialize();
    }

    public void LoadRom(string filePath)
    {
        try
        {
            m_RomBytes = File.ReadAllBytes(filePath);
        }
        catch (Exception)
        {
            m_RomBytes = null;
        }

        if (m_RomBytes != null)
        {
            m_Atari2600 = new Atari2600(m_RomBytes, m_AtariInput ?? new DummyAtariInput());
            m_Atari2600.Reboot();
            m_CpuElapsedTime = TimeSpan.Zero;
            m_RealElapsedTime = TimeSpan.Zero;
        }
    }

    public void UnloadRom()
    {
        m_RomBytes = null;
        m_Atari2600 = null;
        m_CpuElapsedTime = TimeSpan.Zero;
        m_RealElapsedTime = TimeSpan.Zero;
    }

    private void OnWindowLoad()
    {
        if (m_Window == null)
            throw new NullReferenceException(nameof(m_Window));

        m_Gl = m_Window.CreateOpenGL();
        m_InputContext = m_Window.CreateInput();
        m_ImguiController = new ImGuiController(m_Gl, m_Window, m_InputContext);

        // create a texture:
        m_MainTex = m_Gl.GenTexture();
        m_Gl.BindTexture(TextureTarget.Texture2D, m_MainTex);
        m_Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        m_Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        m_Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        m_Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        m_Gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, ScanlineLength, MaxScanlineCount, 0, PixelFormat.Rgba, PixelType.UnsignedByte, null);
        m_Gl.BindTexture(TextureTarget.Texture2D, 0);

        m_AtariInput = new DummyAtariInput();
    }

    private void OnWindowUnload()
    {
        m_ImguiController?.Dispose();
        m_ImguiController = null;
        m_InputContext?.Dispose();
        m_InputContext = null;
        m_Gl?.Dispose();
        m_Gl = null;
    }

    private void OnFileDrop(string[] filePaths)
    {
        if (m_AtariInput != null && filePaths != null && filePaths.Length == 1)
        {
            LoadRom(filePaths[0]);
        }
    }

    private void OnWindowUpdate(double dt)
    {
        if (m_Gl == null)
            throw new NullReferenceException(nameof(m_Gl));

        if (m_Atari2600 != null)
        {
            // Process one frame of emulation:
            m_CpuElapsedTime += m_Atari2600.StepFrame(m_FrameBuffer);

            m_FrameBufferPos0.X = ScanlineLength - 1;
            m_FrameBufferPos1.X = 0;
            m_FrameBufferPos0.Y = MaxScanlineCount - 1;
            m_FrameBufferPos1.Y = 0;
            for (int i = 0; i < m_FrameBuffer.Length; ++i)
            {
                var pixel = m_FrameBuffer[i];
                if (pixel.A != 0)
                {
                    int x = i % ScanlineLength;
                    int y = i / ScanlineLength;
                    m_FrameBufferPos0.X = Math.Min(m_FrameBufferPos0.X, x);
                    m_FrameBufferPos1.X = Math.Max(m_FrameBufferPos1.X, x);
                    m_FrameBufferPos0.Y = Math.Min(m_FrameBufferPos0.Y, y);
                    m_FrameBufferPos1.Y = Math.Max(m_FrameBufferPos1.Y, y);
                }
            }
            m_FrameBufferPos0.Y = Math.Max(m_FrameBufferPos0.Y - 8, 0);
            m_FrameBufferPos1.Y = Math.Min(m_FrameBufferPos1.Y + 8, MaxScanlineCount - 1);

            m_FrameBufferUV0.X = (m_FrameBufferPos0.X + 0.5f) / ScanlineLength;
            m_FrameBufferUV0.Y = (m_FrameBufferPos0.Y + 0.5f) / MaxScanlineCount;
            m_FrameBufferUV1.X = (m_FrameBufferPos1.X - 0.5f) / ScanlineLength;
            m_FrameBufferUV1.Y = (m_FrameBufferPos1.Y - 0.5f) / MaxScanlineCount;

        }
        else
        {
            m_CpuElapsedTime = m_RealElapsedTime;
            Array.Clear(m_FrameBuffer, 0, m_FrameBuffer.Length);
            m_FrameBufferPos0 = default;
            m_FrameBufferPos1 = default;
            m_FrameBufferUV0 = default;
            m_FrameBufferUV1 = default;
        }

        m_Gl.BindTexture(TextureTarget.Texture2D, m_MainTex);
        m_Gl.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, ScanlineLength, MaxScanlineCount, PixelFormat.Rgba, PixelType.UnsignedByte, m_FrameBuffer);
        m_Gl.BindTexture(TextureTarget.Texture2D, 0);
    }

    private void OnWindowRender(double dt)
    {
        if (m_Window == null)
            throw new NullReferenceException(nameof(m_Window));
        if (m_ImguiController == null)
            throw new NullReferenceException(nameof(m_ImguiController));
        if (m_Gl == null)
            throw new NullReferenceException(nameof(m_Gl));

        m_ImguiController.Update((float)dt);

        m_Gl.ClearColor(Color.FromArgb(255, 0, 0, 0));
        m_Gl.Clear((uint)ClearBufferMask.ColorBufferBit);

        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenu("File"))
            {
                ImGui.BeginDisabled(m_Atari2600 == null);
                if (ImGui.MenuItem("Unload ROM"))
                {
                    m_Atari2600 = null;
                    m_RomBytes = null;
                    m_CpuElapsedTime = TimeSpan.Zero;
                    m_RealElapsedTime = TimeSpan.Zero;
                }
                ImGui.EndDisabled();

                ImGui.Separator();

                if (ImGui.MenuItem("Quit"))
                {
                    m_QuitRequested = true;
                }

                ImGui.EndMenu();
            }
            ImGui.EndMenuBar();
        }

        var bg = ImGui.GetBackgroundDrawList();

        var menuHeight = ImGui.GetFrameHeight();

        var windowMin = new Vector2(0, menuHeight);
        var windowMax = new Vector2(m_Window.Size.X, m_Window.Size.Y);

        var gameSize = new Vector2(PixelAspectRatio * (m_FrameBufferPos1.X - m_FrameBufferPos0.X + 1), m_FrameBufferPos1.Y - m_FrameBufferPos0.Y + 1);

        var (gameMin, gameMax) = Fit(gameSize, windowMin, windowMax);

        bg.AddImage((IntPtr)m_MainTex, gameMin, gameMax, m_FrameBufferUV0, m_FrameBufferUV1);

        m_ImguiController.Render();
    }

    private (Vector2 min, Vector2 max) Fit(Vector2 a, Vector2 rMin, Vector2 rMax)
    {
        var rCenter = 0.5f * (rMin + rMax);
        var rWidth = rMax.X - rMin.X;
        var rHeight = rMax.Y - rMin.Y;
        var rAspect = rWidth / rHeight;

        // fit into dstRect while maintaining aspect ratio
        float targetAspect = a.X / a.Y;
        var targetSize = new Vector2();
        if (rAspect > targetAspect)
        {
            targetSize.Y = rHeight;
            targetSize.X = rHeight * targetAspect;
        }
        else
        {
            targetSize.X = rWidth;
            targetSize.Y = rWidth / targetAspect;
        }

        return (
            rCenter - 0.5f * targetSize,
            rCenter + 0.5f * targetSize
        );
    }

    private void OnWindowFramebufferResize(Vector2D<int> size)
    {
        if (m_Gl == null)
            throw new NullReferenceException(nameof(m_Gl));
        m_Gl.Viewport(size);
    }

    public void Dispose()
    {
        m_Window?.Dispose();
        m_Window = null;
    }

    public void Run()
    {
        if (m_Window == null)
            throw new NullReferenceException(nameof(m_Window));

        var sw = Stopwatch.StartNew();
        var prevTime = sw.Elapsed;

        m_Window.Run
        (
            () =>
            {
                do
                {
                    // Process window events:
                    m_Window.DoEvents();

                    var nowTime = sw.Elapsed;
                    m_RealElapsedTime += nowTime - prevTime;
                    prevTime = nowTime;

                    var timeRemaining = m_CpuElapsedTime - m_RealElapsedTime;
                    if (timeRemaining > TimeSpan.FromSeconds(0.2))
                    {
                        // If we're more than 200ms ahead, something weird has happened (no frame rendered for a long time)
                        // just reset the timers to avoid a long sleep:
                        m_RealElapsedTime = m_CpuElapsedTime;
                        Console.WriteLine($"Warning: Emulation is more than 200ms ahead of real time. Resetting timers.");
                    }
                    else if (timeRemaining > TimeSpan.FromMilliseconds(5))
                    {
                        Thread.Sleep(timeRemaining - TimeSpan.FromMilliseconds(2));
                    }
                    else if (timeRemaining <= TimeSpan.FromSeconds(-0.1))
                    {
                        // If we're more than 100ms behind, (user dragged the window around perhaps?)
                        // just reset the timers to avoid a fast-forward:
                        m_RealElapsedTime = m_CpuElapsedTime;
                        Console.WriteLine($"Warning: Emulation is more than 100ms behind real time. Resetting timers.");
                    }
                }
                while (!m_Window.IsClosing && m_RealElapsedTime < m_CpuElapsedTime);

                if (!m_Window.IsClosing)
                {
                    m_Window.DoUpdate();
                }

                if (!m_Window.IsClosing)
                {
                    m_Window.DoRender();
                }

                if (m_QuitRequested)
                {
                    m_Window.Close();
                }
            }
        );

        m_Window.DoEvents();
        m_Window.Reset();
    }
}

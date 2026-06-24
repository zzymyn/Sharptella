using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Threading;
using ImGuiNET;
using Sharptella.Lib;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenAL;
using Silk.NET.OpenGL;
using Silk.NET.OpenGL.Extensions.ImGui;
using Silk.NET.Windowing;

namespace Sharptella.Gui;

internal unsafe sealed class App
    : IDisposable
{
    private const int ScanlineLength = 228;
    private const int MaxScanlineCount = 512;
    private const float PixelAspectRatio = 12.0f / 7.0f;

    private const int AudioSampleRate = 31399;

    private IWindow? m_Window;
    private bool m_QuitRequested;
    private GL? m_Gl;
    private IInputContext? m_InputContext;
    private ImGuiController? m_ImguiController;
    private ALContext? m_Alc;
    private AL? m_Al;
    private Device* m_AlDevice;
    private Context* m_AlContext;
    private readonly List<uint> m_AlBuffers = [];
    private readonly List<uint> m_FreeAlBuffers = [];

    private byte[]? m_RomBytes;
    private SilkAtariInput? m_AtariInput;
    private Atari2600? m_Atari2600;

    private uint m_MainTex;
    private readonly ColorAbgr8888[] m_FrameBuffer = new ColorAbgr8888[ScanlineLength * MaxScanlineCount];
    private Vector2 m_FrameBufferPos0;
    private Vector2 m_FrameBufferPos1;
    private Vector2 m_FrameBufferUV0;
    private Vector2 m_FrameBufferUV1;
    private TimeSpan m_CpuElapsedTime;
    private TimeSpan m_RealElapsedTime;

    private AudioStream[] m_AudioStreams = [new(), new()];

    private string m_LastError = "";

    // debugging:
    private bool m_ViewDebugTools;
    private bool m_PauseEmulation;
    private bool m_StepFrameRequested;
    private bool m_StepScanlineRequested;
    private bool m_StepInstructionRequested;
    private bool m_StepCpuRequested;
    private bool m_StepTiaCycleRequested;

    // memory viewer:
    private bool m_ViewMemory;
    private Mos6502Registers m_CurrDebugCpuRegisters;
    private Mos6502Registers m_PrevDebugCpuRegisters;
    private byte[] m_CurrDebugRam = new byte[128];
    private byte[] m_PrevDebugRam = new byte[128];

    public App()
    {
        m_Window = Window.Create(WindowOptions.Default with
        {
            Title = "Sharptella",
            Size = new Vector2D<int>(1920, 1080),
            WindowState = WindowState.Normal,
            WindowBorder = WindowBorder.Resizable,
            IsVisible = true,
            IsEventDriven = false,
            API = new GraphicsAPI(ContextAPI.OpenGL, new APIVersion(4, 1)), // OpenGL 4.1 for macOS compatibility
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
        if (m_AtariInput == null)
            return;

        try
        {
            m_RomBytes = File.ReadAllBytes(filePath);
        }
        catch (Exception ex)
        {
            m_RomBytes = null;
            m_LastError = ex.Message;
        }

        if (m_RomBytes != null)
        {
            try
            {
                m_Atari2600 = new Atari2600(m_RomBytes, m_AtariInput);
                m_Atari2600.Reboot();
                m_CpuElapsedTime = TimeSpan.Zero;
                m_RealElapsedTime = TimeSpan.Zero;
                m_LastError = "";
            }
            catch (Exception ex)
            {
                m_Atari2600 = null;
                m_RomBytes = null;
                m_LastError = ex.Message;
            }
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

        m_Alc = ALContext.GetApi(soft: true);
        m_Al = AL.GetApi(soft: true);
        if (m_Alc != null)
        {
            m_AlDevice = m_Alc.OpenDevice("");
            if (m_AlDevice != null)
            {
                m_AlContext = m_Alc.CreateContext(m_AlDevice, null);
                if (m_AlContext != null)
                {
                    m_Alc.MakeContextCurrent(m_AlContext);
                    foreach (var v in m_AudioStreams)
                    {
                        v.AlSource = m_Al.GenSource();
                    }
                }
            }
        }

        // create game view texture:
        m_MainTex = m_Gl.GenTexture();
        m_Gl.BindTexture(TextureTarget.Texture2D, m_MainTex);
        m_Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        m_Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        m_Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        m_Gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        m_Gl.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, ScanlineLength, MaxScanlineCount, 0, PixelFormat.Rgba, PixelType.UnsignedByte, null);
        m_Gl.BindTexture(TextureTarget.Texture2D, 0);

        m_AtariInput = new SilkAtariInput(m_InputContext);
        m_AtariInput.RebootPressed += OnRebootPressed;
    }

    private void OnWindowUnload()
    {
        foreach (var alBuffer in m_AlBuffers)
        {
            m_Al?.DeleteBuffer(alBuffer);
        }
        m_AlBuffers.Clear();
        foreach (var v in m_AudioStreams)
        {
            m_Al?.DeleteSource(v.AlSource);
            v.AlSource = 0;
        }
        m_Alc?.DestroyContext(m_AlContext);
        m_AlContext = null;
        m_Alc?.CloseDevice(m_AlDevice);
        m_AlDevice = null;
        m_Al?.Dispose();
        m_Al = null;
        m_Alc?.Dispose();
        m_Alc = null;
        m_AtariInput?.Dispose();
        m_AtariInput = null;
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

    private void OnRebootPressed()
    {
        m_Atari2600?.Reboot();
    }

    private void OnWindowUpdate(double dt)
    {
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

        DoGui();
        DoEmu();
        DoAudio();

        var bg = ImGui.GetBackgroundDrawList();

        var menuHeight = ImGui.GetFrameHeight();

        var windowMin = new Vector2(0, menuHeight);
        var windowMax = new Vector2(m_Window.Size.X, m_Window.Size.Y);

        var gameSize = new Vector2(PixelAspectRatio * (m_FrameBufferPos1.X - m_FrameBufferPos0.X + 1), m_FrameBufferPos1.Y - m_FrameBufferPos0.Y + 1);

        var (gameMin, gameMax) = Fit(gameSize, windowMin, windowMax);

        bg.AddImage((IntPtr)m_MainTex, gameMin, gameMax, m_FrameBufferUV0, m_FrameBufferUV1);

        m_Gl.ClearColor(Color.FromArgb(255, 0, 0, 0));
        m_Gl.Clear((uint)ClearBufferMask.ColorBufferBit);
        m_ImguiController.Render();
    }

    private void DoGui()
    {
        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("Unload ROM", m_Atari2600 != null))
                {
                    m_Atari2600 = null;
                    m_RomBytes = null;
                    m_CpuElapsedTime = TimeSpan.Zero;
                    m_RealElapsedTime = TimeSpan.Zero;
                }

                ImGui.Separator();

                if (ImGui.MenuItem("Quit"))
                {
                    m_QuitRequested = true;
                }

                ImGui.EndMenu();
            }
            if (ImGui.BeginMenu("Debug"))
            {
                ImGui.MenuItem("Debug Tools", "", ref m_ViewDebugTools);
                ImGui.MenuItem("View Memory", "", ref m_ViewMemory);
                ImGui.EndMenu();
            }
            ImGui.EndMenuBar();
        }

        // centered, modal dialog when no ROM is loaded:
        if (m_Atari2600 == null)
        {
            var windowSize = ImGui.GetIO().DisplaySize;
            var dialogPos = 0.5f * windowSize;
            ImGui.SetNextWindowPos(dialogPos, ImGuiCond.Always, new Vector2(0.5f, 0.5f));
            ImGui.Begin("No ROM Loaded",
                ImGuiWindowFlags.NoMove
                | ImGuiWindowFlags.NoResize
                | ImGuiWindowFlags.NoCollapse
                | ImGuiWindowFlags.AlwaysAutoResize
                | ImGuiWindowFlags.NoTitleBar
                | ImGuiWindowFlags.NoSavedSettings);
            if (string.IsNullOrEmpty(m_LastError))
            {
                ImGui.TextColored(new Vector4(1.0f, 0.0f, 0.0f, 1.0f), "No ROM Loaded");
                ImGui.Separator();
                ImGui.Text("Drag and drop a ROM file to load it.");
            }
            else
            {
                ImGui.TextColored(new Vector4(1.0f, 0.0f, 0.0f, 1.0f), "Error");
                ImGui.Separator();
                ImGui.Text(m_LastError);
                ImGui.Separator();
                ImGui.Text("Drag and drop another ROM file to load it.");
            }
            ImGui.End();
        }

        {
            var windowSize = ImGui.GetIO().DisplaySize;
            var dialogPos = new Vector2(0, windowSize.Y);
            ImGui.SetNextWindowPos(dialogPos, ImGuiCond.Always, new Vector2(0, 1.0f));
            if (ImGui.Begin("Console Switches",
                ImGuiWindowFlags.NoMove
                | ImGuiWindowFlags.NoResize
                | ImGuiWindowFlags.NoCollapse
                | ImGuiWindowFlags.NoSavedSettings))
            {
                if (ImGui.Button("Reset (F1)"))
                {
                    m_Atari2600?.Reboot();
                }

                bool tvBW = m_AtariInput?.TvTypeSwitch == true;
                if (ImGui.Checkbox("TV Type B/W (F2)", ref tvBW))
                {
                    m_AtariInput?.ToggleTvTypeSwitch();
                }

                bool player0Hard = m_AtariInput?.PlayerDifficultySwitchA == false;
                if (ImGui.Checkbox("Player 1 Hard Mode (F3)", ref player0Hard))
                {
                    m_AtariInput?.TogglePlayerDifficultySwitchA();
                }

                bool player1Hard = m_AtariInput?.PlayerDifficultySwitchB == false;
                if (ImGui.Checkbox("Player 2 Hard Mode (F4)", ref player1Hard))
                {
                    m_AtariInput?.TogglePlayerDifficultySwitchB();
                }

                if (ImGui.Button("Game Select (F5)"))
                {
                    m_AtariInput?.PressGameSelectSwitch();
                }

                if (ImGui.Button("Game Reset (F6)"))
                {
                    m_AtariInput?.PressGameResetSwitch();
                }
            }
            ImGui.End();
        }

        if (m_ViewDebugTools)
        {
            if (ImGui.Begin("Debug Tools", ref m_ViewDebugTools, ImGuiWindowFlags.AlwaysAutoResize))
            {
                if (ImGui.BeginTable("DebugToolsTable", 6))
                {
                    ImGui.TableNextRow();

                    ImGui.TableNextColumn();
                    if (ImGui.Button(m_PauseEmulation ? "Resume" : "Pause"))
                    {
                        m_PauseEmulation = !m_PauseEmulation;
                    }

                    ImGui.BeginDisabled(!m_PauseEmulation);
                    ImGui.TableNextColumn();
                    if (ImGui.Button("Step Frame"))
                    {
                        m_StepFrameRequested = true;
                    }
                    ImGui.TableNextColumn();
                    if (ImGui.Button("Step Scanline"))
                    {
                        m_StepScanlineRequested = true;
                    }
                    ImGui.TableNextColumn();
                    if (ImGui.Button("Step Instr"))
                    {
                        m_StepInstructionRequested = true;
                    }
                    ImGui.TableNextColumn();
                    if (ImGui.Button("Step CPU"))
                    {
                        m_StepCpuRequested = true;
                    }
                    ImGui.TableNextColumn();
                    if (ImGui.Button("Step TIA"))
                    {
                        m_StepTiaCycleRequested = true;
                    }
                    ImGui.EndDisabled();

                    ImGui.EndTable();
                }

                ImGui.End();
            }
        }

        if (m_ViewMemory)
        {
            if (ImGui.Begin("Memory Viewer", ref m_ViewMemory, ImGuiWindowFlags.AlwaysAutoResize))
            {
                var r = m_CurrDebugCpuRegisters;

                ImGui.Text("Registers:");

                if (ImGui.BeginTable("RegistersTable", 11))
                {
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    ImGui.Text("A");
                    ImGui.TableNextColumn();
                    ImGui.Text("X");
                    ImGui.TableNextColumn();
                    ImGui.Text("Y");
                    ImGui.TableNextColumn();
                    ImGui.Text("S");
                    ImGui.TableNextColumn();
                    ImGui.Text("PC");
                    ImGui.TableNextColumn();
                    ImGui.Text("N");
                    ImGui.TableNextColumn();
                    ImGui.Text("V");
                    ImGui.TableNextColumn();
                    ImGui.Text("D");
                    ImGui.TableNextColumn();
                    ImGui.Text("I");
                    ImGui.TableNextColumn();
                    ImGui.Text("Z");
                    ImGui.TableNextColumn();
                    ImGui.Text("C");

                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    if (r.A != m_PrevDebugCpuRegisters.A)
                        ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0x7700FF00);
                    ImGui.Text($"{r.A:X2}");
                    ImGui.TableNextColumn();
                    if (r.X != m_PrevDebugCpuRegisters.X)
                        ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0x7700FF00);
                    ImGui.Text($"{r.X:X2}");
                    ImGui.TableNextColumn();
                    if (r.Y != m_PrevDebugCpuRegisters.Y)
                        ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0x7700FF00);
                    ImGui.Text($"{r.Y:X2}");
                    ImGui.TableNextColumn();
                    if (r.S != m_PrevDebugCpuRegisters.S)
                        ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0x7700FF00);
                    ImGui.Text($"{r.S:X2}");
                    ImGui.TableNextColumn();
                    if (r.PC != m_PrevDebugCpuRegisters.PC)
                        ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0x7700FF00);
                    ImGui.Text($"{r.PC:X4}");
                    ImGui.TableNextColumn();
                    if (r.PNegative != m_PrevDebugCpuRegisters.PNegative)
                        ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0x7700FF00);
                    ImGui.Text(r.PNegative ? "1" : "-");
                    ImGui.TableNextColumn();
                    if (r.POverflow != m_PrevDebugCpuRegisters.POverflow)
                        ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0x7700FF00);
                    ImGui.Text(r.POverflow ? "1" : "-");
                    ImGui.TableNextColumn();
                    if (r.PDecimal != m_PrevDebugCpuRegisters.PDecimal)
                        ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0x7700FF00);
                    ImGui.Text(r.PDecimal ? "1" : "-");
                    ImGui.TableNextColumn();
                    if (r.PInterruptDisable != m_PrevDebugCpuRegisters.PInterruptDisable)
                        ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0x7700FF00);
                    ImGui.Text(r.PInterruptDisable ? "1" : "-");
                    ImGui.TableNextColumn();
                    if (r.PZero != m_PrevDebugCpuRegisters.PZero)
                        ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0x7700FF00);
                    ImGui.Text(r.PZero ? "1" : "-");
                    ImGui.TableNextColumn();
                    if (r.PCarry != m_PrevDebugCpuRegisters.PCarry)
                        ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0x7700FF00);
                    ImGui.Text(r.PCarry ? "1" : "-");

                    ImGui.EndTable();
                }

                ImGui.Separator();

                ImGui.Text("RAM:");
                if (ImGui.BeginTable("RAMTable", 17))
                {
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    for (int j = 0; j < 16; ++j)
                    {
                        ImGui.TableNextColumn();
                        ImGui.Text($"+{j:X}");
                    }

                    for (int i = 0; i < m_CurrDebugRam.Length; i += 16)
                    {
                        ImGui.TableNextRow();

                        ImGui.TableNextColumn();
                        ImGui.Text($"{0x80 + i:X2}:");

                        for (int j = 0; j < 16 && i + j < m_CurrDebugRam.Length; ++j)
                        {
                            ImGui.TableNextColumn();
                            if (m_CurrDebugRam[i + j] != m_PrevDebugRam[i + j])
                                ImGui.TableSetBgColor(ImGuiTableBgTarget.CellBg, 0x7700FF00);
                            ImGui.Text($"{m_CurrDebugRam[i + j]:X2}");
                        }
                    }

                    ImGui.EndTable();
                }
            }
            ImGui.End();
        }
    }

    private void DoEmu()
    {
        if (m_Gl == null)
            throw new NullReferenceException(nameof(m_Gl));

        if (m_Atari2600 != null)
        {
            bool stepped = true;

            // Process the emulation:
            if (!m_ViewDebugTools)
            {
                m_CpuElapsedTime += m_Atari2600.StepFrame();
            }
            else if (!m_PauseEmulation || m_StepFrameRequested)
            {
                m_CpuElapsedTime += m_Atari2600.DebugStepFrame();
            }
            else if (m_StepScanlineRequested)
            {
                m_CpuElapsedTime += m_Atari2600.DebugStepScanline();
            }
            else if (m_StepInstructionRequested)
            {
                m_CpuElapsedTime += m_Atari2600.DebugStepInstruction();
            }
            else if (m_StepCpuRequested)
            {
                m_CpuElapsedTime += m_Atari2600.DebugStepCpu();
            }
            else if (m_StepTiaCycleRequested)
            {
                m_CpuElapsedTime += m_Atari2600.DebugStep(out _);
            }
            else
            {
                stepped = false;
            }

            if (stepped)
            {
                m_AtariInput?.Step();
            }

            m_Atari2600.ReadFrame(m_FrameBuffer);
            m_AudioStreams[0].BufferLength = m_Atari2600.ReadAudio0(m_AudioStreams[0].Buffer);
            m_AudioStreams[1].BufferLength = m_Atari2600.ReadAudio1(m_AudioStreams[1].Buffer);

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
                m_FrameBuffer[i].A = 255;
            }
            m_FrameBufferPos0.Y = Math.Max(m_FrameBufferPos0.Y - 8, 0);
            m_FrameBufferPos1.Y = Math.Min(m_FrameBufferPos1.Y + 8, MaxScanlineCount - 1);

            m_FrameBufferUV0.X = (m_FrameBufferPos0.X + 0.5f) / ScanlineLength;
            m_FrameBufferUV0.Y = (m_FrameBufferPos0.Y + 0.5f) / MaxScanlineCount;
            m_FrameBufferUV1.X = (m_FrameBufferPos1.X - 0.5f) / ScanlineLength;
            m_FrameBufferUV1.Y = (m_FrameBufferPos1.Y - 0.5f) / MaxScanlineCount;

            if (stepped && m_ViewMemory)
            {
                m_PrevDebugCpuRegisters = m_CurrDebugCpuRegisters;
                Array.Copy(m_CurrDebugRam, m_PrevDebugRam, m_CurrDebugRam.Length);

                m_CurrDebugCpuRegisters = m_Atari2600.DebugCpuRegisters;
                var debugRam = m_Atari2600.DebugRam;
                for (int i = 0; i < debugRam.Count; ++i)
                {
                    m_CurrDebugRam[i] = debugRam[i];
                }
            }
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

        m_StepFrameRequested = false;
        m_StepScanlineRequested = false;
        m_StepInstructionRequested = false;
        m_StepCpuRequested = false;
        m_StepTiaCycleRequested = false;

        if (m_ViewDebugTools && m_PauseEmulation)
        {
            m_RealElapsedTime = m_CpuElapsedTime;
        }
    }

    private void DoAudio()
    {
        if (m_Al == null || m_AlBuffers == null)
            return;

        foreach (var v in m_AudioStreams)
        {
            if (v.AlSource != 0)
            {
                m_Al.GetSourceProperty(v.AlSource, GetSourceInteger.BuffersProcessed, out var processed);

                for (var j = 0; j < processed; ++j)
                {
                    uint alBuffer = 0;
                    m_Al.SourceUnqueueBuffers(v.AlSource, 1, &alBuffer);
                    m_FreeAlBuffers.Add(alBuffer);
                }
            }

            if (v.BufferLength > 0)
            {
                var alBuffer = GetFreeAlBuffer();
                if (alBuffer != 0)
                {
                    fixed (byte* ptr = v.Buffer)
                    {
                        m_Al.BufferData(alBuffer, BufferFormat.Mono8, ptr, v.BufferLength * sizeof(byte), AudioSampleRate);
                    }

                    m_Al.SourceQueueBuffers(v.AlSource, 1, &alBuffer);

                    m_Al.GetSourceProperty(v.AlSource, GetSourceInteger.SourceState, out var state);
                    if (state != (int)SourceState.Playing)
                    {
                        m_Al.SourcePlay(v.AlSource);
                    }
                }
                v.BufferLength = 0;
            }
        }
    }

    private uint GetFreeAlBuffer()
    {
        uint alBuffer;

        if (m_FreeAlBuffers.Count > 0)
        {
            alBuffer = m_FreeAlBuffers[0];
            m_FreeAlBuffers.RemoveAt(0);
        }
        else
        {
            alBuffer = m_Al!.GenBuffer();
            m_AlBuffers.Add(alBuffer);
        }

        return alBuffer;
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

    private class AudioStream
    {
        public uint AlSource;
        public readonly byte[] Buffer = new byte[2 * MaxScanlineCount];
        public int BufferLength;

        public AudioStream()
        {
        }
    }
}

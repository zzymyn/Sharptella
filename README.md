# Sharptari

A small Atari 2600 emulator project written in pure C#.

## What This Is

Sharptari is an emulator created to learn about the Atari 2600 and its components, including:

- 6502/6507 CPU behavior
- Low-Level behaviour of Atari 2600 components (TIA, RIOT, bus, ROM mapping)
- Rendering and audio pipelines in managed code
- Emulator architecture in modern .NET

## Core Principles

- Pure C#: no native emulator core
- Learning-first development
- Practical correctness over absolute hardware-accuracy

## Project Structure

- `Sharptari.Lib`: core emulator library (CPU, bus, RIOT, TIA, ROM, system glue)
- `Sharptari.Gui`: desktop GUI frontend using Silk.NET + ImGui
- `Sharptari.CpuTest`: CPU test runner for JSON-based opcode/state test cases

## Features

- A complete basic Atari 2600 emulator
- Drag and drop ROM loading in the GUI
- Keyboard/gamepad input
- CPU verification runner for large JSON test sets
- Some simple debugging tools (memory viewer, stepper)

## Limitations

- Current ROM support is limited to non-bank switching ROMs (ROMs up to 4KB). Bank switching support is planned but not yet implemented.
- I haven't hooked up player 2 input yet, so only single player games are currently playable.
- PAL ROMs will hard incorrect colors and timing.

## Accuracy Expectations

Sharptari is not necessarily a high-accuracy emulator, although care has been taken to implement the CPU and TIA correctly. Homebrew or cutting edge ROMs using advanced tricks related to CPU and TIA timing may not work correctly. The emulator is intended to be a learning tool, not a preservation tool.

If your goal is strict hardware fidelity or TAS-level determinism, this is currently the wrong target.

If your goal is to learn emulator internals in approachable C#, this is a good fit.

## Requirements

- .NET SDK 10.0+
- Windows/macOS/Linux environment capable of running Silk.NET OpenGL windowing

## Build

```bash
dotnet build Sharptari.sln
```

## Run GUI

```bash
dotnet run --project Sharptari.Gui
```

Run with a ROM path:

```bash
dotnet run --project Sharptari.Gui -- path/to/rom.bin
```

Or launch the GUI and drag-and-drop a ROM file into the window.

## Run CPU Tests

The CPU test runner expects JSON test files (or directories/wildcards that resolve to JSON files).

The JSON CPU tests used by this project come from the SingleStepTests 65x02 repository:
https://github.com/SingleStepTests/65x02

```bash
dotnet run --project Sharptari.CpuTest -- path/to/tests/*.json
```

You can also pass:

- one or more specific JSON files
- a directory containing JSON files
- no arguments (it will scan the current directory for `*.json`)

## Input (Default Keyboard)

- Arrow keys: Player 0 movement
- Space: Player 0 button
- F1: Reboot
- F2: Toggle TV type switch
- F3: Toggle player difficulty A
- F4: Toggle player difficulty B
- F5: Game select
- F6: Game reset

## Notes

- ROM files are not included.
- This repository is for educational development, not archival preservation accuracy.
- Expect ongoing refactoring as understanding improves.

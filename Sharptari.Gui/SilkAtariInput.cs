using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sharptari.Lib;
using Silk.NET.Input;

namespace Sharptari.Gui;

internal sealed class SilkAtariInput
    : IAtariInput
    , IDisposable
{
    private readonly IInputContext m_InputContext;
    private readonly InputVector m_Player0Up = new();
    private readonly InputVector m_Player0Left = new();
    private readonly InputVector m_Player0Right = new();
    private readonly InputVector m_Player0Down = new();
    private readonly InputVector m_Player0Button = new();
    private readonly InputVector m_Player1Up = new();
    private readonly InputVector m_Player1Left = new();
    private readonly InputVector m_Player1Right = new();
    private readonly InputVector m_Player1Down = new();
    private readonly InputVector m_Player1Button = new();
    private readonly InputVector m_GameResetSwitch = new();
    private readonly InputVector m_GameSelectSwitch = new();
    private bool m_TvTypeSwitch;
    private bool m_PlayerDifficultySwitchA = true;
    private bool m_PlayerDifficultySwitchB = true;

    public bool Player0Up => m_Player0Up.IsActive;
    public bool Player0Left => m_Player0Left.IsActive;
    public bool Player0Right => m_Player0Right.IsActive;
    public bool Player0Down => m_Player0Down.IsActive;
    public bool Player0Button => m_Player0Button.IsActive;

    public bool Player1Up => m_Player1Up.IsActive;
    public bool Player1Left => m_Player1Left.IsActive;
    public bool Player1Right => m_Player1Right.IsActive;
    public bool Player1Down => m_Player1Down.IsActive;
    public bool Player1Button => m_Player1Button.IsActive;

    public bool GameResetSwitch => m_GameResetSwitch.IsActive;
    public bool GameSelectSwitch => m_GameSelectSwitch.IsActive;
    public bool TvTypeSwitch => m_TvTypeSwitch;
    public bool PlayerDifficultySwitchA => m_PlayerDifficultySwitchA;
    public bool PlayerDifficultySwitchB => m_PlayerDifficultySwitchB;

    public SilkAtariInput(IInputContext inputContext)
    {
        m_InputContext = inputContext;
        foreach (var kb in m_InputContext.Keyboards)
        {
            OnConnectionChanged(kb, true);
        }
        foreach (var gp in m_InputContext.Gamepads)
        {
            OnConnectionChanged(gp, true);
        }
        m_InputContext.ConnectionChanged += OnConnectionChanged;
    }

    public void Dispose()
    {
        foreach (var kb in m_InputContext.Keyboards)
        {
            OnConnectionChanged(kb, false);
        }
        foreach (var gp in m_InputContext.Gamepads)
        {
            OnConnectionChanged(gp, false);
        }
    }

    public void ToggleTvTypeSwitch()
    {
        m_TvTypeSwitch = !m_TvTypeSwitch;
    }

    public void TogglePlayerDifficultySwitchA()
    {
        m_PlayerDifficultySwitchA = !m_PlayerDifficultySwitchA;
    }

    public void TogglePlayerDifficultySwitchB()
    {
        m_PlayerDifficultySwitchB = !m_PlayerDifficultySwitchB;
    }

    public void PressGameSelectSwitch()
    {
        m_GameSelectSwitch.Set(null, 0, true);
    }

    public void ReleaseGameSelectSwitch()
    {
        m_GameSelectSwitch.Set(null, 0, false);
    }

    public void PressGameResetSwitch()
    {
        m_GameResetSwitch.Set(null, 0, true);
    }

    public void ReleaseGameResetSwitch()
    {
        m_GameResetSwitch.Set(null, 0, false);
    }

    private void KeyboardKeyDown(IKeyboard keyboard, Key key, int arg3)
    {
        KeyboardKey(keyboard, key, true);
    }

    private void KeyboardKeyUp(IKeyboard keyboard, Key key, int arg3)
    {
        KeyboardKey(keyboard, key, false);
    }

    private void KeyboardKey(IKeyboard keyboard, Key key, bool down)
    {
        switch (key)
        {
            case Key.Left:
                m_Player0Left.Set(keyboard, 0, down);
                break;
            case Key.Right:
                m_Player0Right.Set(keyboard, 0, down);
                break;
            case Key.Up:
                m_Player0Up.Set(keyboard, 0, down);
                break;
            case Key.Down:
                m_Player0Down.Set(keyboard, 0, down);
                break;
            case Key.Space:
                m_Player0Button.Set(keyboard, 0, down);
                break;
            case Key.F2:
                if (down)
                {
                    m_TvTypeSwitch = !m_TvTypeSwitch;
                }
                break;
            case Key.F3:
                if (down)
                {
                    m_PlayerDifficultySwitchA = !m_PlayerDifficultySwitchA;
                }
                break;
            case Key.F4:
                if (down)
                {
                    m_PlayerDifficultySwitchB = !m_PlayerDifficultySwitchB;
                }
                break;
            case Key.F5:
                m_GameSelectSwitch.Set(keyboard, 0, down);
                break;
            case Key.F6:
                m_GameResetSwitch.Set(keyboard, 0, down);
                break;
        }
    }

    private void OnGamepadButtonDown(IGamepad gamepad, Button button)
    {
        OnGamepadButton(gamepad, button, true);
    }

    private void OnGamepadButtonUp(IGamepad gamepad, Button button)
    {
        OnGamepadButton(gamepad, button, false);
    }

    private void OnGamepadButton(IGamepad gamepad, Button button, bool down)
    {
        switch (button.Name)
        {
            case ButtonName.DPadUp:
                m_Player0Up.Set(gamepad, 0, down);
                break;
            case ButtonName.DPadLeft:
                m_Player0Left.Set(gamepad, 0, down);
                break;
            case ButtonName.DPadRight:
                m_Player0Right.Set(gamepad, 0, down);
                break;
            case ButtonName.DPadDown:
                m_Player0Down.Set(gamepad, 0, down);
                break;
            case ButtonName.A:
                m_Player0Button.Set(gamepad, 0, down);
                break;
            case ButtonName.B:
                m_Player0Button.Set(gamepad, 1, down);
                break;
            case ButtonName.X:
                m_Player0Button.Set(gamepad, 2, down);
                break;
            case ButtonName.Y:
                m_Player0Button.Set(gamepad, 3, down);
                break;
            case ButtonName.Start:
                m_GameResetSwitch.Set(gamepad, 0, down);
                break;
            case ButtonName.Back:
                m_GameSelectSwitch.Set(gamepad, 0, down);
                break;
        }
    }

    private void OnConnectionChanged(IInputDevice device, bool connected)
    {
        if (connected)
        {
            if (device is IKeyboard keyboard)
            {
                keyboard.KeyDown += KeyboardKeyDown;
                keyboard.KeyUp += KeyboardKeyUp;
            }
            else if (device is IGamepad gamepad)
            {
                gamepad.ButtonDown += OnGamepadButtonDown;
                gamepad.ButtonUp += OnGamepadButtonUp;
            }
        }
        else
        {
            if (device is IKeyboard keyboard)
            {
                keyboard.KeyDown -= KeyboardKeyDown;
                keyboard.KeyUp -= KeyboardKeyUp;
            }
            else if (device is IGamepad gamepad)
            {
                gamepad.ButtonDown -= OnGamepadButtonDown;
                gamepad.ButtonUp -= OnGamepadButtonUp;
            }

            m_Player0Up.Clear(device);
            m_Player0Left.Clear(device);
            m_Player0Right.Clear(device);
            m_Player0Down.Clear(device);
            m_Player0Button.Clear(device);
            m_Player1Up.Clear(device);
            m_Player1Left.Clear(device);
            m_Player1Right.Clear(device);
            m_Player1Down.Clear(device);
            m_Player1Button.Clear(device);
            m_GameResetSwitch.Clear(device);
            m_GameSelectSwitch.Clear(device);
        }
    }

    private readonly struct InputVector
    {
        private readonly HashSet<(IInputDevice? device, int id)> m_Inputs = [];

        public bool IsActive => m_Inputs.Count > 0;

        public InputVector()
        {
        }

        public void Clear(IInputDevice device)
        {
            // copy to avoid modifying collection while enumerating:
            foreach (var input in m_Inputs.ToList())
            {
                if (input.device == device)
                {
                    m_Inputs.Remove(input);
                }
            }
        }

        public void Set(IInputDevice? device, int id, bool down)
        {
            if (down)
            {
                m_Inputs.Add((device, id));
            }
            else
            {
                m_Inputs.Remove((device, id));
            }
        }
    }
}

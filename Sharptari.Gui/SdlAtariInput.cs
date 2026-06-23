//using System;
//using System.Collections.Generic;
//using System.Text;
//using Sharptari.Lib;
//using Silk.NET.SDL;

//namespace Sharptari.Gui;

//internal sealed class SdlAtariInput
//    : IAtariInput
//{
//    public bool Player0Up { get; set; }
//    public bool Player0Left { get; set; }
//    public bool Player0Right { get; set; }
//    public bool Player0Down { get; set; }
//    public bool Player0Button { get; set; }

//    public bool Player1Up { get; set; }
//    public bool Player1Left { get; set; }
//    public bool Player1Right { get; set; }
//    public bool Player1Down { get; set; }
//    public bool Player1Button { get; set; }

//    public bool GameResetSwitch { get; set; }
//    public bool GameSelectSwitch { get; set; }
//    public bool TvTypeSwitch { get; set; }
//    public bool PlayerDifficultySwitchA { get; set; } = true;
//    public bool PlayerDifficultySwitchB { get; set; } = true;

//    public void ProcessKeyboardEvent(Event ev)
//    {
//        var down = ev.Type == (uint)EventType.Keydown;
//        switch (ev.Key.Keysym.Scancode)
//        {
//            case Scancode.ScancodeLeft:
//                Player0Left = down;
//                break;
//            case Scancode.ScancodeRight:
//                Player0Right = down;
//                break;
//            case Scancode.ScancodeUp:
//                Player0Up = down;
//                break;
//            case Scancode.ScancodeDown:
//                Player0Down = down;
//                break;
//            case Scancode.ScancodeSpace:
//                Player0Button = down;
//                break;

//            case Scancode.ScancodeF2:
//                if (down)
//                {
//                    TvTypeSwitch = !TvTypeSwitch;
//                }
//                break;
//            case Scancode.ScancodeF4:
//                if (down)
//                {
//                    PlayerDifficultySwitchA = !PlayerDifficultySwitchA;
//                }
//                break;
//            case Scancode.ScancodeF9:
//                if (down)
//                {
//                    PlayerDifficultySwitchB = !PlayerDifficultySwitchB;
//                }
//                break;
//            case Scancode.ScancodeF11:
//                GameSelectSwitch = down;
//                break;
//            case Scancode.ScancodeF12:
//                GameResetSwitch = down;
//                break;
//        }
//    }
//}

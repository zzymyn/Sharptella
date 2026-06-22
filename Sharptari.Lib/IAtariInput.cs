using System;
using System.Collections.Generic;
using System.Text;

namespace Sharptari.Lib;

public interface IAtariInput
{
    bool Player0Up { get; }
    bool Player0Left { get; }
    bool Player0Right { get; }
    bool Player0Down { get; }
    bool Player0Button { get; }

    bool Player1Up { get; }
    bool Player1Left { get; }
    bool Player1Right { get; }
    bool Player1Down { get; }
    bool Player1Button { get; }

    bool GameResetSwitch { get; }
    bool GameSelectSwitch { get; }
    bool TvTypeSwitch { get; }
    bool PlayerDifficultySwitchA { get; }
    bool PlayerDifficultySwitchB { get; }
}

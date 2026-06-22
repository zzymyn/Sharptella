using System;
using System.Collections.Generic;
using System.Text;

namespace Sharptari.Lib;

public sealed class DummyAtariInput
    : IAtariInput
{
    public bool Player0Up => false;
    public bool Player0Left => false;
    public bool Player0Right => false;
    public bool Player0Down => false;
    public bool Player0Button => false;

    public bool Player1Up => false;
    public bool Player1Left => false;
    public bool Player1Right => false;
    public bool Player1Down => false;
    public bool Player1Button => false;

    public bool GameResetSwitch => false;
    public bool GameSelectSwitch => false;
    public bool TvTypeSwitch => false;
    public bool PlayerDifficultySwitchA => true;
    public bool PlayerDifficultySwitchB => true;
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Sharptella.Lib;

public static class MathEx
{
    public static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * Math.Clamp(t, 0.0f, 1.0f);
    }

    public static double Lerp(double a, double b, double t)
    {
        return a + (b - a) * Math.Clamp(t, 0.0, 1.0);
    }

    public static float InverseLerp(float a, float b, float value)
    {
        if (a == b)
        {
            return 0.0f;
        }
        return Math.Clamp((value - a) / (b - a), 0.0f, 1.0f);
    }

    public static double InverseLerp(double a, double b, double value)
    {
        if (a == b)
        {
            return 0.0;
        }
        return Math.Clamp((value - a) / (b - a), 0.0, 1.0);
    }
}

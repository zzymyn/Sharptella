using System.Runtime.InteropServices;

namespace Sharptari.Lib;

[StructLayout(LayoutKind.Sequential)]
public struct ColorAbgr8888
{
    public byte R;
    public byte G;
    public byte B;
    public byte A;

    public static readonly ColorAbgr8888 Zero = new(0, 0, 0, 0);
    public static readonly ColorAbgr8888 Black = new(0, 0, 0, 255);
    public static readonly ColorAbgr8888 Magenta = new(255, 0, 255, 255);

    public ColorAbgr8888(byte r, byte g, byte b, byte a)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }
}

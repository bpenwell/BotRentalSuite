using System.Runtime.InteropServices;

namespace Utilities.Type
{

    [StructLayout(LayoutKind.Sequential)]
    public struct MousePoint
    {
        public int X;
        public int Y;

        public MousePoint(int x, int y)
        {
            //Needs to be scaled to the screen
            X = (x * 65535) / 1920;
            Y = (y * 65535) / 1080;
        }
    }
}

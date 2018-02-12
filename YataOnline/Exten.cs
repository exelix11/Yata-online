using Bridge.Html5;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YataOnline
{
    static class BinaryMath
    {
        //Some binary magic
        public static bool IsPowerOfTwo(int x) { return (x & (x - 1)) == 0; }

        public static int FastMod(int n, int m) { return n & (m - 1); }

        public static int DivideByPowerOf2(int x, int n) { return (x + ((x >> 31) & ((1 << n) + ~0))) >> n; }

        public static int Log2(int v)
        {
            int r = 0xFFFF - v >> 31 & 0x10;
            v >>= r;
            int shift = 0xFF - v >> 31 & 0x8;
            v >>= shift;
            r |= shift;
            shift = 0xF - v >> 31 & 0x4;
            v >>= shift;
            r |= shift;
            shift = 0x3 - v >> 31 & 0x2;
            v >>= shift;
            r |= shift;
            r |= (v >> 1);
            return r;
        }
    }

    public static class Exten
    {
        public static uint ToU32(this byte[] b)
        {
            return (uint)BitConverter.ToInt32(b, 0);
        }

        public static bool isPower2(this int x)
        {
            return (x != 0) && ((x & (x - 1)) == 0);
        }

        public static void SetPixel(this ImageData img, uint x, uint y, int R, int G, int B, int A)
        {
            int index = (int)(y * img.Width + x) * 4;
            img.Data[index++] = (byte)R;
            img.Data[index++] = (byte)G;
            img.Data[index++] = (byte)B;
            img.Data[index] = (byte)A;
        }

        public static Color GetPixel(this ImageData img, uint x, uint y)
        {
            Color c;
            int index = (int)(y * img.Width + x) * 4;
            c.R = img.Data[index++];
            c.G = img.Data[index++];
            c.B = img.Data[index++];
            c.A = img.Data[index];
            return c;
        }

        public static string ToDataUrl(this ImageData img)
        {
            HTMLCanvasElement tmp = new HTMLCanvasElement();
            ((CanvasRenderingContext2D)tmp.GetContext("2d")).PutImageData(img, 0, 0);
            return tmp.ToDataURL();
        }

        public static uint FastMod(this uint num, int num2)
        {
            return (uint)BinaryMath.FastMod((int)num, num2);
        }

        public static uint FastDiv(this uint num, int num2)
        {
            return (uint)BinaryMath.DivideByPowerOf2((int)num, num2);
        }
    }
}

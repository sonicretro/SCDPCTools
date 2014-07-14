using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace SCDPCSSEd
{
    /// <summary>
    /// Represents the pixel data of a 4bpp Bitmap.
    /// </summary>
    public class BitmapBits
    {
        public byte[] Bits { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Size Size
        {
            get
            {
                return new Size(Width, Height);
            }
        }

        public byte this[int x, int y]
        {
            get
            {
                return Bits[(y * Width) + x];
            }
            set
            {
                Bits[(y * Width) + x] = value;
            }
        }

        public BitmapBits(int width, int height)
        {
            Width = width;
            Height = height;
            Bits = new byte[width * height];
        }

        public BitmapBits(int width, int height, byte[] data)
        {
            Width = width;
            Height = height;
            Bits = new byte[width * height];
            int j = 0;
            for (int i = 0; i < Bits.Length; i += Width)
            {
                for (int x = 0; x < Width; x += 2)
                {
                    Bits[i + x] = (byte)(data[j + (x / 2)] >> 4);
                    if (x + 1 < Width)
                        Bits[i + x + 1] = (byte)(data[j + (x / 2)] & 0xF);
                }
                j += Width / 2;
                if ((Width & 4) == 4)
                    j += 2;
            }
        }

        public BitmapBits(Bitmap bmp)
        {
            if (bmp.PixelFormat != PixelFormat.Format4bppIndexed)
                throw new ArgumentException();
            BitmapData bmpd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);
            Width = bmpd.Width;
            Height = bmpd.Height;
            byte[] tmpbits = new byte[Math.Abs(bmpd.Stride) * bmpd.Height];
            Marshal.Copy(bmpd.Scan0, tmpbits, 0, tmpbits.Length);
            Bits = new byte[Width * Height];
            int j = 0;
            for (int i = 0; i < Bits.Length; i += Width)
            {
                for (int x = 0; x < Width; x += 2)
                {
                    Bits[i + x] = (byte)(tmpbits[j + (x / 2)] >> 4);
                    Bits[i + x + 1] = (byte)(tmpbits[j + (x / 2)] & 0xF);
                }
                j += Math.Abs(bmpd.Stride);
            }
            bmp.UnlockBits(bmpd);
        }

        public BitmapBits(BitmapBits source)
        {
            Width = source.Width;
            Height = source.Height;
            Bits = new byte[source.Bits.Length];
            Array.Copy(source.Bits, Bits, Bits.Length);
        }

        public Bitmap ToBitmap()
        {
            Bitmap newbmp = new Bitmap(Width, Height, PixelFormat.Format4bppIndexed);
            BitmapData newbmpd = newbmp.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, PixelFormat.Format4bppIndexed);
            byte[] bmpbits = new byte[Math.Abs(newbmpd.Stride) * newbmpd.Height];
            int j = 0;
            for (int i = 0; i < Bits.Length; i += Width)
            {
                for (int x = 0; x < Width; x += 2)
                    bmpbits[j + (x / 2)] = (byte)((Bits[i + x] << 4) | Bits[i + x + 1] & 0xF);
                j += Math.Abs(newbmpd.Stride);
            }
            Marshal.Copy(bmpbits, 0, newbmpd.Scan0, bmpbits.Length);
            newbmp.UnlockBits(newbmpd);
            return newbmp;
        }

        public Bitmap ToBitmap(ColorPalette palette)
        {
            Bitmap newbmp = ToBitmap();
            newbmp.Palette = palette;
            return newbmp;
        }

        public Bitmap ToBitmap(params Color[] palette)
        {
            Bitmap newbmp = ToBitmap();
            ColorPalette pal = newbmp.Palette;
            for (int i = 0; i < Math.Min(palette.Length, 16); i++)
                pal.Entries[i] = palette[i];
            newbmp.Palette = pal;
            return newbmp;
        }

        public byte[] ToBytes()
        {
            int pixw = Width / 2;
            if ((Width & 4) == 4)
                pixw += 2;
            byte[] bmpbits = new byte[pixw * Height];
            int j = 0;
            for (int i = 0; i < Bits.Length; i += Width)
            {
                for (int x = 0; x < Width; x += 2)
                    bmpbits[j + (x / 2)] = (byte)((Bits[i + x] << 4) | Bits[i + x + 1] & 0xF);
                j += pixw;
            }
            return bmpbits;
        }

        public void DrawBitmap(BitmapBits source, Point location)
        {
            int dstx = location.X; int dsty = location.Y;
            for (int i = 0; i < source.Height; i++)
            {
                int di = ((dsty + i) * Width) + dstx;
                int si = i * source.Width;
                Array.Copy(source.Bits, si, Bits, di, source.Width);
            }
        }

        public void DrawBitmapComposited(BitmapBits source, Point location)
        {
            int srcl = 0;
            if (location.X < 0)
                srcl = -location.X;
            int srct = 0;
            if (location.Y < 0)
                srct = -location.Y;
            int srcr = source.Width;
            if (srcr > Width - location.X)
                srcr = Width - location.X;
            int srcb = source.Height;
            if (srcb > Height - location.Y)
                srcb = Height - location.Y;
            for (int i = srct; i < srcb; i++)
                for (int x = srcl; x < srcr; x++)
                    if (source[x, i] != 0)
                        this[location.X + x, location.Y + i] = source[x, i];
        }

        public void Flip(bool XFlip, bool YFlip)
        {
            if (!XFlip & !YFlip)
                return;
            if (XFlip)
            {
                for (int y = 0; y < Height; y++)
                {
                    int addr = y * Width;
                    Array.Reverse(Bits, addr, Width);
                }
            }
            if (YFlip)
            {
                byte[] tmppix = new byte[Bits.Length];
                for (int y = 0; y < Height; y++)
                {
                    int srcaddr = y * Width;
                    int dstaddr = (Height - y - 1) * Width;
                    Array.Copy(Bits, srcaddr, tmppix, dstaddr, Width);
                }
                Bits = tmppix;
            }
        }

        public void Rotate(int R)
        {
            byte[] tmppix = new byte[Bits.Length];
            switch (R)
            {
                case 1:
                    for (int y = 0; y < Height; y++)
                    {
                        int srcaddr = y * Width;
                        int dstaddr = (Width * (Width - 1)) + y;
                        for (int x = 0; x < Width; x++)
                        {
                            tmppix[dstaddr] = Bits[srcaddr + x];
                            dstaddr -= Width;
                        }
                    }
                    Bits = tmppix;
                    int h = Height;
                    Height = Width;
                    Width = h;
                    break;
                case 2:
                    Flip(true, true);
                    break;
                case 3:
                    for (int y = 0; y < Height; y++)
                    {
                        int srcaddr = y * Width;
                        int dstaddr = Height - 1 - y;
                        for (int x = 0; x < Width; x++)
                        {
                            tmppix[dstaddr] = Bits[srcaddr + x];
                            dstaddr += Width;
                        }
                    }
                    Bits = tmppix;
                    h = Height;
                    Height = Width;
                    Width = h;
                    break;
            }
        }

        public void Clear()
        {
            Array.Clear(Bits, 0, Bits.Length);
        }

        public BitmapBits Scale(int factor)
        {
            BitmapBits res = new BitmapBits(Width * factor, Height * factor);
            for (int y = 0; y < res.Height; y++)
                for (int x = 0; x < res.Width; x++)
                    res[x, y] = this[(x / factor), (y / factor)];
            return res;
        }

        public static BitmapBits FromBitmap(Bitmap bmp, out int palette)
        {
            BitmapBits bmpbits = new BitmapBits(8, 8);
            BitmapData bmpd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);
            byte[] Bits = new byte[Math.Abs(bmpd.Stride) * bmpd.Height];
            System.Runtime.InteropServices.Marshal.Copy(bmpd.Scan0, Bits, 0, Bits.Length);
            bmp.UnlockBits(bmpd);
            switch (bmpd.PixelFormat)
            {
                case PixelFormat.Format16bppArgb1555:
                    LoadBitmap16BppArgb1555(bmpbits, Bits, bmpd.Stride);
                    break;
                case PixelFormat.Format16bppGrayScale:
                    LoadBitmap16BppGrayScale(bmpbits, Bits, bmpd.Stride);
                    break;
                case PixelFormat.Format16bppRgb555:
                    LoadBitmap16BppRgb555(bmpbits, Bits, bmpd.Stride);
                    break;
                case PixelFormat.Format16bppRgb565:
                    LoadBitmap16BppRgb565(bmpbits, Bits, bmpd.Stride);
                    break;
                case PixelFormat.Format1bppIndexed:
                    LoadBitmap1BppIndexed(bmpbits, Bits, bmpd.Stride);
                    palette = 0;
                    return bmpbits;
                case PixelFormat.Format24bppRgb:
                    LoadBitmap24BppRgb(bmpbits, Bits, bmpd.Stride);
                    break;
                case PixelFormat.Format32bppArgb:
                    LoadBitmap32BppArgb(bmpbits, Bits, bmpd.Stride);
                    break;
                case PixelFormat.Format32bppPArgb:
                    throw new NotImplementedException();
                case PixelFormat.Format32bppRgb:
                    LoadBitmap32BppRgb(bmpbits, Bits, bmpd.Stride);
                    break;
                case PixelFormat.Format48bppRgb:
                    LoadBitmap48BppRgb(bmpbits, Bits, bmpd.Stride);
                    break;
                case PixelFormat.Format4bppIndexed:
                    LoadBitmap4BppIndexed(bmpbits, Bits, bmpd.Stride);
                    palette = 0;
                    return bmpbits;
                case PixelFormat.Format64bppArgb:
                    LoadBitmap64BppArgb(bmpbits, Bits, bmpd.Stride);
                    break;
                case PixelFormat.Format64bppPArgb:
                    throw new NotImplementedException();
                case PixelFormat.Format8bppIndexed:
                    LoadBitmap8BppIndexed(bmpbits, Bits, bmpd.Stride);
                    break;
            }
            int[] palcnt = new int[4];
            for (int y = 0; y < 8; y++)
                for (int x = 0; x < 8; x++)
                {
                    palcnt[bmpbits[x, y] / 16]++;
                    bmpbits[x, y] &= 15;
                }
            palette = 0;
            if (palcnt[1] > palcnt[palette])
                palette = 1;
            if (palcnt[2] > palcnt[palette])
                palette = 2;
            if (palcnt[3] > palcnt[palette])
                palette = 3;
            return bmpbits;
        }

        private static void LoadBitmap16BppArgb1555(BitmapBits bmp, byte[] Bits, int Stride)
        {
            for (int y = 0; y < bmp.Height; y++)
            {
                int srcaddr = y * Math.Abs(Stride);
                for (int x = 0; x < bmp.Width; x++)
                {
                    ushort pix = BitConverter.ToUInt16(Bits, srcaddr + (x * 2));
                    int A = (pix >> 15) * 255;
                    int R = (pix >> 10) & 0x1F;
                    R = R << 3 | R >> 2;
                    int G = (pix >> 5) & 0x1F;
                    G = G << 3 | G >> 2;
                    int B = pix & 0x1F;
                    B = B << 3 | B >> 2;
                    bmp[x, y] = (byte)Array.IndexOf(MainForm.palette, Color.FromArgb(A, R, G, B).FindNearestMatch(MainForm.palette));
                    if (A < 128)
                        bmp[x, y] = 0;
                }
            }
        }

        private static void LoadBitmap16BppGrayScale(BitmapBits bmp, byte[] Bits, int Stride)
        {
            for (int y = 0; y < bmp.Height; y++)
            {
                int srcaddr = y * Math.Abs(Stride);
                for (int x = 0; x < bmp.Width; x++)
                {
                    ushort pix = BitConverter.ToUInt16(Bits, srcaddr + (x * 2));
                    bmp[x, y] = (byte)Array.IndexOf(MainForm.palette, Color.FromArgb(pix >> 8, pix >> 8, pix >> 8).FindNearestMatch(MainForm.palette));
                }
            }
        }

        private static void LoadBitmap16BppRgb555(BitmapBits bmp, byte[] Bits, int Stride)
        {
            for (int y = 0; y < bmp.Height; y++)
            {
                int srcaddr = y * Math.Abs(Stride);
                for (int x = 0; x < bmp.Width; x++)
                {
                    ushort pix = BitConverter.ToUInt16(Bits, srcaddr + (x * 2));
                    int R = (pix >> 10) & 0x1F;
                    R = R << 3 | R >> 2;
                    int G = (pix >> 5) & 0x1F;
                    G = G << 3 | G >> 2;
                    int B = pix & 0x1F;
                    B = B << 3 | B >> 2;
                    bmp[x, y] = (byte)Array.IndexOf(MainForm.palette, Color.FromArgb(R, G, B).FindNearestMatch(MainForm.palette));
                }
            }
        }

        private static void LoadBitmap16BppRgb565(BitmapBits bmp, byte[] Bits, int Stride)
        {
            for (int y = 0; y < bmp.Height; y++)
            {
                int srcaddr = y * Math.Abs(Stride);
                for (int x = 0; x < bmp.Width; x++)
                {
                    ushort pix = BitConverter.ToUInt16(Bits, srcaddr + (x * 2));
                    int R = (pix >> 11) & 0x1F;
                    R = R << 3 | R >> 2;
                    int G = (pix >> 5) & 0x3F;
                    G = G << 2 | G >> 4;
                    int B = pix & 0x1F;
                    B = B << 3 | B >> 2;
                    bmp[x, y] = (byte)Array.IndexOf(MainForm.palette, Color.FromArgb(R, G, B).FindNearestMatch(MainForm.palette));
                }
            }
        }

        private static void LoadBitmap1BppIndexed(BitmapBits bmp, byte[] Bits, int Stride)
        {
            for (int y = 0; y < bmp.Height; y++)
            {
                int srcaddr = y * Math.Abs(Stride);
                for (int x = 0; x < bmp.Width; x += 8)
                {
                    bmp[x + 0, y] = (byte)((Bits[srcaddr + (x / 8)] >> 7) & 1);
                    bmp[x + 1, y] = (byte)((Bits[srcaddr + (x / 8)] >> 6) & 1);
                    bmp[x + 2, y] = (byte)((Bits[srcaddr + (x / 8)] >> 5) & 1);
                    bmp[x + 3, y] = (byte)((Bits[srcaddr + (x / 8)] >> 4) & 1);
                    bmp[x + 4, y] = (byte)((Bits[srcaddr + (x / 8)] >> 3) & 1);
                    bmp[x + 5, y] = (byte)((Bits[srcaddr + (x / 8)] >> 2) & 1);
                    bmp[x + 6, y] = (byte)((Bits[srcaddr + (x / 8)] >> 1) & 1);
                    bmp[x + 7, y] = (byte)(Bits[srcaddr + (x / 8)] & 1);
                }
            }
        }

        private static void LoadBitmap24BppRgb(BitmapBits bmp, byte[] Bits, int Stride)
        {
            for (int y = 0; y < bmp.Height; y++)
            {
                int srcaddr = y * Math.Abs(Stride);
                for (int x = 0; x < bmp.Width; x++)
                    bmp[x, y] = (byte)Array.IndexOf(MainForm.palette, Color.FromArgb(Bits[srcaddr + (x * 3) + 2], Bits[srcaddr + (x * 3) + 1], Bits[srcaddr + (x * 3)]).FindNearestMatch(MainForm.palette));
            }
        }

        private static void LoadBitmap32BppArgb(BitmapBits bmp, byte[] Bits, int Stride)
        {
            for (int y = 0; y < bmp.Height; y++)
            {
                int srcaddr = y * Math.Abs(Stride);
                for (int x = 0; x < bmp.Width; x++)
                {
                    Color col = Color.FromArgb(BitConverter.ToInt32(Bits, srcaddr + (x * 4)));
                    bmp[x, y] = (byte)Array.IndexOf(MainForm.palette, col.FindNearestMatch(MainForm.palette));
                }
            }
        }

        private static void LoadBitmap32BppRgb(BitmapBits bmp, byte[] Bits, int Stride)
        {
            for (int y = 0; y < bmp.Height; y++)
            {
                int srcaddr = y * Math.Abs(Stride);
                for (int x = 0; x < bmp.Width; x++)
                    bmp[x, y] = (byte)Array.IndexOf(MainForm.palette, Color.FromArgb(Bits[srcaddr + (x * 4) + 2], Bits[srcaddr + (x * 4) + 1], Bits[srcaddr + (x * 4)]).FindNearestMatch(MainForm.palette));
            }
        }

        private static void LoadBitmap48BppRgb(BitmapBits bmp, byte[] Bits, int Stride)
        {
            for (int y = 0; y < bmp.Height; y++)
            {
                int srcaddr = y * Math.Abs(Stride);
                for (int x = 0; x < bmp.Width; x++)
                    bmp[x, y] = (byte)Array.IndexOf(MainForm.palette, Color.FromArgb(BitConverter.ToUInt16(Bits, srcaddr + (x * 6) + 4) / 255, BitConverter.ToUInt16(Bits, srcaddr + (x * 6) + 2) / 255, BitConverter.ToUInt16(Bits, srcaddr + (x * 6)) / 255).FindNearestMatch(MainForm.palette));
            }
        }

        private static void LoadBitmap4BppIndexed(BitmapBits bmp, byte[] Bits, int Stride)
        {
            for (int y = 0; y < bmp.Height; y++)
            {
                int srcaddr = y * Math.Abs(Stride);
                for (int x = 0; x < bmp.Width; x += 2)
                {
                    bmp[x, y] = (byte)(Bits[srcaddr + (x / 2)] >> 4);
                    bmp[x + 1, y] = (byte)(Bits[srcaddr + (x / 2)] & 0xF);
                }
            }
        }

        private static void LoadBitmap64BppArgb(BitmapBits bmp, byte[] Bits, int Stride)
        {
            for (int y = 0; y < bmp.Height; y++)
            {
                int srcaddr = y * Math.Abs(Stride);
                for (int x = 0; x < bmp.Width; x++)
                    bmp[x, y] = (byte)Array.IndexOf(MainForm.palette, Color.FromArgb(BitConverter.ToUInt16(Bits, srcaddr + (x * 8) + 6) / 255, BitConverter.ToUInt16(Bits, srcaddr + (x * 8) + 4) / 255, BitConverter.ToUInt16(Bits, srcaddr + (x * 8) + 2) / 255, BitConverter.ToUInt16(Bits, srcaddr + (x * 8)) / 255).FindNearestMatch(MainForm.palette));
            }
        }

        private static void LoadBitmap8BppIndexed(BitmapBits bmp, byte[] Bits, int Stride)
        {
            for (int y = 0; y < bmp.Height; y++)
            {
                int srcaddr = y * Math.Abs(Stride);
                for (int x = 0; x < bmp.Width; x++)
                    bmp[x, y] = (byte)(Bits[srcaddr + x] & 0x3F);
            }
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj)) return true;
            BitmapBits other = obj as BitmapBits;
            if (other == null) return false;
            if (Width != other.Width | Height != other.Height) return false;
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    if (this[x, y] != other[x, y])
                        return false;
            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    static partial class Extensions
    {
        public static Color FindNearestMatch(this Color col, params Color[] palette)
        {
            Color nearest_color = Color.Empty;
            double distance = 250000;
            foreach (Color o in palette)
            {
                double dbl_test_red = Math.Pow(o.R - col.R, 2.0);
                double dbl_test_green = Math.Pow(o.G - col.G, 2.0);
                double dbl_test_blue = Math.Pow(o.B - col.B, 2.0);
                double temp = dbl_test_blue + dbl_test_green + dbl_test_red;
                if (temp == 0.0)
                {
                    nearest_color = o;
                    break;
                }
                else if (temp < distance)
                {
                    distance = temp;
                    nearest_color = o;
                }
            }
            return nearest_color;
        }
    }
}
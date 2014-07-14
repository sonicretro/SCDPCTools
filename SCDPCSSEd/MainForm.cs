using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.IO;

namespace SCDPCSSEd
{
    public partial class MainForm : Form
    {
        bool loaded;
        public static List<BitmapBits> tiles = new List<BitmapBits>();
        public static List<BitmapBits>[] flippedtiles = new List<BitmapBits>[8];
        public static Color[] palette = new Color[16];
        TileIndex[,] map;
        ImageAttributes imageTransparency = new ImageAttributes();
        BitmapBits LevelImg8bpp;
        Bitmap LevelBmp;
        Graphics LevelGfx;
        Graphics PanelGfx;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            ColorMatrix x = new System.Drawing.Imaging.ColorMatrix();
            x.Matrix33 = 0.75f;
            imageTransparency.SetColorMatrix(x, System.Drawing.Imaging.ColorMatrixFlag.Default, System.Drawing.Imaging.ColorAdjustType.Bitmap);
            BitmapBits bits = new BitmapBits(32, 32);
            tiles.Add(bits);
            for (int i = 0; i < 8; i++)
                flippedtiles[i] = new List<BitmapBits>() { bits };
            tileList1.SelectedIndex = 0;
            domainUpDown1.SelectedIndex = 0;
            Bitmap bmp = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format4bppIndexed);
            palette = bmp.Palette.Entries;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog fd = new OpenFileDialog())
            {
                fd.DefaultExt = "map";
                fd.Filter = "Map files|*.map";
                if (fd.ShowDialog() == DialogResult.OK)
                {
                    TextBox1.Text = fd.FileName;
                    byte[] tmp = File.ReadAllBytes(fd.FileName);
                    map = new TileIndex[128, 128];
                    for (int lr = 0; lr < 128; lr++)
                        for (int lc = 0; lc < 128; lc++)
                            if (((lr * 128) + lc) * 2 < tmp.Length)
                                map[lc, lr] = new TileIndex(tmp, ((lr * 128) + lc) * 2);
                    LevelBmp = new Bitmap(Panel1.Width, Panel1.Height);
                    LevelImg8bpp = new BitmapBits(Panel1.Width, Panel1.Height);
                    LevelGfx = Graphics.FromImage(LevelBmp);
                    LevelGfx.SetOptions();
                    PanelGfx = Panel1.CreateGraphics();
                    PanelGfx.SetOptions();
                    loaded = true;
                    Button2.Enabled = true;
                    DrawLevel();
                }
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog fd = new SaveFileDialog())
            {
                fd.DefaultExt = "map";
                fd.Filter = "Map files|*.map";
                if (fd.ShowDialog() == DialogResult.OK)
                {
                    TextBox1.Text = fd.FileName;
                    List<byte> tmp = new List<byte>();
                    for (int lr = 0; lr < 128; lr++)
                        for (int lc = 0; lc < 128; lc++)
                            tmp.AddRange(map[lc, lr].GetBytes());
                    File.WriteAllBytes(fd.FileName, tmp.ToArray());
                }
            }
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog fd = new OpenFileDialog())
            {
                fd.DefaultExt = "cm_";
                fd.Filter = "CM_ Files|*.cm_|All Files|*.*";
                if (fd.ShowDialog() == DialogResult.OK)
                {
                    TextBox2.Text = fd.FileName;
                    byte[] file = SZDDComp.SZDDComp.Decompress(fd.FileName);
                    if (BitConverter.ToInt32(file, 0) != 0x4C524353)
                    {
                        MessageBox.Show("Not a valid tile file.");
                        return;
                    }
                    tiles = new List<BitmapBits>();
                    tiles.Add(new BitmapBits(32, 32));
                    int numSprites = BitConverter.ToInt32(file, 8);
                    int spriteOff = BitConverter.ToInt32(file, 0xC);
                    for (int i = 0; i < numSprites; i++)
                    {
                        int data = (i * 4) + 0x18;
                        int width = BitConverter.ToInt16(file, data);
                        int height = BitConverter.ToInt16(file, data + 2);
                        byte[] sprite = new byte[(width / 2) * height];
                        Array.Copy(file, spriteOff, sprite, 0, sprite.Length);
                        spriteOff += sprite.Length;
                        BitmapBits bmp = new BitmapBits(width, height, sprite);
                        tiles.Add(bmp);
                        for (int l = 0; l < 8; l++)
                        {
                            BitmapBits bmp2 = new BitmapBits(bmp);
                            bmp2.Rotate(l & 3);
                            bmp2.Flip((l & 4) == 4, false);
                            flippedtiles[l].Add(bmp2);
                        }
                    }
                    tileList1.SelectedIndex = 0;
                    tileList1.ChangeSize();
                    tileList1.Invalidate();
                    DrawLevel();
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog fd = new OpenFileDialog())
            {
                fd.DefaultExt = "bin";
                fd.Filter = "Palette Files|*.bin;*.scdpal";
                fd.RestoreDirectory = true;
                if (fd.ShowDialog(this) == DialogResult.OK)
                {
                    byte[] file = File.ReadAllBytes(fd.FileName);
                    Color[] colors = new Color[file.Length / 4];
                    for (int i = 0; i < colors.Length; i++)
                        colors[i] = Color.FromArgb(file[i * 4], file[i * 4 + 1], file[i * 4 + 2]);
                    Array.Copy(colors, palette, 16);
                    DrawLevel();
                    tileList1.Invalidate();
                }
            }
        }

        internal void DrawLevel()
        {
            if (!loaded) return;
            LevelGfx.Clear(palette[0]);
            LevelImg8bpp.Clear();
            Point pnlcur = Panel1.PointToClient(Cursor.Position);
            for (int y = Math.Max(VScrollBar1.Value / 32, 0); y <= Math.Min((VScrollBar1.Value + (Panel1.Height - 1)) / 32, map.GetLength(1) - 1); y++)
                for (int x = Math.Max(HScrollBar1.Value / 32, 0); x <= Math.Min((HScrollBar1.Value + (Panel1.Width - 1)) / 32, map.GetLength(0) - 1); x++)
                    if (map[x, y].Tile < tiles.Count)
                        LevelImg8bpp.DrawBitmapComposited(flippedtiles[map[x, y].List][map[x, y].Tile], new Point(x * 32 - HScrollBar1.Value, y * 32 - VScrollBar1.Value));
            LevelGfx.DrawImage(LevelImg8bpp.ToBitmap(palette), 0, 0, LevelImg8bpp.Width, LevelImg8bpp.Height);
            BitmapBits bmp2 = new BitmapBits(tiles[tileList1.SelectedIndex]);
            bmp2.Rotate((byte)domainUpDown1.SelectedIndex);
            bmp2.Flip(checkBox1.Checked, false);
            LevelGfx.DrawImage(bmp2.ToBitmap(palette),
            new Rectangle((((pnlcur.X + HScrollBar1.Value) / 32) * 32) - HScrollBar1.Value, (((pnlcur.Y + VScrollBar1.Value) / 32) * 32) - VScrollBar1.Value, 32, 32),
            0, 0, 32, 32,
            GraphicsUnit.Pixel, imageTransparency);
            PanelGfx.DrawImage(LevelBmp, 0, 0, Panel1.Width, Panel1.Height);
        }

        private void Panel1_Paint(object sender, PaintEventArgs e)
        {
            DrawLevel();
        }

        private void ScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            DrawLevel();
        }

        private void Panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (!loaded) return;
            Point chunkpoint = new Point((e.X + HScrollBar1.Value) / 32, (e.Y + VScrollBar1.Value) / 32);
            switch (e.Button)
            {
                case MouseButtons.Left:
                    TileIndex ti = new TileIndex();
                    ti.XFlip = checkBox1.Checked;
                    ti.Rotate = (byte)domainUpDown1.SelectedIndex;
                    ti.Tile = (ushort)tileList1.SelectedIndex;
                    map[chunkpoint.X, chunkpoint.Y] = ti;
                    break;
                case MouseButtons.Right:
                    ti = map[chunkpoint.X, chunkpoint.Y];
                    checkBox1.Checked = ti.XFlip;
                    domainUpDown1.SelectedIndex = ti.Rotate;
                    if (ti.Tile < tiles.Count)
                        tileList1.SelectedIndex = ti.Tile;
                    else
                        tileList1.SelectedIndex = 0;
                    break;
            }
            DrawLevel();
        }

        Point lastchunkpoint;
        private void Panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!loaded) return;
            if (!Panel1.Bounds.Contains(Panel1.PointToClient(Cursor.Position))) return;
            Point chunkpoint = new Point((e.X + HScrollBar1.Value) / 32, (e.Y + VScrollBar1.Value) / 32);
            if (e.Button == MouseButtons.Left)
            {
                TileIndex ti = new TileIndex();
                ti.XFlip = checkBox1.Checked;
                ti.Rotate = (byte)domainUpDown1.SelectedIndex;
                ti.Tile = (ushort)tileList1.SelectedIndex;
                map[chunkpoint.X, chunkpoint.Y] = ti;
            }
            if (chunkpoint != lastchunkpoint) DrawLevel();
            lastchunkpoint = chunkpoint;
        }
    }

    class TileIndex
    {
        public bool XFlip { get; set; }
        public byte Rotate { get; set; }

        public int List
        {
            get
            {
                return (XFlip ? 4 : 0) | Rotate;
            }
        }

        private ushort _ind;
        public ushort Tile
        {
            get
            {
                return _ind;
            }
            set
            {
                _ind = (ushort)(value & 0x1FF);
            }
        }

        public static int Size { get { return 2; } }

        public TileIndex() { }

        public TileIndex(byte[] file, int address)
        {
            ushort val = BitConverter.ToUInt16(file, address);
            XFlip = (val & 0x8000) == 0x8000;
            Rotate = (byte)((val & 0x6000) >> 13);
            _ind = (ushort)((val >> 2) & 0x1FF);
        }

        public byte[] GetBytes()
        {
            ushort val = (ushort)(_ind << 2);
            val |= (ushort)(Rotate << 13);
            if (XFlip) val |= 0x8000;
            return BitConverter.GetBytes(val);
        }
    }

    static partial class Extensions
    {
        /// <summary>
        /// Sets options to enable faster rendering.
        /// </summary>
        public static void SetOptions(this Graphics gfx)
        {
            gfx.CompositingQuality = CompositingQuality.HighQuality;
            gfx.InterpolationMode = InterpolationMode.NearestNeighbor;
            gfx.PixelOffsetMode = PixelOffsetMode.None;
            gfx.SmoothingMode = SmoothingMode.HighSpeed;
        }
    }
}
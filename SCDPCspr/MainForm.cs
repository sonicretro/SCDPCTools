using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace SCDPCspr
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        public static List<Sprite> sprites = new List<Sprite>();
        public static Color[] palette = new Color[256];
        public static Color[] defaultpalette = new Color[256];
        public List<string> palettes = new List<string>();

        private void Form1_Load(object sender, EventArgs e)
        {
            Bitmap bmp = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            palette = bmp.Palette.Entries;
            for (int i = 16; i < palette.Length; i++)
                palette[i] = palette[i & 15];
            Array.Copy(palette, defaultpalette, 256);
            if (Directory.Exists("Palette"))
                foreach (string item in Directory.GetFiles("Palette", "*.scdpal"))
                {
                    palettesToolStripMenuItem.DropDownItems.Add(Path.GetFileNameWithoutExtension(item).Replace("&", "&&"));
                    palettes.Add(item);
                }
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length == 1)
            {
                sprites = new List<Sprite>();
            }
            else
                LoadFile(args[1]);
        }

        private void LoadFile(string filename)
        {
            byte[] file = SZDDComp.SZDDComp.Decompress(filename);
            if (BitConverter.ToInt32(file, 0) != 0x54525053)
            {
                MessageBox.Show("Not a valid sprite file.");
                return;
            }
            sprites = new List<Sprite>();
            int numSprites = BitConverter.ToInt32(file, 8);
            int spriteOff = BitConverter.ToInt32(file, 0xC);
            for (int i = 0; i < numSprites; i++)
            {
                int data = (i * 0xC) + 0x10;
                int width = BitConverter.ToInt16(file, data + 4);
                int height = BitConverter.ToInt16(file, data + 6);
                int dw = width;
                if ((dw & 4) == 4)
                    dw += 4;
                byte[] sprite = new byte[(dw / 2) * height];
                Array.Copy(file, spriteOff, sprite, 0, sprite.Length);
                spriteOff += sprite.Length;
                sprites.Add(new Sprite(new BitmapBits(width, height, sprite), new Point(BitConverter.ToInt16(file, data), BitConverter.ToInt16(file, data + 2)), BitConverter.ToInt16(file, data + 8) - 16));
            }
            tileList1.SelectedIndex = 0;
            tileList1.ChangeSize();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog a = new OpenFileDialog()
            {
                DefaultExt = "cm_",
                Filter = "CM_ Files|*.cm_|All Files|*.*",
                RestoreDirectory = true
            };
            if (a.ShowDialog() == DialogResult.OK)
                LoadFile(a.FileName);
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog a = new SaveFileDialog()
            {
                DefaultExt = "cm_",
                Filter = "CM_ Files|*.cmp|All Files|*.*",
                RestoreDirectory = true
            };
            if (a.ShowDialog() == DialogResult.OK)
            {
                Stream file = File.Open(a.FileName, FileMode.Create, FileAccess.Write);
                BinaryWriter bw = new BinaryWriter(file);
                bw.Write(0x54525053);
                int fs = 0;
                foreach (Sprite item in sprites)
                {
                    fs += (item.sprite.Width / 2) * item.sprite.Height;
                    if ((item.sprite.Width & 4) == 4)
                        fs += 2 * item.sprite.Height;
                }
                bw.Write(0x10 + (sprites.Count * 0xC) + fs);
                bw.Write(sprites.Count);
                bw.Write(0x10 + (sprites.Count * 0xC));
                foreach (Sprite item in sprites)
                {
                    bw.Write((short)item.offset.X);
                    bw.Write((short)item.offset.Y);
                    bw.Write((short)item.sprite.Width);
                    bw.Write((short)item.sprite.Height);
                    bw.Write((short)(item.palette + 16));
                    bw.Write((short)0x2BC1);
                }
                foreach (Sprite item in sprites)
                    bw.Write(item.sprite.ToBytes());
                bw.Close();
                file.Close();
            }
        }

        private void extractAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog a = new FolderBrowserDialog() { ShowNewFolderButton = true };
            if (a.ShowDialog(this) == DialogResult.OK)
                for (int i = 0; i < sprites.Count; i++)
                {
                    Color[] pal = new Color[16];
                    for (int j = 0; j < 16; j++)
                        pal[j] = palette[j + sprites[i].palette];
                    sprites[i].sprite.ToBitmap(pal).Save(Path.Combine(a.SelectedPath, i + ".png"));
                }
        }

        private void addFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog a = new OpenFileDialog()
            {
                DefaultExt = "png",
                Filter = "Image Files|*.bmp;*.png;*.jpg;*.gif",
                Multiselect = true,
                RestoreDirectory = true
            };
            if (a.ShowDialog() == DialogResult.OK)
            {
                foreach (string item in a.FileNames)
                    sprites.Add(new Sprite() { sprite = new BitmapBits(new Bitmap(item)) });
                tileList1.ChangeSize();
            }
        }

        private void extractToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tileList1.SelectedIndex == -1) return;
            SaveFileDialog a = new SaveFileDialog()
            {
                DefaultExt = "png",
                Filter = "Image Files|*.bmp;*.png;*.jpg;*.gif",
                FileName = tileList1.SelectedIndex + ".png",
                RestoreDirectory = true
            };
            if (a.ShowDialog() == DialogResult.OK)
            {
                Color[] pal = new Color[16];
                for (int j = 0; j < 16; j++)
                    pal[j] = palette[j + sprites[tileList1.SelectedIndex].palette];
                sprites[tileList1.SelectedIndex].sprite.ToBitmap(pal).Save(a.FileName);
            }
        }

        private void replaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tileList1.SelectedIndex == -1) return;
            OpenFileDialog a = new OpenFileDialog()
            {
                DefaultExt = "png",
                Filter = "Image Files|*.bmp;*.png;*.jpg;*.gif",
                FileName = tileList1.SelectedIndex + ".png",
                RestoreDirectory = true
            };
            if (a.ShowDialog() == DialogResult.OK)
            {
                int pal = 0;
                sprites[tileList1.SelectedIndex].sprite = BitmapBits.FromBitmap(new Bitmap(a.FileName), out pal);
                sprites[tileList1.SelectedIndex].palette = pal * 16;
                tileList1.Invalidate();
                SpritePicture.Invalidate();
            }
        }

        private void insertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tileList1.SelectedIndex == -1) return;
            OpenFileDialog a = new OpenFileDialog()
            {
                DefaultExt = "png",
                Filter = "Image Files|*.bmp;*.png;*.jpg;*.gif",
                Multiselect = true,
                RestoreDirectory = true
            };
            if (a.ShowDialog() == DialogResult.OK)
            {
                int i = tileList1.SelectedIndex;
                foreach (string item in a.FileNames)
                {
                    int pal = 0;
                    sprites.Insert(i, new Sprite() { sprite = BitmapBits.FromBitmap(new Bitmap(a.FileName), out pal), palette = pal * 16 });
                    i++;
                }
                tileList1.SelectedIndex = i;
                tileList1.ChangeSize();
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tileList1.SelectedIndex == -1) return;
            sprites.RemoveAt(tileList1.SelectedIndex);
            tileList1.SelectedIndex = Math.Min(tileList1.SelectedIndex, sprites.Count - 1);
            tileList1.ChangeSize();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            sprites = new List<Sprite>();
            tileList1.SelectedIndex = -1;
            tileList1.ChangeSize();
        }

        Point selectedColor = new Point();

        private void PalettePanel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.Black);
            int i = 0;
            for (int y = 0; y <= 3; y++)
                for (int x = 0; x <= 15; x++)
                {
                    e.Graphics.FillRectangle(new SolidBrush(palette[i]), x * 32, y * 32, 32, 32);
                    e.Graphics.DrawRectangle(Pens.White, x * 32, y * 32, 31, 31);
                    i++;
                }
            e.Graphics.DrawRectangle(new Pen(Color.Yellow, 2), selectedColor.X * 32, selectedColor.Y * 32, 32, 32);
        }

        private void PalettePanel_MouseDown(object sender, MouseEventArgs e)
        {
            selectedColor = new Point(e.X / 32, e.Y / 32);
            PalettePanel.Invalidate();
            if (tileList1.SelectedIndex > -1)
                sprites[tileList1.SelectedIndex].palette = (selectedColor.Y * 16) + selectedColor.X;
            SpritePicture.Invalidate();
        }

        private void SpritePicture_Paint(object sender, PaintEventArgs e)
        {
            if (tileList1.SelectedIndex > -1)
            {
                Sprite spr = sprites[tileList1.SelectedIndex];
                Color[] pal = new Color[16];
                for (int j = 0; j < 16; j++)
                    pal[j] = palette[j + spr.palette];
                e.Graphics.DrawImage(spr.sprite.ToBitmap(pal), 0, 0, SpritePicture.Width, SpritePicture.Height);
                e.Graphics.DrawLine(Pens.Magenta, -spr.offset.X, -spr.offset.Y - 2, -spr.offset.X, -spr.offset.Y + 2);
                e.Graphics.DrawLine(Pens.Magenta, -spr.offset.X - 2, -spr.offset.Y, -spr.offset.X + 2, -spr.offset.Y);
            }
        }

        private void tileList1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tileList1.SelectedIndex != -1)
            {
                SpritePicture.Size = new Size(sprites[tileList1.SelectedIndex].sprite.Width, sprites[tileList1.SelectedIndex].sprite.Height);
                selectedColor = new Point(sprites[tileList1.SelectedIndex].palette % 16, sprites[tileList1.SelectedIndex].palette / 16);
                groupBox1.Enabled = true;
                numericUpDown1.Value = sprites[tileList1.SelectedIndex].offset.X;
                numericUpDown2.Value = sprites[tileList1.SelectedIndex].offset.Y;
            }
            else
            {
                SpritePicture.Size = Size.Empty;
                selectedColor = Point.Empty;
                groupBox1.Enabled = false;
            }
            SpritePicture.Invalidate();
            PalettePanel.Invalidate();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (tileList1.SelectedIndex > -1)
                sprites[tileList1.SelectedIndex].offset.X = (int)numericUpDown1.Value;
            SpritePicture.Invalidate();
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            if (tileList1.SelectedIndex > -1)
                sprites[tileList1.SelectedIndex].offset.Y = (int)numericUpDown2.Value;
            SpritePicture.Invalidate();
        }

        private void fromImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog fd = new OpenFileDialog())
            {
                fd.DefaultExt = "png";
                fd.Filter = "Image Files|*.bmp;*.png;*.jpg;*.gif";
                fd.RestoreDirectory = true;
                if (fd.ShowDialog(this) == DialogResult.OK)
                {
                    Color[] colors;
                    using (Bitmap bmp = new Bitmap(fd.FileName))
                        colors = bmp.Palette.Entries;
                    using (PaletteImportDialog pd = new PaletteImportDialog(colors))
                        if (pd.ShowDialog(this) == DialogResult.OK)
                        {
                            palette = pd.palette;
                            PalettePanel.Invalidate();
                            SpritePicture.Invalidate();
                            tileList1.Invalidate();
                            foreach (ToolStripMenuItem item in palettesToolStripMenuItem.DropDownItems)
                                item.Checked = false;
                        }
                }
            }
        }

        private void fromPCPaletteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog fd = new OpenFileDialog())
            {
                fd.DefaultExt = "png";
                fd.Filter = "Palette Files|*.bin;*.scdpal";
                fd.RestoreDirectory = true;
                if (fd.ShowDialog(this) == DialogResult.OK)
                {
                    byte[] file = File.ReadAllBytes(fd.FileName);
                    Color[] colors = new Color[file.Length / 4];
                    for (int i = 0; i < colors.Length; i++)
                        colors[i] = Color.FromArgb(file[i * 4], file[i * 4 + 1], file[i * 4 + 2]);
                    using (PaletteImportDialog pd = new PaletteImportDialog(colors))
                        if (pd.ShowDialog(this) == DialogResult.OK)
                        {
                            palette = pd.palette;
                            PalettePanel.Invalidate();
                            SpritePicture.Invalidate();
                            tileList1.Invalidate();
                            foreach (ToolStripMenuItem item in palettesToolStripMenuItem.DropDownItems)
                                item.Checked = false;
                        }
                }
            }
        }

        private void fromMDPaletteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog fd = new OpenFileDialog())
            {
                fd.DefaultExt = "png";
                fd.Filter = "Palette Files|*.bin";
                fd.RestoreDirectory = true;
                if (fd.ShowDialog(this) == DialogResult.OK)
                {
                    byte[] file = File.ReadAllBytes(fd.FileName);
                    Color[] colors = new Color[file.Length / 2];
                    for (int i = 0; i < colors.Length; i++)
                        colors[i] = Color.FromArgb(((file[(i * 2) + 1]) & 0xF) << 4, (file[(i * 2) + 1]) & 0xF0, ((file[i * 2]) & 0xF) << 4);
                    using (PaletteImportDialog pd = new PaletteImportDialog(colors))
                        if (pd.ShowDialog(this) == DialogResult.OK)
                        {
                            palette = pd.palette;
                            PalettePanel.Invalidate();
                            SpritePicture.Invalidate();
                            tileList1.Invalidate();
                            foreach (ToolStripMenuItem item in palettesToolStripMenuItem.DropDownItems)
                                item.Checked = false;
                        }
                }
            }
        }

        private void palettesToolStripMenuItem_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            int sel = palettesToolStripMenuItem.DropDownItems.IndexOf(e.ClickedItem);
            foreach (ToolStripMenuItem item in palettesToolStripMenuItem.DropDownItems)
                item.Checked = false;
            ((ToolStripMenuItem)e.ClickedItem).Checked = true;
            if (sel == 0)
            {
                Array.Copy(defaultpalette, palette, 256);
            }
            else
            {
                byte[] file = File.ReadAllBytes(palettes[sel - 1]);
                Color[] colors = new Color[file.Length / 4];
                for (int i = 0; i < colors.Length; i++)
                    colors[i] = Color.FromArgb(file[i * 4], file[i * 4 + 1], file[i * 4 + 2]);
                Array.Copy(colors, palette, colors.Length);
            }
            PalettePanel.Invalidate();
            SpritePicture.Invalidate();
            tileList1.Invalidate();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (PaletteSaveDialog sd = new PaletteSaveDialog())
            {
                if (sd.ShowDialog(this) == DialogResult.OK)
                {
                    if (!Directory.Exists("Palette"))
                        Directory.CreateDirectory("Palette");
                    List<byte> file = new List<byte>();
                    foreach (Color item in palette)
                    {
                        file.Add(item.R);
                        file.Add(item.G);
                        file.Add(item.B);
                        file.Add(1);
                    }
                    string fn = Path.Combine(Path.Combine(Environment.CurrentDirectory, "Palette"), sd.textBox1.Text + ".scdpal");
                    File.WriteAllBytes(fn, file.ToArray());
                    palettes.Add(fn);
                    palettesToolStripMenuItem.DropDownItems.Add(sd.textBox1.Text.Replace("&", "&&"));
                    foreach (ToolStripMenuItem item in palettesToolStripMenuItem.DropDownItems)
                        item.Checked = false;
                    ((ToolStripMenuItem)palettesToolStripMenuItem.DropDownItems[palettesToolStripMenuItem.DropDownItems.Count - 1]).Checked = true;
                }
            }
        }
    }

    public class Sprite
    {
        public Point offset;
        public BitmapBits sprite;
        public int palette;

        public Sprite() { }

        public Sprite(BitmapBits spr, Point off, int pal)
        {
            sprite = spr;
            offset = off;
            palette = pal;
        }
    }
}
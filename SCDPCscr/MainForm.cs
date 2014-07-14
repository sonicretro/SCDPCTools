using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace SCDPCscr
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        public static List<BitmapBits>[] sprites = new List<BitmapBits>[4];
        public static int curpal;
        public static Color[] palette = new Color[256];
        public static Color[] palslice = new Color[16];
        public static Color[] defaultpalette = new Color[256];
        public List<string> palettes = new List<string>();

        private void Form1_Load(object sender, EventArgs e)
        {
            Bitmap bmp = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
            palette = bmp.Palette.Entries;
            for (int i = 16; i < palette.Length; i++)
                palette[i] = palette[i & 15];
            Array.Copy(palette, defaultpalette, 256);
            Array.Copy(palette, palslice, 16);
            if (Directory.Exists("Palette"))
                foreach (string item in Directory.GetFiles("Palette", "*.scdpal"))
                {
                    palettesToolStripMenuItem.DropDownItems.Add(Path.GetFileNameWithoutExtension(item).Replace("&", "&&"));
                    palettes.Add(item);
                }
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < 4; i++)
                sprites[i] = new List<BitmapBits>();
            if (args.Length > 1)
                LoadFile(args[1]);
        }

        private void LoadFile(string filename)
        {
            byte[] file = SZDDComp.SZDDComp.Decompress(filename);
            if (BitConverter.ToInt32(file, 0) != 0x4C524353)
            {
                MessageBox.Show("Not a valid tile file.");
                return;
            }
            for (int i = 0; i < 4; i++)
                sprites[i] = new List<BitmapBits>();
            curpal = 0;
            Array.Copy(palette, curpal * 16, palslice, 0, 16);
            int numSprites = BitConverter.ToInt32(file, 8);
            int spriteOff = BitConverter.ToInt32(file, 0xC);
            int p = 0;
            int pn = BitConverter.ToInt16(file, 0x10);
            for (int i = 0; i < numSprites; i++)
            {
                int data = (i * 4) + 0x18;
                int width = BitConverter.ToInt16(file, data);
                int height = BitConverter.ToInt16(file, data + 2);
                byte[] sprite = new byte[(width / 2) * height];
                Array.Copy(file, spriteOff, sprite, 0, sprite.Length);
                spriteOff += sprite.Length;
                sprites[p].Add(new BitmapBits(width, height, sprite));
                pn--;
                if (--pn == 0)
                {
                    p++;
                    pn = BitConverter.ToInt16(file, 0x10 + (2 * p));
                }
            }
            tileList1.SelectedIndex = 0;
            tileList1.ChangeSize();
            tileList1.Invalidate();
            PalettePanel.Invalidate();
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
                bw.Write(0x4C524353);
                int fs = 0;
                int sc = 0;
                foreach (List<BitmapBits> list in sprites)
                    foreach (BitmapBits item in list)
                    {
                        fs += (item.Width / 2) * item.Height;
                        sc++;
                    }
                bw.Write(0x18 + (sc * 4) + fs);
                bw.Write(sc);
                bw.Write(0x18 + (sc * 4));
                foreach (List<BitmapBits> list in sprites)
                    bw.Write((short)list.Count);
                foreach (List<BitmapBits> list in sprites)
                    foreach (BitmapBits item in list)
                    {
                        bw.Write((short)item.Width);
                        bw.Write((short)item.Height);
                    }
                foreach (List<BitmapBits> list in sprites)
                    foreach (BitmapBits item in list)
                        bw.Write(item.ToBytes());
                bw.Close();
                file.Close();
            }
        }

        private void extractAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog a = new FolderBrowserDialog() { ShowNewFolderButton = true };
            if (a.ShowDialog(this) == DialogResult.OK)
                for (int i = 0; i < sprites[curpal].Count; i++)
                    sprites[curpal][i].ToBitmap(palslice).Save(Path.Combine(a.SelectedPath, i + ".png"));
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
                    sprites[curpal].Add(new BitmapBits(new Bitmap(item)));
                tileList1.ChangeSize();
                tileList1.Invalidate();
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
                sprites[curpal][tileList1.SelectedIndex].ToBitmap(palslice).Save(a.FileName);
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
                sprites[curpal][tileList1.SelectedIndex] = BitmapBits.FromBitmap(new Bitmap(a.FileName));
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
                    sprites[curpal].Insert(i++, BitmapBits.FromBitmap(new Bitmap(a.FileName)));
                tileList1.SelectedIndex = i;
                tileList1.ChangeSize();
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tileList1.SelectedIndex == -1) return;
            sprites[curpal].RemoveAt(tileList1.SelectedIndex);
            tileList1.SelectedIndex = Math.Min(tileList1.SelectedIndex, sprites[curpal].Count - 1);
            tileList1.ChangeSize();
            tileList1.Invalidate();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            curpal = 0;
            Array.Copy(palette, curpal * 16, palslice, 0, 16);
            for (int i = 0; i < 4; i++)
                sprites[i] = new List<BitmapBits>();
            tileList1.SelectedIndex = -1;
            tileList1.ChangeSize();
            tileList1.Invalidate();
            PalettePanel.Invalidate();
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
            curpal = selectedColor.Y;
            Array.Copy(palette, curpal * 16, palslice, 0, 16);
            tileList1.SelectedIndex = -1;
            tileList1.ChangeSize();
            tileList1.Invalidate();
        }

        private void SpritePicture_Paint(object sender, PaintEventArgs e)
        {
            if (tileList1.SelectedIndex > -1)
                e.Graphics.DrawImage(sprites[curpal][tileList1.SelectedIndex].Scale(2).ToBitmap(palslice), 0, 0, SpritePicture.Width, SpritePicture.Height);
        }

        private void tileList1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tileList1.SelectedIndex != -1)
                SpritePicture.Size = new Size(sprites[curpal][tileList1.SelectedIndex].Width * 2, sprites[curpal][tileList1.SelectedIndex].Height * 2);
            else
                SpritePicture.Size = Size.Empty;
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
                            Array.Copy(palette, curpal * 16, palslice, 0, 16);
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
                            Array.Copy(palette, curpal * 16, palslice, 0, 16);
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
                            Array.Copy(palette, curpal * 16, palslice, 0, 16);
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
                Array.Copy(palette, curpal * 16, palslice, 0, 16);
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
}
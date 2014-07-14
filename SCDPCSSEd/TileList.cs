using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SCDPCSSEd
{
    [DefaultEvent("SelectedIndexChanged")]
    public partial class TileList : UserControl
    {
        private int selectedIndex = -1;
        [Browsable(false)]
        public int SelectedIndex
        {
            get { return selectedIndex; }
            set
            {
                selectedIndex = value;
                if (SelectedIndexChanged != null)
                    SelectedIndexChanged(this, EventArgs.Empty);
            }
        }

        public event EventHandler SelectedIndexChanged;

        private int imageSize = 64;
        public int ImageSize
        {
            get { return imageSize; }
            set
            {
                imageSize = value;
                ChangeSize();
            }
        }

        public TileList()
        {
            InitializeComponent();
        }

        public void ChangeSize()
        {
            int tilesPerRow = Width / (imageSize + 4);
            vScrollBar1.Maximum = Math.Max(((int)Math.Ceiling(MainForm.tiles.Count / (double)tilesPerRow) * (imageSize + 4)) - Height, 0);
        }

        private void TileList_Resize(object sender, EventArgs e) { ChangeSize(); }

        private void TileList_Paint(object sender, PaintEventArgs e)
        {
            int actualImageSize = imageSize + 4;
            int tilesPerRow = Width / actualImageSize;
            int numRows = (int)Math.Ceiling(MainForm.tiles.Count / (double)tilesPerRow);
            int str = vScrollBar1.Value / actualImageSize;
            int edr = Math.Min((int)Math.Ceiling((vScrollBar1.Value + Height) / (double)actualImageSize), numRows);
            Graphics g = e.Graphics;
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighSpeed;
            g.Clear(BackColor);
            if (MainForm.tiles.Count == 0) return;
            int i = str * tilesPerRow;
            for (int r = str; r < edr; r++)
                for (int c = 0; c < tilesPerRow; c++)
                {
                    if (i == selectedIndex)
                        g.DrawRectangle(new Pen(Color.Yellow, 2), actualImageSize * c, (actualImageSize * r) - vScrollBar1.Value, actualImageSize - 1, actualImageSize - 1);
                    Bitmap image = MainForm.tiles[i].ToBitmap(MainForm.palette);
                    int mywidth = image.Width;
                    int myheight = image.Height;
                    while (myheight > ImageSize | mywidth > ImageSize)
                    {
                        if (mywidth > ImageSize)
                        {
                            mywidth = ImageSize;
                            myheight = (int)(image.Height * ((double)ImageSize / image.Width));
                        }
                        else if (myheight > ImageSize)
                        {
                            myheight = ImageSize;
                            mywidth = (int)(image.Width * ((double)ImageSize / image.Height));
                        }
                    }
                    g.DrawImage(image, (int)(((double)ImageSize - mywidth) / 2) + (actualImageSize * c) + 2, (int)(((double)ImageSize - myheight) / 2) + (actualImageSize * r) + 2 - vScrollBar1.Value, mywidth, myheight);
                    i++;
                    if (i == MainForm.tiles.Count) return;
                }
        }

        private void TileList_MouseDown(object sender, MouseEventArgs e)
        {
            int actualImageSize = imageSize + 4;
            int tilesPerRow = Width / actualImageSize;
            int numRows = (int)Math.Ceiling(MainForm.tiles.Count / (double)tilesPerRow);
            int selX = e.X / actualImageSize;
            if (selX >= tilesPerRow) return;
            int selY = (e.Y + vScrollBar1.Value) / actualImageSize;
            if (selY * tilesPerRow + selX < MainForm.tiles.Count)
                SelectedIndex = selY * tilesPerRow + selX;
            Invalidate();
        }

        private void vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            Invalidate();
        }
    }
}
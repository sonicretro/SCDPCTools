using System;
using System.Drawing;
using System.Windows.Forms;

namespace SCDPCspr
{
    public partial class PaletteImportDialog : Form
    {
        public PaletteImportDialog(Color[] colors)
        {
            InitializeComponent();
            importColors = colors;
            palette = new Color[MainForm.palette.Length];
            Array.Copy(MainForm.palette, palette, palette.Length);
            numericUpDown3.Maximum = importColors.Length;
            numericUpDown3.Value = importColors.Length;
        }

        public Color[] importColors, palette;

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
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            int maxlen = (int)Math.Min(importColors.Length - numericUpDown1.Value, palette.Length - numericUpDown2.Value);
            if (numericUpDown3.Value > maxlen)
            {
                numericUpDown3.Maximum = maxlen;
                return;
            }
            numericUpDown3.Maximum = maxlen;
            palette = new Color[MainForm.palette.Length];
            Array.Copy(MainForm.palette, palette, palette.Length);
            Array.Copy(importColors, (int)numericUpDown1.Value, palette, (int)numericUpDown2.Value, (int)numericUpDown3.Value);
            PalettePanel.Invalidate();
        }
    }
}
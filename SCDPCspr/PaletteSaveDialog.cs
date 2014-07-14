using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace SCDPCspr
{
    public partial class PaletteSaveDialog : Form
    {
        public PaletteSaveDialog()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            foreach (char item in Path.GetInvalidFileNameChars())
                if (string.IsNullOrEmpty(textBox1.Text) | textBox1.Text.Contains(item))
                    button2.Enabled = false;
        }
    }
}

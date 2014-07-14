namespace SCDPCSSEd
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.Button3 = new System.Windows.Forms.Button();
            this.Label2 = new System.Windows.Forms.Label();
            this.TextBox2 = new System.Windows.Forms.TextBox();
            this.Button2 = new System.Windows.Forms.Button();
            this.Button1 = new System.Windows.Forms.Button();
            this.Label1 = new System.Windows.Forms.Label();
            this.TextBox1 = new System.Windows.Forms.TextBox();
            this.VScrollBar1 = new System.Windows.Forms.VScrollBar();
            this.HScrollBar1 = new System.Windows.Forms.HScrollBar();
            this.Panel1 = new System.Windows.Forms.Panel();
            this.button4 = new System.Windows.Forms.Button();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.domainUpDown1 = new System.Windows.Forms.DomainUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.tileList1 = new SCDPCSSEd.TileList();
            this.SuspendLayout();
            // 
            // Button3
            // 
            this.Button3.AutoSize = true;
            this.Button3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Button3.Location = new System.Drawing.Point(311, 37);
            this.Button3.Name = "Button3";
            this.Button3.Size = new System.Drawing.Size(52, 23);
            this.Button3.TabIndex = 24;
            this.Button3.Text = "Open...";
            this.Button3.UseVisualStyleBackColor = true;
            this.Button3.Click += new System.EventHandler(this.Button3_Click);
            // 
            // Label2
            // 
            this.Label2.AutoSize = true;
            this.Label2.Location = new System.Drawing.Point(12, 42);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(43, 13);
            this.Label2.TabIndex = 23;
            this.Label2.Text = "Tile file:";
            // 
            // TextBox2
            // 
            this.TextBox2.Location = new System.Drawing.Point(61, 39);
            this.TextBox2.Name = "TextBox2";
            this.TextBox2.Size = new System.Drawing.Size(244, 20);
            this.TextBox2.TabIndex = 22;
            // 
            // Button2
            // 
            this.Button2.AutoSize = true;
            this.Button2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Button2.Enabled = false;
            this.Button2.Location = new System.Drawing.Point(309, 10);
            this.Button2.Name = "Button2";
            this.Button2.Size = new System.Drawing.Size(51, 23);
            this.Button2.TabIndex = 21;
            this.Button2.Text = "Save...";
            this.Button2.UseVisualStyleBackColor = true;
            this.Button2.Click += new System.EventHandler(this.Button2_Click);
            // 
            // Button1
            // 
            this.Button1.AutoSize = true;
            this.Button1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Button1.Location = new System.Drawing.Point(251, 10);
            this.Button1.Name = "Button1";
            this.Button1.Size = new System.Drawing.Size(52, 23);
            this.Button1.TabIndex = 20;
            this.Button1.Text = "Open...";
            this.Button1.UseVisualStyleBackColor = true;
            this.Button1.Click += new System.EventHandler(this.Button1_Click);
            // 
            // Label1
            // 
            this.Label1.AutoSize = true;
            this.Label1.Location = new System.Drawing.Point(12, 15);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(47, 13);
            this.Label1.TabIndex = 19;
            this.Label1.Text = "Map file:";
            // 
            // TextBox1
            // 
            this.TextBox1.Location = new System.Drawing.Point(65, 12);
            this.TextBox1.Name = "TextBox1";
            this.TextBox1.Size = new System.Drawing.Size(180, 20);
            this.TextBox1.TabIndex = 18;
            // 
            // VScrollBar1
            // 
            this.VScrollBar1.Location = new System.Drawing.Point(652, 66);
            this.VScrollBar1.Maximum = 3616;
            this.VScrollBar1.Name = "VScrollBar1";
            this.VScrollBar1.Size = new System.Drawing.Size(17, 480);
            this.VScrollBar1.TabIndex = 17;
            this.VScrollBar1.Scroll += new System.Windows.Forms.ScrollEventHandler(this.ScrollBar_Scroll);
            // 
            // HScrollBar1
            // 
            this.HScrollBar1.Location = new System.Drawing.Point(12, 546);
            this.HScrollBar1.Maximum = 3456;
            this.HScrollBar1.Name = "HScrollBar1";
            this.HScrollBar1.Size = new System.Drawing.Size(640, 17);
            this.HScrollBar1.TabIndex = 16;
            this.HScrollBar1.Scroll += new System.Windows.Forms.ScrollEventHandler(this.ScrollBar_Scroll);
            // 
            // Panel1
            // 
            this.Panel1.Location = new System.Drawing.Point(12, 66);
            this.Panel1.Margin = new System.Windows.Forms.Padding(3, 3, 0, 0);
            this.Panel1.Name = "Panel1";
            this.Panel1.Size = new System.Drawing.Size(640, 480);
            this.Panel1.TabIndex = 15;
            this.Panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.Panel1_Paint);
            this.Panel1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Panel1_MouseMove);
            this.Panel1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Panel1_MouseDown);
            // 
            // button4
            // 
            this.button4.AutoSize = true;
            this.button4.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.button4.Location = new System.Drawing.Point(369, 37);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(86, 23);
            this.button4.TabIndex = 27;
            this.button4.Text = "Load Palette...";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(614, 12);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(52, 17);
            this.checkBox1.TabIndex = 29;
            this.checkBox1.Text = "X Flip";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // domainUpDown1
            // 
            this.domainUpDown1.Items.Add("0°");
            this.domainUpDown1.Items.Add("270°");
            this.domainUpDown1.Items.Add("180°");
            this.domainUpDown1.Items.Add("90°");
            this.domainUpDown1.Location = new System.Drawing.Point(609, 35);
            this.domainUpDown1.Name = "domainUpDown1";
            this.domainUpDown1.Size = new System.Drawing.Size(57, 20);
            this.domainUpDown1.TabIndex = 30;
            this.domainUpDown1.Text = "0°";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(553, 37);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(50, 13);
            this.label3.TabIndex = 31;
            this.label3.Text = "Rotation:";
            // 
            // tileList1
            // 
            this.tileList1.BackColor = System.Drawing.SystemColors.Window;
            this.tileList1.Dock = System.Windows.Forms.DockStyle.Right;
            this.tileList1.ImageSize = 32;
            this.tileList1.Location = new System.Drawing.Point(672, 0);
            this.tileList1.Name = "tileList1";
            this.tileList1.SelectedIndex = -1;
            this.tileList1.Size = new System.Drawing.Size(126, 571);
            this.tileList1.TabIndex = 28;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(798, 571);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.domainUpDown1);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.Button3);
            this.Controls.Add(this.Label2);
            this.Controls.Add(this.TextBox2);
            this.Controls.Add(this.Button2);
            this.Controls.Add(this.Button1);
            this.Controls.Add(this.Label1);
            this.Controls.Add(this.TextBox1);
            this.Controls.Add(this.VScrollBar1);
            this.Controls.Add(this.HScrollBar1);
            this.Controls.Add(this.Panel1);
            this.Controls.Add(this.tileList1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "SCDPCSSEd";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.Button Button3;
        internal System.Windows.Forms.Label Label2;
        internal System.Windows.Forms.TextBox TextBox2;
        internal System.Windows.Forms.Button Button2;
        internal System.Windows.Forms.Button Button1;
        internal System.Windows.Forms.Label Label1;
        internal System.Windows.Forms.TextBox TextBox1;
        internal System.Windows.Forms.VScrollBar VScrollBar1;
        internal System.Windows.Forms.HScrollBar HScrollBar1;
        internal System.Windows.Forms.Panel Panel1;
        internal System.Windows.Forms.Button button4;
        private TileList tileList1;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.DomainUpDown domainUpDown1;
        private System.Windows.Forms.Label label3;



    }
}


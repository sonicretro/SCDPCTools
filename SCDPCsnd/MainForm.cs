using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Alvas.Audio;

namespace SCDPCsnd
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private List<byte[]> files;
        private List<string> labels;

        private void Form1_Load(object sender, EventArgs e)
        {
            Properties.Settings.Default.Reload();
            imageList1.Images.Add(GetIcon(".wav"));
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length == 1)
            {
                files = new List<byte[]>();
                labels = new List<string>();
            }
            else
                LoadFile(args[1]);
        }

        private void LoadFile(string filename)
        {
            byte[] file = System.IO.File.ReadAllBytes(filename);
            if (BitConverter.ToInt32(file, 0) != 0x4548544F)
            {
                MessageBox.Show("Not a valid sound file.");
                return;
            }
            int numSounds = BitConverter.ToInt32(file, 8);
            int soundOff = BitConverter.ToInt32(file, 0xC);
            listView1.Items.Clear();
            listView1.BeginUpdate();
            for (int i = 0; i < numSounds; i++)
            {
                int soundLen = BitConverter.ToInt32(file, (i * 4) + 0x10);
                byte[] sound = new byte[soundLen];
                Array.Copy(file, soundOff, sound, 0, soundLen);
                soundOff += soundLen;
                files.Add(sound);
                labels.Add(i.ToString());
                listView1.Items.Add(labels[i], 0);
            }
            listView1.EndUpdate();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog a = new OpenFileDialog()
            {
                DefaultExt = "cmp",
                Filter = "CMP Files|*.cmp|All Files|*.*"
            };
            if (a.ShowDialog() == DialogResult.OK)
                LoadFile(a.FileName);
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog a = new SaveFileDialog()
            {
                DefaultExt = "cmp",
                Filter = "CMP Files|*.cmp|All Files|*.*"
            };
            if (a.ShowDialog() == DialogResult.OK)
            {
                Stream file = File.Open(a.FileName, FileMode.Create, FileAccess.Write);
                BinaryWriter bw = new BinaryWriter(file);
                bw.Write(new byte[] { 0x4F, 0x54, 0x48, 0x45 });
                int fs = 0;
                foreach (byte[] item in files)
                    fs += item.Length;
                bw.Write(0x10 + (files.Count * 4) + fs);
                bw.Write(files.Count);
                bw.Write(0x10 + (files.Count * 4));
                foreach (byte[] item in files)
                    bw.Write(item.Length);
                foreach (byte[] item in files)
                    bw.Write(item);
                bw.Close();
                file.Close();
            }
        }

        private void extractAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog a = new FolderBrowserDialog() { ShowNewFolderButton = true };
            if (a.ShowDialog(this) == DialogResult.OK)
            {
                for (int i = 0; i < files.Count; i++)
                {
                    System.IO.File.WriteAllBytes(System.IO.Path.Combine(a.SelectedPath, labels[i] + ".wav"), PcmToWav(i));
                }
            }
        }

        ListViewItem selectedItem;
        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                selectedItem = listView1.GetItemAt(e.X, e.Y);
                if (selectedItem != null)
                {
                    contextMenuStrip1.Show(listView1, e.Location);
                }
            }
        }

        private void addFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog a = new OpenFileDialog()
            {
                DefaultExt = "wav",
                Filter = "WAV Files|*.wav",
                Multiselect = true
            };
            if (a.ShowDialog() == DialogResult.OK)
            {
                int i = files.Count;
                foreach (string item in a.FileNames)
                {
                    files.Add(WavToPcm(item));
                    labels.Add(Path.GetFileNameWithoutExtension(item));
                    listView1.Items.Add(labels[i], 0);
                    i++;
                }
            }
        }

        private void extractToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (selectedItem == null) return;
            SaveFileDialog a = new SaveFileDialog()
            {
                DefaultExt = "wav",
                Filter = "WAV Files|*.wav",
                FileName = selectedItem.Text + ".wav"
            };
            if (a.ShowDialog() == DialogResult.OK)
            {
                System.IO.File.WriteAllBytes(a.FileName, PcmToWav(listView1.Items.IndexOf(selectedItem)));
            }
        }

        private void replaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (selectedItem == null) return;
            int i = listView1.Items.IndexOf(selectedItem);
            string fn = labels[i] + ".wav";
            OpenFileDialog a = new OpenFileDialog()
            {
                DefaultExt = "wav",
                Filter = "WAV Files|*.wav",
                FileName = fn
            };
            if (a.ShowDialog() == DialogResult.OK)
            {
                files[i] = WavToPcm(a.FileName);
            }
        }

        private void insertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (selectedItem == null) return;
            OpenFileDialog a = new OpenFileDialog()
            {
                DefaultExt = "wav",
                Filter = "WAV Files|*.wav",
                Multiselect = true
            };
            if (a.ShowDialog() == DialogResult.OK)
            {
                int i = listView1.Items.IndexOf(selectedItem);
                foreach (string item in a.FileNames)
                {
                    files.Insert(i, WavToPcm(item));
                    labels.Insert(i, Path.GetFileNameWithoutExtension(item));
                    i++;
                }
                listView1.Items.Clear();
                listView1.BeginUpdate();
                for (int j = 0; j < files.Count; j++)
                {
                    listView1.Items.Add(labels[j], 0);
                }
                listView1.EndUpdate();
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (selectedItem == null) return;
            int i = listView1.Items.IndexOf(selectedItem);
            files.RemoveAt(i);
            labels.RemoveAt(i);
            listView1.Items.RemoveAt(i);
        }

        private string oldName;
        private void listView1_BeforeLabelEdit(object sender, LabelEditEventArgs e)
        {
            oldName = e.Label;
        }

        private void listView1_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            if (oldName == e.Label) return;
            if (e.Label.IndexOfAny(Path.GetInvalidFileNameChars()) > -1)
            {
                e.CancelEdit = true;
                MessageBox.Show("This name contains invalid characters.");
                return;
            }
            labels[e.Item] = e.Label;
        }

        private void listView1_ItemActivate(object sender, EventArgs e)
        {
            string fp = Path.Combine(Path.GetTempPath(), labels[listView1.SelectedIndices[0]] + ".wav");
            File.WriteAllBytes(fp, PcmToWav(listView1.SelectedIndices[0]));
            System.Diagnostics.Process.Start(fp);
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            files = new List<byte[]>();
            labels = new List<string>();
            listView1.Items.Clear();
        }

        private void listView1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.All;
        }

        private void listView1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] dropfiles = (string[])e.Data.GetData(DataFormats.FileDrop, true);
                int i = files.Count;
                foreach (string item in dropfiles)
                {
                    files.Add(WavToPcm(item));
                    labels.Add(Path.GetFileNameWithoutExtension(item));
                    listView1.Items.Add(labels[i], 0);
                    i++;
                }
            }
        }

        private void listView1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            string fn = Path.Combine(Path.GetTempPath(), labels[listView1.SelectedIndices[0]] + ".wav");
            File.WriteAllBytes(fn, PcmToWav(listView1.SelectedIndices[0]));
            DoDragDrop(new DataObject(DataFormats.FileDrop, new string[] { fn }), DragDropEffects.All);
        }

        private WaveFormat GetWaveFormat()
        {
            WaveFormat wf = new WaveFormat();
            wf.wFormatTag = AudioCompressionManager.PcmFormatTag;
            wf.wBitsPerSample = 8;
            wf.nChannels = 1;
            wf.nBlockAlign = (short)(wf.nChannels * (wf.wBitsPerSample / 8));
            wf.nSamplesPerSec = 11025;
            wf.nAvgBytesPerSec = wf.nChannels * wf.nSamplesPerSec * (wf.wBitsPerSample / 8);
            return wf;
        }

        private byte[] PcmToWav(int file)
        {
            WaveFormat wf = GetWaveFormat();
            FormatDetails[] fdArr = AudioCompressionManager.GetFormatList(wf);
            IntPtr format = fdArr[0].FormatHandle;
            MemoryStream ms = new MemoryStream();
            WaveWriter ww = new WaveWriter(ms, AudioCompressionManager.FormatBytes(format));
            ww.WriteData(files[file]);
            ww.Close();
            byte[] wav = ms.GetBuffer();
            ms.Close();
            return wav;
        }

        private byte[] WavToPcm(string wavfile)
        {
            WaveFormat wf = GetWaveFormat();
            FormatDetails[] fdArr = AudioCompressionManager.GetFormatList(wf);
            IntPtr format = fdArr[0].FormatHandle;
            WaveReader wr = new WaveReader(File.Open(wavfile, FileMode.Open));
            return AudioCompressionManager.Convert(wr.ReadFormat(), format, wr.ReadData(), false);
        }

        [System.Runtime.InteropServices.DllImport("shell32.dll")]
        private static extern IntPtr ExtractIconA(int hInst, string lpszExeFileName, int nIconIndex);

        private Icon GetIcon(string file)
        {
            Microsoft.Win32.RegistryKey k = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(file.IndexOf('.') > -1 ? file.Substring(file.LastIndexOf('.')) : file);
            if (k == null)
                k = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey("*");
            k = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey((string)k.GetValue("", "*"));
            k = k.OpenSubKey("DefaultIcon");
            string iconpath = "C:\\Windows\\system32\\shell32.dll,0";
            if (k != null)
                iconpath = (string)k.GetValue("", "C:\\Windows\\system32\\shell32.dll,0");
            int iconind = 0;
            if (iconpath.LastIndexOf(',') > iconpath.LastIndexOf('.'))
            {
                iconind = int.Parse(iconpath.Substring(iconpath.LastIndexOf(',') + 1));
                iconpath = iconpath.Remove(iconpath.LastIndexOf(','));
            }
            try
            {
                return Icon.FromHandle(ExtractIconA(0, iconpath.Replace("%1", file), iconind));
            }
            catch (Exception)
            {
                return Icon.FromHandle(ExtractIconA(0, "C:\\Windows\\system32\\shell32.dll", 0));
            }
        }
    }
}
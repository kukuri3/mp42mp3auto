using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;


namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            textBox2.Text = Path.GetDirectoryName(Application.ExecutablePath);
        }

        private void button1_Click(object sender, EventArgs e)
        {

            //フォルダの指定
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            fbd.Description = "フォルダを選んでください";

            DialogResult dr = fbd.ShowDialog();

            if (dr == DialogResult.OK)
            {
                textBox2.Text = fbd.SelectedPath;
                // Properties.Settings.Default.srcpath = fbd.SelectedPath;

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            xConvertProcess();
        }
        private void xConvertProcess()
        {
            string pathstring = textBox2.Text;
            xLog("xGetList(" + pathstring + ")");
            string apppath = Application.ExecutablePath;
            string appdir = Path.GetDirectoryName(apppath);
            string ffmpegpath = Path.Combine(appdir, "ffmpeg.exe");
            //フォルダ内のファイル一覧を取得
            IEnumerable<string> files = Directory.EnumerateFiles(pathstring, "*.mp4", SearchOption.TopDirectoryOnly);
            //datagridへのデータ追加
            foreach (string file in files)
            {
                xLog(file + "\n");
                string dir = Path.GetDirectoryName(file);
                string fnwoext = Path.GetFileNameWithoutExtension(file);
                string mp3filename = Path.Combine(dir, fnwoext+".mp3");
                string arg=" -y -i \""+file+"\" -vn -ac 2 -ar 44100 -ab 128k "+mp3filename;
                if (!System.IO.File.Exists(mp3filename))
                {

                    System.Diagnostics.Process p = System.Diagnostics.Process.Start(ffmpegpath, arg);
                    p.WaitForExit();
                }
                else
                {
                    xLog(mp3filename + "は存在します。");
                }
            }
        }
        private void xLog(String s)
        {
            //ログ出力
            DateTime dt = DateTime.Now;
            textBox1.AppendText(dt.ToString() + " " + s + "\n");
            System.IO.StreamWriter sw = new System.IO.StreamWriter(@"log.txt", true, System.Text.Encoding.GetEncoding("shift_jis"));
            sw.Write(dt.ToString() + s + "\r\n");
            sw.Close();
        }

    }
}

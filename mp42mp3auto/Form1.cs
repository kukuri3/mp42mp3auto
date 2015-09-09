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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        Queue gQ;
        bool gProcessActive = false;
        int gTick = 0;

        public Form1()
        {
            InitializeComponent();
            gQ=new Queue();
            textBox2.Text = Path.GetDirectoryName(Application.ExecutablePath);
            timer1.Enabled = true;
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
            
            
            //フォルダ内のファイル一覧を取得
            IEnumerable<string> files = Directory.EnumerateFiles(pathstring, "*.mp4", SearchOption.TopDirectoryOnly);
            //datagridへのデータ追加
            int total=files.Count();
            int count=0;
            foreach (string mp4filepath in files)
            {
                count++;
                xLog(count + "/" + total);
                xLog(mp4filepath + "\n");
                label1.Text = "processing "+count+"/"+ total+"...";
                Application.DoEvents();
                xFileProcess(mp4filepath);
             }
            xLog("convert finish.");
        }

        private void xFileProcess(string mp4filepath)
        {

            string dir = Path.GetDirectoryName(mp4filepath);
            string fnwoext = Path.GetFileNameWithoutExtension(mp4filepath);
            string mp3filename = Path.Combine(dir, fnwoext + ".mp3");
            string arg = " -y -i \"" + mp4filepath + "\" -vn -ac 2 -ar 44100 -ab 128k " + mp3filename;
             string apppath = Application.ExecutablePath;
            string appdir = Path.GetDirectoryName(apppath);
            string ffmpegpath = Path.Combine(appdir, "ffmpeg.exe");
 

            if (!System.IO.File.Exists(mp3filename))
            {

                System.Diagnostics.Process p = new Process();

                p.StartInfo.FileName = ffmpegpath; // 実行するファイル
                p.StartInfo.Arguments = arg;
                p.StartInfo.CreateNoWindow = true; // コンソールを開かない
                p.StartInfo.UseShellExecute = false; // シェル機能を使用しない

                p.StartInfo.RedirectStandardOutput = true; // 標準出力をリダイレクト
                p.StartInfo.RedirectStandardError = true;

                xLog("now processing " + mp4filepath + "...");
                Application.DoEvents();

                button2.Enabled = false;
                button2.Refresh();
                gProcessActive = true;  //プロセスの二重起動防止セマフォ
                p.Start(); // アプリの実行開始
                Application.DoEvents();

                //p.WaitForExit();


                
                while (!p.HasExited)
                {
                    //p.WaitForExit(100);
                    //string output = p.StandardError.ReadToEnd(); // 標準出力の読み取り
                    //string output = p.StandardOutput.ReadToEnd(); // 標準出力の読み取り
                    System.Threading.Thread.Sleep(100);
                    //output = output.Replace("\r\r\n", "\n"); // 改行コードの修正
                    //xLog(output); // ［出力］ウィンドウに出力

                    Application.DoEvents();
                }

                xLog("finish.");
                gProcessActive = false; //プロセスの二重起動防止セマフォ
                button2.Enabled = true;
                button2.Refresh();

            }
            else
            {
                xLog(mp3filename + "は存在します。");
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

        private void textBox1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void textBox1_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {

                // ドラッグ中のファイルやディレクトリの取得
                string[] drags = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (string d in drags)
                {
                    if (System.IO.File.Exists(d)) gQ.Enqueue(d);   //ファイルの場合
                    else if (System.IO.Directory.Exists(d))
                    {
                        //ディレクトリ内のファイルをすべて取得
                        string[] files = System.IO.Directory.GetFiles(d, "*", System.IO.SearchOption.AllDirectories);
                        foreach (string d2 in files)
                        {
                            gQ.Enqueue(d2);    //ディレクトリ内のファイルの場合
                        }
                    }
                }
                //                e.Effect = DragDropEffects.Copy;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label1.Text = "remaining " + gQ.Count + " filse. ";
            if (gProcessActive == true)
            {
                label1.Text += "Convert progress.. elspsed " + gTick + " tickes.";
                gTick++;
            }
            else
            {
                label1.Text += "Idel.";
                gTick=0;
            }
            
            if ((gQ.Count > 0)&&(gProcessActive==false))
            {
                xFileProcess(gQ.Dequeue().ToString());
            }
        }

    }
}

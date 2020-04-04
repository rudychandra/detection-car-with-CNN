using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NN;
using System.IO;

namespace CNN_identifikasi_mobil
{
    public partial class Form1 : Form
    {
        Bitmap OriginalImage, GrayscaleImage, ThresoldImage, TestImage;
        double[] TestData;
        double[] HasilTest3;
        NeuralNetwork CNN = new NeuralNetwork();
        List<double[]> CC = new List<double[]>();
        List<double[]> InputSet = new List<double[]>();
        List<double[]> OutputSet = new List<double[]>();
        int EPOCH;
        int HiddenLayer;
        int InputLength, OutputLength, TotalSample;
        double NetError;
        double LR;
        List<Bitmap> TrainingImage = new List<Bitmap>();
        List<String> TrainingFile = new List<String>();
        List<double[]> TrainingData = new List<double[]>();
        String Log;
        List<String> TrainingFile_SUV = new List<String>();
        List<String> TrainingFile_MPV = new List<String>();
        List<String> TrainingFile_sedan = new List<String>();
        List<String> DaftarFile_SUV = new List<String>();
        List<String> DaftarFile_MPV = new List<String>();
        List<String> DaftarFile_sedan = new List<String>();

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "Buka File Uji";
                openFileDialog.InitialDirectory = Application.StartupPath;
                openFileDialog.Filter = "JPG (*.jpg)|*.jpg|BMP (*.bmp)|*.bmp";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    textBox1.Text = Path.GetFullPath(openFileDialog.FileName);
                    OriginalImage = new Bitmap(openFileDialog.FileName);
                    OriginalImage = OriginalImage.Clone(new Rectangle(0, 0, OriginalImage.Width, OriginalImage.Height), System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    pictureBox1.Image = OriginalImage;
                }
            }
            catch { }
        }

        public static Bitmap LinearInterpolationScale(Bitmap bmp, int newXSize, int newYSize)
        {
            Bitmap newBMP = new Bitmap(newXSize, newYSize);
            int[] temp = new int[newXSize * newYSize];
            int x, y;
            Color A, B, C, D;
            float x_ratio = ((float)(bmp.Width - 1)) / newXSize;
            float y_ratio = ((float)(bmp.Height - 1)) / newYSize;
            float w, h;
            for (int i = 0; i < newYSize; i++)
            {
                for (int j = 0; j < newXSize; j++)
                {
                    x = (int)(x_ratio * j);
                    y = (int)(y_ratio * i);
                    w = (x_ratio * j) - x;
                    h = (y_ratio * i) - y;
                    A = bmp.GetPixel(x, y);
                    B = bmp.GetPixel(x + 1, y);
                    C = bmp.GetPixel(x, y + 1);
                    D = bmp.GetPixel(x + 1, y + 1);
                    int r = (int)(A.R * (1 - w) * (1 - h) + B.R * (w) * (1 - h) + C.R * (h) * (1 - w) + D.R * (w * h));
                    int g = (int)(A.G * (1 - w) * (1 - h) + B.G * (w) * (1 - h) + C.G * (h) * (1 - w) + D.G * (w * h));
                    int b = (int)(A.B * (1 - w) * (1 - h) + B.B * (w) * (1 - h) + C.B * (h) * (1 - w) + D.B * (w * h));
                    Color col = Color.FromArgb(r, g, b);
                    newBMP.SetPixel(j, i, col);
                }
            }
            return newBMP;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Buka File Training SUV";
            openFileDialog.InitialDirectory = Application.StartupPath + "\\SUV";
            openFileDialog.Filter = "JPG (*.jpg)|*.jpg|BMP (*.bmp)|*.bmp";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Multiselect = true;
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;
            DaftarFile_SUV = openFileDialog.FileNames.ToList();
            TrainingFile_SUV = DaftarFile_SUV.ToList();

            Log = DaftarFile_SUV.Count().ToString() + " File SUV Telah Dimuat\n";

            textBox12.Text = DaftarFile_SUV.Count().ToString();
            textBox10.Text = (DaftarFile_SUV.Count + DaftarFile_sedan.Count + DaftarFile_MPV.Count).ToString();
            richTextBox1.Text += Log;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Buka File Training Sedan";
            openFileDialog.InitialDirectory = Application.StartupPath + "\\sedan";
            openFileDialog.Filter = "JPG (*.jpg)|*.jpg|BMP (*.bmp)|*.bmp";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Multiselect = true;
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;
            DaftarFile_sedan = openFileDialog.FileNames.ToList();
            TrainingFile_sedan = DaftarFile_sedan.ToList();

            Log = DaftarFile_sedan.Count().ToString() + " File Sedan Telah Dimuat\n";

            textBox3.Text = DaftarFile_sedan.Count().ToString();
            textBox10.Text = (DaftarFile_SUV.Count + DaftarFile_sedan.Count + DaftarFile_MPV.Count).ToString();
            richTextBox1.Text += Log;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Buka File Training MPV";
            openFileDialog.InitialDirectory = Application.StartupPath + "\\MPV";
            openFileDialog.Filter = "JPG (*.jpg)|*.jpg|BMP (*.bmp)|*.bmp";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Multiselect = true;
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;
            DaftarFile_MPV = openFileDialog.FileNames.ToList();
            TrainingFile_MPV = DaftarFile_MPV.ToList();

            Log = DaftarFile_MPV.Count().ToString() + " File MPV Telah Dimuat\n";

            textBox11.Text = DaftarFile_MPV.Count().ToString();
            textBox10.Text = (DaftarFile_SUV.Count + DaftarFile_sedan.Count + DaftarFile_MPV.Count).ToString();
            richTextBox1.Text += Log;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            try
            {
                
            }
            catch { }
        }

        public static Bitmap GrayScale(Bitmap b)
        {
            byte[,] image = new byte[b.Width, b.Height];
            Bitmap hasil = new Bitmap(b.Width, b.Height);
            //applying grayscale
            for (int i = 0; i < b.Width; i++)
            {
                for (int j = 0; j < b.Height; j++)
                {
                    Color c1 = b.GetPixel(i, j);
                    int r1 = c1.R;
                    int g1 = c1.G;
                    int b1 = c1.B;
                    byte gray = (byte)(.299 * r1 + .587 * g1 + .114 * b1);
                    image[i, j] = gray;
                    hasil.SetPixel(i, j, Color.FromArgb(image[i, j], image[i, j], image[i, j]));
                }
            }
            return hasil;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                
            }
            catch { }
        }

        public static Bitmap Thresholdings(Bitmap b, int batas)
        {
            int[,] binary = new int[b.Width, b.Height];
            Bitmap hasil = new Bitmap(b.Width, b.Height);
            for (int i = 0; i < b.Width; i++)
            {
                for (int j = 0; j < b.Height; j++)
                {

                    Color c1 = b.GetPixel(i, j);
                    int thd = c1.G;
                    byte newPix;

                    if (thd >= batas)
                    {
                        newPix = 255;
                    }
                    else
                    {
                        newPix = 0;
                    }

                    binary[i, j] = newPix;

                    hasil.SetPixel(i, j, Color.FromArgb(newPix, newPix, newPix));
                }
            }
            return hasil;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime dp = DateTime.Now;
                GrayscaleImage = GrayScale(OriginalImage);
                pictureBox2.Image = GrayscaleImage;
                TimeSpan ds = DateTime.Now - dp;
                double waktu = ds.TotalMilliseconds;
                textBox15.Text = (waktu / 1000).ToString();

                DateTime dp2 = DateTime.Now;
                ThresoldImage = Thresholdings(GrayscaleImage, Convert.ToInt32(textBox4.Text));
                pictureBox3.Image = ThresoldImage;
                TimeSpan ds2 = DateTime.Now - dp2;
                double waktu2 = ds2.TotalMilliseconds;
                textBox2.Text = (waktu2 / 1000).ToString();

                DateTime dp3 = DateTime.Now;
                Testuji();
                TimeSpan ds3 = DateTime.Now - dp3;
                double waktu3 = ds.TotalMilliseconds;
                textBox6.Text = (waktu3 / 1000).ToString();
            }
            catch { }
        }

        public void Testuji()
        {
            int idx = 0;
            ThresoldImage = LinearInterpolationScale(ThresoldImage, 30, 30);
            TestData = new double[ThresoldImage.Width * ThresoldImage.Height];
            for (int y = 0; y < ThresoldImage.Height; y++)
                for (int x = 0; x < ThresoldImage.Width; x++)
                {
                    if (ThresoldImage.GetPixel(x, y).R == 0)
                        TestData[idx] = 0;
                    else
                        TestData[idx] = 1;
                    idx++;
                }
            double Min = 0;
            int idxx = 0;
            HasilTest3 = CNN.process(TestData);

            for (int i = 0; i < HasilTest3.Length; i++)
            {
                if (Min < HasilTest3[i])
                {
                    Min = HasilTest3[i];
                    idxx = i;
                }
            }
            textBox5.Text = CekKlasifikasi(idxx);
        }

        public String CekKlasifikasi(int idx)
        {
            String R = "";
            int suv = Convert.ToInt32(textBox12.Text);
            int mpv = suv + Convert.ToInt32(textBox11.Text);
            int sedan = mpv + Convert.ToInt32(textBox3.Text);

            if (idx >= 0 && idx < suv)
                R = "SUV";
            else if (idx >= suv && idx < mpv)
                R = "MPV";
            else if (idx >= mpv && idx < sedan)
                R = "Sedan";
            return R;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime dp = DateTime.Now;
                ProsesSample();
                EkstrakFiturTraining();
                InitNetwork();
                Training();
                TimeSpan ds = DateTime.Now - dp;
                double waktu = ds.TotalMilliseconds;
                textBox13.Text = (waktu / 1000).ToString();
            }
            catch { }
        }

        public void Training()
        {

            NetError = CNN.train(ref InputSet, ref OutputSet);
            Application.DoEvents();
            richTextBox1.Text += "Training Selesai\n";
        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void button10_Click(object sender, EventArgs e)
        {
            if (pictureBox3.Image != null)
            {
                SaveFileDialog save = new SaveFileDialog();
                save.Filter = "image Files| *.bmp";
                save.FileName = "*.bmp";
                if (save.ShowDialog() == DialogResult.OK)
                {
                    pictureBox3.Image.Save(save.FileName, System.Drawing.Imaging.ImageFormat.Bmp);
                    MessageBox.Show("Gambar disimpan", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("Gambar masih kosong");
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            if (pictureBox2.Image != null)
            {
                SaveFileDialog save = new SaveFileDialog();
                save.Filter = "image Files| *.bmp";
                save.FileName = "*.bmp";
                if (save.ShowDialog() == DialogResult.OK)
                {
                    pictureBox2.Image.Save(save.FileName, System.Drawing.Imaging.ImageFormat.Bmp);
                    MessageBox.Show("Gambar disimpan", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("Gambar masih kosong");
            }
        }

        private void tabPage3_Click(object sender, EventArgs e)
        {

        }

        private void InitNetwork()
        {
            CC.Clear();
            EPOCH = Convert.ToInt32(textBox7.Text);
            LR = Convert.ToDouble(textBox8.Text);
            HiddenLayer = Convert.ToInt32(textBox9.Text);
            InputLength = TrainingData[0].Count();
            OutputLength = TotalSample;
            InputSet = TrainingData;
            CNN.Init(ref LR, ref InputLength, ref HiddenLayer, ref OutputLength, EPOCH);
            for (int j = 0; j < OutputLength; j++)
            {
                double[] T = new double[OutputLength];
                for (int i = 0; i < OutputLength; i++)
                    if (i == j)
                        T[i] = 1;
                    else
                        T[i] = 0;
                OutputSet.Add(T);
            }
            richTextBox1.Text += "Inisialisasi Selesai\n";
        }

        public void EkstrakFiturTraining()
        {
            int idx = 0;
            TrainingData.Clear();
            for (int i = 0; i < TrainingImage.Count(); i++)
            {
                idx = 0;
                TrainingData.Add(new double[TrainingImage[i].Width * TrainingImage[i].Height]);

                for (int y = 0; y < TrainingImage[i].Height; y++)
                    for (int x = 0; x < TrainingImage[i].Width; x++)
                    {
                        if (TrainingImage[i].GetPixel(x, y).R == 0)
                            TrainingData[i][idx] = 0;
                        else
                            TrainingData[i][idx] = 1;
                        idx++;
                    }
            }

            richTextBox1.Text += "Ektraksi Fitur Selesai\n";
        }

        public void ProsesSample()
        {
            TrainingImage.Clear();
            for (int i = 0; i < TrainingFile_SUV.Count(); i++)
                TrainingImage.Add(Thresholdings(GrayScale(FiletoImage(TrainingFile_SUV[i])), Convert.ToInt32(textBox4.Text)));
            for (int i = 0; i < TrainingFile_MPV.Count(); i++)
                TrainingImage.Add(Thresholdings(GrayScale(FiletoImage(TrainingFile_MPV[i])), Convert.ToInt32(textBox4.Text)));
            for (int i = 0; i < TrainingFile_sedan.Count(); i++)
                TrainingImage.Add(Thresholdings(GrayScale(FiletoImage(TrainingFile_sedan[i])), Convert.ToInt32(textBox4.Text)));
            TotalSample = TrainingImage.Count();
        }

        public Bitmap FiletoImage(String Path)
        {
            Bitmap imgSJ;
            imgSJ = new Bitmap(Path);
            int StepX, StepY;
            int Lebar, Tinggi;
            int MaxPool = 0;
            Lebar = 30;
            Tinggi = 30;
            StepX = imgSJ.Width / Lebar;
            StepY = imgSJ.Height / Tinggi;
            Log = "";
            int[] MaxPoolList = new int[Lebar * Tinggi];
            for (int j = 0; j < Tinggi; j++)
            {
                for (int i = 0; i < Lebar; i++)
                {
                    MaxPool = 0;
                    for (int y = 0; y < StepY; y++)
                    {
                        for (int x = 0; x < StepX; x++)
                        {
                            int Pxl = imgSJ.GetPixel((i * StepX) + x, (j * StepY) + y).R;
                            if (MaxPool < Pxl)
                            {
                                MaxPool = Pxl;
                                MaxPoolList[(j * Tinggi) + i] = MaxPool;
                            }
                        }
                    }
                }
            }
            imgSJ = LinearInterpolationScale(imgSJ, Lebar, Tinggi);
            return imgSJ;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            DaftarFile_SUV.Clear();
            DaftarFile_MPV.Clear();
            DaftarFile_sedan.Clear();

            TrainingFile_SUV.Clear();
            TrainingFile_MPV.Clear();
            TrainingFile_sedan.Clear();
            TrainingFile.Clear();
            TrainingImage.Clear();
            TrainingData.Clear();

            textBox12.Text = "0";
            textBox11.Text = "0";
            textBox10.Text = "0";
            textBox3.Text = "0";
            richTextBox1.Clear();
            textBox13.Clear();
        }

    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace Wykrywanie_Krawędzi_Staż
{
    
    public partial class Form1 : Form
    {
        Bitmap bmp;
        Bitmap bmpbackup;
        private Bitmap cbmp
        {
            get
            {
                if (bmp == null)
                {
                    bmp = new Bitmap(1, 1);
                }
                return bmp;
            }
            set
            {
                bmp = value;
            }
        }

        //Sobel X
        private static double[,] xSobel
        {
            get
            {
                return new double[,]
                {
                    { -1, 0, 1 },
                    { -2, 0, 2 },
                    { -1, 0, 1 }
                };
            }
        }

        //Sobel y
        private static double[,] ySobel
        {
            get
            {
                return new double[,]
                {
                    {  1,  2,  1 },
                    {  0,  0,  0 },
                    { -1, -2, -1 }
                };
            }
        }




        private static Bitmap CornerDetect(Bitmap sourceImage, double[,] xkernel, double[,] ykernel, double factor = 1, int bias = 0, bool grayscale = false)
        {
            int width = sourceImage.Width;
            int height = sourceImage.Height;
            BitmapData srcData = sourceImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);


            int bytes = srcData.Stride * srcData.Height;


            byte[] pixelBuffer = new byte[bytes];
            byte[] resultBuffer = new byte[bytes];


            IntPtr srcScan0 = srcData.Scan0;


            Marshal.Copy(srcScan0, pixelBuffer, 0, bytes);


            sourceImage.UnlockBits(srcData);

            if (grayscale == true)
            {
                float rgb = 0;
                for (int i = 0; i < pixelBuffer.Length; i += 4)
                {
                    rgb = pixelBuffer[i] * .21f;
                    rgb += pixelBuffer[i + 1] * .71f;
                    rgb += pixelBuffer[i + 2] * .071f;
                    pixelBuffer[i] = (byte)rgb;
                    pixelBuffer[i + 1] = pixelBuffer[i];
                    pixelBuffer[i + 2] = pixelBuffer[i];
                    pixelBuffer[i + 3] = 255;
                }
            }


            double xr = 0.0;
            double xg = 0.0;
            double xb = 0.0;
            double yr = 0.0;
            double yg = 0.0;
            double yb = 0.0;
            double rt = 0.0;
            double gt = 0.0;
            double bt = 0.0;

            int filterOffset = 1;
            int calcOffset = 0;
            int byteOffset = 0;

           
            for (int OffsetY = filterOffset; OffsetY < height - filterOffset; OffsetY++)
            {
                for (int OffsetX = filterOffset; OffsetX < width - filterOffset; OffsetX++)
                {
                 
                    xr = xg = xb = yr = yg = yb = 0;
                    rt = gt = bt = 0.0;

                 
                    byteOffset = OffsetY * srcData.Stride + OffsetX * 4;

                   
                    for (int filterY = -filterOffset; filterY <= filterOffset; filterY++)
                    {
                        for (int filterX = -filterOffset; filterX <= filterOffset; filterX++)
                        {
                            calcOffset = byteOffset + filterX * 4 + filterY * srcData.Stride;
                            xb += (double)(pixelBuffer[calcOffset]) * xkernel[filterY + filterOffset, filterX + filterOffset];
                            xg += (double)(pixelBuffer[calcOffset + 1]) * xkernel[filterY + filterOffset, filterX + filterOffset];
                            xr += (double)(pixelBuffer[calcOffset + 2]) * xkernel[filterY + filterOffset, filterX + filterOffset];
                            yb += (double)(pixelBuffer[calcOffset]) * ykernel[filterY + filterOffset, filterX + filterOffset];
                            yg += (double)(pixelBuffer[calcOffset + 1]) * ykernel[filterY + filterOffset, filterX + filterOffset];
                            yr += (double)(pixelBuffer[calcOffset + 2]) * ykernel[filterY + filterOffset, filterX + filterOffset];
                        }
                    }

                    
                    bt = Math.Sqrt((xb * xb) + (yb * yb));
                    gt = Math.Sqrt((xg * xg) + (yg * yg));
                    rt = Math.Sqrt((xr * xr) + (yr * yr));

                    
                    if (bt > 255) bt = 255;
                    else if (bt < 0) bt = 0;
                    if (gt > 255) gt = 255;
                    else if (gt < 0) gt = 0;
                    if (rt > 255) rt = 255;
                    else if (rt < 0) rt = 0;

                  
                    resultBuffer[byteOffset] = (byte)(bt);
                    resultBuffer[byteOffset + 1] = (byte)(gt);
                    resultBuffer[byteOffset + 2] = (byte)(rt);
                    resultBuffer[byteOffset + 3] = 255;
                }
            }

            Bitmap resultImage = new Bitmap(width, height);


            BitmapData resultData = resultImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

          
            Marshal.Copy(resultBuffer, 0, resultData.Scan0, resultBuffer.Length);

           
            resultImage.UnlockBits(resultData);

            return resultImage;
        }
        public static Bitmap ImageSegment(Bitmap image, int x, int y)
        {
            int w = image.Width;
            int h = image.Height;
            BitmapData image_data = image.LockBits(
                new Rectangle(0, 0, w, h),
                ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);
            int bytes = image_data.Stride * image_data.Height;
            byte[] buffer = new byte[bytes];
            byte[] result = new byte[bytes];
            Marshal.Copy(image_data.Scan0, buffer, 0, bytes);
            image.UnlockBits(image_data);
            
            int d0 = 30;
            int sample_position = x * 3 + y * image_data.Stride;
            for (int i = 0; i < bytes - 3; i += 3)
            {
                double eu = 0;
                for (int c = 0; c < 3; c++)
                {
                    eu += Math.Pow(buffer[i + c] - buffer[sample_position + c], 2);
                }
                eu = Math.Sqrt(eu);
                for (int c = 0; c < 3; c++)
                {
                    result[i + c] = (byte)(eu > d0 ? 0 : 255);
                }
            }
            Bitmap res_img = new Bitmap(w, h);
            BitmapData res_data = res_img.LockBits(
                new Rectangle(0, 0, w, h),
                ImageLockMode.WriteOnly,
                PixelFormat.Format24bppRgb);
            Marshal.Copy(result, 0, res_data.Scan0, bytes);
            res_img.UnlockBits(res_data);
            return res_img;
        }

        public Form1()
        {
            
            InitializeComponent();
            

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                bmp = (Bitmap)Bitmap.FromFile(openFileDialog1.FileName);
                bmpbackup = (Bitmap)Bitmap.FromFile(openFileDialog1.FileName);
                Image image = Image.FromFile(openFileDialog1.FileName);
                pictureBox1.BackgroundImage = image;
                

              
                
            }
        }
        void Clear()
        {
            bmp = bmpbackup;
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Clear();
            bmp = CornerDetect(bmp, xSobel, ySobel, 1.0, 0, true);
            
            pictureBox2.Image = cbmp;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if(saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                bmp.Save(saveFileDialog1.FileName);
            }
           
            
        }

        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {

           
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Clear();
            bmp = ImageSegment(bmp, 80, 80);
            pictureBox2.Image = cbmp;
        }
    }
}

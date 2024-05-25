using System;

public class GraphicEngine
{
	public GraphicEngine()
	{
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
        public static Bitmap ImageSegment(this Bitmap image, int x, int y)
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
                double euclidean = 0;
                for (int c = 0; c < 3; c++)
                {
                    euclidean += Math.Pow(buffer[i + c] - buffer[sample_position + c], 2);
                }
                euclidean = Math.Sqrt(euclidean);
                for (int c = 0; c < 3; c++)
                {
                    result[i + c] = (byte)(euclidean > d0 ? 0 : 255);
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
    }
}

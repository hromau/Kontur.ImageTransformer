using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kontur.ImageTransformer
{
    public class NewFilter
    {
        public static Bitmap DrawAsGrayscale(Image sourceImage,byte x,byte y,byte w,byte h)
        {
            ColorMatrix colorMatrix = new ColorMatrix(new float[][]
                                                {
                                                    new float[] {.3f, .3f, .3f, 0, 0},
                                                    new float[] {.59f, .59f, .59f, 0, 0},
                                                    new float[] {.11f, .11f, .11f, 0, 0},
                                                    new float[] {0, 0, 0, 1, 0},
                                                    new float[] {0, 0, 0, 0, 1}
                                                });

            return ApplyColorMatrix(sourceImage, colorMatrix,x,y,w,h);
        }
        public static Bitmap DrawAsSepiaTone(Image sourceImage, byte x, byte y, byte w, byte h)
        {
            ColorMatrix colorMatrix = new ColorMatrix(new float[][]
                                                {
                                                    new float[] {.393f, .349f, .272f, 0, 0},
                                                    new float[] {.769f, .686f, .534f, 0, 0},
                                                    new float[] {.189f, .168f, .131f, 0, 0},
                                                    new float[] {0, 0, 0, 1, 0},
                                                    new float[] {0, 0, 0, 0, 1}
                                                });

            return ApplyColorMatrix(sourceImage, colorMatrix,x,y, w, h);
        }
        public static byte[] DrawAsThreshold(byte[] byteBuffer, byte thresholdX, byte x, byte y, byte w, byte h)
        {
            byte[] pixelBuffer = new byte[byteBuffer.LongLength];
            double blue = 0;
            double green = 0;
            double red = 0;


            ImageAttributes imageAttributes = new ImageAttributes();
            imageAttributes.SetThreshold(1,)

            for (int k = 8; k + 4 < pixelBuffer.Length; k += 4)
            {
                blue = byteBuffer[k];
                green = byteBuffer[k + 1];
                red = byteBuffer[k + 2];

                var intens = (byteBuffer[k] + byteBuffer[k + 1] + byteBuffer[k + 2]) / 3;

                if (blue >= 255 * thresholdX / 100)
                {
                    blue = 255;
                }
                else
                {
                    blue = 0;
                }


                if (green >= 255 * thresholdX / 100)
                {
                    green = 255;
                }
                else
                {
                    green = 0;
                }


                if (red > 255 * thresholdX / 100)
                {
                    red = 255;
                }
                else
                {
                    red = 0;
                }


                pixelBuffer[k] = (byte)blue;
                pixelBuffer[k + 1] = (byte)green;
                pixelBuffer[k + 2] = (byte)red;
            }

            return GetByteOutputArray(GetArgbCopy(ArrayToImage(pixelBuffer),x,y,w,h));
        }

        private static Bitmap GetArgbCopy(Image sourceImage, byte x, byte y, byte w,byte h)
        {
            Bitmap bmpNew = new Bitmap(w, h, PixelFormat.Format32bppArgb);

            using (Graphics graphics = Graphics.FromImage(bmpNew))
            {
                graphics.DrawImage(sourceImage, new Rectangle(x,y, bmpNew.Width, bmpNew.Height), new Rectangle(x,y, bmpNew.Width, bmpNew.Height), GraphicsUnit.Pixel);
                graphics.Flush();
                //graphics.DrawImage(sourceImage,new Rectangle(x,y,w,h),new Rectangle(x,y,w,h),)
            }

            return bmpNew;
        }
        private static Bitmap ApplyColorMatrix(Image sourceImage, ColorMatrix colorMatrix,byte x,byte y,byte w,byte h)
        {
            Bitmap bmp32BppSource = GetArgbCopy(sourceImage,x,y,w,h);
            Bitmap bmp32BppDest = new Bitmap(bmp32BppSource.Width, bmp32BppSource.Height, PixelFormat.Format32bppArgb);

            using (Graphics graphics = Graphics.FromImage(bmp32BppDest))
            {
                ImageAttributes bmpAttributes = new ImageAttributes();
                bmpAttributes.SetColorMatrix(colorMatrix);

                graphics.DrawImage(bmp32BppSource, new Rectangle(0, 0, bmp32BppSource.Width, bmp32BppSource.Height),
                                 0, 0, bmp32BppSource.Width, bmp32BppSource.Height, GraphicsUnit.Pixel, bmpAttributes);

            }

            bmp32BppSource.Dispose();

            return bmp32BppDest;
        }
        //Получаем количество байт в загруженном изображении
        public static byte[] GetByteOutputArray(Bitmap input_image)
        {
            if (input_image == null)
                return null;
            int in_bytes = input_image.Width * input_image.Height * 3;
            Rectangle rect = new Rectangle
                (
                    0,
                    0,
                    input_image.Width,
                    input_image.Height
                );

            BitmapData input_image_Data = input_image.LockBits
                (
                    rect,
                    ImageLockMode.ReadOnly,
                    PixelFormat.Format24bppRgb
                );


            byte[] out_bytes = new byte[in_bytes];

            System.Runtime.InteropServices.Marshal.Copy
                (
                    input_image_Data.Scan0,
                    out_bytes,
                    0,
                    in_bytes
                );
            input_image.UnlockBits(input_image_Data);

            return out_bytes;//На выходе - массив байт
        }


        public static Image ArrayToImage(byte[] inputArray)
        {
            MemoryStream ms = new MemoryStream(inputArray, 0, inputArray.Length);
            ms.Write(inputArray, 0, inputArray.Length);
            try
            {
                Image a = Image.FromStream(ms);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            return Image.FromStream(ms);
        }
    }
}
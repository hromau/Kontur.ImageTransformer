using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Kontur.ImageTransformer
{
    public static class ImageFiltering
    {
        //private static Bitmap GetArgbCopy(this Image sourceImage, ref byte[] byteBuffer, byte x,byte y,byte w,byte h)
        //{
        //    //Bitmap bmpNew = new Bitmap(w, h, PixelFormat.Format32bppArgb);

        //    Bitmap bmpNew = MakeBitmap(byteBuffer, w, h);

        //    //Bitmap bitmap = AsBitmap(ref sourceImage, ref byteBuffer, x, y, w, h);

        //    BitmapData bmpData = bmpNew.LockBits(new Rectangle(x, y, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

        //    IntPtr ptr = bmpData.Scan0;

        //    //обратно
        //    Marshal.Copy(byteBuffer, 0, ptr, byteBuffer.Length);

        //    bmpNew.UnlockBits(bmpData);

        //    //bmpData = null;
        //    //byteBuffer = null;

        //    using (Graphics graphics = Graphics.FromImage(bmpNew))
        //    {
        //        graphics.DrawImage(sourceImage, new Rectangle(x, y, bmpNew.Width, bmpNew.Height), new Rectangle(x, y, bmpNew.Width, bmpNew.Height), GraphicsUnit.Pixel);
        //        graphics.Flush();
        //    }

        //    return bmpNew;
        //}

        //идет кодирование и обратно
        //public static Bitmap AsBitmap(ref Image sourceImage,ref byte[] byteBuffer, byte x, byte y, byte w, byte h)
        //{
        //    Bitmap bmpNew = GetArgbCopy(ref sourceImage,x,y,w,h);
        //    BitmapData bmpData = bmpNew.LockBits(new Rectangle(x, y, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

        //    IntPtr ptr = bmpData.Scan0;

        //    //обратно
        //    Marshal.Copy(byteBuffer, 0, ptr, byteBuffer.Length);

        //    bmpNew.UnlockBits(bmpData);

        //    bmpData = null;
        //    byteBuffer = null;

        //    return bmpNew;
        //}

        public static Bitmap DrawAsGrayscale(ref byte[] byteBuffer, byte x, byte y, byte w, byte h)
        {
            ColorMatrix colorMatrix = new ColorMatrix(new float[][]
                                                {
                                                    new float[] {.3f, .3f, .3f, 0, 0},
                                                    new float[] {.59f, .59f, .59f, 0, 0},
                                                    new float[] {.11f, .11f, .11f, 0, 0},
                                                    new float[] {0, 0, 0, 1, 0},
                                                    new float[] {0, 0, 0, 0, 1}
                                                });

            return ApplyColorMatrix(colorMatrix,ref byteBuffer,x,y,w,h);
        }

        private static Bitmap ApplyColorMatrix(ColorMatrix colorMatrix,ref byte[] byteBuffer, byte x, byte y, byte w, byte h)
        {
            //Bitmap bmp32BppSource = GetArgbCopy(sourceImage, ref byteBuffer,x,y,w,h);
            Bitmap bmp32BppSource = MakeBitmap(byteBuffer, w, h);
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

        public static Bitmap DrawAsSepiaTone(ref byte[] byteBuffer, byte x, byte y, byte w, byte h)
        {
            ColorMatrix colorMatrix = new ColorMatrix(new float[][]
                                                {
                                                    new float[] {.393f, .349f, .272f, 0, 0},
                                                    new float[] {.769f, .686f, .534f, 0, 0},
                                                    new float[] {.189f, .168f, .131f, 0, 0},
                                                    new float[] {0, 0, 0, 1, 0},
                                                    new float[] {0, 0, 0, 0, 1}
                                                });

            return ApplyColorMatrix(colorMatrix,ref byteBuffer, x, y, w, h);
        }

        //Формируем изображение из массива пикселей
        public static Bitmap MakeBitmap(byte[] input, byte width, byte height)
        {
            if (input.Length % 3 != 0) return null;

            Bitmap output = new Bitmap(width, height);
            BitmapData image_output_Data = output.LockBits
                (
                    new Rectangle(0, 0, width, height),
                    ImageLockMode.ReadWrite,
                    PixelFormat.Format32bppArgb
                );

            System.Runtime.InteropServices.Marshal.Copy
                (
                    input,
                    0,
                    image_output_Data.Scan0,
                    input.Length
                );
            output.UnlockBits(image_output_Data);

            return output;
        }

        //Получаем количество байт в загруженном изображении
        public static byte[] GetBytesOutputArray(Bitmap input_image)
        {
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

    }
}
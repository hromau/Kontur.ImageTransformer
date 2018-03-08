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
    public class ModificationImage
    {
        private static Bitmap CropPhoto(Image sourceImage, byte x, byte y, byte w, byte h)
        {
            if (x < 0)
            {
                w -= x;
                x = 0;
            }
            if (y < 0)
            {
                h -= y;
                y = 0;
            }
            Bitmap bmpNew = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            using (Graphics graphics = Graphics.FromImage(bmpNew))
            {
                graphics.DrawImage(sourceImage, new Rectangle(x, y, bmpNew.Width, bmpNew.Height), new Rectangle(x, y, bmpNew.Width, bmpNew.Height), GraphicsUnit.Pixel);
                graphics.Flush();
            }
            return bmpNew;
        }


        private static Image ArrayToImage(byte[] inputArray)
        {
            MemoryStream ms = new MemoryStream(inputArray, 0, inputArray.Length);
            ms.Write(inputArray, 0, inputArray.Length);
            try
            {
                Image a = Image.FromStream(ms);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " " + DateTime.Now);
                Console.WriteLine(ex.StackTrace);
            }
            return Image.FromStream(ms);
        }


        public static byte[] FlipV(byte[] inputArray, byte x,byte y,byte w,byte h)
        {

            Image sourceImage = ArrayToImage(inputArray);

            Bitmap outputBitmap = CropPhoto(sourceImage, x, y, w, h);

            if (outputBitmap == null)
            {
                return null;
            }

               
            outputBitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);


            int inputBytes = outputBitmap.Width * outputBitmap.Height * 3;
            Rectangle rect = new Rectangle
                (
                    0,
                    0,
                    outputBitmap.Width,
                    outputBitmap.Height
                );

            BitmapData inputImageData = outputBitmap.LockBits
                (
                    rect,
                    ImageLockMode.ReadOnly,
                    PixelFormat.Format24bppRgb
                );


            byte[] outputBytes = new byte[inputBytes];

            System.Runtime.InteropServices.Marshal.Copy
                (
                    inputImageData.Scan0,
                    outputBytes,
                    0,
                    inputBytes
                );
            outputBitmap.UnlockBits(inputImageData);

            return outputBytes;//На выходе - массив байт

        }


        public static byte[] FlipH(byte[] inputArray, byte x, byte y, byte w, byte h)
        {
            Image sourceImage = ArrayToImage(inputArray);

            Bitmap outputBitmap = CropPhoto(sourceImage, x, y, w, h);

            if (outputBitmap == null)
            {
                return null;
            }

 outputBitmap.RotateFlip(RotateFlipType.RotateNoneFlipX);

            int inputBytes = outputBitmap.Width * outputBitmap.Height * 3;
            Rectangle rect = new Rectangle
                (
                    0,
                    0,
                    outputBitmap.Width,
                    outputBitmap.Height
                );

            BitmapData inputImageData = outputBitmap.LockBits
                (
                    rect,
                    ImageLockMode.ReadOnly,
                    PixelFormat.Format24bppRgb
                );


            byte[] outputBytes = new byte[inputBytes];

            System.Runtime.InteropServices.Marshal.Copy
                (
                    inputImageData.Scan0,
                    outputBytes,
                    0,
                    inputBytes
                );
            outputBitmap.UnlockBits(inputImageData);

            return outputBytes;//На выходе - массив байт
        }


        public static byte[] RotateCW(byte[] inputArray, byte x, byte y, byte w, byte h)
        {
            Image sourceImage = ArrayToImage(inputArray);

            Bitmap outputBitmap = CropPhoto(sourceImage, x, y, w, h);

            if (outputBitmap == null)
            {
                return null;
            }

outputBitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);


            int inputBytes = outputBitmap.Width * outputBitmap.Height * 3;
            Rectangle rect = new Rectangle
                (
                    0,
                    0,
                    outputBitmap.Width,
                    outputBitmap.Height
                );

            BitmapData inputImageData = outputBitmap.LockBits
                (
                    rect,
                    ImageLockMode.ReadOnly,
                    PixelFormat.Format24bppRgb
                );


            byte[] outputBytes = new byte[inputBytes];

            System.Runtime.InteropServices.Marshal.Copy
                (
                    inputImageData.Scan0,
                    outputBytes,
                    0,
                    inputBytes
                );
            outputBitmap.UnlockBits(inputImageData);

            return outputBytes;//На выходе - массив байт
        }


        public static byte[] RotateCWW(byte [] inputArray, byte x, byte y, byte w, byte h)
        {
            Image sourceImage = ArrayToImage(inputArray);

            Bitmap outputBitmap = CropPhoto(sourceImage, x, y, w, h);

            if (outputBitmap == null)
            {
                return null;
            }

 outputBitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);

            int inputBytes = outputBitmap.Width * outputBitmap.Height * 3;
            Rectangle rect = new Rectangle
                (
                    0,
                    0,
                    outputBitmap.Width,
                    outputBitmap.Height
                );

            BitmapData inputImageData = outputBitmap.LockBits
                (
                    rect,
                    ImageLockMode.ReadOnly,
                    PixelFormat.Format24bppRgb
                );


            byte[] outputBytes = new byte[inputBytes];

            System.Runtime.InteropServices.Marshal.Copy
                (
                    inputImageData.Scan0,
                    outputBytes,
                    0,
                    inputBytes
                );
            outputBitmap.UnlockBits(inputImageData);

            return outputBytes;//На выходе - массив байт
      
        }

    }
}

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
    public class Filter2
    {
        public static Bitmap DrawAsGrayscale(Stream stream)
        {
            ColorMatrix colorMatrix = new ColorMatrix(new float[][]
                                                {
                                                    new float[] {.3f, .3f, .3f, 0, 0},
                                                    new float[] {.59f, .59f, .59f, 0, 0},
                                                    new float[] {.11f, .11f, .11f, 0, 0},
                                                    new float[] {0, 0, 0, 1, 0},
                                                    new float[] {0, 0, 0, 0, 1}
                                                });

            return ApplyColorMatrix(colorMatrix,stream);
        }
        private static Bitmap ApplyColorMatrix(ColorMatrix colorMatrix,Stream stream)
        {
            //Bitmap bmp32BppSource = GetArgbCopy(sourceImage);
            //Bitmap bmp32BppSource = new Bitmap(stream);
            Bitmap bmp32BppSource = null;
            try
            {
                bmp32BppSource = new Bitmap(stream);
            }
            catch(Exception ex)
            {
                return null;
            }
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
    }
}

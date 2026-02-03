using System.Drawing;
using System.Drawing.Imaging;

namespace DocumentTools.Services
{
    /// <summary>
    /// 图像处理服务：封装图像转换和处理操作
    /// </summary>
    public class ImageProcessingService : IImageProcessingService
    {
        /// <summary>
        /// 将位图转换为256色
        /// </summary>
        public Bitmap ConvertTo256Color(Bitmap inputBitmap)
        {
            if (inputBitmap == null) return null;

            Bitmap outputBitmap = new Bitmap(
                inputBitmap.Width,
                inputBitmap.Height,
                PixelFormat.Format8bppIndexed);

            ColorPalette palette = outputBitmap.Palette;
            for (int i = 0; i < 256; i++)
            {
                palette.Entries[i] = Color.FromArgb(i, i, i);
            }
            outputBitmap.Palette = palette;

            BitmapData inputData = inputBitmap.LockBits(
                new Rectangle(0, 0, inputBitmap.Width, inputBitmap.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);
            BitmapData outputData = outputBitmap.LockBits(
                new Rectangle(0, 0, outputBitmap.Width, outputBitmap.Height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format8bppIndexed);

            unsafe
            {
                byte* inputPointer = (byte*)inputData.Scan0;
                byte* outputPointer = (byte*)outputData.Scan0;

                for (int y = 0; y < inputBitmap.Height; y++)
                {
                    for (int x = 0; x < inputBitmap.Width; x++)
                    {
                        byte r = inputPointer[2];
                        byte g = inputPointer[1];
                        byte b = inputPointer[0];
                        byte grayValue = (byte)(0.299 * r + 0.587 * g + 0.114 * b);

                        outputPointer[0] = grayValue;

                        inputPointer += 3;
                        outputPointer++;
                    }

                    inputPointer += inputData.Stride - inputBitmap.Width * 3;
                    outputPointer += outputData.Stride - outputBitmap.Width;
                }
            }

            inputBitmap.UnlockBits(inputData);
            outputBitmap.UnlockBits(outputData);

            return outputBitmap;
        }
    }

    public interface IImageProcessingService
    {
        Bitmap ConvertTo256Color(Bitmap inputBitmap);
    }
}

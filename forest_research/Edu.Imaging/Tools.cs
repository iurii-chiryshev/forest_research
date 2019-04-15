using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;

namespace Edu.Imaging
{
    /// <summary>
    /// Инструменты для работы с изображениями
    /// статический класс
    /// </summary>
    public static class Tools
    {
        /// <summary>
        /// Вырезать из входного изображения
        /// область интереса
        /// ! поддрерживается только Format24bppRgb
        /// </summary>
        /// <param name="src">входное изображенние</param>
        /// <param name="rect">прямоугольник</param>
        /// <param name="pixelFormat">формат пикселей, если не указано будет такойже как у src</param>
        /// <returns>либо изображение либо null</returns>
        public static Bitmap ROI(Bitmap src,Rectangle rect,PixelFormat pf = PixelFormat.Undefined)
        {
            if (src == null) return null;
            Rectangle src_rect = new Rectangle(Point.Empty,src.Size);
            if (src_rect.Contains(rect) == false || // если rect не полностью содержится в src_rect
                rect.Width <= 0 || // или имеет нулевые размеры
                rect.Height <= 0) return null;

            PixelFormat src_pf = src.PixelFormat;
            PixelFormat dst_pf = pf == PixelFormat.Undefined ? src_pf : pf;
            bool valid_src_pf = (src_pf == PixelFormat.Format24bppRgb) /*|| (src_pf == PixelFormat.Format32bppRgb)*/;
            bool valid_dst_pf = (dst_pf == PixelFormat.Format24bppRgb) /*|| (dst_pf == PixelFormat.Format32bppRgb)*/;
            if (valid_src_pf == false)
            {
                throw new Exception("Формат пикселе входного изображения не поддерживается: " + src_pf.ToString());
            }
            if (valid_dst_pf == false)
            {
                throw new Exception("Формат пикселе выходного изображения не поддерживается: " + dst_pf.ToString());
            }
            return _ROI_24bpp_24bpp(src, rect);
        }

        /// <summary>
        /// Вырезать из входного изображения в вформате Format24bppRgb
        /// область интереса в формате Format24bppRgb
        /// </summary>
        /// <param name="src">входное изображенние Format24bppRgb</param>
        /// <param name="rect">прямоугольник</param>
        /// <returns>область интереса в формате Format24bppRgb </returns>
        private static Bitmap _ROI_24bpp_24bpp(Bitmap src, Rectangle rect)
        {
            PixelFormat pf = PixelFormat.Format24bppRgb;
            Bitmap dst = new Bitmap(rect.Width, rect.Height, pf);
            BitmapData src_bmp_data = src.LockBits(new Rectangle(Point.Empty, src.Size), ImageLockMode.ReadOnly, pf);
            {

                BitmapData dst_bmp_data = dst.LockBits(new Rectangle(Point.Empty, dst.Size), ImageLockMode.WriteOnly, pf);
                {
                    unsafe
                    {
                        int src_stride = src_bmp_data.Stride;
                        int dst_stride = dst_bmp_data.Stride;
                        byte* src0 = (byte*)src_bmp_data.Scan0.ToPointer() + rect.Y * src_stride + rect.X * 3;
                        byte* dst0 = (byte*)dst_bmp_data.Scan0.ToPointer();

                        for (int y = 0; y < rect.Height; y++, src0 += src_stride, dst0 += dst_stride)
                        {
                            for (int x = 0; x < rect.Width * 3; x++)
                            {
                                dst0[x] = src0[x];
                            }
                        }

                    }
                }
                dst.UnlockBits(dst_bmp_data);
            }
            src.UnlockBits(src_bmp_data);
            return dst;
        }
    }
}

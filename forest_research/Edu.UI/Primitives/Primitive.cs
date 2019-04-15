using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Edu.UI.Primitives
{
    /// <summary>
    /// Абстрактный класс всех графических примитивов
    /// </summary>
    public abstract class Primitive
    {
        /// <summary>
        /// Ограничивающий прямоугольник
        /// </summary>
        protected Rectangle _boundRect = Rectangle.Empty;

        /// <summary>
        /// Конструтор без параметров
        /// </summary>
        public Primitive()
        {

        }

        /// <summary>
        /// Функция рисования, реализуется дочерними классами
        /// </summary>
        /// <param name="g">graphics</param>
        /// <returns></returns>
        public abstract void Draw(Graphics g);

        /// <summary>
        /// Ограничивающий прямоугольник
        /// </summary>
        public Rectangle BoundRect
        {
            get
            {
                return _boundRect;
            }
        }
    }
}

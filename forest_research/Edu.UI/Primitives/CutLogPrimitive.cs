using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Edu.UI.Primitives
{
    /// <summary>
    /// Графический примитив
    /// рисует окружность по двум точкам,
    /// и прямоугольник вокруг нее
    /// </summary>
    public class CutLogPrimitive: Primitive
    {

        /// <summary>
        /// Конструктор копирования
        /// </summary>
        /// <param name="primitive">гра</param>
        /// <returns></returns>
        public CutLogPrimitive(CutLogPrimitive primitive)
            : this()
        {
            if (primitive == null) return;
            this._pen.Color = primitive._pen.Color;
            this._pen.Width = primitive._pen.Width;
            SetPoints(primitive._circlePoint, primitive._centerPoint);
        }
        
        /// <summary>
        /// Конструктор без параметроввв
        /// </summary>
        /// <returns></returns>
        public CutLogPrimitive()
            : this(Color.Red,1.0f)
        {

        }

        /// <summary>
        /// Конструктор с параметрами
        /// </summary>
        /// <param name="color">цвет</param>
        /// <param name="width">ширина пера</param>
        /// <returns></returns>
        public CutLogPrimitive(Color color,float width)
            : base()
        {
            _pen = new Pen(color, width);
        }


        /// <summary>
        /// центр окружности
        /// </summary>
        private Point _centerPoint = Point.Empty;
        /// <summary>
        /// точка на окружности
        /// </summary>
        private Point _circlePoint = Point.Empty;
        /// <summary>
        /// карандаш для рисования
        /// </summary>
        private Pen _pen = null;
        /// <summary>
        /// ограничивающий прямоугольник для окружности
        /// </summary>
        private Rectangle _circleRect = Rectangle.Empty;
        /// <summary>
        /// Возвращает или задает центр окружности
        /// </summary>
        public Point CenterPoint
        {
            get
            {
                return _centerPoint;
            }
            set
            {
                SetPoints(_circlePoint,value);
            }
        }
        /// <summary>
        /// возвращает или задает точку на оружности
        /// </summary>
        public Point CirclePoint
        {
            get
            {
                return _circlePoint;
            }
            set
            {
                SetPoints(value, _centerPoint);
            }
        }

        /// <summary>
        /// Метод рисования графического примитива
        /// </summary>
        /// <param name="g"></param>
        /// <returns></returns>
        public override void Draw(System.Drawing.Graphics g)
        {
            GraphicsContainer containerState = null;
            try
            {
                /*создать графический контейнер, в нем можно делать с графиксом что угодно*/
                containerState = g.BeginContainer();
                using (Matrix transform = g.Transform)
                {
                    /*
                     * чтобы нарисовать примитив в нужном масштабе и положении
                     * нужно умножить матрицу преобразования графикса на матрицу ей обратную
                     * т.е на единичной матрице графикса рисуем примитив
                     */
                    transform.Invert();
                    g.MultiplyTransform(transform);
                    if (g.VisibleClipBounds.IntersectsWith(_boundRect) == true)
                    {
                        /*
                         * если видимый прямоугольник графикса пересекается 
                         * с ограничивающим прямоугольноком примитива
                         * рисуем его
                         */
                        g.DrawEllipse(_pen, _circleRect);
                        g.DrawRectangle(_pen, _boundRect);
                        g.DrawLine(_pen, _circlePoint, _centerPoint);
                    }
                }
            }
            finally
            {
                if (containerState != null)
                {
                    /*закрыть графический контейнер*/
                    g.EndContainer(containerState);
                }
            }
            
        }

        /// <summary>
        /// Установить новые значения точек
        /// </summary>
        /// <param name="circlePoint">точка на окружности</param>
        /// <param name="centerPoint">центр окружности</param>
        /// <returns></returns>
        public void SetPoints(Point circlePoint,Point centerPoint)
        {
            _circlePoint = circlePoint;
            _centerPoint = centerPoint;
            int radii = (int)Math.Sqrt((_circlePoint.X - _centerPoint.X)*(_circlePoint.X - _centerPoint.X) +
                                        (_circlePoint.Y - _centerPoint.Y) * (_circlePoint.Y - _centerPoint.Y));
            int width = radii * 2;
            int offset = (int)(radii * 0.5f);
            int br_width = width + 2 * offset;
            _circleRect = new Rectangle(_centerPoint.X - radii, _centerPoint.Y - radii, width, width);
            _boundRect = new Rectangle(_circleRect.X - offset, _circleRect.Y - offset, br_width, br_width);
        }
    }
}

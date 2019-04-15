using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Edu.UI.Properties;
using Edu.UI.Controls;
using Edu.UI.Primitives;

namespace Edu.UI.Tools
{
    public class CutLogTool: Tool
    {

        public CutLogTool(ScrollablePictureBox scrollablePictureBox)
            : base(scrollablePictureBox)
        {
            
        }

        private MouseButtons _mouseDownButton = MouseButtons.None;
        private Matrix _transforms = new Matrix();
        private CutLogPrimitive _cutLog = null;
        /// <summary>
        /// _points[0] - точка на окружности;
        /// _points[1] - центральная точка.
        /// </summary>
        private Point[] _points = new Point[2];
        
        internal override void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (_mouseDownButton != MouseButtons.None)
            {
                /*если уже чего нить нажато выходи*/
                return;
            }
            if (_scrollablePictureBox.ImageRectangle.Contains(e.Location) &&
                e.Button == MouseButtons.Left)
            {
                /*
                 * если попали левой кнопкой мыши в область изображения
                 */

                /*сохранить себе матицу преобразования*/
                using (Matrix transform = _scrollablePictureBox.Transform)
                {
                    /*
                     * мы же рисуем на pictureBox, на котором в какомто масштабе
                     * и с каким-то сдвигом нарисовано изображение
                     * а нам нужно рисовать примитив с привязкой к исходному изображению в начеле координат (0,0)
                     * поэтому делаем вот такую штуку
                     * теперь если умножить лубую точку, кудабы не ткнул пользователь на _transforms
                     * получем ее же только относительно начала координат (0,0) немасштабированного изображения
                     */
                    _transforms.Reset();
                    _transforms.Multiply(transform);
                    _transforms.Invert();
                }
                _cutLog = new CutLogPrimitive();
                _mouseDownButton = e.Button;
                _points[0] = _points[1] = e.Location;
            }
            else
            {
                /* не попали на изображение -> сбрасывай и выходи
                 */
                _points[0] = _points[1] = Point.Empty;
                _mouseDownButton = MouseButtons.None;
            }
        }

        internal override void OnMouseUp(object sender, MouseEventArgs e)
        {
            if (_mouseDownButton == MouseButtons.Left &&
                e.Button == MouseButtons.Left &&
                _points[0] != _points[1])
            {
                /*
                 * если была нажата левая клавиша мыши
                 * и отпущена левая клавиша мыши
                 * и точки разные
                 */
                    _points[1] = e.Location;
                    Point[] transformPoints = TransformPoints(_transforms, _points);
                    _cutLog.CirclePoint = transformPoints[0];
                    _cutLog.CenterPoint = transformPoints[1];
                    OnPrimitiveDrawn(this, new PrimitiveEventArgs(new CutLogPrimitive(_cutLog)));
            }
            _mouseDownButton = MouseButtons.None;
            _points[0] = _points[1] = Point.Empty;
        }

        internal override void OnMouseEnter(object sender, EventArgs e)
        {
            /*throw new NotImplementedException();*/
        }

        internal override void OnResize(object sender, EventArgs e)
        {
            /*throw new NotImplementedException();*/
        }

        internal override void OnMouseWheel(object sender, MouseEventArgs e)
        {
           /* throw new NotImplementedException();*/
        }

        internal override void OnPaint(PaintEventArgs pe)
        {
            if (_mouseDownButton == MouseButtons.Left)
            {
                _cutLog.Draw(pe.Graphics);                
            }
        }

        internal override void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_mouseDownButton == MouseButtons.Left && 
                _points[0] != e.Location)
            {
                /*если нажата левая клавиша мыши и было движение*/
                _points[1] = e.Location;
                Point[] transformPoints = TransformPoints(_transforms, _points);
                _cutLog.SetPoints(transformPoints[0],transformPoints[1]);
            }
        }

        internal override void OnToolChanged(object sender, EventArgs e)
        {
            /*
             * Только что был изменен инструмент
             * инициализируй курсор
             */
            SetCursor(Cursors.Cross);
        }

        internal override void OnToolChanging(object sender, EventArgs e)
        {
            /*
             * Инструмент собираются менять
             * инициализируй курсор по умолчанию
             */
            SetCursor(Cursors.Default);
        }

        private Point[] TransformPoints(Matrix transform, Point[] points)
        {
            if (points == null || points.Length == 0) return null;
            Point[] result = new Point[points.Length];
            points.CopyTo(result,0);
            if (transform == null) return result;
            transform.TransformPoints(result);
            return result;
        }
    }
}

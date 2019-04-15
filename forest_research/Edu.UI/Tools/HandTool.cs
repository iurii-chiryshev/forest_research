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

namespace Edu.UI.Tools
{
    public class HandTool: Tool
    {
        public HandTool(ScrollablePictureBox scrollablePictureBox)
            : base(scrollablePictureBox)
        {
            
        }

        /// <summary>
        /// статический конструктор
        /// </summary>
        /// <returns></returns>
        static HandTool()
        {
            using (var stream = new MemoryStream(Resources.CaptureHand))
            {
                captureHandCursor = new Cursor(stream);
            }

            using (var stream = new MemoryStream(Resources.Hand))
            {
                handCursor = new Cursor(stream);
            }
        }

        static readonly Cursor captureHandCursor;
        static readonly Cursor handCursor;

        private Point _mouseDownPosition = Point.Empty;
        private MouseButtons _mouseDownButton = MouseButtons.None;
        private Point _bufferPoint = Point.Empty;

        internal override void OnMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (_mouseDownButton != MouseButtons.None)
            {
                /*если уже чего нить нажато выходи*/
                return;
            }
            if (_scrollablePictureBox.ImageRectangle.Contains(e.Location) &&
                e.Button == MouseButtons.Left )
            {
                /*
                 * если попали левой кнопкой мыши в область изображения
                 * запомнить точку и клавишу
                 * и изменить курсор
                 */
                _mouseDownPosition = e.Location;
                _mouseDownButton = e.Button;
                SetCursor(captureHandCursor);
            }
            else
            {
                /* не попали на изображение -> сбрасывай и выходи
                 */
                _mouseDownPosition = Point.Empty;
                _mouseDownButton = MouseButtons.None;
            }
            
        }

        internal override void OnMouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            /*сбросить все переменные
             * установаить курсор (рука)
             */
            _mouseDownPosition = Point.Empty;
            _mouseDownButton = MouseButtons.None;
            SetCursor(handCursor);
        }

        internal override void OnMouseEnter(object sender, EventArgs e)
        {
            
        }

        internal override void OnResize(object sender, EventArgs e)
        {
            
        }

        internal override void OnMouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (_mouseDownButton != MouseButtons.None) return; // нельзя менять масштаб, если нажата кнопка
            SetZoomScale(e.Delta, e.Location);

        }

        internal override void OnPaint(System.Windows.Forms.PaintEventArgs pe)
        {
            /*ничего рисовать не нужно*/
        }

        internal override void OnMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (_mouseDownButton == MouseButtons.Left // нажата левая клавиша мыши
                && (_scrollablePictureBox.HScrollBar.Visible 
                    || _scrollablePictureBox.VScrollBar.Visible 
                    ) // какая-нибудь полоса прокрутки видима 
                )
            {
                int hShift = (int)((e.Location.X - _mouseDownPosition.X) * _scrollablePictureBox.InverseZoomScale);
                int vShift = (int)((e.Location.Y - _mouseDownPosition.Y) * _scrollablePictureBox.InverseZoomScale);

                if (hShift == 0 && vShift == 0) return;

                //if (horizontalScrollBar.Visible)
                _scrollablePictureBox.HScrollBar.Value =
                      Math.Max(Math.Min(_scrollablePictureBox.HScrollBar.Value - hShift, _scrollablePictureBox.HScrollBar.Maximum), _scrollablePictureBox.HScrollBar.Minimum);
                //if (verticalScrollBar.Visible)
                _scrollablePictureBox.VScrollBar.Value =
                      Math.Max(Math.Min(_scrollablePictureBox.VScrollBar.Value - vShift, _scrollablePictureBox.VScrollBar.Maximum), _scrollablePictureBox.VScrollBar.Minimum);

                if (hShift != 0) _mouseDownPosition.X = e.Location.X;
                if (vShift != 0) _mouseDownPosition.Y = e.Location.Y;
            }
        }

        internal override void OnToolChanged(object sender, EventArgs e)
        {
            /*
             * Только что был изменен инструмент
             * инициализируй курсор
             */
            SetCursor(handCursor);
        }

        internal override void OnToolChanging(object sender, EventArgs e)
        {
            /*
             * Инструмент собираются менять
             * инициализируй курсор по умолчанию
             */
            SetCursor(Cursors.Default);
        }
        
    }
}

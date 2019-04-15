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
    public class ZoomTool: Tool
    {
        public ZoomTool(ScrollablePictureBox scrollablePictureBox)
            : base(scrollablePictureBox)
        {
            
        }

        /// <summary>
        /// статический конструктор
        /// </summary>
        /// <returns></returns>
        static ZoomTool()
        {
            // zoom
            using (var stream = new MemoryStream(Resources.Zoom))
            {
                zoomCursor = new Cursor(stream);
            }
            // zoom +
            using (var stream = new MemoryStream(Resources.ZoomIn))
            {
                zoomInCursor = new Cursor(stream);
            }
            // zoom -
            using (var stream = new MemoryStream(Resources.ZoomOut))
            {
                zoomOutCursor = new Cursor(stream);
            }
        }

        static readonly Cursor zoomCursor;
        static readonly Cursor zoomInCursor;
        static readonly Cursor zoomOutCursor;

        private Point _mouseDownPosition = Point.Empty;
        private MouseButtons _mouseDownButton = MouseButtons.None;

        
        
        internal override void OnMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (_mouseDownButton != MouseButtons.None)
            {
                /*если уже чего нить нажато выходи*/
                return;
            }
            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
            {
                /*
                 * если была нажата правая или левая кнопка
                 * запомни, выстави курсор
                 */
                _mouseDownPosition = e.Location;
                _mouseDownButton = e.Button;
                SetCursor(_mouseDownButton == MouseButtons.Left ? zoomInCursor : zoomOutCursor);
            }
            else
            {
                /*
                 * не попали на изображение или кнопка не та -> сбрасывай и выходи
                 */
                _mouseDownPosition = Point.Empty;
                _mouseDownButton = MouseButtons.None;
            }
            
        }

        internal override void OnMouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (_mouseDownButton == MouseButtons.Left || _mouseDownButton == MouseButtons.Right)
            {
                SetZoomScale(_mouseDownButton == MouseButtons.Left ? 1 : -1, _mouseDownPosition);
            }
            _mouseDownPosition = Point.Empty;
            _mouseDownButton = MouseButtons.None;
            SetCursor(zoomCursor);
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
            
        }

        internal override void OnMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            
        }

        internal override void OnToolChanged(object sender, EventArgs e)
        {
            /*
             * Только что был изменен инструмент
             * инициализируй курсор
             */
            SetCursor(zoomCursor);
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

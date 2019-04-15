using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Edu.UI.Controls;
using Edu.UI.Primitives;

namespace Edu.UI.Tools
{
    public abstract class Tool
    {
        protected ScrollablePictureBox _scrollablePictureBox = null;

        public Tool(ScrollablePictureBox scrollablePictureBox)
        {
            this._scrollablePictureBox = scrollablePictureBox;
        }

        internal abstract void OnMouseDown(object sender, MouseEventArgs e);

        internal abstract void OnMouseUp(object sender, MouseEventArgs e);

        internal abstract void OnMouseEnter(object sender, EventArgs e);

        internal abstract void OnResize(object sender, EventArgs e);

        internal abstract void OnMouseWheel(object sender, MouseEventArgs e);

        internal abstract void OnPaint(PaintEventArgs pe);

        internal abstract void OnMouseMove(object sender, MouseEventArgs e);

        internal abstract void OnToolChanged(object sender, EventArgs e);

        internal abstract void OnToolChanging(object sender, EventArgs e);

        /// <summary>
        /// Установвить курсор в pictureBox
        /// </summary>
        /// <param name="cursor"></param>
        /// <returns></returns>
        protected void SetCursor(Cursor cursor)
        {
            if (_scrollablePictureBox == null) return;
            _scrollablePictureBox.Cursor =  cursor != null ? cursor : Cursors.Default;
            
        }

        internal void SetZoomScale(int delta, Point location)
        {
            if (_scrollablePictureBox == null) return;
            float scale = 1.0f;
            if (delta > 0)
            {
                //увеличить масштаб
                scale = 2.0f;
            }
            else if (delta < 0)
            {
                //уменьшить масштаб
                scale = 0.5f;
            }
            else
            {
                //непонятно чего -> на выход
                return;
            }
            scale = scale * _scrollablePictureBox.ZoomScale;
            _scrollablePictureBox.SetZoomScale(scale, location);
        }

        public EventHandler<PrimitiveEventArgs> PrimitiveDrawn;

        protected void OnPrimitiveDrawn(object sender, PrimitiveEventArgs e)
        {
            if (PrimitiveDrawn != null)
            {
                PrimitiveDrawn(sender, e);
            }
        }
    }
}

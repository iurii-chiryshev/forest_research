using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;
using Edu.UI.Tools;

namespace Edu.UI.Controls
{
   /// <summary>
   /// PictureBox c ���������� ��������� � ��������������� �����������
   /// ��� ���������� ���� ����� �����������
   /// </summary>
    public class ScrollablePictureBox : PictureBox
    {
        /// <summary>
        /// ���������� ��� �������� ����������
        /// </summary>
        [Browsable(false)]
        public Tool Tool
        {
            get
            {
                return _tool;
            }
            set
            {

                OnToolChanging(this, null);
                _tool = value;
                OnToolChanged(this, null);
            }
        }
     
        /// <summary>
        /// ������� �����������, ������������ ��������� ������� 
        /// </summary>
        [Browsable(false)]
        public float ZoomScale
        {
            get
            {
                return _zoomScale;
            }
            private set
            {
                _zoomScale = value;
                /*����������� �������� ��������*/
                _inverseZoomScale = 1.0f / _zoomScale;
            }
        }

        /// <summary>
        /// �������� �������� ZoomScale
        /// </summary>
        [Browsable(false)]
        public float InverseZoomScale
        {
            get
            {
                return _inverseZoomScale;
            }
        }

        /// <summary>
        /// ������ ������ � ������ �������� �����������
        /// </summary>
        [Browsable(false)]
        public Point ZoomOffset
        {
            get
            {
                Point result = Point.Empty;
                if (hScrollBar.Visible)
                {
                    /*���� �������������� ����� �����, ������ �������� �� �������*/
                    result.X = -hScrollBar.Value;
                }
                else
                {
                    /*���� �������������� ����� �������, ������ �������� ������ ������� -> � ����� ��*/
                    result.X = (int)(Math.Max((ViewSize.Width - ImageSize.Width), 0) * _inverseZoomScale * 0.5f);
                }

                if (vScrollBar.Visible)
                {
                    /*���� ������������ ����� �����, ������ �������� �� �������*/
                    result.Y = -vScrollBar.Value;
                }
                else
                {
                    /*���� ������������ ����� �������, ������ �������� ������ ������� -> � ����� ��*/
                    result.Y = (int)(Math.Max((ViewSize.Height - ImageSize.Height), 0) * _inverseZoomScale * 0.5f);
                }
                return result;
            }
        }

        /// <summary>
        /// ������� ��������������
        /// �� ���������� ��������� � ���������� �����������
        /// </summary>
        [Browsable(false)]
        public Matrix Transform
        {
            get
            {
                Matrix mat = new Matrix();
                mat.Scale(_zoomScale, _zoomScale);
                Point zoomOffset = this.ZoomOffset;
                mat.Translate(zoomOffset.X, zoomOffset.Y);
                return mat;
            }
        }

        /// <summary>
        /// ���������� ������ ����������� ������������
        /// ������������ ������� ��������� ��������
        /// ����� ���� ��� �������������, ��� � �������������
        /// </summary>
        [Browsable(false)]
        public Point ImageOffset
        {
            get
            {
                Point result = Point.Empty;
                if (hScrollBar.Visible)
                {
                    /*���� �������������� ����� �����, ������ �������� �� �������*/
                    result.X = -(int)(hScrollBar.Value * _zoomScale);
                }
                else
                {
                    /*���� �������������� ����� �������, ������ �������� ������ ������� -> � ����� ��*/
                    result.X = Math.Max((int)((ViewSize.Width - ImageSize.Width) * 0.5f), 0);
                }

                if (vScrollBar.Visible)
                {
                    /*���� ������������ ����� �����, ������ �������� �� �������*/
                    result.Y = -(int)(vScrollBar.Value * _zoomScale);
                }
                else
                {
                    /*���� ������������ ����� �������, ������ �������� ������ ������� -> � ����� ��*/
                    result.Y = Math.Max((int)((ViewSize.Height - ImageSize.Height) * 0.5f), 0);
                }
                return result;
            }
        }

        /// <summary>
        /// ������ �����������,
        /// ����������, � ������ �������� 
        /// </summary>
        [Browsable(false)]
        public Size ImageSize
        {
            get
            {
                if (Image == null) return new Size();
                Size imgSize = Image.Size;
                return new Size(
                    (int)Math.Round(imgSize.Width * _zoomScale),
                    (int)Math.Round(imgSize.Height * _zoomScale));
            }

        }

        /// <summary>
        /// �������������, � ������� ����� �����������
        /// � ������ �������� � ������ ����� ���������
        /// </summary>
        [Browsable(false)]
        public Rectangle ImageRectangle
        {
            get
            {
                return new Rectangle(ImageOffset, ImageSize);
            }

        }

        /// <summary>
        /// ������ ���������� �������
        /// � ������ ����������/������������ ����� ���������
        /// </summary>
        [Browsable(false)]
        public Size ViewSize
        {
            get
            {
                return new Size(
                ClientSize.Width - (vScrollBar.Visible ? vScrollBar.Width : 0),
                ClientSize.Height - (hScrollBar.Visible ? hScrollBar.Height : 0));
            }

        }

        /// <summary>
        /// ������������� ���������� �������
        /// </summary>
        [Browsable(false)]
        public Rectangle ViewRectangle
        {
            get
            {
                return new Rectangle(Point.Empty, ViewSize);
            }

        }

        /// <summary>
        /// ���������� �������� �����������
        /// </summary>
        [Browsable(false)]
        public new Image Image
        {
            get
            {
                return base.Image;
            }
            set
            {
                base.Image = value;
                this.ZoomScale = 1.0f;
                SetScrollBarVisibilityAndMaxMin();
            }
        }

        /// <summary>
        /// �������������� ������ ���������
        /// </summary>
        [Browsable(false)]
        public HScrollBar HScrollBar
        {
            get
            {
                return hScrollBar;
            }
        }

        /// <summary>
        /// ������������ ������ ���������
        /// </summary>
        [Browsable(false)]
        public VScrollBar VScrollBar
        {
            get
            {
                return vScrollBar;
            }
        }

        /// <summary>
        /// ������������ ��� ���������������
        /// </summary>
        [Bindable(false)]
        [Category("Design")]
        [DefaultValue(InterpolationMode.NearestNeighbor)]
        public InterpolationMode InterpolationMode
        {
            get
            {
                return _interpolationMode;
            }
            set
            {
                _interpolationMode = value;
            }
        }

        #region �������
        /// <summary>
        /// ������� �������� ��������
        /// </summary>
        public event EventHandler ZoomScaleChanged;
        /// <summary>
        /// ������� ��������� ����� ���������� �����������
        /// </summary>
        public event EventHandler ToolChanged;
        /// <summary>
        /// ������� ��������� ����� ���������� �����������
        /// </summary>
        public event EventHandler ToolChanging;
        #endregion

        /// <summary>
        /// �����������
        /// </summary>
        public ScrollablePictureBox()
        {
            InitializeComponent();

            this.ZoomScale = 1.0f;
            SetScrollBarVisibilityAndMaxMin();

            ResizeRedraw = false; //��� �����������
            DoubleBuffered = true; // ������� �����������

            /*������������� �� ������� ������*/
            MouseEnter += OnMouseEnter;
            MouseWheel += OnMouseWheel;
            MouseMove += OnMouseMove;
            Resize += OnResize;
            MouseDown += OnMouseDown;
            MouseUp += OnMouseUp;
        }

        /// <summary>
        /// ���������� ����������� ��������������� ��� �����������
        /// </summary>
        /// <param name="zoomScale">����� ����������� ���������������</param>
        /// <param name="fixPoint">�����, ����� ������� �������������� � ���������� �����������</param>
        public void SetZoomScale(float zoomScale, Point fixPoint)
        {
            if (
            Image != null &&
            _zoomScale != zoomScale //the scale has been changed
            && //and, the scale is not too small
            !(zoomScale < _zoomScale &&
                (Image.Size.Width * zoomScale < 8.0f
                || Image.Size.Height * zoomScale < 8.0f))
            && //and, the scale is not too big
            !(zoomScale > _zoomScale &&
                (ViewSize.Width < zoomScale * 8
                || ViewSize.Height < zoomScale * 8)))
            {
                //constrain the coordinate to be within valide range
                fixPoint.X = Math.Min(fixPoint.X, (int)(Image.Size.Width * _zoomScale));
                fixPoint.Y = Math.Min(fixPoint.Y, (int)(Image.Size.Height * _zoomScale));

                float koeff = (zoomScale - _zoomScale) * _inverseZoomScale / zoomScale;
                int shiftX = (int)(fixPoint.X * koeff);
                int shiftY = (int)(fixPoint.Y * koeff);

                this.ZoomScale = zoomScale;

                int h = (int)(hScrollBar.Value + shiftX);
                int v = (int)(vScrollBar.Value + shiftY);
                SetScrollBarVisibilityAndMaxMin();
                hScrollBar.Value = Math.Min(Math.Max(hScrollBar.Minimum, h), hScrollBar.Maximum); ;
                vScrollBar.Value = Math.Min(Math.Max(vScrollBar.Minimum, v), vScrollBar.Maximum);

                Invalidate();

                if (ZoomScaleChanged != null)
                    ZoomScaleChanged(this, new EventArgs());

            }
        }

        /// <summary>
        /// ���������������� ���������� ��������� �����������
        /// </summary>
        /// <param name="pe">The paint event</param>
        protected override void OnPaint(PaintEventArgs pe)
        {
            if (IsDisposed) return;
            if (base.Image != null)
            {
                if (pe.Graphics.InterpolationMode != _interpolationMode)
                {
                    /*�������� ������������, ���� �����*/
                    pe.Graphics.InterpolationMode = _interpolationMode;
                }
                if (_zoomScale != 1.0f)
                {
                    /*��������� ������� �����������������, ���� �����*/
                    pe.Graphics.ScaleTransform(_zoomScale, _zoomScale, MatrixOrder.Append);
                }

                Point offset = this.ZoomOffset;
                if (offset != Point.Empty)
                {
                    /*��������� ������� ��������, ���� �����*/
                    pe.Graphics.TranslateTransform(offset.X, offset.Y);
                }
                base.OnPaint(pe);

                if (_tool != null)
                {
                    /*��������� ������� �����������*/
                    _tool.OnPaint(pe);
                }
            }
            else
            {
                base.OnPaint(pe);
            }
        }

        #region ����
        /// <summary>
        /// ����������� ���������������
        /// </summary>
        private float _zoomScale;
        /// <summary>
        /// �������� �������� ������������ ��������������� 1/_zoomScale;
        /// ����� ���������� ������ �� _zoomScale, ����� ����� ��������
        /// ����� �������� �� �������� �� ��������
        /// </summary>
        private float _inverseZoomScale;
        /// <summary>
        /// ����������
        /// </summary>
        private Tool _tool;
        /// <summary>
        /// �������������� ������ ���������
        /// </summary>
        private HScrollBar hScrollBar;
        /// <summary>
        /// ������������ ������ ���������
        /// </summary>
        private VScrollBar vScrollBar;
        /// <summary>
        /// ������������ ��� ��������� �����������
        /// </summary>
        private InterpolationMode _interpolationMode = InterpolationMode.NearestNeighbor;
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (_tool == null) return;
            _tool.OnMouseDown(sender, e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            if (_tool == null) return;
            _tool.OnMouseUp(sender, e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private void OnMouseEnter(object sender, EventArgs e)
        {  //������� ���� �������� ���������
            Form f = TopLevelControl as Form;
            if (f != null) f.ActiveControl = this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private void OnResize(object sender, EventArgs e)
        {
            Size viewSize = ViewSize;
            if (base.Image != null && viewSize.Width > 0 && viewSize.Height > 0)
            {
                SetScrollBarVisibilityAndMaxMin();
                Invalidate();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private void OnMouseWheel(object sender, MouseEventArgs e)
        {  
            if (_tool == null) return;
            _tool.OnMouseWheel(sender, e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private void OnToolChanged(object sender, EventArgs e)
        {
            if (_tool != null)
            {
                /*��������� ����������*/
                _tool.OnToolChanged(sender, e);
            }
            if (ToolChanged != null)
            {
                /*��������� �������� �����������*/
                ToolChanged(sender, e);
                return;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private void OnToolChanging(object sender, EventArgs e)
        {
            if (_tool != null)
            {
                _tool.OnToolChanging(sender, e);
            }
            if (ToolChanging != null)
            {
                ToolChanging(sender, e);
                return;
            }
        }

        /// <summary>
        /// ���������� ��������� ������ ���� ��� ���������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private void OnScroll(object sender, ScrollEventArgs e)
        {
            Invalidate();
        }

        /// <summary>
        /// ���������� ��������� ����� ���������
        /// � �� �������� ���������
        /// </summary>
        /// <returns></returns>
        private void SetScrollBarVisibilityAndMaxMin()
        {
            #region ����������, �������� �� �������� ������ ���������
            hScrollBar.Visible = false;
            vScrollBar.Visible = false;

            if (Image == null) return;

            // If the image is wider than the PictureBox, show the HScrollBar.
            Size imgSize = ImageSize;
            hScrollBar.Visible = imgSize.Width > ClientSize.Width;

            // If the image is taller than the PictureBox, show the VScrollBar.
            vScrollBar.Visible = imgSize.Height > ClientSize.Height;

            #endregion
            Size vwSize = ViewSize;
            // Set the Maximum, LargeChange and SmallChange properties.
            if (hScrollBar.Visible)
            {  // If the offset does not make the Maximum less than zero, set its value.            
            hScrollBar.Maximum =
                Image.Size.Width - (int)(Math.Max(0, vwSize.Width) * _inverseZoomScale);
            }
            else
            {
            hScrollBar.Maximum = 0;
            }
            hScrollBar.LargeChange = 2/*(int)Math.Max(hScrollBar.Maximum / 10, 1)*/;
            hScrollBar.SmallChange = 1/*(int)Math.Max(hScrollBar.Maximum / 20, 1)*/;

            if (vScrollBar.Visible)
            {  // If the offset does not make the Maximum less than zero, set its value.            
            vScrollBar.Maximum =
                Image.Size.Height - (int)(Math.Max(0, vwSize.Height) * _inverseZoomScale);
            }
            else
            {
            vScrollBar.Maximum = 0;
            }
            vScrollBar.LargeChange = 2/*(int)Math.Max(vScrollBar.Maximum / 10, 1)*/;
            vScrollBar.SmallChange = 1/*(int)Math.Max(vScrollBar.Maximum / 20, 1)*/;
        }

        /// <summary>
        /// ��������� ��������� ���� ��� ���������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_tool == null) return;
            _tool.OnMouseMove(sender, e);
            Invalidate();
        }

        private void InitializeComponent()
        {
            this.hScrollBar = new System.Windows.Forms.HScrollBar();
            this.vScrollBar = new System.Windows.Forms.VScrollBar();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            this.SuspendLayout();
            // 
            // hScrollBar
            // 
            this.hScrollBar.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.hScrollBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.hScrollBar.Location = new System.Drawing.Point(0, 33);
            this.hScrollBar.Name = "hScrollBar";
            this.hScrollBar.Size = new System.Drawing.Size(83, 17);
            this.hScrollBar.TabIndex = 2;
            this.hScrollBar.Scroll += new System.Windows.Forms.ScrollEventHandler(this.OnScroll);
            // 
            // vScrollBar
            // 
            this.vScrollBar.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.vScrollBar.Dock = System.Windows.Forms.DockStyle.Right;
            this.vScrollBar.Location = new System.Drawing.Point(83, 0);
            this.vScrollBar.Name = "vScrollBar";
            this.vScrollBar.Size = new System.Drawing.Size(17, 50);
            this.vScrollBar.TabIndex = 1;
            this.vScrollBar.Scroll += new System.Windows.Forms.ScrollEventHandler(this.OnScroll);
            // 
            // ScrollablePictureBox
            // 
            this.Controls.Add(this.hScrollBar);
            this.Controls.Add(this.vScrollBar);
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
            this.ResumeLayout(false);

        }
    }
}

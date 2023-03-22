
using System;
namespace UWB_Client
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using UWB_Client.Models;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Windows.Input;
    using System.Windows.Media.Imaging;
    using System.IO;
    using SharpSoft.Geometries;
    using SharpSoft.Geometries.Shapes;

    /// <summary>
    /// 场景渲染控件
    /// </summary>
    public class SceneRenderControl : Control
    {
        static SceneRenderControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SceneRenderControl), new FrameworkPropertyMetadata(typeof(SceneRenderControl)));
        }
        public SceneRenderControl()
        {
            InputMethod.SetIsInputMethodEnabled(this, false);
            this.FontWeight = FontWeights.Bold;
            this.ClipToBounds = true;
            this.Background = Brushes.Black;
            this.Loaded += SceneRenderControl_Loaded;
        }


        private void SceneRenderControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Reset();
        }


        public void Reset()
        {
            if (Scene == null)
            {
                return;
            }
            this.transform = new System.Windows.Media.Matrix();
            // var v = inv_transform.Transform(new Vector(Scene.X_Length * 1000 / 2, Scene.Y_Length * 1000 / 2));
            this.transform.Translate(this.ActualWidth / 2, this.ActualHeight / 2);

            var rx = this.ActualWidth / (Scene.X_Length * 1000);
            var ry = this.ActualHeight / (Scene.Y_Length * 1000);

            var ratio = Math.Min(rx, ry);
            this.transform.ScaleAt(ratio, ratio, this.ActualWidth / 2, this.ActualHeight / 2);
            this.InvalidateVisual();
        }
        /// <summary>
        /// 视图自动缩放到适应控件大小
        /// </summary>
        public void Adaptive()
        {
            if (Scene == null)
            {
                return;
            }
            this.transform = new System.Windows.Media.Matrix();
            var rx = this.ActualWidth / (Scene.X_Length * 1000);
            var ry = this.ActualHeight / (Scene.Y_Length * 1000);

            var ratio = Math.Min(rx, ry);
            var nW = Scene.X_Length * 1000 * ratio;
            var nH = Scene.Y_Length * 1000 * ratio;

            var xo = (this.ActualWidth - nW) / 2d;
            var yo = (this.ActualHeight - nH) / 2d;

            this.transform.Scale(ratio, ratio);
            this.transform.Translate(xo, yo);
            this.InvalidateVisual();
        }


        public ImageSource MapImage
        {
            get { return (ImageSource)GetValue(MapImageProperty); }
            set { SetValue(MapImageProperty, value); }
        }

        public static readonly DependencyProperty MapImageProperty =
            DependencyProperty.Register("MapImage", typeof(ImageSource), typeof(SceneRenderControl), new PropertyMetadata(null, (obj, e) =>
            {
                var nv = e.NewValue;
                if (obj is SceneRenderControl ctl)
                {
                    if (nv is ImageSource img)
                    {
                        //ctl.reSizeImage();
                    }
                    if (ctl.IsInitialized)
                    {
                        ctl.InvalidateVisual();
                    }
                }
            }));


        #region 控制
        private const double scaleDelta1 = 0.15;//较大的缩放量
        private const double scaleDelta2 = 0.03;//较小的缩放量
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            if (Scene == null)
            {
                return;
            }
            var ys = scaleDelta1;//缩放更改量
            if (Keyboard.GetKeyStates(Key.LeftCtrl & Key.RightCtrl) == KeyStates.Down)
            {
                ys = scaleDelta2;//按住Ctrl键缩放较慢
            }
            var s = 1 + (e.Delta / 120) * ys;
            var cp = e.GetPosition(this);
            var v = transform.Transform(new Vector(this.Scene.X_Length * 1000, this.Scene.Y_Length * 1000));
            if (s < 1)
            {//缩小
                if (v.X < this.ActualWidth / 10)
                {//禁止再缩小
                    return;
                }
            }
            else
            {//放大
                if (v.X > this.ActualWidth * 10)
                {//禁止再放大
                    return;
                }
            }

            transform.ScaleAt(s, s, cp.X, cp.Y);
            this.InvalidateVisual();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            var p = e.GetPosition(this);
            mouse_x = p.X;
            mouse_y = p.Y;
            if (ShowMousePosition)
            {
                this.InvalidateVisual();
            }
            if (iscreatingnewregion)
            {
                this.InvalidateVisual();
                return;
            }
            if (Scene == null)
            {
                return;
            }
            var inv = inv_transform;
            if (transtool != null)
            {
                var hr = transtool.HitTest(inv.Transform(p).GetPoint2D());
                switch (hr.Target)
                {
                    case TransformTool.HitTestTarget.Center:
                    case TransformTool.HitTestTarget.InnerSpace:
                        this.Cursor = Cursors.SizeAll;
                        break;
                    case TransformTool.HitTestTarget.P1:
                    case TransformTool.HitTestTarget.P3:
                        this.Cursor = Cursors.SizeNWSE;
                        break;
                    case TransformTool.HitTestTarget.P2:
                    case TransformTool.HitTestTarget.P4:
                        this.Cursor = Cursors.SizeNESW;
                        break;
                    case TransformTool.HitTestTarget.P1_P2:
                    case TransformTool.HitTestTarget.P3_P4:
                        this.Cursor = Cursors.SizeNS;
                        break;
                    case TransformTool.HitTestTarget.P2_P3:
                    case TransformTool.HitTestTarget.P4_P1:
                        this.Cursor = Cursors.SizeWE;
                        break;
                    case TransformTool.HitTestTarget.Outskirts:
                    case TransformTool.HitTestTarget.Empty:
                    default:
                        this.Cursor = Cursors.Arrow;
                        break;
                }
            }
            if (e.LeftButton == MouseButtonState.Pressed)
            {

                if (transtool != null)
                {
                    transtool.DoAction(inv.Transform(p).GetPoint2D());

                }

                var ox = p.X - mouse_last_x;
                var oy = p.Y - mouse_last_y;
                switch (md_tartget)
                {
                    case MouseDownTarget.Map://选中地图及地图外部范围，平移视图。
                    case MouseDownTarget.MapOuter:
                        transform.Translate(ox, oy);
                        break;
                    case MouseDownTarget.A0://移动基站0的位置
                        var op = inv.Transform(new Point(mouse_last_x, mouse_last_y));
                        var np = inv.Transform(p);
                        var ox1 = np.X - op.X;
                        var oy1 = np.Y - op.Y;
                        if (this.Scene != null)
                        {
                            this.Scene.A0_Position = this.Scene.A0_Position.Translate(ox1, oy1);
                        }
                        break;
                    case MouseDownTarget.A1://调整基站的方位角
                    case MouseDownTarget.A2:
                        var op1 = inv.Transform(new Point(mouse_last_x, mouse_last_y));
                        StraightLine2D l1 = new StraightLine2D(this.Station.PointA, new Point2D(op1.X, op1.Y));
                        var np1 = inv.Transform(p);
                        StraightLine2D l2 = new StraightLine2D(this.Station.PointA, new Point2D(np1.X, np1.Y));
                        var r = l1.GetAngleTo(l2);
                        if (this.Scene != null)
                        {
                            this.Scene.Angle_A0_A1 = this.Scene.Angle_A0_A1 + (double)r;
                        }
                        break;
                    case MouseDownTarget.Tag:
                        break;
                    default:
                        break;
                }
                mouse_last_x = p.X;
                mouse_last_y = p.Y;
                this.InvalidateVisual();
            }
        }
        /// <summary>
        /// 鼠标按下时点中的目标
        /// </summary>

        public enum MouseDownTarget
        {
            None = 0,
            /// <summary>
            /// 地图之外
            /// </summary>

            MapOuter,
            /// <summary>
            /// 基站0
            /// </summary>
            A0,
            /// <summary>
            /// 基站1
            /// </summary>
            A1,
            /// <summary>
            /// 基站2
            /// </summary>
            A2,
            /// <summary>
            /// 标签
            /// </summary>
            Tag,
            /// <summary>
            /// 地图
            /// </summary>
            Map,
        }

        private MouseDownTarget md_tartget;
        private double mouse_x = 0, mouse_y = 0;
        private double mouse_last_x = 0, mouse_last_y = 0;
        private double mouse_down_x = 0, mouse_down_y = 0;
        private bool willnewregion = false;//将要创建新选区
        private bool iscreatingnewregion = false;

        private TransformTool transtool;
        private RectSel editrectsel;
        public void EditSel(RectSel sel)
        {
            editrectsel = sel;
            transtool = new TransformTool(new Rect2D(sel.X, sel.Y, sel.Width, sel.Height)) { AllowRotation = false };
            this.InvalidateVisual();
        }
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            md_tartget = MouseDownTarget.None;
            var p = e.GetPosition(this);
            mouse_down_x = mouse_last_x = p.X;
            mouse_down_y = mouse_last_y = p.Y;

            if (e.LeftButton == MouseButtonState.Pressed)
            {

                if (willnewregion)
                {
                    willnewregion = false;
                    iscreatingnewregion = true;
                }


                if (Scene == null)
                {
                    return;
                }

                var np = inv_transform.Transform(p).GetPoint2D();

                if (transtool != null)
                {
                    var hr = transtool.HitTest(np);
                    if (hr.Target != TransformTool.HitTestTarget.Empty)
                    {
                        transtool.StartAction(np);
                        return;//不再进行其他击中测试
                    }
                }

                hitTest(np);
                this.InvalidateVisual();
            }

        }
        /// <summary>
        /// 开始创建新区域
        /// </summary>
        public void StartCreateNewRegion()
        {
            willnewregion = true;
            this.Cursor = Cursors.Cross;
        }

        /// <summary>
        /// 创建了新选区
        /// </summary>
        public Action<Point, Point> NewRegionCreated;

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            var p = e.GetPosition(this);
            if (e.ChangedButton == MouseButton.Left)
            {
                if (iscreatingnewregion)
                {//完成新建选区
                    iscreatingnewregion = false;
                    var inv = inv_transform;
                    var p1 = inv.Transform(new Point(mouse_down_x, mouse_down_y));
                    var p2 = inv.Transform(p);
                    NewRegionCreated?.Invoke(p1, p2);
                    this.InvalidateVisual();
                    this.Cursor = Cursors.Arrow;
                }
                if (transtool != null)
                {
                    var rp = new Point2D(editrectsel.X, editrectsel.Y);
                    var rv = new Vector2D(editrectsel.Width, editrectsel.Height);
                    var new_rp = transtool.Transform.Transform(rp);
                    var new_rv = transtool.Transform.Transform(rv);

                    Rect2D rec = new Rect2D(new_rp, new_rv);

                    editrectsel.X = rec.X;
                    editrectsel.Y = rec.Y;
                    editrectsel.Width = rec.Width;
                    editrectsel.Height = rec.Height;

                    transtool.EndAction(p.GetPoint2D());
                    editrectsel = null;
                    transtool = null;
                    this.Cursor = Cursors.Arrow;

                }
            }


            md_tartget = MouseDownTarget.None;
            this.InvalidateVisual();
        }

        /// <summary>
        /// 基站击中测试
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private bool hitTest(Point2D p)
        {
            if (new SharpSoft.Geometries.Line2D(Station.PointA, p).Length <= StationRadius)
            {
                md_tartget = MouseDownTarget.A0;
                return true;
            }
            if (new SharpSoft.Geometries.Line2D(Station.PointB, p).Length <= StationRadius)
            {
                md_tartget = MouseDownTarget.A1;
                return true;
            }
            if (new SharpSoft.Geometries.Line2D(Station.PointC, p).Length <= StationRadius)
            {
                md_tartget = MouseDownTarget.A2;
                return true;
            }
            if (new SharpSoft.Geometries.Line2D(TagPosition, p).Length <= TagRadius)
            {
                md_tartget = MouseDownTarget.Tag;
                return true;
            }
            if (p.X >= 0 && p.Y >= 0 && p.X <= Scene.X_Length * 1000 && p.Y < Scene.Y_Length * 1000)
            {//地图内
                md_tartget = MouseDownTarget.Map;
                return true;
            }
            md_tartget = MouseDownTarget.MapOuter;

            return false;
        }



        #endregion

        #region 模型
        /// <summary>
        /// 基站的三角形信息
        /// </summary>  
        public Triangle2D Station
        {
            get { return (Triangle2D)GetValue(StationProperty); }
            set { SetValue(StationProperty, value); }
        }

        public static readonly DependencyProperty StationProperty =
            DependencyProperty.Register("Station", typeof(Triangle2D), typeof(SceneRenderControl), new PropertyMetadata(default(Triangle2D)));


        /// <summary>
        /// 标签的位置
        /// </summary> 
        public Point2D TagPosition
        {
            get { return (Point2D)GetValue(TagPositionProperty); }
            set { SetValue(TagPositionProperty, value); }
        }

        public static readonly DependencyProperty TagPositionProperty =
            DependencyProperty.Register("TagPosition", typeof(Point2D), typeof(SceneRenderControl), new PropertyMetadata(default(Point2D)));


        public SceneModel Scene
        {
            get { return (SceneModel)GetValue(SceneProperty); }
            set { SetValue(SceneProperty, value); }
        }

        public static readonly DependencyProperty SceneProperty =
            DependencyProperty.Register("Scene", typeof(SceneModel), typeof(SceneRenderControl), new PropertyMetadata(default(SceneModel), (obj, e) =>
            {
                var nv = e.NewValue;
                if (nv is SceneModel sm)
                {
                    if (obj is SceneRenderControl ctl)
                    {
                        sm.PropertyChanged += (s, pe) =>
                        {
                            if (pe.PropertyName == SceneModel.MapImageFileProperty.Name)
                            {
                                ctl.MapImage = string.IsNullOrWhiteSpace(sm.MapImageFile) ? null : new BitmapImage(new Uri(Path.Combine(ClientHelper.MapsFolder, sm.MapImageFile)));
                            }
                            //ctl.reSizeImage();
                            if (ctl.IsInitialized)
                            {
                                ctl.InvalidateVisual();
                            }
                        };
                        //ctl.reSizeImage();
                        if (ctl.IsInitialized)
                        {
                            ctl.InvalidateVisual();
                        }
                    }
                }
            }));

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == SceneProperty)
            {
                if (this.Scene != null && !string.IsNullOrWhiteSpace(this.Scene.MapImageFile))
                {
                    var file = Path.Combine(ClientHelper.MapsFolder, this.Scene.MapImageFile);
                    this.MapImage = new BitmapImage(new Uri(file));
                }
                else
                {
                    this.MapImage = null;
                }
                this.Adaptive();

            }
            else if (e.Property == StationProperty || e.Property == TagPositionProperty)
            {
                this.InvalidateVisual();
            }
        }



        /// <summary>
        /// 区域列表
        /// </summary>
        public RegionList Regions
        {
            get { return (RegionList)GetValue(RegionsProperty); }
            set { SetValue(RegionsProperty, value); }
        }

        public static readonly DependencyProperty RegionsProperty =
            DependencyProperty.Register("Regions", typeof(RegionList), typeof(SceneRenderControl), new PropertyMetadata(new RegionList(), (o, e) =>
            {
                var nv = e.NewValue;
                if (nv is RegionList rl)
                {
                    rl.CollectionChanged += (oo, ee) =>
                    {
                        foreach (RegionModel item in ee.NewItems)
                        {
                            item.PropertyChanged += (OOO, EEE) =>
                            {
                                ((SceneRenderControl)o).InvalidateVisual();
                            };
                        }
                        ((SceneRenderControl)o).InvalidateVisual();
                    };
                }
            }));




        #endregion

        #region 外观

        public new double FontSize
        {
            get
            {
                var fs = StationRadius * 1.25;
                if (fs > 35791)
                {
                    return 35791;
                }
                if (fs <= 0)
                {
                    fs = 12;
                }
                return fs;
            }
        }

        /// <summary>
        /// 渲染基站的背景
        /// </summary>
        public Brush StationBackground { get; set; } = Brushes.Red;
        //public Brush StationForeground { get; set; } = Brushes.White;
        /// <summary>
        /// 渲染标签的背景
        /// </summary>
        public Brush TagBackground { get; set; } = Brushes.Blue;
        //public Brush TagForeground { get; set; } = Brushes.White;
        /// <summary>
        /// 基站半径
        /// </summary>
        public double StationRadius
        {
            get
            {
                if (Scene == null)
                {
                    return 0;
                }

                return Math.Max(Scene.X_Length, Scene.Y_Length) * 10;
            }
        }
        /// <summary>
        /// 标签半径
        /// </summary>
        public double TagRadius
        {
            get
            {
                if (Scene == null)
                {
                    return 0;
                }
                return Math.Max(Scene.X_Length, Scene.Y_Length) * 10;
            }
        }
        #endregion

        #region 渲染
        #region 渲染过程变量
        private System.Windows.Media.Matrix transform;//变换矩阵
        /// <summary>
        /// 获取当前变换矩阵的逆矩阵
        /// </summary>
        private System.Windows.Media.Matrix inv_transform
        {
            get
            {
                if (transform.HasInverse)
                {//获取逆矩阵
                    var inv = transform;
                    inv.Invert();
                    return inv;
                }
                throw new Exception("当前矩阵不可逆转。");
            }
        }

        /// <summary>
        /// 是否现实鼠标的位置
        /// </summary>
        public bool ShowMousePosition { get; set; } = false;
        #endregion


        protected override void OnRender(DrawingContext dc)
        {
            dc.DrawRectangle(this.Background, null, new Rect(this.RenderSize));

            DrawingVisual dv = new DrawingVisual();
            var dc1 = dv.RenderOpen();
            dc1.PushTransform(new MatrixTransform(transform));
            if (Scene != null)
            {
                RenderMapImage(dc1);

                RenderCoordinate(dc1);

                RenderStations(dc1);

                RenderTag(dc1);

                RenderRegions(dc1);

                RenderNewRegion(dc1);

                RenderTransformTool(dc1);
            }
            dc1.Close();

            dc.DrawDrawing(dv.Drawing);

            RenderMousePosition(dc);

        }

        private void RenderTransformTool(DrawingContext dc)
        {
            if (transtool != null)
            {
                var v = inv_transform.Transform(new Vector(2, 12));

                var hw = v.Y;
                transtool.HandleWidth = hw;

                Pen pen = new Pen(Brushes.Blue, v.X) { DashStyle = DashStyles.Dash };
                var p1 = transtool.Transform.Transform(transtool.P1);
                var p2 = transtool.Transform.Transform(transtool.P2);
                var p3 = transtool.Transform.Transform(transtool.P3);
                var p4 = transtool.Transform.Transform(transtool.P4);

                dc.DrawRectangle(null, pen,
                    new Rect(p1.X - v.Y / 2, p1.Y - v.Y / 2, v.Y, v.Y));
                dc.DrawRectangle(null, pen,
                    new Rect(p2.X - v.Y / 2, p2.Y - v.Y / 2, v.Y, v.Y));
                dc.DrawRectangle(null, pen,
                    new Rect(p3.X - v.Y / 2, p3.Y - v.Y / 2, v.Y, v.Y));
                dc.DrawRectangle(null, pen,
                    new Rect(p4.X - v.Y / 2, p4.Y - v.Y / 2, v.Y, v.Y));

                dc.DrawLine(pen, p1.GetPoint(), p2.GetPoint());
                dc.DrawLine(pen, p2.GetPoint(), p3.GetPoint());
                dc.DrawLine(pen, p3.GetPoint(), p4.GetPoint());
                dc.DrawLine(pen, p4.GetPoint(), p1.GetPoint());

                dc.DrawEllipse(null, new Pen(Brushes.Blue, 1), transtool.GetCenter().GetPoint(),
                    v.Y / 2, v.Y / 2);
            }
        }

        protected void RenderMousePosition(DrawingContext dc)
        {
            if (ShowMousePosition)
            {
                var inv = inv_transform;
                var p1 = transform.Transform(new Point(0, 0));
                var mp = new Point(mouse_x, mouse_y);

                Pen pen1 = new Pen(Brushes.White, 2) { DashStyle = DashStyles.Solid };
                Pen pen2 = new Pen(Brushes.Black, 2) { DashStyle = DashStyles.Dot, DashCap = PenLineCap.Triangle };

                dc.DrawLine(pen1, mp, new Point(mp.X, p1.Y));
                dc.DrawLine(pen1, mp, new Point(p1.X, mp.Y));

                dc.DrawLine(pen2, mp, new Point(mp.X, p1.Y));
                dc.DrawLine(pen2, mp, new Point(p1.X, mp.Y));

            }
        }

        //渲染正在创建的新选区
        protected void RenderNewRegion(DrawingContext dc)
        {
            if (iscreatingnewregion)
            {
                var inv = inv_transform;
                var p1 = inv.Transform(new Point(mouse_down_x, mouse_down_y));
                var p2 = inv.Transform(new Point(mouse_x, mouse_y));
                var v = inv.Transform(new Vector(2, 2));
                dc.DrawRectangle(null, new Pen(Brushes.LightGray, v.X), new Rect(p1, p2));
                dc.DrawRectangle(null, new Pen(Brushes.DarkGreen, v.X) { DashStyle = new DashStyle(new double[] { 5, 5, 5 }, 5) }, new Rect(p1, p2));
            }
        }

        protected virtual void RenderRegions(DrawingContext dc)
        {
            var inv = inv_transform;
            var v = inv.Transform(new Vector(1, 2));
            foreach (var region in Regions)
            {
                foreach (var sel in region.Sels)
                {
                    Brush brush = region.IsTrigger ? new SolidColorBrush(Color.FromArgb(50, 0, 0, 255)) : null;
                    if (sel != editrectsel)
                    {
                        if (sel.IsTrigger)
                        {
                            brush = new SolidColorBrush(Color.FromArgb(50, 255, 0, 0));
                        }
                        Pen pen = region.IsFocused ?
                            new Pen(Brushes.Red, v.Y) { DashStyle = (sel.IsSelected ? DashStyles.Dash : null) }
                            : new Pen(Brushes.Blue, v.Y) { DashStyle = (sel.IsSelected ? DashStyles.Dash : null) };

                        dc.DrawRectangle(brush, pen, new Rect(sel.X, sel.Y, sel.Width, sel.Height));
                    }

                }
            }
        }

        /// <summary>
        /// 渲染坐标系
        /// </summary>
        /// <param name="dc"></param>
        protected void RenderCoordinate(DrawingContext dc)
        {
            var o = transform.Transform(new Point());//原点在控件坐标系中的坐标
            var inv = inv_transform;
            var v = inv.Transform(new Vector(1, 10));
            Pen pen = new Pen(Brushes.Yellow, v.X);
            var px1 = inv.Transform(new Point(0, o.Y));
            var px2 = inv.Transform(new Point(this.ActualWidth, o.Y));

            var py1 = inv.Transform(new Point(o.X, 0));
            var py2 = inv.Transform(new Point(o.X, this.ActualHeight));
            dc.DrawLine(pen, px1, px2);
            dc.DrawLine(pen, py1, py2);

            var wh = transform.Transform(new Vector(Scene.X_Length * 1000, Scene.Y_Length * 1000));
            var rx = wh.X / (Scene.X_Length * 1000);//X比例
            var ry = wh.Y / (Scene.Y_Length * 1000);//Y比例

            var visual_width = this.ActualWidth / rx;//计算当前可视范围的现实宽度
            var visual_height = this.ActualHeight / ry;//计算当前可视范围的现实宽度

            var unit_x = Math.Pow(10, getDigit(visual_width) - 2);
            var unit_y = unit_x;// Math.Pow(10, getDigit(visual_height) - 2);


            if (o.X > 0)
            {//绘制X轴的负数部分
                for (int i = 0; i < Math.Ceiling((o.X / this.ActualWidth * visual_width) / unit_x); i++)
                {
                    var p1 = new Point(-(i + 1) * unit_x, -v.Y);
                    var p2 = new Point(-(i + 1) * unit_x, 0);
                    dc.DrawLine(pen, p1, p2);
                    FormattedText formattedText = new FormattedText($"-{i + 1}", CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight, this.Typeface,
                        Math.Min(v.Y, 35791), Brushes.Yellow);
                    RenderText(dc, formattedText, new Point(-(i + 1) * unit_x, -v.Y * 1.5));
                }
            }
            if (o.X < this.ActualWidth)
            {//绘制X轴的正数部分
                for (int i = 0; i < Math.Ceiling(((this.ActualWidth - o.X) / this.ActualWidth * visual_width) / unit_x); i++)
                {
                    var p1 = new Point((i + 1) * unit_x, -v.Y);
                    var p2 = new Point((i + 1) * unit_x, 0);
                    dc.DrawLine(pen, p1, p2);

                    FormattedText formattedText = new FormattedText($"{i + 1}", CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight, this.Typeface,
                        Math.Min(v.Y, 35791), Brushes.Yellow);
                    RenderText(dc, formattedText, new Point((i + 1) * unit_x, -v.Y * 1.5));
                }
            }


            if (o.Y > 0)
            {//绘制Y轴的负数部分
                for (int i = 0; i < Math.Ceiling((o.Y / this.ActualHeight * visual_height) / unit_y); i++)
                {
                    var p1 = new Point(-v.Y, -(i + 1) * unit_y);
                    var p2 = new Point(0, -(i + 1) * unit_y);
                    dc.DrawLine(pen, p1, p2);
                    FormattedText formattedText = new FormattedText($"-{i + 1}", CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight, this.Typeface,
                        Math.Min(v.Y, 35791), Brushes.Yellow);
                    RenderText(dc, formattedText, new Point(-v.Y * 1.5, -(i + 1) * unit_y));
                }
            }
            if (o.Y < this.ActualHeight)
            {//绘制Y轴的正数部分
                for (int i = 0; i < Math.Ceiling(((this.ActualHeight - o.Y) / this.ActualHeight * visual_height) / unit_y); i++)
                {
                    var p1 = new Point(-v.Y, (i + 1) * unit_y);
                    var p2 = new Point(0, (i + 1) * unit_y);
                    dc.DrawLine(pen, p1, p2);

                    FormattedText formattedText = new FormattedText($"{i + 1}", CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight, this.Typeface,
                        Math.Min(v.Y, 35791), Brushes.Yellow);
                    RenderText(dc, formattedText, new Point(-v.Y * 1.5, (i + 1) * unit_y));
                }
            }
            var dg = getDigit(unit_x);

            FormattedText ft = new FormattedText($"坐标轴单位：{units[dg]}", CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight, this.Typeface,
                Math.Min(v.Y, 35791), Brushes.Yellow);
            dc.DrawText(ft, inv.Transform(new Point(5, 5)));
        }
        private static string[] units = new string[] { "最小单位", "毫米", "厘米", "分米", "米", "十米", "百米", "千米", "万米", "十万米" };
        /// <summary>
        /// 获取十进制数字的整数位数
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        private int getDigit(double d)
        {
            int i = 0;
            int v = (int)d;
            while (v > 0)
            {
                v = v / 10;
                i++;
            }
            return i;
        }

        /// <summary>
        /// 渲染地图
        /// </summary>
        /// <param name="drawingContext"></param>
        protected virtual void RenderMapImage(DrawingContext drawingContext)
        {
            if (this.MapImage != null)
            {
                drawingContext.DrawImage(this.MapImage, new Rect(0, 0, Scene.X_Length * 1000, Scene.Y_Length * 1000));
            }
        }
        /// <summary>
        /// 将现实坐标转换为渲染坐标
        /// </summary>
        /// <param name="actual_point"></param>
        /// <returns></returns>
        private Point convertPoint(Point2D actual_point)
        {//注意场景宽度以米为单位，而uwb计量单位为毫米。
            //double sx = img_width / (Scene.X_Length * 1000);
            //double sy = img_height / (Scene.Y_Length * 1000);
            //var x = actual_point.X * sx;
            //var y = actual_point.Y * sy;
            return new Point(actual_point.X, actual_point.Y);
        }

        protected virtual void RenderStations(DrawingContext drawingContext)
        {
            RenderStation(drawingContext, Station.PointA, 0);
            RenderStation(drawingContext, Station.PointB, 1);
            RenderStation(drawingContext, Station.PointC, 2);
        }
        /// <summary>
        /// 绘制基站
        /// </summary>
        /// <param name="drawingContext"></param>
        /// <param name="point"></param>
        protected virtual void RenderStation(DrawingContext drawingContext, Point2D point, int index)
        {
            if (point.IsNaP())
            {//不绘制无效的点
                return;
            }
            var p = convertPoint(point);
            var pen = new Pen(Brushes.DarkGreen, StationRadius / 15);
            if ((md_tartget == MouseDownTarget.A0 && index == 0)
                || (md_tartget == MouseDownTarget.A1 && index == 1)
                || (md_tartget == MouseDownTarget.A2 && index == 2))
            {
                pen = new Pen(Brushes.Red, StationRadius / 15);
                drawingContext.DrawEllipse(Brushes.Green, pen, p, StationRadius, StationRadius);
            }
            else
            {
                drawingContext.DrawEllipse(StationBackground, pen, p, StationRadius, StationRadius);
            }
            this.RenderText(drawingContext, $"A{index}", p);
        }
        protected virtual void RenderTag(DrawingContext drawingContext)
        {
            if (TagPosition.IsNaP())
            {//不绘制无效的点
                return;
            }
            var p = convertPoint(TagPosition);
            var pen = new Pen(Brushes.DarkGreen, TagRadius / 15);
            if (md_tartget == MouseDownTarget.Tag)
            {
                pen = new Pen(Brushes.DarkBlue, TagRadius / 15);
            }
            drawingContext.DrawEllipse(TagBackground, pen, p, TagRadius, TagRadius);
            this.RenderText(drawingContext, "T", p);
        }
        #region 文字
        protected Typeface Typeface { get => new Typeface(this.FontFamily, this.FontStyle, this.FontWeight, this.FontStretch); }

        private FormattedText getFormattedText(string text)
        {
            return new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, this.Typeface, this.FontSize, this.Foreground);
        }
        /// <summary>
        /// 渲染文本，指定文本居中的坐标
        /// </summary>
        /// <param name="drawingContext"></param>
        /// <param name="formattedText"></param>
        /// <param name="center_point"></param>
        protected virtual void RenderText(DrawingContext drawingContext, FormattedText formattedText, Point center_point)
        {
            double x = center_point.X - formattedText.Width / 2d;
            double y = center_point.Y - formattedText.Height / 2d;
            drawingContext.DrawText(formattedText, new Point(x, y));
        } /// <summary>
          /// 渲染文本，指定文本居中的坐标
          /// </summary> 
        protected virtual void RenderText(DrawingContext drawingContext, string text, Point center_point)
        {
            this.RenderText(drawingContext, getFormattedText(text), center_point);
        }
        #endregion

        #endregion
    }
}

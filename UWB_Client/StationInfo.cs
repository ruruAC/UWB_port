using SharpSoft.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWB_Mini3s_Plus;

namespace UWB_Client
{
    using System.Threading;
    using System.Windows;
    using UWB_Client.Models;

    /// <summary>
    /// 基站信息
    /// </summary>
    public class StationInfo : DependencyObject, IDisposable
    {
        public StationInfo()
        {
            timer = new Timer(new TimerCallback(time));
            timer.Change(1000, 1000);
        }
        Timer timer;
        void time(object state)
        {
            lock (asyncstate)
            {
                macounter++;
                mccounter++;
            }
            List<string> s = new List<string>();
            if (macounter > 3)
            {
                s.Add("基站数据超时");
            }
            if (mccounter > 3)
            {
                s.Add("标签数据超时");
            }
            if (s.Count == 0)
            {
                s.Add("数据正常");
            }
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate { StatusChanged?.Invoke(string.Join(",", s.ToArray())); }));

        }
        /// <summary>
        /// 基站应用到的场景
        /// </summary>
        public SceneModel Scene { get; set; }

        /// <summary>
        /// 基站的三角形信息
        /// </summary>  
        public Triangle2D Station
        {
            get { return (Triangle2D)GetValue(StationProperty); }
            set { SetValue(StationProperty, value); }
        }

        public static readonly DependencyProperty StationProperty =
            DependencyProperty.Register("Station", typeof(Triangle2D), typeof(StationInfo), new PropertyMetadata(default(Triangle2D)));



        /// <summary>
        /// 标签的位置
        /// </summary> 
        public Point2D TagPosition
        {
            get { return (Point2D)GetValue(TagPositionProperty); }
            set { SetValue(TagPositionProperty, value); }
        }

        public static readonly DependencyProperty TagPositionProperty =
            DependencyProperty.Register("TagPosition", typeof(Point2D), typeof(StationInfo), new PropertyMetadata(default(Point2D)));



        /// <summary>
        /// 状态改变
        /// </summary>
        public event Action<string> StatusChanged;

        private int macounter = 0;
        private int mccounter = 0;
        private object asyncstate = new object();
        /// <summary>
        /// 应用基站数据
        /// </summary>
        /// <param name="ma"></param>
        public void PushMA(UWB_Data ma)
        {
            lock (asyncstate)
            {
                macounter = 0;
            }

            if (this.Scene == null)
            {
                return;
            }
            Triangle2D_V t_v = new Triangle2D_V(ma.range1, ma.range2, ma.range3);
            Triangle2D triangle = new Triangle2D(t_v, Scene.A0_Position, (Radian)Scene.Angle_A0_A1, Scene.Flip_A2);

            Station = triangle;


        }
        /// <summary>
        /// 应用标签数据
        /// </summary>
        /// <param name="mc"></param>
        public void PushMC(UWB_Data mc)
        {
            lock (asyncstate)
            {
                mccounter = 0;
            }

            TagPosition = Station.GetPoint(mc.range0, mc.range1, mc.range2);


        }

        public void Dispose()
        {
            timer.Dispose();
            timer = null;
        }
    }
}

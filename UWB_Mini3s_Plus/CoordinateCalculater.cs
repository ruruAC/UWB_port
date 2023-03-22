//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows;

//namespace UWB_Mini3s_Plus
//{
//    /// <summary>
//    /// 坐标计算器
//    /// </summary>
//    public class CoordinateCalculater
//    {
//        /// <summary>
//        /// 基站0的坐标
//        /// </summary>
//        public Point p_a0 { get; set; }
//        /// <summary>
//        /// 基站0-基站1所在直线与场景坐标系中X轴的倾角(以弧度计)
//        /// </summary>
//        public double angle { get; set; }
//        /// <summary>
//        /// 是否翻转第三个基站的方向
//        /// </summary>
//        public bool flip_a2 { get; set; } = false;
//        /// <summary>
//        /// 获取得到的基站坐标
//        /// </summary>
//        public Point[] station_points { get; private set; }
//        /// <summary>
//        /// 获取得到的标签坐标
//        /// </summary>
//        public Point tag_position { get; private set; }

//        public void DataIn(UWB_Data data)
//        {
//            if (data.mid == UWB_mid.ma)
//            {
//                stationData sd = new stationData()
//                {
//                    mainStationId = data.stationid,
//                    a0_a1 = data.range1,
//                    a0_a2 = data.range2,
//                    a1_a2 = data.range3
//                };
//                station_points = CalculateStation(sd);
//            }
//            else if (data.mid == UWB_mid.mc)
//            {
//                tagData td = new tagData(new double[] { data.range0, data.range1, data.range2, data.range3 })
//                {
//                    mainStationId = data.stationid,
//                    tagid = data.tagid
//                };
//                tag_position = CalculateTag(td);
//            }
//        }
//        /// <summary>
//        /// 计算3个基站的坐标
//        /// </summary>
//        /// <param name="data"></param>
//        /// <returns></returns>
//        public Point[] CalculateStation(stationData data)
//        {
//            //****************计算a1坐标*********************
//            var a1_y_offset = data.a0_a1 * Math.Sin(angle);
//            var a1_x_offset = data.a0_a1 * Math.Cos(angle);
//            var p_a1 = new Point(p_a0.X + a1_x_offset, p_a0.Y + a1_y_offset);

//            //****************计算a2坐标 * ********************
//            //计算a0所在夹角的角度（以弧度计）
//            double angle_a0 = Math.Acos(((Math.Pow(data.a0_a1, 2) + Math.Pow(data.a0_a2, 2) - Math.Pow(data.a1_a2, 2)) / (2 * data.a0_a1 * data.a0_a2)));

//            double angle_a0_a2_x = 0d;//这个变量为直线a0-a2与x轴的夹角
//            //flip_a2决定a2在直线a0-a1的哪一侧
//            angle_a0_a2_x = flip_a2 ? angle_a0 - angle : angle_a0 + angle;

//            var a2_y_offset = data.a0_a2 * Math.Sin(angle_a0_a2_x);
//            var a2_x_offset = data.a0_a2 * Math.Cos(angle_a0_a2_x);
//            var p_a2 = new Point(p_a0.X + a2_x_offset, p_a0.Y + a2_y_offset);

//            return new Point[] { p_a0, p_a1, p_a2 };
//        }
//        /// <summary>
//        /// 计算标签坐标。在未计算基站坐标之前无法计算标签坐标。
//        /// </summary>
//        /// <param name="data"></param>
//        /// <returns></returns>
//        public Point CalculateTag(tagData data)
//        {
//            if (station_points == null)
//            {
//                return new Point();
//            }
//            return calc_the4_Position(
//                station_points[0].X, station_points[0].Y, data.lengthes[0],
//                station_points[1].X, station_points[1].Y, data.lengthes[1],
//                station_points[2].X, station_points[2].Y, data.lengthes[2]
//                );
//        }


//        //三边测量法
//        // 通过三点坐标和到三点的距离，返回第4点位置
//        public Point calc_the4_Position(double x1, double y1, double d1,
//                                          double x2, double y2, double d2,
//                                          double x3, double y3, double d3)
//        {
//            double a11 = 2 * (x1 - x3);
//            double a12 = 2 * (y1 - y3);
//            double b1 = Math.Pow(x1, 2) - Math.Pow(x3, 2)
//                    + Math.Pow(y1, 2) - Math.Pow(y3, 2)
//                    + Math.Pow(d3, 2) - Math.Pow(d1, 2);
//            double a21 = 2 * (x2 - x3);
//            double a22 = 2 * (y2 - y3);
//            double b2 = Math.Pow(x2, 2) - Math.Pow(x3, 2)
//                    + Math.Pow(y2, 2) - Math.Pow(y3, 2)
//                    + Math.Pow(d3, 2) - Math.Pow(d2, 2);

//            var x = (b1 * a22 - a12 * b2) / (a11 * a22 - a12 * a21);
//            var y = (a11 * b2 - b1 * a21) / (a11 * a22 - a12 * a21);

//            return new Point(x, y);

//        }
//    }
//    /// <summary>
//    /// 基站数据
//    /// </summary>
//    public class stationData
//    {
//        public int mainStationId { get; set; }
//        public double a0_a1 { get; set; }
//        public double a0_a2 { get; set; }
//        public double a1_a2 { get; set; }
//    }
//    /// <summary>
//    /// 标签数据
//    /// </summary>
//    public class tagData
//    {
//        public tagData(double[] ls)
//        {
//            lengthes = ls;
//        }
//        public int mainStationId { get; set; }
//        public int tagid { get; set; }
//        public double[] lengthes { get; set; }
//    }
//}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWB_Client
{
    using SharpSoft.Geometries;
    using System.Windows;
    public static class Extensions
    {
        public static Point GetPoint(this Point2D p)
        {
            return new Point(p.X, p.Y);
        }
        public static Point2D GetPoint2D(this Point p)
        {
            return new Point2D(p.X, p.Y);
        }
    }
}

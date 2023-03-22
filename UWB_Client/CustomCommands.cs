using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWB_Client
{
    using System.Windows;
    using System.Windows.Input;
    public class CustomCommands
    {
        static CustomCommands()
        {

        }
        public static readonly RoutedUICommand CloseWindow =
               new RoutedUICommand("关闭窗体", "CloseWindow", typeof(Window));
        public static readonly RoutedUICommand MaxWindow =
               new RoutedUICommand("最大化窗体", "MaxWindow", typeof(Window));
    }
}

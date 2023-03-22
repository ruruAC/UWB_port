using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UWB_Client
{
    using System.Collections.ObjectModel;
    using UWB_Mini3s_Plus;

    /// <summary>
    /// UWB_DataViewControl.xaml 的交互逻辑
    /// </summary>
    public partial class UWB_DataViewControl : UserControl
    {
        public UWB_DataViewControl()
        { 
            InitializeComponent();
        }





        public UWB_Data MA
        {
            get { return (UWB_Data)GetValue(MAProperty); }
            set { SetValue(MAProperty, value); }
        }
         
        public static readonly DependencyProperty MAProperty =
            DependencyProperty.Register("MA", typeof(UWB_Data), typeof(UWB_DataViewControl), new PropertyMetadata(default(UWB_Data)));



        public UWB_Data MC
        {
            get { return (UWB_Data)GetValue(MCProperty); }
            set { SetValue(MCProperty, value); }
        }
         
        public static readonly DependencyProperty MCProperty =
            DependencyProperty.Register("MC", typeof(UWB_Data), typeof(UWB_DataViewControl), new PropertyMetadata(default(UWB_Data)));


    }
}

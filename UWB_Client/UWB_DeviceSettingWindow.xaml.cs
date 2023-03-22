using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;

namespace UWB_Client
{
    /// <summary>
    /// UWB_DeviceSettingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class UWB_DeviceSettingWindow : Window
    {
        public UWB_DeviceSettingWindow()
        {
            InitializeComponent();
            var names = System.IO.Ports.SerialPort.GetPortNames();
            cbox_comlist1.ItemsSource = names;
            cbox_comlist2.ItemsSource = names;
            this.Loaded += UWB_DeviceSettingWindow_Loaded;
        }



        public DeviceSetting Setting
        {
            get { return (DeviceSetting)GetValue(SettingProperty); }
            set { SetValue(SettingProperty, value); }
        }

        public static readonly DependencyProperty SettingProperty =
            DependencyProperty.Register("Setting", typeof(DeviceSetting), typeof(UWB_DeviceSettingWindow), new PropertyMetadata(null));



        private void UWB_DeviceSettingWindow_Loaded(object sender, RoutedEventArgs e)
        {

            Setting = DeviceSetting.Load();
             
        }



        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void cbox_comlist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Setting != null)
            {
                Setting.Save();
            }
        }
    }

    public class DeviceSetting
    {
        static DeviceSetting()
        {
            filename = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "device.json");
        }
        private static readonly string filename;
        /// <summary>
        /// UWB设备串口号
        /// </summary>
        public string UWBComPort { get; set; }
        /// <summary>
        /// 控制设备的串口号
        /// </summary>
        public string ControlComPort { get; set; }

        public void Save()
        {
            var json = JsonConvert.SerializeObject(this);
            using (var fs = new FileStream(filename, FileMode.Create))
            {
                using (var sw = new StreamWriter(fs))
                {
                    sw.Write(json);
                }
            }
        }

        public static DeviceSetting Load()
        {
            if (!File.Exists(filename))
            {
                return new DeviceSetting();
            }
            using (var fs = new FileStream(filename, FileMode.Open))
            {
                using (var sr = new StreamReader(fs))
                {
                    var json = sr.ReadToEnd();
                    return JsonConvert.DeserializeObject<DeviceSetting>(json);
                }
            }
        }
    }
}

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
    using Newtonsoft.Json;
    using SharpSoft.Geometries;
    using System.ComponentModel;
    using System.IO;
    using System.IO.Ports;
    using UWB_Client.Models;
    using UWB_Mini3s_Plus;
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            this.Closed += MainWindow_Closed;

            btn_adaptive_view.Click += (ss, e) => { src.Adaptive(); };
            btn_reset_view.Click += (ss, e) => { src.Reset(); };
            this.StationInfo = new StationInfo();
            this.StationInfo.StatusChanged += StationInfo_StatusChanged;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            var result = MessageBox.Show("关闭后会丢失未保存的数据，请确定您已保存所需要的数据！\n确认要关闭吗？", "关闭确认", MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
            }
            base.OnClosing(e);
        }

        private void StationInfo_StatusChanged(string obj)
        {
            txt_connect_status.Text = obj;
        }


        /// <summary>
        /// 基站信息
        /// </summary>
        public StationInfo StationInfo
        {
            get { return (StationInfo)GetValue(StationInfoProperty); }
            set { SetValue(StationInfoProperty, value); }
        }

        public static readonly DependencyProperty StationInfoProperty =
            DependencyProperty.Register("StationInfo", typeof(StationInfo), typeof(MainWindow), new PropertyMetadata(null));


        /// <summary>
        /// 是否暂停更新基站数据
        /// </summary>
        public bool PauseStationData
        {
            get { return (bool)GetValue(PauseStationDataProperty); }
            set { SetValue(PauseStationDataProperty, value); }
        }

        public static readonly DependencyProperty PauseStationDataProperty =
            DependencyProperty.Register("PauseStationData", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));



        /// <summary>
        /// 基站接口
        /// </summary>
        StationPort staPort;

        SerialPort sendserial;
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            rlc.Regions = RegionList.Load();


            rlc.AddNewSel += Rlc_AddNewSel;
            rlc.EditSel += Rlc_EditSel;
            rlc.NeedRefresh += delegate
            {
                src.InvalidateVisual();
            };

            reloadDeviceSetting();

            var files = Directory.GetFiles(ClientHelper.ScenesFolder, "*.json");

            if (files.Length > 0)
            {
                var file = files[0];
                using (var sr = File.OpenText(file))
                {
                    var json = sr.ReadToEnd();
                    try
                    {
                        var scene = JsonConvert.DeserializeObject<SceneModel>(json);
                        scene.FileName = Path.GetFileName(file);
                        this.Scene = scene;
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("加载默认场景失败。");
                    }
                }
            }
        }

        private void reloadDeviceSetting()
        {
            if (staPort != null)
            {
                staPort.Dispose();
                staPort = null;
            }
            this.DeviceSetting = DeviceSetting.Load();
            if (string.IsNullOrWhiteSpace(this.DeviceSetting.UWBComPort))
            {
                MessageBox.Show("未配置定位设备串口。请到主菜单【硬件】->【设备设置】进行配置。");
                return;
            }

            staPort = new StationPort(this.DeviceSetting.UWBComPort);
            staPort.DataReceived += Station_DataReceived;
            try
            {
                staPort.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show("连接定位设备失败。");
                setDevice();
                return;
            }



            if (sendserial != null)
            {
                sendserial.Dispose();
                sendserial = null;
            }
            sendserial = new SerialPort(this.DeviceSetting.ControlComPort);
            try
            {
                sendserial.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show("连接控制设备失败。");
                setDevice();
                return;
            }

        }

        private void Rlc_EditSel(RectSel obj)
        {
            src.EditSel(obj);
        }

        private void Rlc_AddNewSel(RegionModel obj)
        {
            src.StartCreateNewRegion();
            src.NewRegionCreated = (p1, p2) =>
           {
               obj.Sels.Add(new RectSel(p1, p2));
               src.NewRegionCreated = null;
           };
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            staPort.Dispose();
        }

        private void Station_DataReceived(object sender, StationPort.DataReceivedEventArgs e)
        {
            gotDataAsync(e.Data);
        }

        private async void gotDataAsync(UWB_Data data)
        {
            if (!this.Dispatcher.CheckAccess())
            {
                try
                {
                    await this.Dispatcher.BeginInvoke(new Action<UWB_Data>(processData), data);
                }
                catch (System.Threading.Tasks.TaskCanceledException)
                {

                }

                return;
            }
            processData(data);
        }
        /// <summary>
        /// 处理定位数据，该函数在UI线程上执行。
        /// </summary>
        /// <param name="data"></param>
        private void processData(UWB_Data data)
        {
            if (PauseStationData)
            {
                return;
            }
            try
            {
                if (data.mid == UWB_mid.ma)
                {
                    uwb_dataview.MA = data;
                    StationInfo.PushMA(data);
                }
                else if (data.mid == UWB_mid.mc)
                {
                    uwb_dataview.MC = data;
                    StationInfo.PushMC(data);
                }
                else
                {
                    //忽略mr数据
                }
            }
            catch (Exception ex)
            {
                addErrorLog(ex.Message, data.ToString());
            }

            foreach (var item in rlc.Regions)
            {
                item.HitTest(StationInfo.TagPosition.GetPoint());
                if (item.IsTrigger)
                {
                    Action<byte[]> action = new Action<byte[]>(SenData);
                    action.BeginInvoke(item.Data, iar =>
                    {
                        try
                        {
                            action.EndInvoke(iar);
                        }
                        catch (Exception ex)
                        {
                            addErrorLog($"区域：[{item.Name}]发送控制数据失败", "");
                        }
                    }, null);

                }
            }


        }
        /// <summary>
        /// 发送触发区域的数据
        /// </summary>
        /// <param name="data"></param>
        private void SenData(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                return;
            }
            sendserial.Write(data, 0, data.Length);
        }

        private void addErrorLog(string err, string data)
        {
            try
            {
                lst_errorlog.Items.Insert(0, err + "[" + data + "]");

                if (lst_errorlog.Items.Count > 100)
                {
                    lst_errorlog.Items.RemoveAt(lst_errorlog.Items.Count - 1);
                }
            }
            catch (Exception)
            {

            }

        }

        /// <summary>
        /// 设备设置
        /// </summary>
        public DeviceSetting DeviceSetting { get; set; }


        /// <summary>
        /// 场景
        /// </summary>
        public SceneModel Scene
        {
            get { return (SceneModel)GetValue(SceneProperty); }
            set { SetValue(SceneProperty, value); }
        }

        public static readonly DependencyProperty SceneProperty =
            DependencyProperty.Register("Scene", typeof(SceneModel), typeof(MainWindow), new PropertyMetadata(null,
                new PropertyChangedCallback((o, e) =>
            {

                ((MainWindow)o).StationInfo.Scene = (SceneModel)e.NewValue;
            })));

        #region eventhandlers

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            if (e.Key == Key.Pause)
            {
                this.PauseStationData = true;
            }
        }

        protected override void OnPreviewKeyUp(KeyEventArgs e)
        {
            base.OnPreviewKeyUp(e);
            if (e.Key == Key.Pause)
            {
                this.PauseStationData = false;
            }
        }


        private void menu_choose_mapimage_Click(object sender, RoutedEventArgs e)
        {
            ClientHelper.ImportMapImage();
        }
        private void menu_open_scenee_Click(object sender, RoutedEventArgs e)
        {
            SceneListWindow slw = new SceneListWindow();
            var result = slw.ShowDialog();
            if (result.HasValue && result.Value)
            {
                this.Scene = slw.SelectedScene;
            }
        }

        private void menu_exit_Click(object sender, RoutedEventArgs e)
        {
            CustomCommands.CloseWindow.Execute(null, this);
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == CustomCommands.CloseWindow)
            {
                this.Close();
            }
            else if (e.Command == CustomCommands.MaxWindow)
            {
                if (this.WindowStyle == WindowStyle.None)
                {
                    this.WindowStyle = WindowStyle.SingleBorderWindow;
                    this.WindowState = WindowState.Normal;
                    txt_win_status.Text = "F11全屏";
                    txt_win_status.Foreground = Brushes.Black;
                }
                else
                {
                    this.WindowStyle = WindowStyle.None;
                    this.WindowState = WindowState.Maximized;
                    txt_win_status.Text = "F11退出全屏";
                    txt_win_status.Foreground = Brushes.Red;
                }
            }
        }

        private void menu_new_scenee_Click(object sender, RoutedEventArgs e)
        {
            var files = Directory.GetFiles(ClientHelper.ScenesFolder, "*.json");
            SceneModel scene = new SceneModel() { Name = $"新场景-{files.Length + 1}" };
            scene.FileName = DateTime.Now.ToString("yyyyMMddHHmmss") + ".json";
            this.Scene = scene;
            scene.Save();
            MessageBox.Show("创建成功。");
        }
        #endregion

        private void setDevice()
        {
            UWB_DeviceSettingWindow ds = new UWB_DeviceSettingWindow();
            ds.Owner = this;
            ds.ShowDialog();
            this.reloadDeviceSetting();
        }

        private void menu_device_setting_Click(object sender, RoutedEventArgs e)
        {
            setDevice();
        }

    }
}

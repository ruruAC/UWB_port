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
    using UWB_Client.Models;
    /// <summary>
    /// SceneSettingControl.xaml 的交互逻辑
    /// </summary>
    public partial class SceneSettingControl : UserControl
    {
        public SceneSettingControl()
        {
            InitializeComponent();
            this.IsEnabled = false;
        }



        public SceneModel Scene
        {
            get { return (SceneModel)GetValue(SceneProperty); }
            set { SetValue(SceneProperty, value); }
        }

        public static readonly DependencyProperty SceneProperty =
            DependencyProperty.Register("Scene", typeof(SceneModel), typeof(SceneSettingControl), new PropertyMetadata(null));

        private void btn_choose_mapImageFile_Click(object sender, RoutedEventArgs e)
        {
            MapListWindow mlw = new MapListWindow();
            mlw.Owner = Application.Current.MainWindow;
            var result = mlw.ShowDialog();
            if (result.HasValue && result.Value)
            {
                this.Scene.MapImageFile = mlw.SelectedMap;
            }
        }

        private void btn_choose_scene_Click(object sender, RoutedEventArgs e)
        {
            SceneListWindow slw = new SceneListWindow();
            var result = slw.ShowDialog();
            if (result.HasValue && result.Value)
            {
                this.Scene = slw.SelectedScene;
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == SceneSettingControl.SceneProperty)
            {
                if (e.NewValue == null)
                {//禁用
                    this.IsEnabled = false;
                    txt_noscene_waring.Visibility = Visibility.Visible;
                }
                else
                {//可用
                    this.IsEnabled = true;
                    txt_noscene_waring.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void btn_save_Click(object sender, RoutedEventArgs e)
        {
            if (this.Scene != null)
            {
                try
                {
                    this.Scene.Save();
                    MessageBox.Show(Application.Current.MainWindow, "保存成功。", "完成", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Application.Current.MainWindow, $"保存失败。{ex.Message}", "发生异常", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}

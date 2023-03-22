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
using System.Windows.Shapes;

namespace UWB_Client
{
    using System.IO;
    /// <summary>
    /// MapListWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MapListWindow : Window
    {
        public MapListWindow()
        {
            InitializeComponent();
            this.Loaded += MapListWindow_Loaded;
            lst.MouseDoubleClick += Lst_MouseDoubleClick;
            lst.SelectionChanged += Lst_SelectionChanged;
        }

        private void Lst_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lst.SelectedValue is MapFile mf)
            {
                img.Source = new BitmapImage(new Uri(Path.Combine(ClientHelper.MapsFolder, mf.Name)));
            }
            else
            {
                img.Source = null;
            }
        }

        private void Lst_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            btn_ok.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }

        private void MapListWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var files = Directory.GetFiles(ClientHelper.MapsFolder);
            List<MapFile> mapfiles = new List<MapFile>();
            foreach (var file in files)
            {
                var ext = Path.GetExtension(file).ToLower();
                switch (ext)
                {
                    case ".png":
                    case ".bmp":
                    case ".jpg":
                    case ".jpeg":
                        mapfiles.Add(new MapFile() { Name = Path.GetFileName(file), ThumbImage = new BitmapImage(new Uri(ClientHelper.GetImageThumbFile(file))) });
                        break;
                    default:
                        break;
                }
            }
            lst.ItemsSource = mapfiles;
        }

        public class MapFile
        {
            public string Name { get; set; }
            public ImageSource ThumbImage { get; set; }
        }

        public string SelectedMap { get; private set; }

        private void btn_ok_Click(object sender, RoutedEventArgs e)
        {
            if (lst.SelectedValue is MapFile mf)
            {
                this.SelectedMap = mf.Name;
                this.DialogResult = true;
            }
            else
            {
                MessageBox.Show(this, "必须选择地图文件。", "请选择", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}

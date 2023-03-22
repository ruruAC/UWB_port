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
    using UWB_Client.Models;
    using Newtonsoft.Json;
    /// <summary>
    /// SceneListWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SceneListWindow : Window
    {
        public SceneListWindow()
        {
            InitializeComponent();
            this.Owner = Application.Current.MainWindow;
            this.Loaded += SceneListWindow_Loaded;
        }


        private void SceneListWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var files = Directory.GetFiles(ClientHelper.ScenesFolder, "*.json");
            List<SceneModel> sceneList = new List<SceneModel>();
            List<string> el = new List<string>();
            foreach (var file in files)
            {
                using (var sr = File.OpenText(file))
                {
                    var json = sr.ReadToEnd();
                    try
                    {
                        var scene = JsonConvert.DeserializeObject<SceneModel>(json);
                        scene.FileName = Path.GetFileName(file);
                        sceneList.Add(scene);
                    }
                    catch (Exception)
                    {
                        el.Add(file);
                    }
                }
            }
            this.lst.ItemsSource = sceneList;
            if (el.Count > 0)
            {
                MessageBox.Show(this, $"文件[\n{string.Join("\n", el.ToArray())}\n]不是有效的场景文件或已损坏。", "文件损坏", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        public SceneModel SelectedScene
        {
            get { return (SceneModel)GetValue(SelectedSceneProperty); }
            set { SetValue(SelectedSceneProperty, value); }
        }

        public static readonly DependencyProperty SelectedSceneProperty =
            DependencyProperty.Register("SelectedScene", typeof(SceneModel), typeof(SceneListWindow), new PropertyMetadata(null));

        private void btn_ok_Click(object sender, RoutedEventArgs e)
        {
            if (this.SelectedScene != null)
            {
                this.DialogResult = true;
            }
            else
            {
                MessageBox.Show(this, "请选择一个场景。", "请选择", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void lst_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            btn_ok.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }
    }
}

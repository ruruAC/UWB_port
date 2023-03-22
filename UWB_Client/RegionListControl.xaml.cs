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
    /// RegionListControl.xaml 的交互逻辑
    /// </summary>
    public partial class RegionListControl : UserControl
    {
        public RegionListControl()
        {
            InitializeComponent();
        }



        public RegionList Regions
        {
            get { return (RegionList)GetValue(RegionsProperty); }
            set { SetValue(RegionsProperty, value); }
        }

        public static readonly DependencyProperty RegionsProperty =
            DependencyProperty.Register("Regions", typeof(RegionList), typeof(RegionListControl), new PropertyMetadata(new RegionList()));

        private void btn_expand_all_Click(object sender, RoutedEventArgs e)
        {
            bool hase = false;//是否有展开的
            foreach (var item in Regions)
            {
                if (item.IsFocused)
                {
                    hase = true;
                    break;
                }
            }
            foreach (var item in Regions)
            {
                item.IsFocused = !hase;
            }
        }
        /// <summary>
        /// 需要增加新的选区
        /// </summary>
        public event Action<RegionModel> AddNewSel;
        /// <summary>
        /// 编辑选区
        /// </summary>
        public event Action<RectSel> EditSel;

        public event Action NeedRefresh;

        private void btn_new_Click(object sender, RoutedEventArgs e)
        {
            Regions.Add(new RegionModel() { Name = $"区域{Regions.Count + 1}" });
        }

        private void btn_NewSel_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            var region = button.Tag as RegionModel;
            AddNewSel?.Invoke(region);
        }

        private void btn_editsel_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            var sel = button.Tag as RectSel;
            EditSel?.Invoke(sel);

        }
        private void btn_delsel_Click(object sender, RoutedEventArgs e)
        { 
            Button button = (Button)sender;
            var sel = button.Tag as RectSel;
            EditSel?.Invoke(sel);
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = (ListBox)sender;
            if (list.SelectedItem is RectSel sel)
            {
                sel.IsSelected = true;
                if (Regions != null)
                {
                    foreach (var region in Regions)
                    {
                        foreach (var s in region.Sels)
                        {
                            if (s != sel)
                            {
                                s.IsSelected = false;
                            }
                        }
                    }
                }
                NeedRefresh?.Invoke();
            }
        }

        private void ListBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var list = (ListBox)sender;
            list.SelectedItem = null;
        }

        private void btn_save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Regions.Save();
                MessageBox.Show("保存成功。", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存失败。{ex.Message}", "失败", MessageBoxButton.OK, MessageBoxImage.Error);

            }

        }

        private void btn_edit_TriggerData_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            var region = (RegionModel)btn.Tag;
            if (region == null)
            {
                return;
            }
            HexEditWindow win = new HexEditWindow();
            win.Data = region.Data;
            var result = win.ShowDialog();
            if (result.HasValue && result.Value)
            {
                region.Data = win.Data;
            }
        }

    }
}

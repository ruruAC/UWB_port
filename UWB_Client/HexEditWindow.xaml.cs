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
    /// <summary>
    /// HexEditWindow.xaml 的交互逻辑
    /// </summary>
    public partial class HexEditWindow : Window
    {
        public HexEditWindow()
        {
            InitializeComponent();
            txt_ascii.LostFocus += Txt_ascii_LostFocus;
            txt_hex.LostFocus += Txt_hex_LostFocus;
            this.Loaded += HexEditWindow_Loaded;
        }

        private void HexEditWindow_Loaded(object sender, RoutedEventArgs e)
        {
            txt_ascii.Text = GetASCII();
            txt_hex.Text = GetHex();
        }

        private void Txt_hex_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                Data = GetBytes(txt_hex.Text);
                txt_ascii.Text = System.Text.Encoding.ASCII.GetString(Data);
            }
            catch (Exception)
            {
                txt_hex.SelectAll();
                txt_hex.Focus();
            }
        }

        private void Txt_ascii_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txt_hex.Text))
            {
                Data = System.Text.Encoding.ASCII.GetBytes(txt_ascii.Text);
                txt_hex.Text = GetHex();
            }

        }

        public byte[] Data
        {
            get { return (byte[])GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(byte[]), typeof(HexEditWindow), new PropertyMetadata(null));
        private string GetASCII()
        {
            if (Data == null)
            {
                return "";
            }
            return System.Text.Encoding.ASCII.GetString(Data);
        }
        private string GetHex()
        {
            if (Data == null)
            {
                return "";
            }
            List<string> l = new List<string>();
            foreach (var item in Data)
            {
                l.Add($"{item:X2}");
            }
            return string.Join(" ", l.ToArray());
        }

        private byte[] GetBytes(string hex)
        {
            List<byte> l = new List<byte>();
            var strs = hex.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in strs)
            {
                l.Add(Convert.ToByte(item, 16));
            }
            return l.ToArray();
        }

        private void btn_ok_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Data = GetBytes(txt_hex.Text);
            }
            catch (Exception)
            {
                MessageBox.Show("数据有误。");
                txt_hex.Focus();
                return;
            }

            this.DialogResult = true;
        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace UWB_Client
{
    using System.Windows;
    using System.IO;
    using Microsoft.Win32;
    using System.Windows.Media.Imaging;
    using System.Windows.Media;
    using System.Net.NetworkInformation;

    public static class ClientHelper
    {
        static ClientHelper()
        {
            MapsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Maps");
            if (!Directory.Exists(MapsFolder))
            {
                Directory.CreateDirectory(MapsFolder);
            }
            ScenesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scenes");
            if (!Directory.Exists(ScenesFolder))
            {
                Directory.CreateDirectory(ScenesFolder);
            }
        }

        public readonly static string ScenesFolder;
        public readonly static string MapsFolder;
        /// <summary>
        /// 导入地图文件
        /// </summary>
        /// <returns></returns>
        public static string ImportMapImage()
        {
            start:
            OpenFileDialog dialog = new OpenFileDialog() { CheckFileExists = true };
            dialog.Filter = "支持的图形文件|*.png;*.bmp;*.jpg;*.jpeg";
            var result = dialog.ShowDialog();
            string filename = null;
            if (result.HasValue && result.Value)
            {
                filename = dialog.FileName;
            }
            else
            {
                return null;
            }
            var name = Path.GetFileNameWithoutExtension(filename);
            var ext = Path.GetExtension(filename);
            name = Microsoft.VisualBasic.Interaction.InputBox("名称不能为空且不能包含特殊字符。", "请输入新的地图名称", name);
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }
            var files = Directory.GetFiles(MapsFolder);
            foreach (var f in files)
            {
                if (Path.GetFileNameWithoutExtension(f).ToLower() == name.ToLower())
                {
                    MessageBox.Show($"地图文件[{name}]已存在。", "地图已存在", MessageBoxButton.OK, MessageBoxImage.Error);
                    goto start;
                }
            }
            try
            {
                var newfile = Path.Combine(MapsFolder, name + ext);
                File.Copy(filename, newfile);
                CreateImageThumb(newfile);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导入文件时发生异常。{ex.Message}", "发生异常", MessageBoxButton.OK, MessageBoxImage.Error);
                goto start;
            }
            MessageBox.Show($"成功导入地图文：{name + ext}", "导入成功", MessageBoxButton.OK, MessageBoxImage.Information);

            return name + ext;
        }
        /// <summary>
        /// 创建缩略图
        /// </summary>
        /// <param name="sourcefile"></param>
        public static void CreateImageThumb(String sourcefile)
        {
            var newsize = new Size(200, 200);
            BitmapImage image = new BitmapImage(new Uri(sourcefile));
            var img = image.Clone();

            RenderTargetBitmap rtb = new RenderTargetBitmap((int)newsize.Width, (int)newsize.Height, 96, 96, PixelFormats.Default);
            var drawingVisual = new DrawingVisual();
            using (var dc = drawingVisual.RenderOpen())
            {
                dc.DrawImage(img, new Rect(new Point(0, 0), newsize));
            }
            rtb.Render(drawingVisual);

            var thumbfolder = Path.Combine(ClientHelper.MapsFolder, ".thumb");
            if (!Directory.Exists(thumbfolder))
            {
                Directory.CreateDirectory(thumbfolder);
            }
            var newfilename = Path.Combine(thumbfolder, Path.GetFileName(sourcefile) + ".jpg");

            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            using (FileStream fs = new FileStream(newfilename, FileMode.Create, FileAccess.ReadWrite))
            {
                encoder.Save(fs);
            }
            ;
        }
        //获取图片的缩略图
        public static string GetImageThumbFile(string imgfile)
        {
            var thumbfolder = Path.Combine(ClientHelper.MapsFolder, ".thumb");

            var thumbfilename = Path.Combine(thumbfolder, Path.GetFileName(imgfile) + ".jpg");
            if (!File.Exists(thumbfilename))
            {
                CreateImageThumb(imgfile);
            }
            return thumbfilename;
        }
    }
}

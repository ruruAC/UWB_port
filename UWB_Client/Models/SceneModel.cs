using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWB_Client.Models
{
    using Newtonsoft.Json;
    using SharpSoft.Geometries;
    using System.ComponentModel;
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// 场景
    /// </summary>

    [JsonObject(MemberSerialization.OptIn)]
    public class SceneModel : DependencyObject, INotifyPropertyChanged
    {
        [JsonIgnore]
        public string FileName { get; set; }

       

        public void Save()
        {
            if (string.IsNullOrWhiteSpace(FileName))
            {
                throw new Exception("未指定有效的文件名称。");
            }
            var folder = ClientHelper.ScenesFolder;
            var file = Path.Combine(folder, FileName);
            var json = JsonConvert.SerializeObject(this);
            File.WriteAllText(file, json);
        }
        /// <summary>
        /// 场景名称
        /// </summary>
        [JsonProperty]
        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        public static readonly DependencyProperty NameProperty =
            DependencyProperty.Register("Name", typeof(string), typeof(SceneModel), new PropertyMetadata("新场景"));


        /// <summary>
        /// 地图图形文件
        /// </summary>  
        [JsonProperty]
        public string MapImageFile
        {
            get { return (string)GetValue(MapImageFileProperty); }
            set { SetValue(MapImageFileProperty, value); }
        }

        public static readonly DependencyProperty MapImageFileProperty =
            DependencyProperty.Register("MapImageFile", typeof(string), typeof(SceneModel), new PropertyMetadata(null));

        public ImageSource MapThumbImage
        {
            get
            {
                if (string.IsNullOrWhiteSpace(MapImageFile))
                {
                    return null;
                }
                var mapfile = Path.Combine(ClientHelper.MapsFolder, MapImageFile);
                if (!File.Exists(mapfile))
                {
                    return null;
                }
                var thumb = ClientHelper.GetImageThumbFile(mapfile);

                return new BitmapImage(new Uri(thumb));
            }
        }

        /// <summary>
        /// 横向长度
        /// </summary>

        [JsonProperty]
        public double X_Length
        {
            get { return (double)GetValue(X_LengthProperty); }
            set { SetValue(X_LengthProperty, value); }
        }

        public static readonly DependencyProperty X_LengthProperty =
            DependencyProperty.Register("X_Length", typeof(double), typeof(SceneModel), new PropertyMetadata(10000d));


        /// <summary>
        /// 纵向长度
        /// </summary>

        [JsonProperty]
        public double Y_Length
        {
            get { return (double)GetValue(Y_LengthProperty); }
            set { SetValue(Y_LengthProperty, value); }
        }

        public static readonly DependencyProperty Y_LengthProperty =
            DependencyProperty.Register("Y_Length", typeof(double), typeof(SceneModel), new PropertyMetadata(5000d));



        /// <summary>
        /// 基站0的坐标
        /// </summary>
        [JsonProperty]
        public Point2D A0_Position
        {
            get { return (Point2D)GetValue(A0_PositionProperty); }
            set { SetValue(A0_PositionProperty, value); }
        }

        public static readonly DependencyProperty A0_PositionProperty =
            DependencyProperty.Register("A0_Position", typeof(Point2D), typeof(SceneModel), new PropertyMetadata(default(Point2D)));


        /// <summary>
        /// 基站A0-A1所在直线与X轴的夹角（弧度）
        /// </summary>
        [JsonProperty]
        public double Angle_A0_A1
        {
            get { return (double)GetValue(Angle_A0_A1Property); }
            set { SetValue(Angle_A0_A1Property, value); }
        }

        public static readonly DependencyProperty Angle_A0_A1Property =
            DependencyProperty.Register("Angle_A0_A1", typeof(double), typeof(SceneModel), new PropertyMetadata(0d));


        /// <summary>
        /// 是否翻转第三个基站的位置
        /// </summary>
        [JsonProperty]
        public bool Flip_A2
        {
            get { return (bool)GetValue(Flip_A2Property); }
            set { SetValue(Flip_A2Property, value); }
        }

        public static readonly DependencyProperty Flip_A2Property =
            DependencyProperty.Register("Flip_A2", typeof(bool), typeof(SceneModel), new PropertyMetadata(false));

        public event PropertyChangedEventHandler PropertyChanged;

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(e.Property.Name));
        }


         
    }
}

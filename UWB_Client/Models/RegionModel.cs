using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWB_Client.Models
{
    using Newtonsoft.Json;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Windows;

    [JsonObject(MemberSerialization.OptIn)]
    public class RegionModel : DependencyObject, INotifyPropertyChanged
    {
        public RegionModel()
        {
            Sels = new ObservableCollection<RectSel>();
            Sels.CollectionChanged += Sels_CollectionChanged;
        }

        private void Sels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Sels"));
        }


        /// <summary>
        /// 名称
        /// </summary>
        [JsonProperty]
        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        public static readonly DependencyProperty NameProperty =
            DependencyProperty.Register("Name", typeof(string), typeof(RegionModel), new PropertyMetadata(""));

        /// <summary>
        /// 数据
        /// </summary>
        [JsonProperty] 
        public byte[] Data
        {
            get { return (byte[])GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(byte[]), typeof(RegionModel), new PropertyMetadata(null));



        /// <summary>
        /// 是否具有焦点
        /// </summary>
        public bool IsFocused
        {
            get { return (bool)GetValue(IsFocusedProperty); }
            set { SetValue(IsFocusedProperty, value); }
        }

        public static readonly DependencyProperty IsFocusedProperty =
            DependencyProperty.Register("IsFocused", typeof(bool), typeof(RegionModel), new PropertyMetadata(false));


        /// <summary>
        /// 是否已触发
        /// </summary>
        public bool IsTrigger
        {
            get { return (bool)GetValue(IsTriggerProperty); }
            set { SetValue(IsTriggerProperty, value); }
        }

        public static readonly DependencyProperty IsTriggerProperty =
            DependencyProperty.Register("IsTrigger", typeof(bool), typeof(RegionModel), new PropertyMetadata(false));

        public void HitTest(Point p)
        { 
            foreach (var item in Sels)
            {
                if (item.Contains(p))
                { 
                    item.IsTrigger = true;
                }
                else
                {
                    item.IsTrigger = false;
                }
            }
            this.IsTrigger = true;
        }

        [JsonProperty]
        public ObservableCollection<RectSel> Sels { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(e.Property.Name));
        }
    }

    public class RegionList : ObservableCollection<RegionModel>
    {
        static RegionList()
        {
            filename = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "regions.json");
        }
        public static readonly string filename;
        public void Save()
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            using (var fs = new FileStream(filename, FileMode.Create))
            {
                using (var sw = new StreamWriter(fs))
                {
                    sw.Write(json);
                }
            }
        }

        public static RegionList Load()
        {
            if (!File.Exists(filename))
            {
                return new RegionList();
            }
            using (var fs = new FileStream(filename, FileMode.Open))
            {
                using (var sr = new StreamReader(fs))
                {
                    var json = sr.ReadToEnd();
                    return JsonConvert.DeserializeObject<RegionList>(json);
                }
            }
        }
    }

    /// <summary>
    /// 矩形选区
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class RectSel : DependencyObject
    {
        public RectSel()
        {

        }
        public RectSel(Point point1, Point point2)
        {
            X = Math.Min(point1.X, point2.X);
            Y = Math.Min(point1.Y, point2.Y);
            Width = Math.Max(Math.Max(point1.X, point2.X) - X, 0.0);
            Height = Math.Max(Math.Max(point1.Y, point2.Y) - Y, 0.0);
            IsSelected = false;
        }


        [JsonProperty]
        public double X
        {
            get { return (double)GetValue(XProperty); }
            set { SetValue(XProperty, value); }
        }

        public static readonly DependencyProperty XProperty =
            DependencyProperty.Register("X", typeof(double), typeof(RectSel), new PropertyMetadata(0d));



        [JsonProperty]
        public double Y
        {
            get { return (double)GetValue(YProperty); }
            set { SetValue(YProperty, value); }
        }

        public static readonly DependencyProperty YProperty =
            DependencyProperty.Register("Y", typeof(double), typeof(RectSel), new PropertyMetadata(0d));



        [JsonProperty]
        public double Width
        {
            get { return (double)GetValue(WidthProperty); }
            set { SetValue(WidthProperty, value); }
        }

        public static readonly DependencyProperty WidthProperty =
            DependencyProperty.Register("Width", typeof(double), typeof(RectSel), new PropertyMetadata(0d));




        [JsonProperty]
        public double Height
        {
            get { return (double)GetValue(HeightProperty); }
            set { SetValue(HeightProperty, value); }
        }

        public static readonly DependencyProperty HeightProperty =
            DependencyProperty.Register("Height", typeof(double), typeof(RectSel), new PropertyMetadata(0d));


        /// <summary>
        /// 该选区是否已触发
        /// </summary>
        public bool IsTrigger
        {
            get { return (bool)GetValue(IsTriggerProperty); }
            set { SetValue(IsTriggerProperty, value); }
        }
        public static readonly DependencyProperty IsTriggerProperty =
            DependencyProperty.Register("IsTrigger", typeof(bool), typeof(RectSel), new PropertyMetadata(false));



        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(RectSel), new PropertyMetadata(false));



        public bool Contains(Point p)
        {
            return p.X >= this.X && p.Y >= this.Y && p.X <= this.X + this.Width && p.Y <= this.Y + this.Height;
        }

    }
}

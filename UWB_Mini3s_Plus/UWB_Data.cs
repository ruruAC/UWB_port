using System;

namespace UWB_Mini3s_Plus
{
    public struct UWB_Data
    {
        /// <summary>
        /// 数据类别
        /// </summary>
        public UWB_mid mid { get; set; }
        /// <summary>
        /// 按位表示range0-range3的有效性
        /// 为15表示全部有效，为7表示range0无效
        /// </summary>
        public byte mask { get; set; }

        public Int32 range0 { get; set; }
        public Int32 range1 { get; set; }
        public Int32 range2 { get; set; }
        public Int32 range3 { get; set; }
        /// <summary>
        /// unit raw range 计数值（会不断累加） 
        /// </summary>
        public Int16 ranges { get; set; }
        /// <summary>
        /// range sequence number 计数值（会不断累加） 
        /// </summary>
        public byte rseq { get; set; }
        /// <summary>
        /// 如果 MID=ma，代表 TX/RX 天线延迟 
        /// </summary>
        public Int32 debug { get; set; }
        /// <summary>
        /// 标签id
        /// </summary>
        public int tagid { get; set; }
        /// <summary>
        /// 基站id
        /// </summary>
        public int stationid { get; set; }

        public override string ToString()
        {
            return $"{Enum.GetName(typeof(UWB_mid),mid)} {range0} {range1} {range2} {range3}";
        }
    }
}

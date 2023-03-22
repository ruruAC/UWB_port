namespace UWB_Mini3s_Plus
{
    public enum UWB_mid
    {
        /// <summary>
        /// 代表标签-基站距离（原生数据）
        /// 此时range0-range3表示标签到4个基站的距离
        /// </summary>
        mr,
        /// <summary>
        /// 代表标签-基站距离（优化修正过的数据，用于定位标签） 
        /// 此时range0-range3表示标签到4个基站的优化距离
        /// </summary>
        mc,
        /// <summary>
        /// 代表基站-基站距离（修正优化过，用于基站自动定位）
        /// 此时range0无效，range1表示a0-a1,range2表示a0-a2,range3表示a1-a2
        /// </summary>
        ma
    }
}

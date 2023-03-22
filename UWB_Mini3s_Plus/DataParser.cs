using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWB_Mini3s_Plus
{
    /// <summary>
    /// MID 消息 ID, 一共有三类，分别为 mr, mc, ma
    /// mr 代表标签-基站距离（原生数据） 
    /// mc 代表标签-基站距离（优化修正过的数据，用于定位标签）
    /// ma 代表基站-基站距离（修正优化过，用于基站自动定位）
    /// MASK 表示 RANGE0, RANGE1, RANGE2, RANGE3 有哪几个消息是有效的；例如: MASK=7 (0000 0111) 表示 RANGE0, RANGE1, RANGE2 都有效
    /// 
    ///RANGE0 如果 MID = mc 或 mr，表示标签 x 到基站 0 的距离，单位：毫米 
    ///RANGE1 如果 MID = mc 或 mr，表示标签 x 到基站 1 的距离，单位：毫米 如果 MID = ma，     表示基站 0 到基站 1 的距离，单位：毫米
    ///RANGE2 如果 MID = mc 或 mr，表示标签 x 到基站 2 的距离，单位：毫米 如果 MID = ma，     表示基站 0 到基站 2 的距离，单位：毫米
    ///RANGE3 如果 MID = mc 或 mr，表示标签 x 到基站 3 的距离，单位：毫米 如果 MID = ma，     表示基站 1 到基站 2 的距离，单位：毫米
    ///  NRANGES unit raw range 计数值（会不断累加）
    ///RSEQ range sequence number 计数值（会不断累加）
    ///DEBUG 如果 MID=ma，代表 TX/RX 天线延迟
    /// aT:A T 是标签 ID，A 是基站 ID 此处提到的 ID 只是一个 short ID，完整的 ID 是 64 bit 的 ID
    /// </summary>
    /// <remarks>
    /// 1. mr 0f 000005a4 000004c8 00000436 000003f9 0958 c0 40424042 a0:0 
    /// 2. ma 07 00000000 0000085c 00000659 000006b7 095b 26 00024bed a0:0 
    /// 3. mc 0f 00000663 000005a3 00000512 000004cb 095f c1 00024c24 a0:0 
    /// </remarks>
    public class DataParser
    {

        public static UWB_Data Parse(string data)
        {
            var ts = data.Split(' ');
            UWB_Data d = new UWB_Data();
            switch (ts[0])
            {
                case "ma":
                    d.mid = UWB_mid.ma;
                    break;
                case "mc":
                    d.mid = UWB_mid.mc;
                    break;
                case "mr":
                    d.mid = UWB_mid.mr;
                    break;
                default:
                    fail("无法识别");
                    break;
            }

            d.mask = str2byte(ts[1]);

            d.range0 = str2int(ts[2]);
            d.range1 = str2int(ts[3]);
            d.range2 = str2int(ts[4]);
            d.range3 = str2int(ts[5]);

            d.ranges = str2short(ts[6]);

            d.rseq = str2byte(ts[7]);

            d.debug = str2int(ts[8]);

            var ts9 = ts[9];
            var i = ts9.IndexOf(':');
            d.tagid = int.Parse(ts9.Substring(1, i - 1));
            d.stationid = int.Parse(ts9.Substring(i + 1, ts9.Length - i - 1));
            return d;
        }

        private static Int32 str2int(string str)
        {
            return Convert.ToInt32(str, 16);
        }
        private static Byte str2byte(string str)
        {
            return Convert.ToByte(str, 16);

        }
        private static Int16 str2short(string str)
        {
            return Convert.ToInt16(str, 16);
        }

        public static void fail(string err)
        {
            throw new Exception(err);
        }
    }
}

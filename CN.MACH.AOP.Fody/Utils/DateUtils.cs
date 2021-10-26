using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace CN.MACH.AI.UnitTest.Core.Utils
{
    public class DateUtils
    {
        static public string GetDateFormat(int nStyle = 0)
        {
            string style = "";
            switch (nStyle)
            {
                case 0:
                    style = "yyyyMMddHHmmss";
                    break;
                case 1:
                    style = "yyyy/M/d tt hh:mm:ss";
                    break;
                case 2:
                    style = "yyyy/MM/dd tt hh:mm:ss";
                    break;
                case 3:
                    style = "yyyy/MM/dd HH:mm:ss";
                    break;
                case 4:
                    style = "yyyy-MM-dd HH:mm:ss";
                    break;
                case 5:
                    style = "yyyy-MM-dd";
                    break;
                case 6:
                    style = "yyyy/M/d HH:mm:ss";
                    break;
                case 7:
                    style = "yyyy/M/d";
                    break;
                case 8:
                    style = "yyyy/MM/dd ";
                    break;
                default:
                    style = "yyyyMMddHHmmss";
                    break;
            }
            return style;
        }

        static public string GetNowString(int nStyle = 0)
        {
            return GetNowString(GetDateFormat(nStyle));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sFormat"></param>
        /// <param name="culture">en-US zh-CN</param>
        /// <returns></returns>
        static public string GetNowString(string sFormat, string culture = null)
        {
            return Date2Str(DateTime.Now, sFormat, culture);
        }
        static public string Date2Str(DateTime dtDate, string sFormat = "yyyy-MM-dd", string culture = null)
        {
            if (StringUtils.IsEmptyOrSpace(culture)) return dtDate.ToString(sFormat);
            CultureInfo cn = CultureInfo.GetCultureInfo(culture);
            //if (dtDate == null) return DateTime.Now.ToString(sFormat, cn.DateTimeFormat);
            return dtDate.ToString(sFormat, cn.DateTimeFormat);
        }

    }
}

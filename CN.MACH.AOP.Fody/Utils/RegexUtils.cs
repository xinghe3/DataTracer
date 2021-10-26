using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CN.MACH.AI.UnitTest.Core.Utils
{
    public class RegexUtils
    {
        public static string Escape(string str)
        {
            if (str == null) return "";
            return Regex.Escape(str);
        }

        static private bool Valid(Regex rx, bool IsNeedWarning)
        {
            if (rx == null)
            {
                if (IsNeedWarning) Logs.WriteError("Null Regex", rx);
                return false;
            }
            return true;
        }

        static public bool MatchExact(string str, string regex)
        {
            regex = "^" + regex + "$";
            return Match(str, regex);
        }
        static public bool MatchExact(string str, string regex, RegexOptions opt, bool IsNeedWarning = true)
        {
            regex = "^" + regex + "$";
            return Match(str, regex, opt, IsNeedWarning);
        }

        static public bool Match(string str, string regex, bool IsNeedWarning = true)
        {
            return Match(str, regex, RegexOptions.None, IsNeedWarning);
        }
        static public bool Match(string str, string regex, RegexOptions opt, bool IsNeedWarning = true)
        {
            bool isMatchRegex = false;
            Regex rx = GetRegex(regex, opt, IsNeedWarning);
            if (!Valid(rx, IsNeedWarning))
                return isMatchRegex;
            Match m = null;
            try
            {
                m = rx.Match(str);
                isMatchRegex = m.Success;
            }
            catch (Exception ex)
            {
                Logs.WriteExLog(ex, "ERROR REGEX: Target:", str, " REGEX:", regex);
            }
            return isMatchRegex;
        }
        private static Regex GetRegex(string str, RegexOptions opt, bool IsNeedWarning = true)
        {
            Regex rx = null;
            //Regex.CacheSize = 1024;
            try
            {
                rx = new Regex(str, opt);
            }
            catch (Exception ex)
            {
                if (IsNeedWarning) Logs.WriteExLog(ex, "ERROR REGEX:" + str);
            }
            return rx;
        }
        static public List<int> GetMatchPositionList(string str, string regex, bool IgnoreCase = true)
        {
            List<int> list = new List<int>();
            RegexOptions opt = RegexOptions.Multiline;
            if (IgnoreCase) opt |= RegexOptions.IgnoreCase;
            //Regex rx = GetRegex(regex);
            MatchCollection match = BuildMatch(str, regex, RegexOptions.None);
            if (match == null) return list;
            foreach (Match m in match)
            {
                if (m.Index >= 0)
                {
                    list.Add(m.Index);
                }
            }
            return list;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="regex"></param>
        /// <param name="opt"></param>
        /// <param name="replaceList">
        /// List<string> replaceList = new List<string>();
        /// replaceList.Add("$1");
        /// replaceList.Add("$2");
        /// replaceList.Add("$3");
        /// </param>
        /// <returns></returns>
        static public List<List<string>> GetMatchList(string str, string regex, RegexOptions opt, List<string> replaceList)
        {
            List<List<string>> list = new List<List<string>>();
            if (StringUtils.IsEmptyOrSpace(str))
                return list;
            if (!CollectionUtils.IsNullOrEmpty<string>(replaceList))
            {
                foreach (string item in replaceList)
                    list.Add(new List<string>());
            }
            else
            {
                list.Add(new List<string>());
            }

            MatchCollection match = BuildMatch(str, regex, opt);
            if (match == null) return list;
            foreach (Match Item in match)
            {
                if (Item.Value == null)
                    continue;
                if (Item.Groups != null && Item.Groups.Count > 1)
                {
                    //string[] strParamsArr = GetGroupStringArr(Item.Groups);
                    string input = Item.Value;
                    for (int i = 0; i < list.Count; i++)
                        list[i].Add(Replace(input, regex, CollectionUtils.GetString(replaceList, i), opt));
                }
                else
                {
                    list[0].Add(Item.Value);
                    //list.Add(Item.Value);
                }

            }

            return list;
        }
        static public List<List<string>> GetMatchList(string str, string regex, bool IgnoreCase, List<string> replaceList)
        {
            RegexOptions opt = RegexOptions.Multiline;
            if (IgnoreCase) opt |= RegexOptions.IgnoreCase;
            return GetMatchList(str, regex, opt, replaceList);
        }
        static public List<string> GetMatchList(string str, string regex, bool IgnoreCase = true, string replace = null)
        {
            List<string> replaceList = new List<string>();
            replaceList.Add(replace);
            List<List<string>> matchlist = GetMatchList(str, regex, IgnoreCase, replaceList);
            return CollectionUtils.Get<List<string>>(matchlist, 0, new List<string>());
        }
        /// <summary>
        /// get regex object
        /// </summary>
        /// <param name="str"></param>
        /// <param name="IgnoreCase"></param>
        /// <param name="NeedCapture"></param>
        /// <param name="RightToLeft"></param>
        /// <returns></returns>
        public static Regex GetRegex(ref string str, bool IgnoreCase, bool NeedCapture = false, bool RightToLeft = false)
        {
            RegexOptions opt = RegexOptions.Multiline;
            if (IgnoreCase) opt |= RegexOptions.IgnoreCase;
            if (RightToLeft) opt |= RegexOptions.RightToLeft;
            return GetRegex(str, opt, NeedCapture);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="pattern"></param>
        /// <param name="replacement">like $1 etc.</param>
        /// <param name="opt"></param>
        /// <returns></returns>
        public static string Replace(string input, string pattern, string replacement, RegexOptions opt = RegexOptions.None)
        {
            if (StringUtils.IsEmptyOrSpace(input)) return "";
            if (StringUtils.IsEmptyOrSpace(pattern)) return input;
            if (replacement == null) return input;
            return Regex.Replace(input, pattern, replacement, opt);
        }
        static public int GetMatchIndexLength(string str, string regex, int nIndex, bool IgnoreCase = true)
        {
            Dictionary<int, Dictionary<string, object>> data = GetMatchPositionData(str, regex, IgnoreCase);
            Dictionary<string, object> attrs = DictionaryUtils.GetValue<int, Dictionary<string, object>>(data, nIndex);
            object obj = DictionaryUtils.GetValue<string, object>(attrs, "Length");
            int nLength = obj != null ? (int)obj : 0;
            return nLength;
        }
        static private Dictionary<int, Dictionary<string, object>> GetMatchPositionData(string str, string regex, bool IgnoreCase = true)
        {
            Dictionary<int, Dictionary<string, object>> dict = new Dictionary<int, Dictionary<string, object>>();
            RegexOptions opt = RegexOptions.Multiline;
            if (IgnoreCase) opt |= RegexOptions.IgnoreCase;
            //Regex rx = GetRegex(regex);
            MatchCollection match = BuildMatch(str, regex, RegexOptions.None);
            if (match == null) return dict;
            foreach (Match m in match)
            {
                if (m.Index >= 0)
                {
                    Dictionary<string, object> attrs = new Dictionary<string, object>();
                    attrs.Add("Length", m.Length);
                    dict.Add(m.Index, attrs);
                }
            }

            return dict;
        }
        private static MatchCollection BuildMatch(string str, string defreg, RegexOptions opt)
        {
            string str1 = defreg;
            if (str == null) str = "";
            if (str1 == null) str1 = "";
            Regex rx = GetRegex(str1, opt);
            if (!Valid(rx, true))
                return null;
            MatchCollection mc = rx.Matches(str);
            return mc;
        }
    }
}

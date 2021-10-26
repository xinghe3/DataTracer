using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CN.MACH.AI.UnitTest.Core.Utils
{
    class StringReplace
    {
        public string str;
        public bool isReplaced = false;
        public StringReplace()
        {
        }
        public StringReplace(string newstr)
        {
            str = newstr;
        }
        /// <summary>
        /// modify replaced attribute.
        /// or split to substring.
        /// </summary>
        /// <param name="rep"></param>
        /// <param name="tar"></param>
        /// <param name="IsRegex"></param>
        /// <param name="IgnoreCase"></param>
        /// <returns></returns>
        public List<string> Replace(string rep, string tar, bool IsRegex = false, bool IgnoreCase = false)
        {
            List<string> list = new List<string>();
            if (isReplaced) return list;
            bool isMatchRegex = false;
            if (IsRegex)
            {
                // regex replace
                isMatchRegex = IgnoreCase ? RegexUtils.MatchExact(str, rep, RegexOptions.IgnoreCase) : RegexUtils.MatchExact(str, rep);
            }
            if (rep == str || // IgnoreCase case.
                (IgnoreCase && rep.ToUpper() == str.ToUpper()))
            {
                str = tar;
                isReplaced = true;
            }
            else if (isMatchRegex)
            {
                str = tar;
                isReplaced = true;
            }
            else // ?
            {
                list = StringUtils.SplitIncludeFlg(str, rep, IsRegex).ToList();
            }
            return list;
        }
    }
    class StringReplaceList : List<StringReplace>
    {
        static public StringReplaceList FromArray(string[] arr)
        {
            if (arr == null) return new StringReplaceList();
            return FromList(arr.ToList());
        }
        static public StringReplaceList FromList(List<string> list)
        {
            StringReplaceList replacelist = new StringReplaceList();
            foreach (string item in list)
            {
                StringReplace sr = new StringReplace();
                sr.str = item;
                replacelist.Add(sr);
            }
            return replacelist;
        }
        public void DeleteItemAndInsert(int nItem, List<string> list)
        {
            if (list == null || list.Count < 2) return;
            this.RemoveAt(nItem);
            foreach (string item in list)
            {
                StringReplace sr = new StringReplace(item);
                this.Insert(nItem++, sr);
            }
        }
        public override string ToString()
        {
            string str = "";
            foreach (StringReplace item in this)
            {
                str += item.str;
            }
            return str;
        }
    }
    class StringReplaceDict : Dictionary<string, List<StringReplaceList>>
    {
        private static object LockReplaceObject = new object();

        public StringReplaceList Pop(string key)
        {
            StringReplaceList srlist = null;
            lock (LockReplaceObject)
            {
                List<StringReplaceList> list = DictionaryUtils.GetValue<string, List<StringReplaceList>>(this, key, null);
                if (list == null) return null;
                srlist = CollectionUtils.Pop<StringReplaceList>(list, null);
                if (srlist == null)
                {
                    this.Remove(key);
                }
            }
            return srlist;
        }
        public int Push(string key, StringReplaceList list)
        {
            int nRet = ErrorCode.Success;
            lock (LockReplaceObject)
            {
                if (DictionaryUtils.ExistKey<string, List<StringReplaceList>>(this, key))
                {
                    this[key].Add(list);
                }
                else
                {
                    List<StringReplaceList> newlist = new List<StringReplaceList>();
                    newlist.Add(list);
                    this.Add(key, newlist);
                }
            }
            return nRet;
        }
    }
    public class StringUtils
    {
        public const int ReplaceBegin = 0;
        public const int ReplaceEnd = 1;
        //static StringReplaceList replaceList = null;
        private static StringReplaceDict replaceDict = new StringReplaceDict();


        public const int NoOperation = 0;
        public const int Trim = 1;

        #region Replace
        public static int Replace(ref string str, string rep, string tar, int Flg = ReplaceEnd, bool IgnoreCase = false, bool IsRegex = false)
        {
            string[] arr = null;
            // set dict of replace
            StringReplaceList replaceList = replaceDict.Pop(str);
            // note: need reset replacelist. when return without operation.
            if (IsEmptyOrSpace(str) || IsEmptyOrSpace(rep))
            {
                BuildReplaceString(replaceList, ref str, Flg);
                return ErrorCode.ErrParams;
            }

            // replace string is same with target string.
            // seems logic is error. if ignorecase is true. then we think two string is not equal.
            if (tar == rep || (!IgnoreCase && ToUpper(tar) == ToUpper(rep)))
            {
                BuildReplaceString(replaceList, ref str, Flg);
                return ErrorCode.Success;
            }


            if (Flg == ReplaceBegin || Flg == ReplaceEnd)
            {
                if (replaceList == null)
                {
                    arr = SplitIncludeFlg(str, rep, IsRegex, IgnoreCase);
                    replaceList = StringReplaceList.FromArray(arr);
                }
            }
            else
            {
                BuildReplaceString(replaceList, ref str, Flg);
                return ErrorCode.ErrParams;
            }
            // 说明：原本的算法需要修改 随着字符串分割 链表长度会增长
            // 修改者：张白玉
            // 修改日期：2015-7-16 11:46:02
            for (int i = 0; i < replaceList.Count; i++)
            {
                List<string> repsec = replaceList[i].Replace(rep, tar, IsRegex, IgnoreCase);
                replaceList.DeleteItemAndInsert(i, repsec);
            }
            BuildReplaceString(replaceList, ref str, Flg);
            return ErrorCode.Success;
        }
        #endregion
        /// <summary>
        /// exist if keys contains in str. not need to equal.
        /// str=123abc456 keys=['abc'] return true
        /// </summary>
        /// <param name="str"></param>
        /// <param name="keys"></param>
        /// <param name="IgnoreCase"></param>
        /// <param name="IsAnd">false = only need one keys contains in str; true = need all keys contains in str.</param>
        /// <returns></returns>
        public static bool Exist(string str, string[] keys, bool IgnoreCase = false, bool IsAnd = false)
        {
            if (keys == null) return false;
            if (IsEmptyOrSpace(str)) return false;
            foreach (string key in keys)
            {
                // if (IsEmptyOrSpace(key)) continue;
                bool bRet = Contains(str, key, IgnoreCase);
                if (!bRet && IsAnd) return false;
                else if (bRet && !IsAnd) return true;
            }
            return IsAnd ? true : false;
        }
        public static bool Contains(string str, string key, bool IgnoreCase = false)
        {
            if (IsEmptyOrSpace(str) || IsEmptyOrSpace(key)) return false;
            if (IgnoreCase)
            {
                if (str.ToUpper().IndexOf(key.ToUpper()) >= 0)
                    return true;
            }
            else
            {
                if (str.IndexOf(key) >= 0)
                    return true;
            }
            return false;
        }
        /// <summary>
        /// exist if tar contains in str. not need to equal.
        /// str='123abc' tar='a' return true
        /// </summary>
        /// <param name="str"></param>
        /// <param name="tar"></param>
        /// <param name="IgnoreCase"></param>
        /// <param name="IsRegex"></param>
        /// <returns></returns>
        public static bool Exist(string str, string tar, bool IgnoreCase = false, bool IsRegex = false)
        {
            List<int> list = GetPositions(str, tar, IgnoreCase, IsRegex);
            if (list != null && list.Count > 0) return true;
            return false;
        }
        public static string[] SplitByLine(string str, int extOperationFlg = NoOperation)
        {
            if (IsEmptyOrSpace(str)) return new string[] { };
            string[] arr = SplitRegex(str, "[\r]*\n", extOperationFlg);
            List<string> list = arr.ToList();
            return list.ToArray();
        }
        /// <summary>
        /// split string include split flg.
        /// if not found splitflg then return empty list.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="splitflg"></param>
        /// <param name="IsRegex">not use now. why.</param>
        /// <param name="IgnoreCase"></param>
        /// <returns></returns>
        public static string[] SplitIncludeFlg(string str, string splitflg, bool IsRegex = false, bool IgnoreCase = false)
        {
            if (IsEmptyOrSpace(str)) return new string[] { };
            string[] arr = !IsRegex ? Split(str, splitflg, NoOperation, IsRegex, IgnoreCase) : SplitRegex(str, splitflg, NoOperation, IgnoreCase);
            if (arr == null) return new string[] { };
            List<string> list = arr.ToList();
            if (!IsRegex)
            {
                for (int i = list.Count - 1; i > 0; i--)
                {
                    list.Insert(i, splitflg);
                }
            }
            else
            {
                // remove empty head and tail arr item.
                //if(StringHelper.IsEmptyOrSpace(ArrayHelper.Get<string>(arr,0,"")))
                //{
                //    arr = ArrayHelper.Remove<string>(arr, 0);
                //}
                //else if (StringHelper.IsEmptyOrSpace(ArrayHelper.GetLast<string>(arr, "")))
                //{
                //    arr = ArrayHelper.Remove<string>(arr, ArrayHelper.Count<string>(arr) - 1);
                //}
                // need to add include flg. in regex.
                // this part use in strHelper.GetCodeMatchRegexString now.
                // old version is delete this part. dont know if has impart.
                // 2015-12-03 23:49:28
                List<string> MatchSplitFlgList = RegexUtils.GetMatchList(str, splitflg);
                int max = list.Count - 1 <= MatchSplitFlgList.Count ? list.Count - 1 : MatchSplitFlgList.Count - 1;
                for (int i = max; i > 0; i--)
                {
                    list.Insert(i, CollectionUtils.GetString(MatchSplitFlgList, i - 1));
                }
            }
            return list.ToArray();
        }
        /// <summary>
        /// compute same sub string. from left or right side.
        /// from right compute is end with error.
        /// </summary>
        /// <param name="str1"></param>
        /// <param name="str2"></param>
        /// <param name="IsLeft"></param>
        /// <param name="IgnoreCase"></param>
        /// <returns></returns>
        public static string SameSubString(string str1, string str2, bool IsLeft = true, bool IgnoreCase = false)
        {
            if (IsEmptyOrSpace(str1) || IsEmptyOrSpace(str2))
                return "";
            if (IgnoreCase)
            {
                str1 = str1.ToUpper();
                str2 = str2.ToUpper();
            }
            string substr = "";
            if (IsLeft)
            {
                int i = 0;
                for (i = 0; i < str1.Length && i < str2.Length; i++)
                {
                    if (str1.ElementAt(i) != str2.ElementAt(i))
                    {
                        substr = Left(str1, i);
                        break;
                    }
                }
                if (i == str1.Length) substr = str1;
                else if (i == str2.Length) substr = str2;
            }
            else
            {
                int i = str1.Length - 1, j = str2.Length - 1;
                for (; i >= 0 && j >= 0; i--, j--)
                {
                    if (str1.ElementAt(i) != str2.ElementAt(j))
                    {
                        substr = Right(str1, str1.Length - i - 1);
                        break;
                    }
                }
                if (i < 0) substr = str1;
                else if (j < 0) substr = str2;
            }
            return substr;
        }
        /// <summary>
        /// split string by regex
        /// if first section or last section is splitflg, then returned array will start or end with "".
        /// for example str="a1" splitflg="[a-z]+" return will be {"","1"}
        /// </summary>
        /// <param name="str">target string</param>
        /// <param name="splitflg">split regex string flag</param>
        /// <param name="extOperationFlg"></param>
        /// <returns></returns>
        private static string[] SplitRegex(string str, string splitflg, int extOperationFlg = NoOperation, bool IgnoreCase = false)
        {
            if (IsEmptyOrSpace(str)) return new string[] { };
            if (extOperationFlg == Trim)
                str = str.Trim();//
            if (IsEmptyOrSpace(splitflg))
            {
                return new string[] { str };
            }
            //if (str.Length < splitflg.Length) return new string[] { str };
            string[] strArr = null;
            if (IsExistVisibleChar(splitflg))
            {
                //splitflg = Regex.Escape(splitflg);
                Regex regex = RegexUtils.GetRegex(ref splitflg, IgnoreCase, false);
                strArr = regex.Split(str);
            }
            if (extOperationFlg == Trim)
            {
                for (int i = 0; i < strArr.Length; i++)
                {
                    strArr[i] = strArr[i].Trim();
                }
            }
            return strArr;
        }
        /// <summary>
        /// split string by another string.
        /// when splitflg longer than str,return str.
        /// splitflg empty return array of str.
        /// 从.NET Framework 2.0 开始，所有捕获的文本是也添加到返回的数组中
        /// so may include match str in return array.
        /// </summary>
        /// <param name="str">目标字符串</param>
        /// <param name="splitflg">分割标志 不区分大小写</param>
        /// <param name="extOperationFlg">其他加工字符串标志 如去空格</param>
        /// <param name="IsRegex">use regex split flg</param>
        /// <returns>array not null.</returns>
        public static string[] Split(string str, string splitflg, int extOperationFlg = NoOperation, bool IsRegex = false, bool IgnoreCase = false)
        {
            if (IsEmptyOrSpace(str)) return new string[] { };
            // 说明：这是搞毛的 外部使用Trim再进行这一步 不然会影响Replace
            // 修改者：张白玉
            // 修改日期：2015-8-17 11:05:04
            if (extOperationFlg == Trim)
                str = str.Trim();
            if (IsEmptyOrSpace(splitflg))
            {
                return new string[] { str };
            }
            // is regex not check.
            if (str.Length < splitflg.Length && !IsRegex) return new string[] { str };
            string[] strArr = null;
            // 说明：增加了正则情况判断 影响未知
            // 修改者：张白玉
            // 修改日期：2015-8-17 0:55:34
            if (IsExistVisibleChar(splitflg) || IsRegex)
            {
                // 说明：使用特定字符串进行分割 不能使用String 的 Split
                // wait for modify 带有特殊字符时正则会使原本的字符串发生转义
                // 为什么会捕获到空元素 还是正则的内容也被分割结果包含 ((\r\n)|(\n)){2,}
                // 修改者：张白玉
                // 修改日期：2015-08-16 10:19:09
                if (!IsRegex)
                    splitflg = RegexUtils.Escape(splitflg);
                // add IgnoreCase
                // 修改者：张白玉
                // 修改日期：2015-9-2 0:09:56
                // add ExplicitCapture
                // 2016-06-01 09:45:41
                RegexOptions ro = RegexOptions.Multiline | RegexOptions.ExplicitCapture;
                if (IgnoreCase)
                    ro |= RegexOptions.IgnoreCase;
                strArr = Regex.Split(str, splitflg, ro);
                //char[] splitflgArr = splitflg.ToArray();
                //strArr = str.Split(splitflgArr);
            }
            // 说明："\\n" 修改 为 "\n"
            // 修改者：张白玉
            // 修改日期：2015-6-3 20:37:03
            else if (splitflg == "\n" || splitflg == "\\n" || splitflg == "\r\n" || splitflg == "\\r\\n")
            {
                strArr = Regex.Split(str, "[\\r\\n]+", RegexOptions.IgnoreCase);
            }
            else
            {
                strArr = Regex.Split(str, "[\\s\\t]+", RegexOptions.IgnoreCase);
            }
            if (extOperationFlg == Trim)
            {
                for (int i = 0; i < strArr.Length; i++)
                {
                    strArr[i] = strArr[i].Trim();
                }
            }
            return strArr;
        }
        public static bool IsExistVisibleChar(string str)
        {
            if (IsEmptyOrSpace(str)) return false;
            Regex VisibleChar = new Regex(@"[^\r\n\t\s\v]+");
            return VisibleChar.IsMatch(str);
        }
        public static string Left(string str, string ch, bool IsIncludeCh, bool SearchFormRight = false, bool IsRegex = false)
        {
            if (str == null) return "";
            // 说明：这里逻辑不正确 没有找到字符也会返回整个字符串
            // 修改者：张白玉
            // 修改日期：2015-8-16 16:41:06
            if (!IsRegex && str.Length < ch.Length) return "";
            //int nlen = str.IndexOf(ch);
            //return Left(str, nlen);
            int n = 0;
            int index = 0;
            if (SearchFormRight)
            {
                index = LastIndexOf(str, ch, str.Length, -1, IsRegex);
            }
            else
            {
                index = IndexOf(str, ch, 0, -1, IsRegex);
            }
            if (IsIncludeCh)
            {
                if (IsRegex)
                {
                    n = RegexUtils.GetMatchIndexLength(str, ch, index, false);
                }
                else n = ch.Length;
            }
            int nlen = index + n;
            return Left(str, nlen);
        }
        public static string Left(string str, int nlength)
        {
            if (str == null || nlength < 0) return "";
            if (str.Length <= System.Math.Abs(nlength)) return str;
            return Substring(str, 0, nlength);
        }
        public static string Right(string str, int nlength)
        {
            if (str == null || nlength < 0) return string.Empty;
            if (str.Length <= System.Math.Abs(nlength)) return str;
            return Substring(str, str.Length - nlength, nlength);
        }
        /// <summary>
        /// substring of str.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="nStartIndex"></param>
        /// <param name="nlength"></param>
        /// <returns></returns>
        public static string Mid(string str, int nStartIndex, int nlength)
        {
            if (str == null || nlength < 0) return "";
            nStartIndex = nStartIndex < 0 ? 0 : nStartIndex;
            if (str.Length <= System.Math.Abs(nStartIndex + nlength))
            {
                nlength = str.Length - nStartIndex;
            }
            if (nlength <= 0) return "";
            return Substring(str, nStartIndex, nlength);
        }
        public static bool IsEmptyOrSpace(string str)
        {
            return str == null || str.Length == 0;
        }

        public static string Substring(string str, int nPos, int nlength = -1)
        {
            if (IsEmptyOrSpace(str)) return string.Empty;
            if (nlength == -1) nlength = str.Length - nPos;

            if (nlength < 0) return string.Empty;
            //if (nPos < 0) nPos = 0;// check pos need test
            //if (nPos >= str.Length) nPos = str.Length - 1;// check pos need test
            char[] ch = new char[nlength];
            int pos = nPos;
            for (int i = 0; i < nlength; i++, pos++)
            {
                if (pos < str.Length && pos >= 0)
                    ch[i] = str[pos];
                else break;
            }
            //StringBuilder sb = new StringBuilder();
            //sb.Append(ch);
            return new string(ch);
        }
        public static string ToString(params object[] inputs)
        {
            string input = "";
            foreach (object str in inputs)
            {
                if (str != null) input += str;
                else input += " NULL";
            }
            return input;
        }
        public static string ToUpper(string str)
        {
            if (str == null) return "";
            return str.ToUpper();
        }
        public static string ToLower(string str)
        {
            if (str == null) return "";
            return str.ToLower();
        }
        private static int BuildReplaceString(StringReplaceList list, ref string str, int Flg)
        {
            if (Flg == ReplaceEnd)
            {
                replaceDict.Pop(str);
                if (!CollectionUtils.IsNullOrEmpty<StringReplace>(list)) str = list.ToString();
                list = null;
            }
            else if (Flg == ReplaceBegin)
            {
                replaceDict.Push(str, list);
            }
            else
            {
                Logs.WriteError("Replace must in ReplaceEnd or ReplaceBegin.");
            }
            return ErrorCode.Success;
        }
        public static List<int> GetPositions(string str, string tar, bool IgnoreCase = false, bool IsRegex = false)
        {
            List<int> poslist = new List<int>();
            if (IsEmptyOrSpace(str) || IsEmptyOrSpace(tar)) return poslist;
            int pos = -1;
            if (IsRegex)
            {
                // regex operation. 2015-08-28 01:30:35
                // move to regexhelper. 2015-12-27 11:40:59
                poslist = RegexUtils.GetMatchPositionList(str, tar, IgnoreCase);
                return poslist;
            }
            else if (IgnoreCase)
            {
                if (str.ToUpper().Length > 0 && tar.ToUpper().Length > 0)
                {
                    str = str.ToUpper();
                    tar = tar.ToUpper();
                }
            }
            do
            {
                pos++;
                if (pos >= str.Length) { pos = -1; break; }
                pos = str.IndexOf(tar, pos);
                if (pos >= 0) poslist.Add(pos);
                else break;
            }
            while (pos >= 0);
            return poslist;
        }
        public static int IndexOf(string str, string tar, int nStart = 0, int nDef = -1, bool IsRegex = false)
        {
            if (IsEmptyOrSpace(str) || IsEmptyOrSpace(tar)) return nDef;
            // if start is length then no need to find.
            // 2017-04-21 10:11:13
            if (nStart < 0) nStart = 0;
            else if (nStart > str.Length - 1) return nDef;
            int nIndex = nDef;
            if (!IsRegex)
                nIndex = str.IndexOf(tar, nStart);
            else // regex
            {
                List<int> PosList = GetPositions(str, tar, false, IsRegex);
                foreach (int Pos in PosList)
                {
                    if (Pos >= nStart && (Pos < nIndex || nIndex == nDef))
                    {
                        nIndex = Pos;
                        break;
                    }
                }
            }
            if (nIndex < 0) nIndex = nDef;
            return nIndex;
        }
        /// <summary>
        /// modify. nStart define.
        /// if not find return will be nDef.
        /// 2016-04-28 22:46:24
        /// </summary>
        /// <param name="str"></param>
        /// <param name="tar"></param>
        /// <param name="nStart"></param>
        /// <param name="nDef"></param>
        /// <returns></returns>
        public static int LastIndexOf(string str, string tar, int nStart, int nDef = -1, bool IsRegex = false)
        {
            if (IsEmptyOrSpace(str) || IsEmptyOrSpace(tar)) return nDef;
            // 2016-01-10 10:57:01
            // if start is 0 then no need to find.
            // 2017-04-21 10:11:13
            if (nStart < 0) return nDef;
            else if (nStart > str.Length - 1) nStart = str.Length;
            int nIndex = nDef;
            if (!IsRegex)
                nIndex = str.LastIndexOf(tar, nStart);
            else // regex
            {
                List<int> PosList = GetPositions(str, tar, false, IsRegex);
                foreach (int Pos in PosList)
                {
                    if (Pos <= nStart && (Pos > nIndex || nIndex == nDef))
                    {
                        nIndex = Pos;
                    }
                }
            }
            if (nIndex < 0) nIndex = nDef;
            return nIndex;
        }
    }
}

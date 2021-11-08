using CN.MACH.AI.UnitTest.Core.Utils;
using CN.MACH.AOP.Fody.Comparers;
using System.Collections.Generic;
using System.Text;

namespace CN.MACH.AOP.Fody.Utils
{
    public class WordSplitUtils
    {

        static HashSet<string> hs = new HashSet<string>();
        static StringBuilder sb = new StringBuilder();
        public static HashSet<string> SplitRegex(string data)
        {
            hs.Clear();
            sb.Clear();
            foreach (char ch in data)
            {
                switch(ch)
                {
                    case '<':
                    case '>':
                    case '(':
                    case ')':
                    case ':':
                    case ';':
                    case ' ':
                    case '{':
                    case '}':
                    case '[':
                    case ']':
                    case ',':
                    case '=':
                    case '_':
                    case '.':
                    case '"':
                    case '-':
                        if (sb.Length <= 1) continue;
                        string val = string.Intern(sb.ToString());
                        if (!hs.Contains(val)) hs.Add(val);
                        sb.Clear();
                        break;
                    default:
                        sb.Append(ch);
                        break;
                }
            }
            //string[] elements = StringUtils.Split(data, @"[<>():;\s\{\}\[\],{}=_\.""]", StringUtils.Trim, true);
            //foreach (string element in elements)
            //{
            //    if (string.IsNullOrEmpty(element)) continue;
            //    string val = element.Trim();
            //    if (!hs.Contains(val)) hs.Add(val);
            //}
            return hs;
        }
        public static HashSet<byte[]> SplitRegexBytes(string data)
        {
            HashSet<byte[]> hs = new HashSet<byte[]>(new ByteArrayComparer());
            StringBuilder sb = new StringBuilder();
            foreach (char ch in data)
            {
                switch (ch)
                {
                    case '<':
                    case '>':
                    case '(':
                    case ')':
                    case ':':
                    case ';':
                    case ' ':
                    case '{':
                    case '}':
                    case '[':
                    case ']':
                    case ',':
                    case '=':
                    case '_':
                    case '.':
                    case '"':
                    case '-':
                        if (sb.Length <= 1) continue;
                        byte[] bytes = Encoding.UTF8.GetBytes(sb.ToString());
                        if (!hs.Contains(bytes)) 
                            hs.Add(bytes);
                        sb.Clear();
                        break;
                    default:
                        sb.Append(ch);
                        break;
                }
            }
            //string[] elements = StringUtils.Split(data, @"[<>():;\s\{\}\[\],{}=_\.""]", StringUtils.Trim, true);
            //foreach (string element in elements)
            //{
            //    if (string.IsNullOrEmpty(element)) continue;
            //    string val = element.Trim();
            //    if (!hs.Contains(val)) hs.Add(val);
            //}
            return hs;
        }
        /// <summary>
        /// 分割json
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static HashSet<string> SplitJson(string json)
        {
            JsonUtils jsonHelper = new JsonUtils();
            jsonHelper.NInit(json);
            string jsonFormatStr = jsonHelper.Output();
            string[] lines = jsonFormatStr.Split('\n');
            HashSet<string> hsDatasInJson = new HashSet<string>();
            foreach (string line in lines)
            {
                if (!line.Contains(":"))
                    continue;
                string[] elements = StringUtils.Split(line, @"[:\{\}\[\],]", StringUtils.Trim, true);
                foreach (string element in elements)
                {
                    if (string.IsNullOrEmpty(element)) continue;
                    string val = JsonUtils.RemoveStrQuotes(element);
                    if (string.IsNullOrEmpty(val)) continue;
                    if(!hsDatasInJson.Contains(val)) hsDatasInJson.Add(val);
                }
            }
            return hsDatasInJson;
        }

        public static HashSet<string> SplitChars(string str)
        {
            HashSet<string> hs = new HashSet<string>();
            if (string.IsNullOrEmpty(str)) return hs;
            for (int i = 0; i < str.Length; i++)
            {
                for (int j = i + 1; j <= str.Length; j++)
                {
                    if (j == str.Length && i == 0) continue;
                    string val = StringUtils.Substring(str, i, j - i);
                    if (!hs.Contains(val)) hs.Add(val);
                }
            }
            return hs;
        }
    }
}

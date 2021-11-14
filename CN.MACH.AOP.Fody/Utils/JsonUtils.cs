using CN.MACH.AI.UnitTest.Core.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CN.MACH.AOP.Fody.Utils
{
    public class JsonUtils
    {
        private JArray _jArray = null;
        /// <summary>
        /// root object of json
        /// </summary>
        private JObject _jObject = null;
        /// <summary>
        /// 将字典类型序列化为json字符串
        /// </summary>
        /// <typeparam name="TKey">字典key</typeparam>
        /// <typeparam name="TValue">字典value</typeparam>
        /// <param name="dict">要序列化的字典数据</param>
        /// <returns>json字符串</returns>
        public static string Serialize<TKey, TValue>(Dictionary<TKey, TValue> dict)
        {
            if (dict == null || dict.Count == 0)
                return "";

            string jsonStr = JsonConvert.SerializeObject(dict);
            return jsonStr;
        }

        /// <summary>
        /// 将json字符串反序列化为字典类型
        /// </summary>
        /// <typeparam name="TKey">字典key</typeparam>
        /// <typeparam name="TValue">字典value</typeparam>
        /// <param name="jsonStr">json字符串</param>
        /// <returns>字典数据</returns>
        public static Dictionary<TKey, TValue> Deserialize<TKey, TValue>(string jsonStr)
        {
            if (string.IsNullOrEmpty(jsonStr))
                return new Dictionary<TKey, TValue>();

            Dictionary<TKey, TValue> jsonDict = JsonConvert.DeserializeObject<Dictionary<TKey, TValue>>(jsonStr);

            return jsonDict;

        }
        /// <summary>
        /// init a json str to jobject. so we can get value or set value.
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public int NInit(string json)
        {
            if (IsJsonObject(json))
            {
                _jObject = ToJObject(json);
            }
            else if (IsJsonArray(json))
            {
                _jArray = ToJArray(json);
            }
            else
            {
                Logs.WriteError("错误JSON格式?", json);
            }
            return ErrorCode.Success;
        }
        private static bool IsJsonObject(string jsonText)
        {
            if (string.IsNullOrWhiteSpace(jsonText)) return false;
            jsonText = jsonText.Trim();
            if (jsonText.StartsWith("{") && jsonText.EndsWith("}"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// check if is json
        /// </summary>
        /// <param name="jsonText"></param>
        /// <returns></returns>
        private static bool IsJsonArray(string jsonText)
        {
            if (string.IsNullOrWhiteSpace(jsonText)) return false;
            jsonText = jsonText.Trim();
            if (jsonText.StartsWith("[") && jsonText.EndsWith("]"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private static JObject ToJObject(string json)
        {
            try
            {
                if (string.IsNullOrEmpty(json)) json = "{}";
                return JObject.Parse(json);
            }
            catch (Exception ex)
            {
                Logs.WriteExLog(ex);
            }
            return null;
        }
        private static JArray ToJArray(string json)
        {
            try
            {
                if (string.IsNullOrEmpty(json)) json = "[]";
                return JArray.Parse(json);
            }
            catch (Exception ex)
            {
                Logs.WriteExLog(ex);
            }
            return null;
        }
        public string Output(string path = null)
        {
            if (_jObject == null && _jArray == null) return string.Empty;
            string name = string.Empty;
            if (_jObject != null)
            {
                if (string.IsNullOrEmpty(path))
                    return _jObject.ToString();
                try
                {
                    name = (string)_jObject.SelectToken(path);
                }
                catch (Exception ex)
                {
                    Logs.WriteExLog(ex);
                }
            }
            else if (_jArray != null)
            {
                if (string.IsNullOrEmpty(path))
                    return _jArray.ToString();
                throw new NotImplementedException("未测试实现数组 路径查询");
            }
            return name;
        }
        public static string Serialize(object obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }

            return JsonConvert.SerializeObject(obj, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                             // MaxDepth = 5,
                        });
        }

        public static string Serialize(object obj, JsonSerializerSettings settings)
        {
            if (obj == null)
            {
                return string.Empty;
            }

            return JsonConvert.SerializeObject(obj, settings);
        }
        /// <summary>
        /// remove str quote of two side.
        /// not change special chars
        /// </summary>
        /// <param name="TxtCode"></param>
        /// <returns></returns>
        static public string RemoveStrQuotes(string quoteStr)
        {
            if (!StringUtils.IsExistVisibleChar(quoteStr))
                return quoteStr;
            string res = quoteStr.Trim();
            if (res.First() != '"' || res.Last() != '"')
                return quoteStr;

            return StringUtils.Substring(res, 1, res.Length - 2);
        }

        public static T Deserialize<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
                return default(T);
            try
            {
                var jSetting = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Error = (obj, args2) =>
                    {
                        args2.ErrorContext.Handled = true;
                    },

                };
                return JsonConvert.DeserializeObject<T>(json, jSetting);
            }
            catch (Exception ex)
            {
                var c = ex;
                return default(T);
            }
        }
    }
}

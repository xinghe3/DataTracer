using CN.MACH.AI.UI.Controls.Models;
using CN.MACH.AI.UnitTest.Core.Utils;
using CN.MACH.AOP.Fody.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace CN.MACH.AI.UI.Controls
{
    internal enum JsonType
    {
        Object,
        Array,
        Value,
        Property,
        Unknown
    }
    internal class JsonValue : NotifyPropertyBase
    {
        //用于对应Json对象类型的格式化字符
        private const string NULL_TEXT = "<null>";
        private const string PROPERTY = "{0}";
        //用于界面绑定的属性定义
        private DataTable data;

        public DataTable Datas { get; private set; }
        public string Header { get; private set; }

        public List<JsonValue> Children { get; private set; }
        public JsonType Type { get; private set; }
        public JToken Token { get; private set; }

        JsonValue(JToken token, JsonType Type, List<JsonValue> children, string header)
        {
            Token = token;
            JArray jArray = GetArray(token, Type);
            Datas = JsonUtils.ToDataTable(jArray);
            Header = header;
            this.Type = Type;
            Children = children;
        }

        private static JArray GetArray(JToken token, JsonType Type)
        {
            JArray jArray = null;
            if (Type == JsonType.Array)
            {
                jArray = token as JArray;
            }
            else if (Type == JsonType.Object)
            {
                jArray = new JArray();
                jArray.Add(token);
            }
            return jArray;
        }

        //外部的从JToken创建JsonHeaderLogic的方法

        public static JsonValue FromJToken(JToken jtoken)
        {
            if (jtoken == null)
            {
                throw new ArgumentNullException("jtoken");
            }
            var type = jtoken.GetType();
            if (typeof(JValue).IsAssignableFrom(type))
            {
                var jvalue = (JValue)jtoken;
                var value = jvalue.Value;
                if (value == null)
                    value = NULL_TEXT;
                return null;
            }
            if (!typeof(JContainer).IsAssignableFrom(type))
            {
                throw new Exception("不支持的JToken类型");
            }
            var jcontainer = jtoken as JContainer;
            var children = jcontainer.Children().Select(c => FromJToken(c)).Where(c => c != null).ToList();
            JsonType jsonType = JsonType.Unknown;
            string header = string.Empty;
            if (typeof(JProperty).IsAssignableFrom(type))
            {
                header = String.Format(PROPERTY, ((JProperty)jcontainer).Name);
                jsonType = JsonType.Property;
            }
            else if (typeof(JArray).IsAssignableFrom(type))
                jsonType = JsonType.Array;
            else if (typeof(JObject).IsAssignableFrom(type))
            {
                jsonType = JsonType.Object;
            }
            else
                throw new Exception("不支持的JContainer类型");

            // 特殊处理 修改层级关系 主要是删除没有必要显示的层级，减少显示复杂性
            if (!CollectionUtils.IsNullOrEmpty(children))
            {
                JsonValue childValue = CollectionUtils.Get<JsonValue>(children, 0, null);
                // 如果子级是单个值 则将子集删除
                if (childValue != null && childValue.Type == JsonType.Value && children.Count == 1)
                {
                    children = null;
                }
                // 属性 值为对象或数组- 则将子级的数据和子级列表提升到本级
                else if (childValue != null && childValue.Datas != null
                    //&&
                    //(
                    //    CollectionUtils.IsNullOrEmpty(childValue.Children) || // 没有子集的对象
                    //    childValue.Children.All(c => (c.Type == JsonType.Property || c.Type == JsonType.Value) && CollectionUtils.IsNullOrEmpty(c.Children) && c.Datas == null) // 值 数组
                    //)
                    && jsonType == JsonType.Property && children.Count == 1)
                {
                    JContainer cnr = childValue.Token as JContainer;
                    if (cnr != null)
                    {
                        jcontainer = cnr;
                        jsonType = childValue.Type;
                        children = childValue.Children;
                    }
                    else
                    {
                        throw new NotImplementedException("未实现");
                    }
                    // children = null;
                }
                // 子级属性 为空 -- 说明父级表格已包含所有值信息 - 删除符合条件的子级
                // 如果是对象 则子级类型为属性 如果为值数组 则子级类型为Value
                if (children != null)
                {
                    int ret = children.RemoveAll(c => (c.Type == JsonType.Property || c.Type == JsonType.Value) && CollectionUtils.IsNullOrEmpty(c.Children) && c.Datas == null);
                    if (ret > 0)
                    {
                        if (CollectionUtils.IsNullOrEmpty(children))
                            children = null;
                    }
                }
            }

            return new JsonValue(jcontainer, jsonType, children, header);
        }

    }
}
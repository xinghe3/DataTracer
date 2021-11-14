using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfBlendTest1
{
    public class FuncModel
    {
        public FuncModel()
        {
            // 在此点下面插入创建对象所需的代码。
        }

        public string Name { get; set; }
        public List<FuncParamModel> Params { get; set; }
        public bool IsStatic { get; set; }
    }

    public class FuncParamModel
    {
        public FuncParamModel()
        {
        }

        public string Type { get; set; }
        /// <summary>
        /// 判断数据类型 目前想到的有 单值 Single 列表 List 对象 Json
        /// </summary>
        public DisplayType DisplayType { get; set; }
        public string Value { get; set; }

    }
    public enum DisplayType
    {
        Single = 1,
        List = 2,
        Json = 3
    }
}
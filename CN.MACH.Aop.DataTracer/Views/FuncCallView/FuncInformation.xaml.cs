using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfBlendTest1.FuncCallView
{
    /// <summary>
    /// 此控件显示函数调用信息 包括 函数名 是否静态 函数输入参数等信息，
    /// 点击函数输入参数可以展开显示对象数据，可以显示是否
    /// 在参数对象中存在外界搜索的关键字 区别显示其所在字段
    /// </summary>
    public partial class FuncInformation : UserControl
    {
        public FuncInformation()
        {
            InitializeComponent();
        }
    }
}

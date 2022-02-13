using CN.MACH.AOP.Fody.Utils;
using Microsoft.Win32;
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

namespace CN.MACH.Aop.DataTracer.Views.JsonData
{
    /// <summary>
    /// JsonFileView.xaml 的交互逻辑
    /// </summary>
    public partial class JsonFileView : UserControl
    {


        public string JsonStr
        {
            get { return (string)GetValue(JsonStrProperty); }
            set { SetValue(JsonStrProperty, value); }
        }

        // Using a DependencyProperty as the backing store for JsonStr.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty JsonStrProperty =
            DependencyProperty.Register("JsonStr", typeof(string), typeof(JsonFileView), new PropertyMetadata(string.Empty));


        public JsonFileView()
        {
            InitializeComponent();
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            //创建一个打开文件式的对话框
            OpenFileDialog ofd = new OpenFileDialog();
            //设置这个对话框的起始打开路径
            ofd.InitialDirectory = @"D:\";
            //设置打开的文件的类型，注意过滤器的语法
            ofd.Filter = "Json文本|*.json";
            //调用ShowDialog()方法显示该对话框，该方法的返回值代表用户是否点击了确定按钮
            if (ofd.ShowDialog() == true)
            {
                JsonStr = FileUtils.Read(ofd.FileName,Encoding.UTF8);
            }
            else
            {
                MessageBox.Show("没有选择JSON数据");
            }
        }
    }
}

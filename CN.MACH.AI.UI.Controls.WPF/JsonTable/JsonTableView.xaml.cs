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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CN.MACH.AI.UI.Controls
{
	/// <summary>
	/// JsonTableView.xaml 的交互逻辑
	/// </summary>
	public partial class JsonTableView : UserControl
	{

        /// <summary>
        /// 查询关键字 高亮显示 展开
        /// </summary>
        public string Keyword
        {
            get { return (string)GetValue(KeywordProperty); }
            set { SetValue(KeywordProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Keyword.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty KeywordProperty =
            DependencyProperty.Register("Keyword", typeof(string), typeof(JsonTableView), new PropertyMetadata(null));


        /// <summary>
        /// json source
        /// </summary>
        public string JsonString
        {
            get 
            {
                return (string)GetValue(JsonStringProperty); 
            }
            set 
            {
                // 绑定不会触发set
                SetValue(JsonStringProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for JsonString.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty JsonStringProperty =
            DependencyProperty.Register("JsonString", typeof(string), typeof(JsonTableView), new PropertyMetadata(string.Empty));

		
		public JsonTableView()
		{
			this.InitializeComponent();
        }
	}
}
using System;
using System.Collections.Generic;
using System.Data;
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

namespace CN.MACH.AI.UI.Controls.WPF.ListViews
{
    /// <summary>
    /// DataTableListView.xaml 的交互逻辑
    /// </summary>
    public partial class DataTableListView : UserControl
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
            DependencyProperty.Register("Keyword", typeof(string), typeof(DataTableListView), new PropertyMetadata(null, KeywordPropertyChangedCallback));
        private static void KeywordPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DataTableListView dtlv = d as DataTableListView;
            ListView lv = dtlv.listView;
            GridView gv = lv.View as GridView;
            
        }

        public DataTable DataSource
        {
            get { return (DataTable)GetValue(DataSourceProperty); }
            set 
            {
                SetValue(DataSourceProperty, value);
                GridView gv = GetView(value, this);
                listView.View = gv;
                listView.ItemsSource = value.DefaultView;
            }
        }

        // Using a DependencyProperty as the backing store for DataSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataSourceProperty =
            DependencyProperty.Register("DataSource", typeof(DataTable), typeof(DataTableListView), new PropertyMetadata(null, DataSourcePropertyChangedCallback));

        private static void DataSourcePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DataTableListView dtlv = d as DataTableListView;
            ListView lv = dtlv.listView;
            GridView gv = GetView(dtlv.DataSource, dtlv);
            lv.View = gv;
            lv.ItemsSource = dtlv.DataSource.DefaultView;
        }

        private static GridView GetView(DataTable dt, DataTableListView dtlv)
        {
            GridView gv = new GridView();
            if (dt.Columns.Count > 0)
            {
                foreach (DataColumn dc in dt.Columns)
                {
                    GridViewColumn column = new GridViewColumn();
                    GridViewColumnHeader h = new GridViewColumnHeader();
                    h.Content = dc.ColumnName;
                    column.Header = h;
                    // 自定义 单元格 无法使用  DisplayMemberBinding 绑定路径
                    Binding binding = new Binding();
                    binding.Path = new PropertyPath(dc.ColumnName);
                    column.DisplayMemberBinding = binding;
                    // 根据keyword 决定单元格颜色
                    //DataTemplate cellDataTemplate = CreateCellTemplate(dc.ColumnName);
                    //column.CellTemplate = cellDataTemplate;
                    // 限制单列宽度 显示更多列数据
                    column.Width = 50;
                    gv.Columns.Add(column);
                }
                //listView.View = gv;
                //listView.ItemsSource = value.DefaultView;
            }
            return gv;
        }

        private static DataTemplate CreateCellTemplate(string column)
        {
            DataTemplate template = new DataTemplate();
            FrameworkElementFactory factoryText =
                                new FrameworkElementFactory(typeof(TextBlock));
            factoryText.SetValue(TextBlock.TextProperty, new Binding(column));
            var bc = new BrushConverter();
            factoryText.SetValue(TextBlock.BackgroundProperty, (Brush)bc.ConvertFrom("#FF3D3D3D"));
            template.VisualTree = factoryText;
            return template;
        }

        public DataTableListView()
        {
            InitializeComponent();
        }
    }
}

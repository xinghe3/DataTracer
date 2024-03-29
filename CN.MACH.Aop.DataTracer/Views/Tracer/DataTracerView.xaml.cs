﻿using CN.MACH.AOP.Fody.Index;
using DC.ETL.Infrastructure.Cache;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace CN.MACH.Aop.DataTracer.Views.Tracer
{
    /// <summary>
    /// DataTracerView.xaml 的交互逻辑
    /// </summary>
    public partial class DataTracerView : UserControl
    {
        private readonly ICacheProvider cacheProvider = null;
        private ObservableCollection<RecordInfo> records = new ObservableCollection<RecordInfo>();

        public ObservableCollection<RecordInfo> Records
        {
            get { return records; }
            set { records = value; }
        }
        // 但他本身就是语句 再查什么细节呢??? 先不管 看看需不需要展开表操作 还需要各个代码路径



        public DataTracerView()
        {
            InitializeComponent();
            DataContext = this;
            cacheProvider = FodyCacheManager.GetInterface();
            IndexGenerator indexGenerator = new IndexGenerator(cacheProvider);
            indexGenerator.ProcessNotice = IndexGenerateProcessDisplay;
            Task.Run(() =>
            {
                indexGenerator.Build();
            });
        }

        private void IndexGenerateProcessDisplay(int n, long max)
        {
            Dispatcher.Invoke(() =>
            {
                // Title = string.Format("{0}/{1}", n, max);
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Search()
        {
            IndexSearcher indexSearcher = new IndexSearcher(cacheProvider);
            List<RecordInfo> recordInfos = indexSearcher.Search(searchKeyWords.Text);
            Records.Clear();
            if (recordInfos == null || recordInfos.Count <= 0)
            {
                MessageBox.Show("no result.");
                return;
            }
            foreach (var item in recordInfos)
            {
                Records.Add(item);
            }
        }

        private void searchKeyWords_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Search();
            }
        }

        private void SearchResultGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            RecordInfo mySelectedElement = (RecordInfo)SearchResultGrid.SelectedItem;
            int result = mySelectedElement.ID;
            FuncCallDetailsView funcCallDetailsView = new FuncCallDetailsView();
            funcCallDetailsView.ShowFuncCallDetails(result);
        }
    }
}

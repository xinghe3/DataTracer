using CN.MACH.Aop.DataTracer.Models;
using CN.MACH.AOP.Fody.Index;
using DC.ETL.Infrastructure.Cache;
using DC.ETL.Infrastructure.Cache.Redis;
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

namespace CN.MACH.Aop.DataTracer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ICacheProvider cacheProvider = new RedisCacheProvider(
            new CN.MACH.AI.Cache.CacheSetting()
            {
                Connection = "127.0.0.1",
                Port = 6379,
                PefixKey = "zbytest:"
            }
        );
        private ObservableCollection<Record> records = new ObservableCollection<Record>();

        public ObservableCollection<Record> Records
        {
            get { return records; }
            set { records = value; }
        }
        // 但他本身就是语句 再查什么细节呢??? 先不管 看看需不需要展开表操作 还需要各个代码路径
        private ObservableCollection<Record> recordDetails;

        public ObservableCollection<Record> RecordDetails
        {
            get { return recordDetails; }
            set { recordDetails = value; }
        }


        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            IndexGenerator indexGenerator = new IndexGenerator(cacheProvider);
            indexGenerator.Build();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            IndexSearcher indexSearcher = new IndexSearcher(cacheProvider);
            List<RecordInfo> recordInfos = indexSearcher.Search(searchKeyWords.Text);
            Records.Clear();
            if (recordInfos == null) return;
            foreach (RecordInfo recordInfo in recordInfos.OrderBy(r=>r.ID))
            {
                Records.Add(new Record()
                {
                     ID = recordInfo.ID, ThreadID = recordInfo.ThreadID, Txt = recordInfo.Txt
                });
            }
        }
    }
}

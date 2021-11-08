using CN.MACH.AOP.Fody.Index;
using DC.ETL.Infrastructure.Cache;
using DC.ETL.Infrastructure.Cache.Redis;
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
using System.Windows.Shapes;

namespace CN.MACH.Aop.DataTracer.Views
{
    /// <summary>
    /// SettingsView.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsView : Window
    {
        private readonly ICacheProvider cacheProvider = new RedisCacheProvider(
            new CN.MACH.AI.Cache.CacheSetting()
            {
                Connection = "127.0.0.1",
                Port = 6379,
                PefixKey = "zbytest:"
            }
        );
        private IndexSettings indexSettings;
        public SettingsView()
        {
            InitializeComponent();
            indexSettings = new IndexSettings(cacheProvider);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            indexSettings.indexOptions.IsRecord = true;
            indexSettings.Update();
        }

        private void CheckBox_Unloaded(object sender, RoutedEventArgs e)
        {
            indexSettings.indexOptions.IsRecord = true;
            indexSettings.Update();
        }
    }
}

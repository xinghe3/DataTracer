using CN.MACH.AI.Cache;
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
        private ICacheProvider cacheProvider = null;
        private IndexSettings indexSettings;





        public CacheSetting CacheSettings
        {
            get { return (CacheSetting)GetValue(CacheSettingsProperty); }
            set { SetValue(CacheSettingsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CacheSettings.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CacheSettingsProperty =
            DependencyProperty.Register("CacheSettings", typeof(CacheSetting), typeof(SettingsView), new PropertyMetadata(null));




        public SettingsView()
        {
            InitializeComponent();
            DataContext = this;
            CacheSettings  = new CacheSetting()
            {
                Connection = "127.0.0.1",
                Port = 6379,
                PefixKey = "zbytest:"
            };

            cacheProvider = FodyCacheManager.GetInterface(CacheSettings);
            indexSettings = new IndexSettings(cacheProvider);
            indexSettings.CacheSetting = CacheSettings;
            FodyCacheManager.Setting(indexSettings);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            indexSettings.indexOptions.IsRecord = true;
            indexSettings.Update();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            indexSettings.indexOptions.IsRecord = false;
            indexSettings.Update();
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            CacheSettings.IsChangeToNewServer = true;
            cacheProvider = FodyCacheManager.GetInterface(CacheSettings);
            MessageBox.Show("应用更改");
        }
    }
}

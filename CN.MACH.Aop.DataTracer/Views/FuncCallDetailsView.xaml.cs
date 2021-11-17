using CN.MACH.AOP.Fody.Index;
using CN.MACH.AOP.Fody.Utils;
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
using System.Windows.Shapes;
using WpfBlendTest1;

namespace CN.MACH.Aop.DataTracer.Views
{
    /// <summary>
    /// FuncCallDetailsView.xaml 的交互逻辑
    /// </summary>
    public partial class FuncCallDetailsView : Window
    {
        private readonly ICacheProvider cacheProvider = FodyCacheManager.GetInterface();

        public ObservableCollection<FuncModel> Funcs { get; set; }

        public FuncCallDetailsView()
        {
            InitializeComponent();
            Funcs = new ObservableCollection<FuncModel>();
        }

        public void ShowFuncCallDetails(int id)
        {
            IndexSearcher indexSearcher = new IndexSearcher(cacheProvider);
            List<RecordDetailInfo> recordInfos = indexSearcher.Search(id - 10, id);
            Funcs.Clear();
            funcList.DataContext = this;
            if (recordInfos == null || recordInfos.Count <= 0)
            {
                MessageBox.Show("no result.");
                return;
            }
            foreach (RecordDetailInfo item in recordInfos)
            {
                FuncModel funcModel = new FuncModel();
                if(!string.IsNullOrEmpty(item.PropertyName))
                {
                    funcModel.Name = item.InstanceName + "." + item.PropertyName;
                }
                else
                {
                    funcModel.Name = item.InstanceName + "." + item.MethodName;
                }
                funcModel.Params = new List<FuncParamModel>();
                foreach (var param in item.Params)
                {
                    FuncParamModel funcParamModel = new FuncParamModel();
                    funcParamModel.Type = param.Type;
                    funcParamModel.DisplayType = GetDisplayType(param.Type, param.Value);
                    funcParamModel.Value = param.Value;
                    funcModel.Params.Add(funcParamModel);
                }
                Funcs.Add(funcModel);
            }
            ShowDialog();
        }

        private DisplayType GetDisplayType(string type, string value)
        {
            DisplayType displayType = DisplayType.Single;
            if (JsonUtils.IsJson(value))
                displayType = DisplayType.Json;
            return displayType;
        }
    }
}

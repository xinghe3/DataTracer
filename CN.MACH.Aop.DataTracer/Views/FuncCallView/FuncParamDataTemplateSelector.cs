using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WpfBlendTest1.FuncCallView
{
    public class FuncParamDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;
            FuncParamModel taskitem = item as FuncParamModel;
            if (element != null && taskitem != null)
            {
                if (taskitem.DisplayType == DisplayType.Single)
                    return element.FindResource("singleParamDataTemplate") as DataTemplate;
                else if (taskitem.DisplayType == DisplayType.List)
                    return element.FindResource("listParamDataTemplate") as DataTemplate;
                else
                    return element.FindResource("jsonParamDataTemplate") as DataTemplate;
            }
            return null;
        }
    }
}

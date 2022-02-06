using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace CN.MACH.AI.UI.Controls
{
    public class JsonTableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string str = value as string;
            if (string.IsNullOrEmpty(str))
                return DependencyProperty.UnsetValue;
            //创建JObject
            JToken jobj = JToken.Parse(value as string);
            JArray jArray = new JArray();
            jArray.Add(jobj);
            IEnumerable<JsonValue> jsonValues = jArray.Children().Select(c => JsonValue.FromJToken(c));
            return jsonValues;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

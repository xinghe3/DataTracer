using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CN.MACH.AI.UI.Controls.Models
{
    public class NotifyPropertyBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;


        protected void SetValue(string name, object value)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged.Invoke(value, new PropertyChangedEventArgs(name));
            }
        }


    }
}

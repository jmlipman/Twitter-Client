using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Twitter2.Common
{
    public class DateConverter : DependencyObject, IValueConverter
    {
        public string whatever { get; set; }

        public void setear(string valor)
        {
            this.whatever = valor;
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            DateTime time = ((DateTime)value);
            string timetext;

            if ((DateTime.Now.ToUniversalTime() - time).Days > 3)
                timetext = time.ToString();
            else if ((DateTime.Now.ToUniversalTime() - time).Days > 1)
                timetext = (DateTime.Now.ToUniversalTime() - time).Days + " d";
            else if ((DateTime.Now.ToUniversalTime() - time).Days == 1)
                timetext = "1 d";
            else if ((DateTime.Now.ToUniversalTime() - time).Hours >= 1)
                timetext = (DateTime.Now.ToUniversalTime() - time).Hours + " h";
            else if ((DateTime.Now.ToUniversalTime() - time).Minutes >= 1)
                timetext = (DateTime.Now.ToUniversalTime() - time).Minutes + " min";
            else
                timetext = (DateTime.Now.ToUniversalTime() - time).Seconds + " sec";
            
            //Debug.WriteLine((DateTime.Now.ToUniversalTime() - time).Seconds.ToString());
            return timetext;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

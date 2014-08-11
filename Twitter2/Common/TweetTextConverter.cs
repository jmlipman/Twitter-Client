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
    public class TweetTextConverter : DependencyObject, IValueConverter
    {
        public string whatever { get; set; }

        public void setear(string valor)
        {
            this.whatever = valor;
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            //IMPLEMENTAR EXPRESIONES REGULARES PARA CAPTURAR HASTAGS Y SCREENNAMES
            return (string) value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

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
    public class BackgroundConverter : DependencyObject, IValueConverter
    {
        public string whatever { get; set; }

        public void setear(string valor)
        {
            this.whatever = valor;
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            //Esto seria para cuando quiera empezar a marcar los vistos y no vistos.
            //return (bool)value ? "Transparent" : "#D8D8D8";
            return "Transparent";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}

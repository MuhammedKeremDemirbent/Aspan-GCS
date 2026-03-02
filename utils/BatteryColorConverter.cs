using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Aspan_Ground.utils
{
    public class BatteryColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                int batteryPercentage = System.Convert.ToInt32(value);

                if (!Aspan_Ground.cardconnection.SerialManager.Instance.IsConnected || 
                    !Aspan_Ground.cardconnection.SerialManager.Instance.IsSynchronized)
                {
                    return new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#8B949E")); // Gri (Bağlı Değil)
                }

                // Bağlıyken gelen gerçek veri renkleri
                if (batteryPercentage >= 60)
                {
                    return new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#3FB950")); // Yeşil (%60 - %100)
                }
                else if (batteryPercentage >= 20)
                {
                    return new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#E3B341")); // Sarı (%20 - %59)
                }
                else
                {
                    return new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F85149")); // Kırmızı (%0 - %19)
                }
            }
            
            // Varsayılan Gri Döndür (Hata Durumunda)
            return new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#8B949E"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

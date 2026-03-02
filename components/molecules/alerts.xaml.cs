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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Aspan_Ground.components.molecules
{
    /// <summary>
    /// alerts.xaml etkileşim mantığı
    /// </summary>
    public partial class alerts : UserControl
    {
        public event RoutedEventHandler OnYesClicked;
        public event RoutedEventHandler OnNoClicked;
        public event RoutedEventHandler OnOkClicked;

        public alerts()
        {
            InitializeComponent();
        }

        public void ShowAlert(string title, string message, bool isError = false)
        {
            AlertTitle.Text = title;
            AlertMessage.Text = message;
            
            if (isError)
            {
                BtnYes.Visibility = Visibility.Collapsed;
                BtnNo.Visibility = Visibility.Collapsed;
                BtnOk.Visibility = Visibility.Visible;
                AlertTitle.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#E3B341"));
            }
            else
            {
                BtnYes.Visibility = Visibility.Visible;
                BtnNo.Visibility = Visibility.Visible;
                BtnOk.Visibility = Visibility.Collapsed;
                AlertTitle.Foreground = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#F85149"));
            }

            // Pencere boyutunu ve konumunu al, Popup'ı pencerenin tam üstüne getir
            var window = Window.GetWindow(this);
            if (window != null)
            {
                UpdatePopupPosition(window);
                window.SizeChanged += Window_SizeChanged;
                window.LocationChanged += Window_LocationChanged;
            }

            AlertPopup.IsOpen = true;

            // Alert açıkken arka plandaki tüm tıklamaları engelle
            AlertOverlay.Background = System.Windows.Media.Brushes.Transparent;
        }

        public void HideAlert()
        {
            AlertPopup.IsOpen = false;
            // Alert kapanınca tıklamalar yeniden arka plana geçebilsin
            AlertOverlay.Background = null;
            var window = Window.GetWindow(this);
            if (window != null)
            {
                window.SizeChanged -= Window_SizeChanged;
                window.LocationChanged -= Window_LocationChanged;
            }
        }

        private void UpdatePopupPosition(Window window)
        {
            AlertPopup.HorizontalOffset = window.Left;
            AlertPopup.VerticalOffset = window.Top;
            DimGrid.Width = window.ActualWidth;
            DimGrid.Height = window.ActualHeight;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var window = sender as Window;
            if (window != null) UpdatePopupPosition(window);
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            var window = sender as Window;
            if (window != null) UpdatePopupPosition(window);
        }

        private void BtnYes_Click(object sender, RoutedEventArgs e)
        {
            OnYesClicked?.Invoke(this, e);
        }

        private void BtnNo_Click(object sender, RoutedEventArgs e)
        {
            OnNoClicked?.Invoke(this, e);
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            OnOkClicked?.Invoke(this, e);
        }
    }
}

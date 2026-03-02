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

namespace Aspan_Ground.components.atoms
{
    /// <summary>
    /// DisconnectButton.xaml etkileşim mantığı
    /// </summary>
    public partial class DisconnectButton : UserControl
    {
        public event RoutedEventHandler DisconnectClicked;

        public DisconnectButton()
        {
            InitializeComponent();
        }

        private void BtnDisconnect_Click(object sender, RoutedEventArgs e)
        {
            DisconnectClicked?.Invoke(this, e);
        }
    }
}

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
    /// UpperPanelLabels.xaml etkileşim mantığı
    /// </summary>
    public enum ConnectionStatus
    {
        Disconnected,
        Checking,
        Connected
    }

    public partial class UpperPanelLabels : UserControl
    {
        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register("Status", typeof(ConnectionStatus), typeof(UpperPanelLabels), new PropertyMetadata(ConnectionStatus.Disconnected));

        public ConnectionStatus Status
        {
            get { return (ConnectionStatus)GetValue(StatusProperty); }
            set { SetValue(StatusProperty, value); }
        }

        public UpperPanelLabels()
        {
            InitializeComponent();
        }

        public void SetStatus(bool isConnected, bool isVerified)
        {
            if (!isConnected) Status = ConnectionStatus.Disconnected;
            else if (!isVerified) Status = ConnectionStatus.Checking;
            else Status = ConnectionStatus.Connected;
        }
    }
}

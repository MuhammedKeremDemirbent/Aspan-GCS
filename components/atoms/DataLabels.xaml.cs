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
    public partial class DataLabels : UserControl
    {
        public DataLabels()
        {
            InitializeComponent();
            
            // Veri Bağlamasını (Data Binding) kod üzerinden statik Singleton'a zorluyoruz
            this.DataContext = Aspan_Ground.services.DataDesc.Instance;
        }
    }
}

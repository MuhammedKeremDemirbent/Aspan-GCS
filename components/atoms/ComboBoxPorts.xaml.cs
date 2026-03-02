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
using System.Windows.Controls;
using Aspan_Ground.cardconnection; // SerialManager'e ulaşmak için ekledik

namespace Aspan_Ground.components.atoms
{
    public partial class ComboBoxPorts : UserControl
    {
        public ComboBoxPorts()
        {
            InitializeComponent();
            
            // Ekran ilk açıldığında da portları eklesin diye bir kere çağırıyoruz
            LoadAvailablePorts();
        }

        // Kullanıcı combobox'a (ok tuşuna) HER tıkladığında burası tetiklenir
        private void CbPort_DropDownOpened(object sender, System.EventArgs e)
        {
            LoadAvailablePorts();
        }

        // SerialManager'a gidip güncel port listesini getiren metot
        private void LoadAvailablePorts()
        {
            // Mevcut seçimi kaybetmemek için hafızaya alalım
            string currentSelection = CbPort.SelectedItem as string;

            // SerialManager'dan port listesini isteyelim
            string[] availablePorts = SerialManager.Instance.GetAvailablePorts();

            // Sadece portlarda bir değişiklik varsa arayüzü güncelleyelim (titremeyi önler)
            if (CbPort.ItemsSource == null || !CbPort.ItemsSource.Cast<string>().SequenceEqual(availablePorts))
            {
                CbPort.ItemsSource = availablePorts;

                // Eğer hafızadaki seçim hala takılı olan portlar listesinde varsa onu geri seç
                if (currentSelection != null && availablePorts.Contains(currentSelection))
                {
                    CbPort.SelectedItem = currentSelection;
                }
                // Port varsa ama hiçbir şey seçili değilse ilkini otomatik seç
                else if (availablePorts.Length > 0)
                {
                    CbPort.SelectedIndex = 0;
                }
            }
        }
    }
}

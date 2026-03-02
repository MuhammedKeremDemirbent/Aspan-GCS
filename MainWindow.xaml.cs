using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Ports;
using MahApps.Metro.Controls;

namespace Aspan_Ground
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            
            // Alert Buton Dinleyicilerini Bağla
            MainAlert.OnYesClicked += MainAlert_OnYesClicked;
            MainAlert.OnNoClicked += MainAlert_OnNoClicked;
            MainAlert.OnOkClicked += MainAlert_OnOkClicked;
            
            // Uygulama kapanırken araya gir
            this.Closing += MainWindow_Closing;

            // Beklenmeyen USB kopmalarını dinle
            Aspan_Ground.cardconnection.SerialManager.Instance.OnConnectionChanged += SerialManager_OnConnectionChanged;
        }

        private void SerialManager_OnConnectionChanged(bool isConnected)
        {
            // Eğer port kapatıldıysa (kopma durumu)
            if (!isConnected)
            {
                Dispatcher.InvokeAsync(() =>
                {
                    // Uygulama kapanmıyorsa ve kasten "Disconnect" butonuna basılmadıysa kopma uyarısı ver
                    if (!forceClose && !Aspan_Ground.cardconnection.SerialManager.Instance.IsUserRequestedDisconnect)
                    {
                        MainAlert.ShowAlert("BAĞLANTI KOPTU", "Cihaz ile olan seri port bağlantısı fiziksel bir müdahale veya hata sonucu kesildi.", isError: true);
                    }
                });
            }
        }

        private bool forceClose = false;

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Zaten zorunlu kapatma emri verdiysek (Yes'e basıldıysa) direkt kapat
            if (forceClose) return;

            // Eğer bağlantı varsa kapanmayı iptal et ve uyarı göster
            if (Aspan_Ground.cardconnection.SerialManager.Instance.IsConnected)
            {
                e.Cancel = true; 
                MainAlert.ShowAlert("ÇIKIŞ YAPILIYOR", "Uygulamadan çıkmak istediğinize emin misiniz?\nAktif bağlantınız koparılacaktır.");
            }
        }

        // UpperPanel'den Disconnect isteği geldiğinde Alert göster
        private void TopPanel_RequestDisconnect(object sender, RoutedEventArgs e)
        {
            MainAlert.ShowAlert("DİKKAT", "Çıkmak istediğinize emin misiniz?\nBağlantı kopacak.");
        }

        // EVET'e basılırsa
        private void MainAlert_OnYesClicked(object sender, RoutedEventArgs e)
        {
            MainAlert.HideAlert();
            TopPanel.ExecuteDisconnect(); // Asıl koparma işlemini çalıştır
            
            // Uygulamayı tamamen kapat
            forceClose = true;
            this.Close();
        }

        // HAYIR'a basılırsa
        private void MainAlert_OnNoClicked(object sender, RoutedEventArgs e)
        {
            MainAlert.HideAlert();
        }

        // TAMAM (Hata Onay) basılırsa
        private void MainAlert_OnOkClicked(object sender, RoutedEventArgs e)
        {
            MainAlert.HideAlert();
        }

        // Dışarıdan Hata Mesajı Göstermek İçin Açık Metot
        public void ShowError(string title, string message)
        {
            MainAlert.ShowAlert(title, message, isError: true);
        }
    }
}
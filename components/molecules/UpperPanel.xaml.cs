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
    /// UpperPanel.xaml etkileşim mantığı
    /// </summary>
    public partial class UpperPanel : UserControl
    {
        // Ana pencereye (MainWindow) disconnect talebini iletecek event
        public event RoutedEventHandler RequestDisconnect;

        public UpperPanel()
        {
            InitializeComponent();
            
            // Yüklendiğinde eventleri bağla ve başlangıç UI durumunu ayarla
            this.Loaded += UpperPanel_Loaded;
        }

        private void UpperPanel_Loaded(object sender, RoutedEventArgs e)
        {
            BtnConnectControl.ConnectClicked += BtnConnect_Click;
            BtnDisconnectControl.DisconnectClicked += BtnDisconnect_Click;
            
            Aspan_Ground.cardconnection.SerialManager.Instance.ConnectionSynchronized += SerialManager_ConnectionSynchronized;

            // Arka planda donanımsal kopma olursa UI da haberdar olsun
            Aspan_Ground.cardconnection.SerialManager.Instance.OnConnectionChanged += UI_OnConnectionChanged;
            
            // Handshake başarılı olduğunda UI'ı tam sese (yeşile) çek
            Aspan_Ground.cardconnection.SerialManager.Instance.OnConnectionVerified += () => {
                Dispatcher.InvokeAsync(() => SetUIState(true, true));
            };

            // Başlangıç Durumu
            SetUIState(false, false);
        }

        private void UI_OnConnectionChanged(bool isConnected)
        {
            if (!isConnected)
            {
                Dispatcher.InvokeAsync(() =>
                {
                    // DataDesc (Kiler) verilerini sıfırla (Kullanıcı Talebi / Bağlantı Koptu)
                    Aspan_Ground.services.DataDesc.Instance.currentRoll = 0f;
                    Aspan_Ground.services.DataDesc.Instance.currentPitch = 0f;
                    Aspan_Ground.services.DataDesc.Instance.currentYaw = 0f;
                    Aspan_Ground.services.DataDesc.Instance.currentAltitude = 0f;
                    Aspan_Ground.services.DataDesc.Instance.currentRH = 0f;
                    Aspan_Ground.services.DataDesc.Instance.currentRV = 0f;
                    Aspan_Ground.services.DataDesc.Instance.currentLH = 0f;
                    Aspan_Ground.services.DataDesc.Instance.currentLV = 0;
                    Aspan_Ground.services.DataDesc.Instance.currentLat = 0.0;
                    Aspan_Ground.services.DataDesc.Instance.currentLon = 0.0;
                    
                    Aspan_Ground.services.DataDesc.Instance.rollSetpoint = 0f;
                    Aspan_Ground.services.DataDesc.Instance.pitchSetpoint = 0f;
                    Aspan_Ground.services.DataDesc.Instance.yawSetpoint = 0f;

                    Aspan_Ground.services.DataDesc.Instance.currentBattery = 0;
                    Aspan_Ground.services.DataDesc.Instance.currentSatellites = 0;
                    Aspan_Ground.services.DataDesc.Instance.currentHorizontalSpeed = 0;
                    Aspan_Ground.services.DataDesc.Instance.currentVerticalSpeed = 0;
                    Aspan_Ground.services.DataDesc.Instance.currentChannel5 = 0;
                    Aspan_Ground.services.DataDesc.Instance.currentChannel6 = 0;
                    Aspan_Ground.services.DataDesc.Instance.currentChannel7 = 0;
                    Aspan_Ground.services.DataDesc.Instance.currentChannel8 = 0;
                    Aspan_Ground.services.DataDesc.Instance.currentModelID = 0;
                    Aspan_Ground.services.DataDesc.Instance.currentFree1 = 0;

                    // Converter'larin (Örn. Batarya Rengi) tetiklenmesi için DataDesc'te topyekün uyarı gönder
                    Aspan_Ground.services.DataDesc.Instance.NotifyAllPropertiesChanged();

                    // UI renk değişikliklerini uygula
                    SetUIState(false, false);

                    // Eğer kullanıcı istemeden koptuysa ve doğrulama aşamasındayken koptuysa hata göster
                    if (!Aspan_Ground.cardconnection.SerialManager.Instance.IsUserRequestedDisconnect && 
                        !Aspan_Ground.cardconnection.SerialManager.Instance.IsVerified)
                    {
                        var mainWindow = Window.GetWindow(this) as MainWindow;
                        mainWindow?.ShowError("BAĞLANTI HATASI", "Cihaz doğrulanamadı. Lütfen doğru portun seçili olduğundan emin olun.");
                    }
                });
            }
        }

        private void SerialManager_ConnectionSynchronized()
        {
        }

        private void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            string selectedPort = PortSelector.CbPort.SelectedItem as string;
            string selectedBaudRateStr = BaudSelector.CbBaud.Text;

            if (string.IsNullOrEmpty(selectedPort) || string.IsNullOrEmpty(selectedBaudRateStr))
            {
                var mainWindow = Window.GetWindow(this) as MainWindow;
                mainWindow?.ShowError("BAĞLANTI HATASI", "Lütfen port ve baud rate ayarlarını seçin.");
                return;
            }

            if (int.TryParse(selectedBaudRateStr, out int baudRate))
            {
                try
                {
                    // Seri porta bağlanılıyor
                    Aspan_Ground.cardconnection.SerialManager.Instance.Connect(selectedPort, baudRate);

                    if (Aspan_Ground.cardconnection.SerialManager.Instance.IsConnected)
                    {
                        // Bağlandığı an önce "DOĞRULANIYOR" (Verifying) durumuna geç
                        SetUIState(true, false);
                    }
                }
                catch (Exception ex)
                {
                    var mainWindow = Window.GetWindow(this) as MainWindow;
                    mainWindow?.ShowError("BAĞLANTI HATASI", ex.Message);
                }
            }
        }

        private void BtnDisconnect_Click(object sender, RoutedEventArgs e)
        {
            ExecuteDisconnect();
        }

        public void ExecuteDisconnect()
        {
            // Seri port kasten koparılıyor (Buton üzerinden)
            Aspan_Ground.cardconnection.SerialManager.Instance.Disconnect();
            // DataDesc sıfırlaması artık UI_OnConnectionChanged event'i tetiklendiğinde otomatik yapılacak
        }

        private void SetUIState(bool isConnected, bool isVerified)
        {
            // Buton aktif/pasif durumları
            BtnConnectControl.IsEnabled = !isConnected;
            BtnDisconnectControl.IsEnabled = isConnected;

            // Görsel Opaklık (Pasif buton soluk görünür)
            BtnConnectControl.Opacity = isConnected ? 0.4 : 1.0;
            BtnDisconnectControl.Opacity = isConnected ? 1.0 : 0.4;

            // Status Label ve Border Güncellemesi
            ConnectionLabels.SetStatus(isConnected, isVerified);
            
            // Kullanıcı comboboxlara tıklamak istedi (Her zaman açık bırakılıyor)
            PortSelector.IsEnabled = true;
            BaudSelector.IsEnabled = true;
        }
    }
}

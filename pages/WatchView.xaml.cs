using System;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Media;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using Aspan_Ground.services;
using Aspan_Ground.cardconnection;

namespace Aspan_Ground.pages
{
    /// <summary>
    /// WatchView.xaml etkileşim mantığı
    /// </summary>
    public partial class WatchView : UserControl
    {
        private GMapMarker _droneMarker;
        private RotateTransform _droneRotation;
        private bool _isFirstLocationReceived = false;

        public WatchView()
        {
            InitializeComponent();
            InitializeMap();

            // Veriler değiştikçe göstergeleri güncelle
            DataDesc.Instance.PropertyChanged += DataDesc_PropertyChanged;

            // İlk açılışta verileri sıfırla
            AttitudeIndicator.SetAttitudeIndicatorParameters(0, 0);
            HeadingIndicator.SetHeadingIndicatorParameters(0);
        }

        private void DataDesc_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // C# Thread güvenliği için Dispatcher üzerinden çalıştır
            Dispatcher.InvokeAsync(() =>
            {
                if (e.PropertyName == nameof(DataDesc.currentPitch) || e.PropertyName == nameof(DataDesc.currentRoll))
                {
                    float pitch = (float)DataDesc.Instance.currentPitch;
                    float roll = (float)DataDesc.Instance.currentRoll;
                    AttitudeIndicator.SetAttitudeIndicatorParameters(pitch, roll);
                }
                else if (e.PropertyName == nameof(DataDesc.currentYaw))
                {
                    int yaw = (int)DataDesc.Instance.currentYaw;
                    HeadingIndicator.SetHeadingIndicatorParameters(yaw);

                    if (_droneRotation != null)
                    {
                        _droneRotation.Angle = yaw;
                    }
                }
                else if (e.PropertyName == nameof(DataDesc.currentLat) || e.PropertyName == nameof(DataDesc.currentLon))
                {
                    if (_droneMarker != null && SerialManager.Instance.IsConnected)
                    {
                        var pos = new PointLatLng(DataDesc.Instance.currentLat, DataDesc.Instance.currentLon);
                        _droneMarker.Position = pos;

                        // Uçak pozisyonu ilk defa geliyorsa haritayı oraya odakla ve görünür yap
                        if (!_isFirstLocationReceived && pos.Lat != 0 && pos.Lng != 0)
                        {
                            _isFirstLocationReceived = true;
                            MapControl.Position = pos;
                            _droneMarker.Shape.Visibility = Visibility.Visible;
                        }
                    }
                }
                else if (string.IsNullOrEmpty(e.PropertyName)) // NotifyAllPropertiesChanged çağrıldığında
                {
                    // Bağlantı koptuysa uçağı gizle ve flag'i sıfırla
                    if (!SerialManager.Instance.IsConnected && _droneMarker != null)
                    {
                        _droneMarker.Shape.Visibility = Visibility.Hidden;
                        _isFirstLocationReceived = false; // Bir dahaki bağlantıda tekrar odaklansın
                    }
                }
            });
        }

        private void InitializeMap()
        {
            try
            {
                //BingSatelliteMap
                MapControl.MapProvider = GMapProviders.BingSatelliteMap;
                
                //Offline Mode Ayarı
                GMaps.Instance.Mode = AccessMode.ServerAndCache; 

                // Harita başlangıç pozisyonu
                MapControl.Position = new PointLatLng(39.92077, 32.85411);

                // Yakınlaştırma ayarları
                MapControl.MinZoom = 2;
                MapControl.MaxZoom = 20;
                MapControl.Zoom = 13;

                // Fare tekerleği ile zoom yapmaya izin ver
                MapControl.MouseWheelZoomType = MouseWheelZoomType.MousePositionAndCenter;

                // Haritayı sürüklemek için sol tık kullan
                MapControl.DragButton = System.Windows.Input.MouseButton.Left;

                // Haritadaki kırmızı çarpı işaretini gizle
                MapControl.ShowCenter = false;

                // --- Uçak (Drone) İkonunu Haritaya Ekleme ---
                System.Windows.Controls.Image droneImage = new System.Windows.Controls.Image();
                droneImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/assets/icons/Vector_Union.png"));
                droneImage.Width = 32;
                droneImage.Height = 32;

                _droneRotation = new RotateTransform();
                _droneRotation.Angle = DataDesc.Instance.currentYaw; // İlk açı
                _droneRotation.CenterX = 16; // 32'nin yarısı
                _droneRotation.CenterY = 16; 

                droneImage.RenderTransform = _droneRotation;

                // Lat, Lon 0 ise varsayılan konumu kullan
                double startLat = DataDesc.Instance.currentLat == 0 ? 39.92077 : DataDesc.Instance.currentLat;
                double startLon = DataDesc.Instance.currentLon == 0 ? 32.85411 : DataDesc.Instance.currentLon;

                _droneMarker = new GMapMarker(new PointLatLng(startLat, startLon));
                _droneMarker.Shape = droneImage;
                _droneMarker.Offset = new System.Windows.Point(-16, -16); // İkonun tam ortasını kordinata oturtmak için X ve Y'yi geri kaydır
                _droneMarker.ZIndex = 100; // Her zaman üstte olsun

                // İlk başta ikon gizli olsun, telemetri (veya connection) gelince görünür olacak
                _droneMarker.Shape.Visibility = SerialManager.Instance.IsConnected ? Visibility.Visible : Visibility.Hidden;

                MapControl.Markers.Add(_droneMarker);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Harita yüklenirken bir hata oluştu: {ex.Message}", "HATA", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

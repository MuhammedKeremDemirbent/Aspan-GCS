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
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Aspan_Ground.pages
{
    // Mission Planner tarzı DataGrid gösterimi için Model
    public class WaypointItem : INotifyPropertyChanged
    {
        private int _index;
        public int Index { get => _index; set { _index = value; OnPropertyChanged(); } }

        private string _type = "WAYPOINT";
        public string Type { get => _type; set { _type = value; OnPropertyChanged(); } }

        private double _lat;
        public double Lat { get => _lat; set { _lat = value; OnPropertyChanged(); } }

        private double _lng;
        public double Lng { get => _lng; set { _lng = value; OnPropertyChanged(); } }

        private double _alt = 100;
        public double Alt { get => _alt; set { _alt = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    /// <summary>
    /// MissionView.xaml etkileşim mantığı
    /// </summary>
    public partial class MissionView : UserControl
    {
        // UI ile bağlı Waypoint (Görev Noktaları) Listesi
        public ObservableCollection<WaypointItem> Waypoints { get; set; } = new ObservableCollection<WaypointItem>();

        // Sürükleme (Drag & Drop) Takip Değişkenleri
        private GMapMarker? _draggedMarker = null;
        private int _draggedIndex = -1;
        private bool _isDragging = false;

        public MissionView()
        {
            InitializeComponent();
            WpDataGrid.ItemsSource = Waypoints;
            InitializeMap();

            // Alert pop-up kapatma eventleri
            MissionAlert.OnOkClicked += (s, ev) => MissionAlert.HideAlert();
            MissionAlert.OnNoClicked += (s, ev) => MissionAlert.HideAlert();
            MissionAlert.OnYesClicked += (s, ev) => MissionAlert.HideAlert();
        }

        private void InitializeMap()
        {
            try
            {
                // Bing Uydu Haritası
                MapControl.MapProvider = GMapProviders.BingSatelliteMap;
                GMaps.Instance.Mode = AccessMode.ServerAndCache; 
                MapControl.Position = new PointLatLng(39.92077, 32.85411);

                MapControl.MinZoom = 2;
                MapControl.MaxZoom = 20;
                MapControl.Zoom = 15;

                MapControl.MouseWheelZoomType = MouseWheelZoomType.MousePositionAndCenter;
                MapControl.IgnoreMarkerOnMouseWheel = true;
                
                // Harita kaydırma işlemi Sağ Tuşa (veya farenin orta tuşuna) alındı (Sol tuş WP ekleyecek)
                MapControl.DragButton = MouseButton.Right;
                MapControl.CanDragMap = true;

                // Harita Tıklama Eventleri (Sol tık ekle, Sürükle vb)
                MapControl.MouseLeftButtonDown += MapControl_MouseLeftButtonDown;
                MapControl.MouseMove += MapControl_MouseMove;
                MapControl.MouseLeftButtonUp += MapControl_MouseLeftButtonUp;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MissionView] Harita Yükleme Hatası: {ex.Message}");
            }
        }

        // Listede WP eklendikçe veya çıkarıldıkça Index numerasyonunu günceller
        private void RecalculateIndexes()
        {
            for (int i = 0; i < Waypoints.Count; i++)
            {
                Waypoints[i].Index = i + 1;
            }
        }

        private void MapControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Eğer bir Shape (Marker) üzerine tıklandıysa, Event durdurulmuştur, haritaya WP atma.
            if (e.Handled) return;

            // Boşluğa sol tıklandı => Yeni Waypoint ekle
            System.Windows.Point clickPoint = e.GetPosition(MapControl);
            PointLatLng latLng = MapControl.FromLocalToLatLng((int)clickPoint.X, (int)clickPoint.Y);

            Waypoints.Add(new WaypointItem { Lat = latLng.Lat, Lng = latLng.Lng });
            RecalculateIndexes();
            RedrawMap();
        }

        private void MapControl_MouseMove(object sender, MouseEventArgs e)
        {
            // Sürükleme işlemi aktifse marker'ın ve datanın konumunu güncelle
            if (_isDragging && _draggedMarker != null && e.LeftButton == MouseButtonState.Pressed)
            {
                System.Windows.Point p = e.GetPosition(MapControl);
                PointLatLng latLng = MapControl.FromLocalToLatLng((int)p.X, (int)p.Y);

                _draggedMarker.Position = latLng;     // Görsel Kaydırma
                Waypoints[_draggedIndex].Lat = latLng.Lat; // Data Güncelleme
                Waypoints[_draggedIndex].Lng = latLng.Lng; // Data Güncelleme
                
                RedrawMap(); // Çizgileri anında güncellemek için yeniden çiz (GMap.NET WPF'te hızlıdır)
            }
        }

        private void MapControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Eğer sürükleme bırakıldıysa sistemi sıfırla
            if (_isDragging)
            {
                _isDragging = false;
                _draggedMarker = null;
                _draggedIndex = -1;
                MapControl.Cursor = Cursors.Arrow;
                RedrawMap();
            }
        }

        private void RedrawMap()
        {
            MapControl.Markers.Clear();

            if (Waypoints.Count == 0) return;

            var routePoints = Waypoints.Select(w => new PointLatLng(w.Lat, w.Lng)).ToList();

            // 0. (Ekstra) Checkbox İşaretli ise İç Alanı Boya (Polygon)
            if (FillAreaCb.IsChecked == true)
            {
                if (Waypoints.Count >= 3)
                {
                    GMapPolygon polygon = new GMapPolygon(routePoints);
                    System.Windows.Shapes.Path polyPath = new System.Windows.Shapes.Path
                    {
                        Stroke = System.Windows.Media.Brushes.Transparent,
                        StrokeThickness = 0,
                        Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(80, 50, 205, 50)) // Yarı saydam yeşil dolgu
                    };
                    polygon.Shape = polyPath;
                    polygon.ZIndex = 5; // En altta kalsın (Çizgiler 9-10, Pinler 100)
                    MapControl.Markers.Add(polygon);
                }
                else
                {
                    // Noktalar silinerek 3'ün altına inildiyse otomatik kapat.
                    FillAreaCb.IsChecked = false;
                }
            }

            // 1. Önce Route (Sarı Çizgi) ve Midpoint (+ ları) Çiz
            if (Waypoints.Count > 1)
            {
                GMapRoute route = new GMapRoute(routePoints);
                System.Windows.Shapes.Path routePath = new System.Windows.Shapes.Path
                {
                    Stroke = System.Windows.Media.Brushes.Yellow,
                    StrokeThickness = 3,
                    StrokeDashArray = new DoubleCollection { 4, 4 } // Kesik çizgi (Dashed)
                };
                route.Shape = routePath;
                route.ZIndex = 10;
                MapControl.Markers.Add(route);

                // Kesik çizgili dönüş rotasını ekle (Son noktadan ilk noktaya)
                if (Waypoints.Count > 2)
                {
                    var returnPoints = new List<PointLatLng> { routePoints.Last(), routePoints.First() };
                    GMapRoute returnRoute = new GMapRoute(returnPoints);
                    System.Windows.Shapes.Path returnPath = new System.Windows.Shapes.Path
                    {
                        Stroke = System.Windows.Media.Brushes.Yellow,
                        StrokeThickness = 3,
                        StrokeDashArray = new DoubleCollection { 4, 4 } // Kesik çizgi (Dashed)
                    };
                    returnRoute.Shape = returnPath;
                    returnRoute.ZIndex = 9; // Normal çizgilerin bir tık altında
                    MapControl.Markers.Add(returnRoute);
                }

                // Ardışık iki noktanın arasına Midpoint (+) at
                for (int i = 0; i < routePoints.Count - 1; i++)
                {
                    MapControl.Markers.Add(CreateMidpointMarker(routePoints[i], routePoints[i + 1], i + 1));
                }
            }

            // 2. Ana Waypoint Pinlerini (Yeşil) en üste Çiz
            for (int i = 0; i < Waypoints.Count; i++)
            {
                MapControl.Markers.Add(CreateWaypointMarker(Waypoints[i], i));
            }
        }

        private GMapMarker CreateWaypointMarker(WaypointItem wp, int index)
        {
            PointLatLng latLng = new PointLatLng(wp.Lat, wp.Lng);
            GMapMarker marker = new GMapMarker(latLng);

            // Daha küçük bir Yeşil Pin Çizimi (Eski çap 15, yeni çap 12)
            System.Windows.Shapes.Path pinShape = new System.Windows.Shapes.Path
            {
                Fill = System.Windows.Media.Brushes.LimeGreen,
                Stroke = System.Windows.Media.Brushes.Black,
                StrokeThickness = 1.2,
                Data = System.Windows.Media.Geometry.Parse("M 0,0 C -8,-8 -12,-16 -12,-24 A 12,12 0 1,1 12,-24 C 12,-16 8,-8 0,0 Z"),
            };

            // İçerisine yazı ve pin'i koyacağımız Container Grid
            Grid pinGrid = new Grid();
            pinGrid.Cursor = Cursors.Hand;
            pinGrid.Background = System.Windows.Media.Brushes.Transparent; // Etkileşimi güçlendirmek için
            pinGrid.Children.Add(pinShape);

            // Sadece Hover ile gözüken Numara Text'i (İkonun tam ortasında yuvarlak bölüme)
            TextBlock textNum = new TextBlock
            {
                Text = (index + 1).ToString(),
                FontWeight = FontWeights.Bold,
                Foreground = System.Windows.Media.Brushes.White,
                FontSize = 10,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 12), // Merkezden pin'in dairesel göbeğine doğru kaydır
                Visibility = Visibility.Hidden
            };
            pinGrid.Children.Add(textNum);

            // Sürükleme Eventleri
            pinGrid.MouseLeftButtonDown += (s, ev) =>
            {
                _draggedMarker = marker;
                _draggedIndex = index;
                _isDragging = true;
                MapControl.Cursor = Cursors.Hand;
                ev.Handled = true; // Harita tıklamasını iptal et
            };

            pinGrid.MouseLeftButtonUp += (s, ev) =>
            {
                if (_isDragging) {
                    _isDragging = false;
                    _draggedMarker = null;
                    _draggedIndex = -1;
                    MapControl.Cursor = Cursors.Arrow;
                }
                ev.Handled = true;
            };

            // Fare üzerine geldiğinde/ayrıldığında Numarayı gösterip gizle
            pinGrid.MouseEnter += (s, ev) => textNum.Visibility = Visibility.Visible;
            pinGrid.MouseLeave += (s, ev) => textNum.Visibility = Visibility.Hidden;

            // Context Menu (Sil)
            ContextMenu cm = new ContextMenu();
            MenuItem miDelete = new MenuItem { Header = "Sil (Delete WP)" };
            miDelete.Click += (s, ev) => DeleteWaypoint(index);
            cm.Items.Add(miDelete);
            pinGrid.ContextMenu = cm;

            marker.Shape = pinGrid;
            marker.ZIndex = 100;
            return marker;
        }

        private GMapMarker CreateMidpointMarker(PointLatLng p1, PointLatLng p2, int insertIndex)
        {
            // Ortadaki koordinatı bul
            PointLatLng mid = new PointLatLng((p1.Lat + p2.Lat) / 2, (p1.Lng + p2.Lng) / 2);
            GMapMarker marker = new GMapMarker(mid);

            // Küçük sarı yuvarlak ve ortasında siyah +
            Grid grid = new Grid { Width = 18, Height = 18, Background = System.Windows.Media.Brushes.Transparent };
            Ellipse bg = new Ellipse { Fill = System.Windows.Media.Brushes.Yellow, Stroke = System.Windows.Media.Brushes.Black, StrokeThickness = 1 };
            TextBlock text = new TextBlock
            {
                Text = "+",
                FontWeight = FontWeights.Bold,
                Foreground = System.Windows.Media.Brushes.Black,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, -2, 0, 0) // Yazıyı ortalamak için hafif ayar
            };

            grid.Children.Add(bg);
            grid.Children.Add(text);
            grid.Cursor = Cursors.Hand;
            grid.ToolTip = "Yeni Waypoint Ekle";

            // Artı işaretine tıklanınca araya Pin at
            grid.MouseLeftButtonDown += (s, ev) =>
            {
                Waypoints.Insert(insertIndex, new WaypointItem { Lat = mid.Lat, Lng = mid.Lng });
                RecalculateIndexes();
                RedrawMap(); 
                ev.Handled = true;
            };
            marker.Shape = grid;
            marker.Offset = new System.Windows.Point(-9, -9); // 18/2 tam merkezlemek için
            marker.ZIndex = 50;

            return marker;
        }

        private void DeleteWaypoint(int index)
        {
            if (index >= 0 && index < Waypoints.Count)
            {
                Waypoints.RemoveAt(index);
                RecalculateIndexes();
                RedrawMap();
            }
        }

        // Polygon Çizme Özelliği - CheckBox Tıklandığında
        private void FillAreaCb_Click(object sender, RoutedEventArgs e)
        {
            if (FillAreaCb.IsChecked == true)
            {
                if (Waypoints.Count < 3)
                {
                    FillAreaCb.IsChecked = false;
                    MissionAlert.ShowAlert("Alan Oluşturulamıyor", "Alanın taranıp boyanabilmesi için harita üzerinde en az 3 adet Waypoint bulunmalıdır. Lütfen daha fazla nokta ekleyin.", true);
                    return; // RedrawMap de çalışmasın çünkü iptal ettik
                }
            }
            
            RedrawMap();
        }

        // DataGrid'den satır içi X (Sil) Butonuna basıldığında
        private void DeleteWp_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int index)
            {
                // UI 'No' sütunu yani index'i 1'den başlar, list indexi 0'dan
                DeleteWaypoint(index - 1);
            }
        }

        // DataGrid'den satır içi Yukarı (Up) Butonuna basıldığında
        private void MoveWpUp_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int wpNumber)
            {
                int listIndex = wpNumber - 1;
                
                // En üstteki eleman daha yukarı çıkamaz
                if (listIndex > 0 && listIndex < Waypoints.Count)
                {
                    // Yer değiştir (Swap)
                    var temp = Waypoints[listIndex];
                    Waypoints[listIndex] = Waypoints[listIndex - 1];
                    Waypoints[listIndex - 1] = temp;
                    
                    RecalculateIndexes();
                    RedrawMap();
                }
            }
        }

        // DataGrid'den satır içi Aşağı (Down) Butonuna basıldığında
        private void MoveWpDown_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int wpNumber)
            {
                int listIndex = wpNumber - 1;

                // En alttaki eleman daha aşağı inemez
                if (listIndex >= 0 && listIndex < Waypoints.Count - 1)
                {
                    // Yer değiştir (Swap)
                    var temp = Waypoints[listIndex];
                    Waypoints[listIndex] = Waypoints[listIndex + 1];
                    Waypoints[listIndex + 1] = temp;

                    RecalculateIndexes();
                    RedrawMap();
                }
            }
        }
    }
}

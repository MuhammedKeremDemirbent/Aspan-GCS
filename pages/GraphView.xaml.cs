using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using Aspan_Ground.services;
using Aspan_Ground.cardconnection;

namespace Aspan_Ground.pages
{
    public partial class GraphView : UserControl, INotifyPropertyChanged
    {
        public ObservableCollection<ISeries> VisibleSeries { get; set; } = new ObservableCollection<ISeries>();

        public Axis[] XAxes { get; set; } = new Axis[]
        {
            new Axis
            {
                IsVisible = false // Hide X-axis labels for a clean flowing look
            }
        };

        public Axis[] YAxes { get; set; } = new Axis[]
        {
            new Axis
            {
                LabelsPaint = new SolidColorPaint(SKColors.LightGray),
                SeparatorsPaint = new SolidColorPaint(new SKColor(48, 54, 61)) { StrokeThickness = 1 }
            }
        };

        private readonly DispatcherTimer _graphTimer;

        private readonly ObservableCollection<ObservableValue> _rollData = new();
        private readonly ObservableCollection<ObservableValue> _pitchData = new();
        private readonly ObservableCollection<ObservableValue> _yawData = new();
        private readonly ObservableCollection<ObservableValue> _rollSpData = new();
        private readonly ObservableCollection<ObservableValue> _pitchSpData = new();
        private readonly ObservableCollection<ObservableValue> _yawSpData = new();

        private LineSeries<ObservableValue> _seriesRoll;
        private LineSeries<ObservableValue> _seriesPitch;
        private LineSeries<ObservableValue> _seriesYaw;
        private LineSeries<ObservableValue> _seriesRollSp;
        private LineSeries<ObservableValue> _seriesPitchSp;
        private LineSeries<ObservableValue> _seriesYawSp;

        private bool _isLogging = false;
        private StreamWriter _logWriter;
        private readonly string _logsFolder;

        public GraphView()
        {
            DataContext = this;

            _logsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

            _seriesRoll = CreateSeries("Roll", _rollData, SKColors.LightBlue);
            _seriesPitch = CreateSeries("Pitch", _pitchData, SKColors.LightGreen);
            _seriesYaw = CreateSeries("Yaw", _yawData, SKColors.Orange);
            _seriesRollSp = CreateSeries("Roll Setpoint", _rollSpData, SKColors.Orchid);
            _seriesPitchSp = CreateSeries("Pitch Setpoint", _pitchSpData, SKColors.Orchid);
            _seriesYawSp = CreateSeries("Yaw Setpoint", _yawSpData, SKColors.Orchid);

            _graphTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _graphTimer.Tick += GraphTimer_Tick;

            InitializeComponent();

            // Alert pop-up kapatma eventleri
            GraphAlert.OnOkClicked += (s, ev) => GraphAlert.HideAlert();
            GraphAlert.OnNoClicked += (s, ev) => GraphAlert.HideAlert();
            GraphAlert.OnYesClicked += (s, ev) => GraphAlert.HideAlert();

            // Port koptuğunda grafiği durdurma eventi
            SerialManager.Instance.OnConnectionChanged += OnConnectionChanged;
        }

        private void OnConnectionChanged(bool isConnected)
        {
            if (!isConnected)
            {
                Dispatcher.InvokeAsync(() =>
                {
                    RadioNone.IsChecked = true;
                    // Timer zaten Radio_Checked eventinde 'None' seçilince duracaktır.
                    // Fakat çift garanti olması için;
                    _graphTimer.Stop();

                    if (_isLogging)
                    {
                        BtnSaveLog_Click(this, new RoutedEventArgs()); // Mevcut Logu kurtar & kaydet
                    }
                });
            }
        }

        private LineSeries<ObservableValue> CreateSeries(string name, ObservableCollection<ObservableValue> data, SKColor color)
        {
            return new LineSeries<ObservableValue>
            {
                Name = name,
                Values = data,
                Fill = null,
                GeometrySize = 6,
                Stroke = new SolidColorPaint(color) { StrokeThickness = 2 }
            };
        }

        private void GraphTimer_Tick(object sender, EventArgs e)
        {
            // Update Graph Data
            if (RadioNone.IsChecked != true)
            {
                if (RadioRoll.IsChecked == true || RadioRollSp.IsChecked == true || RadioRollPitch.IsChecked == true)
                    AddData(_rollData, DataDesc.Instance.currentRoll);

                if (RadioPitch.IsChecked == true || RadioPitchSp.IsChecked == true || RadioRollPitch.IsChecked == true)
                    AddData(_pitchData, DataDesc.Instance.currentPitch);

                if (RadioYaw.IsChecked == true || RadioYawSp.IsChecked == true)
                    AddData(_yawData, DataDesc.Instance.currentYaw);

                if (RadioRollSp.IsChecked == true)
                    AddData(_rollSpData, DataDesc.Instance.rollSetpoint);

                if (RadioPitchSp.IsChecked == true)
                    AddData(_pitchSpData, DataDesc.Instance.pitchSetpoint);

                if (RadioYawSp.IsChecked == true)
                    AddData(_yawSpData, DataDesc.Instance.yawSetpoint);
            }

            // Write to Log
            if (_isLogging && _logWriter != null)
            {
                try
                {
                    string time = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
                    string line = $"{DataDesc.Instance.currentRoll:F2}\t{DataDesc.Instance.currentPitch:F2}\t{DataDesc.Instance.currentYaw:F2}\t" +
                                  $"{DataDesc.Instance.rollSetpoint:F2}\t{DataDesc.Instance.pitchSetpoint:F2}\t{DataDesc.Instance.yawSetpoint:F2}\t{time}";
                    _logWriter.WriteLine(line);
                    _logWriter.Flush();
                }
                catch { } // Silently handle IO errors during tick
            }

            // Auto-stop condition
            if (RadioNone.IsChecked == true && !_isLogging)
            {
                _graphTimer.Stop();
            }
        }

        private void AddData(ObservableCollection<ObservableValue> collection, float value)
        {
            collection.Add(new ObservableValue(value));
            if (collection.Count > 50)
            {
                collection.RemoveAt(0); // Flows from right to left, inserting on right, falling off left
            }
        }

        private void Radio_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded || VisibleSeries == null) return;

            // Eğer "None" haricinde bir tuşa tıklandıysa ve porta bağlı değilse
            if (sender != RadioNone && RadioNone.IsChecked != true && !SerialManager.Instance.IsConnected)
            {
                // İşlemi iptal edip 'None' olarak geri seç ve uyarı ver
                RadioNone.IsChecked = true;
                GraphAlert.ShowAlert("Bağlantı Hatası", "Port bağlantısı sağlanmadı. Canlı veri akışı için lütfen cihaza bağlanın.", true);
                return;
            }

            VisibleSeries.Clear();
            _rollData.Clear();
            _pitchData.Clear();
            _yawData.Clear();
            _rollSpData.Clear();
            _pitchSpData.Clear();
            _yawSpData.Clear();

            if (RadioRoll.IsChecked == true) { VisibleSeries.Add(_seriesRoll); }
            else if (RadioPitch.IsChecked == true) { VisibleSeries.Add(_seriesPitch); }
            else if (RadioYaw.IsChecked == true) { VisibleSeries.Add(_seriesYaw); }
            else if (RadioRollSp.IsChecked == true) { VisibleSeries.Add(_seriesRoll); VisibleSeries.Add(_seriesRollSp); }
            else if (RadioPitchSp.IsChecked == true) { VisibleSeries.Add(_seriesPitch); VisibleSeries.Add(_seriesPitchSp); }
            else if (RadioYawSp.IsChecked == true) { VisibleSeries.Add(_seriesYaw); VisibleSeries.Add(_seriesYawSp); }
            else if (RadioRollPitch.IsChecked == true) { VisibleSeries.Add(_seriesRoll); VisibleSeries.Add(_seriesPitch); }

            if (RadioNone.IsChecked == true && !_isLogging)
                _graphTimer.Stop();
            else
                _graphTimer.Start();
        }

        private void BtnStartLog_Click(object sender, RoutedEventArgs e)
        {
            if (_isLogging) return;

            if (!SerialManager.Instance.IsConnected)
            {
                GraphAlert.ShowAlert("Bağlantı Hatası", "Port bağlantısı sağlanmadı. Log kaydetmeye başlamak için lütfen cihaza bağlanın.", true);
                return;
            }

            try
            {
                if (!Directory.Exists(_logsFolder))
                    Directory.CreateDirectory(_logsFolder);

                string filename = $"TelemetryLog_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                _logWriter = new StreamWriter(Path.Combine(_logsFolder, filename), false, Encoding.UTF8);
                _logWriter.WriteLine("Roll\tPitch\tYaw\tRollSetpoint\tPitchSetpoint\tYawSetpoint\tDate_Time");
                
                _isLogging = true;
                _graphTimer.Start(); // Ensure timer ticks for logging even if None is checked
                
                // Switch Content via Dispatcher (since standard WPF Button Content binding is implicit without MVVM)
                BtnStartLog.Content = "Logging...";
                BtnStartLog.Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#3FB950")
                );
            }
            catch (Exception ex)
            {
                GraphAlert.ShowAlert("Hata", $"Log başlatılamadı: {ex.Message}", true);
            }
        }

        private void BtnSaveLog_Click(object sender, RoutedEventArgs e)
        {
            if (!_isLogging) return;

            _isLogging = false;
            _logWriter?.Close();
            _logWriter = null;

            BtnStartLog.Content = "Start Log";
            BtnStartLog.Foreground = System.Windows.Media.Brushes.White;

            if (RadioNone.IsChecked == true)
                _graphTimer.Stop();

            GraphAlert.ShowAlert("Log Kaydedildi", "Log dosyası başarıyla /Logs klasörüne kaydedildi.", true);
        }

        private void BtnOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Directory.Exists(_logsFolder))
                    Directory.CreateDirectory(_logsFolder);

                Process.Start("explorer.exe", _logsFolder);
            }
            catch (Exception ex)
            {
                GraphAlert.ShowAlert("Hata", $"Klasör açılamadı: {ex.Message}", true);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

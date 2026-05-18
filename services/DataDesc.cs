using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using LiveChartsCore.Defaults;

namespace Aspan_Ground.services
{
    internal class DataDesc : INotifyPropertyChanged 
    {
        //SINGLETON
        private static DataDesc _instance;
        public static DataDesc Instance
        {
            get
            {
                if (_instance == null) _instance = new DataDesc();
                return _instance;
            }
        }

        // Dışarıdan "new DataDesc()" yapılmasını yasakla
        private DataDesc() 
        {
            // Sabit 100 iterasyonluk array'ler (RemoveAt Crash'i önlemek için)
            for (int i = 0; i < 100; i++)
            {
                RollValues.Add(new ObservableValue(0f));
                PitchValues.Add(new ObservableValue(0f));
                YawValues.Add(new ObservableValue(0f));
                RollSetpointValues.Add(new ObservableValue(0f));
                PitchSetpointValues.Add(new ObservableValue(0f));
                YawSetpointValues.Add(new ObservableValue(0f));
            }
        }


        //GRAFİKLER İÇİN CANLI LİSTELER
        public ObservableCollection<ObservableValue> RollValues { get; } = new ObservableCollection<ObservableValue>();
        public ObservableCollection<ObservableValue> PitchValues { get; } = new ObservableCollection<ObservableValue>();
        public ObservableCollection<ObservableValue> YawValues { get; } = new ObservableCollection<ObservableValue>();

        public ObservableCollection<ObservableValue> RollSetpointValues { get; } = new ObservableCollection<ObservableValue>();
        public ObservableCollection<ObservableValue> PitchSetpointValues { get; } = new ObservableCollection<ObservableValue>();
        public ObservableCollection<ObservableValue> YawSetpointValues { get; } = new ObservableCollection<ObservableValue>();


        //Arka Plan Değişkenleri
        private float _currentPitch;
        private float _currentRoll;
        private float _currentYaw;

        private float _rollSetpoint;
        private float _pitchSetpoint;
        private float _yawSetpoint;
        
        private float _currentAltitude;       
        
        private float _currentRH;
        private float _currentRV;
        private float _currentLH;

        private double _currentLat;
        private double _currentLon;
        
        private short _currentModelID;
        private short _currentFree1;
        private byte _currentChannel7;
        private byte _currentChannel8;
        
        private short _currentLV;
        private byte _currentSatellites;
        private byte _currentBattery;
        private byte _currentChannel5;
        private byte _currentChannel6;
        private short _currentHorizontalSpeed;
        private short _currentVerticalSpeed;
        private byte _currentMissionStatus;
        private byte _currentFlightMode;
        private byte _currentArmStatus;

        //Ön Plan WPF Özellikleri
        public float currentPitch 
        { 
            get => _currentPitch; 
            set 
            {
                SetProperty(ref _currentPitch, value);
                App.Current.Dispatcher.InvokeAsync(() => {
                    PitchValues.Add(new ObservableValue(value));
                    if(PitchValues.Count > 100) PitchValues.RemoveAt(0);
                });
            } 
        }

        public float currentRoll //100 veri aynı anda graphlarda gözükür
        { 
            get => _currentRoll; 
            set 
            {
                SetProperty(ref _currentRoll, value);
                App.Current.Dispatcher.InvokeAsync(() => {
                    RollValues.Add(new ObservableValue(value));
                    if(RollValues.Count > 100) RollValues.RemoveAt(0);
                });
            } 
        }

        public float currentYaw 
        { 
            get => _currentYaw; 
            set 
            {
                SetProperty(ref _currentYaw, value);
                App.Current.Dispatcher.InvokeAsync(() => {
                    YawValues.Add(new ObservableValue(value));
                    if(YawValues.Count > 100) YawValues.RemoveAt(0);
                });
            } 
        }

        public float rollSetpoint 
        { 
            get => _rollSetpoint; 
            set 
            {
                SetProperty(ref _rollSetpoint, value);
                App.Current.Dispatcher.InvokeAsync(() => {
                    RollSetpointValues.Add(new ObservableValue(value));
                    if(RollSetpointValues.Count > 100) RollSetpointValues.RemoveAt(0);
                });
            } 
        }

        public float pitchSetpoint 
        { 
            get => _pitchSetpoint; 
            set 
            {
                SetProperty(ref _pitchSetpoint, value);
                App.Current.Dispatcher.InvokeAsync(() => {
                    PitchSetpointValues.Add(new ObservableValue(value));
                    if(PitchSetpointValues.Count > 100) PitchSetpointValues.RemoveAt(0);
                });
            } 
        }

        public float yawSetpoint 
        { 
            get => _yawSetpoint; 
            set 
            {
                SetProperty(ref _yawSetpoint, value);
                App.Current.Dispatcher.InvokeAsync(() => {
                    YawSetpointValues.Add(new ObservableValue(value));
                    if(YawSetpointValues.Count > 100) YawSetpointValues.RemoveAt(0);
                });
            } 
        }
        
        //Anlık veri verir ekrana
        public float currentAltitude { get => _currentAltitude; set => SetProperty(ref _currentAltitude, value); }
        public float currentRH { get => _currentRH; set => SetProperty(ref _currentRH, value); }
        public float currentRV { get => _currentRV; set => SetProperty(ref _currentRV, value); }
        public float currentLH { get => _currentLH; set => SetProperty(ref _currentLH, value); }
        public short currentLV { get => _currentLV; set => SetProperty(ref _currentLV, value); }

        public double currentLat { get => _currentLat; set => SetProperty(ref _currentLat, value); }
        public double currentLon { get => _currentLon; set => SetProperty(ref _currentLon, value); }
        
        public byte currentSatellites { get => _currentSatellites; set => SetProperty(ref _currentSatellites, value); }
        public byte currentBattery { get => _currentBattery; set => SetProperty(ref _currentBattery, value); }
        public byte currentChannel5 { get => _currentChannel5; set => SetProperty(ref _currentChannel5, value); }
        public byte currentChannel6 { get => _currentChannel6; set => SetProperty(ref _currentChannel6, value); }
        public short currentHorizontalSpeed { get => _currentHorizontalSpeed; set => SetProperty(ref _currentHorizontalSpeed, value); }
        public short currentVerticalSpeed { get => _currentVerticalSpeed; set => SetProperty(ref _currentVerticalSpeed, value); }
        
        public short currentModelID { get => _currentModelID; set => SetProperty(ref _currentModelID, value); }
        public short currentFree1 { get => _currentFree1; set => SetProperty(ref _currentFree1, value); }
        public byte currentChannel7 { get => _currentChannel7; set => SetProperty(ref _currentChannel7, value); }
        public byte currentChannel8 { get => _currentChannel8; set => SetProperty(ref _currentChannel8, value); }
        public byte currentMissionStatus { get => _currentMissionStatus; set => SetProperty(ref _currentMissionStatus, value); }
        
        public byte currentFlightMode 
        { 
            get => _currentFlightMode; 
            set 
            {
                if (SetProperty(ref _currentFlightMode, value))
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(currentFlightModeString)));
                }
            } 
        }

        public string currentFlightModeString
        {
            get
            {
                switch (_currentFlightMode)
                {
                    case 1: return "MANUAL";
                    case 2: return "STABILIZE";
                    case 3: return "AUTONOMOUS";
                    default: return $"MODE {_currentFlightMode}";
                }
            }
        }

        public byte currentArmStatus
        {
            get => _currentArmStatus;
            set
            {
                if (SetProperty(ref _currentArmStatus, value))
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(currentArmStatusString)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(currentArmStatusColor)));
                }
            }
        }

        public string currentArmStatusString
        {
            get { return _currentArmStatus == 1 ? "ARMED" : "DISARMED"; }
        }

        public string currentArmStatusColor
        {
            get { return _currentArmStatus == 1 ? "#39FF14" : "#FF073A"; } // Armed = Yeşil, Disarmed = Kırmızı
        }

        //INotifyPropertyChanged Kısa Yolu
        public event PropertyChangedEventHandler PropertyChanged;
        
        // Bütün özelliklerin değiştiğini UI'a bildirir (Örn: Disconnect sonrası)
        public void NotifyAllPropertiesChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }

    }
}

//INotifyPropertyChanged: İçerideki veri değiştiği anda, ekrana (arayüze) mesaj atıp güncellemeleri sağlar.
//SetProperty: Veri değiştiyse günceller, değişmediyse hiçbir şey yapmaz.
//ObservableValue: LiveCharts2'nin kullandığı özel veri yapısıdır. İçine eklenen her değer, grafiğe otomatik olarak çizilir.
//Singleton: Tekil Kutu Mimarisi tek bir nesne oluşturur. Her yerden bu tek nesneye ulaşılır.
//MVVM: Model-View-ViewModel: Arayüz (View) ve Mantık (Model) katmanlarını ayırır. Veri akışını düzenler. MVVM sayesinde kodla arayüz asla birbirine girmez
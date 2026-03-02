using System;
using System.IO.Ports;
using System.Windows;
using System.Diagnostics;
using System.Diagnostics;
using Aspan_Ground.services;

namespace Aspan_Ground.cardconnection
{
    public class SerialManager
    {
        // --- SINGLETON YAPI ---
        private static SerialManager _instance;
        public static SerialManager Instance
        {
            get
            {
                if (_instance == null) _instance = new SerialManager();
                return _instance;
            }
        }

        private System.IO.Ports.SerialPort _serialPort;
        private Telemetry _telemetryParser;

        // Dışarıdan "new SerialManager()" yapılmasını yasakla
        private SerialManager()
        {
            _serialPort = new System.IO.Ports.SerialPort();
            _serialPort.ReadTimeout = 500; // Okuma Timeout'u eklendi ki takılı kalmasın
            _serialPort.WriteTimeout = 500;
            _serialPort.DataReceived += SerialPort_DataReceived;
            _serialPort.ErrorReceived += SerialPort_ErrorReceived;
            
            _telemetryParser = new Telemetry(); //Nesne oluşturma
        } 

        public bool IsConnected { get; private set; }
        public bool IsSynchronized { get; private set; }
        
        // Kullanıcı isteyerek mi kopardı, yoksa fiziksel kablo mu çekildi ayrımını yapmak için:
        public bool IsUserRequestedDisconnect { get; private set; }

        public event Action ConnectionSynchronized;
        
        // true=bağlandı, false=kesildi. Ayrıca dışarıya (isConnected, isUserRequested) çift parametre dönmüyoruz (eski eventleri bozmamak için),
        // Sınıf üzerinden IsUserRequestedDisconnect okunarak ayrım yapılacak.
        public event Action<bool> OnConnectionChanged;   
        public event Action<PID.PidAxis, PID.PIDController>? OnPidReceived;

        public string[] GetAvailablePorts()
        {
            return System.IO.Ports.SerialPort.GetPortNames(); // Ekrana Portları Listelemek İçin
        }

        private System.Threading.Tasks.Task _connectionMonitorTask;
        private System.Threading.CancellationTokenSource _monitorCts;
        private string _activePortName = "";
        
        // Asenkron kilitlenmeleri önlemek ve bağlantı kesilirken veriyi anında kesmek için
        private volatile bool _isDisconnecting = false;

        public void Connect(string portName, int baudRate)
        {
            if (!_serialPort.IsOpen)
            {
                IsUserRequestedDisconnect = false; // Yeni bağlantıda bayrağı sıfırla
                _isDisconnecting = false; // Veri alımı açık

                _serialPort.PortName = portName;
                _serialPort.BaudRate = baudRate;
                _serialPort.DtrEnable = true;  // Cihazın (STM/Teensy vb.) veri akışına başlamasını sağlar
                _serialPort.RtsEnable = true;  // Aynı şekilde akış kontrol mandalı
                _serialPort.Open();
                IsConnected = true; // YÖNETİM BİZDE (OS otomatik false yapsa bile bizimki true kalacak ve kopmayı yakalayacak)
                _activePortName = portName;
                OnConnectionChanged?.Invoke(true);
                Debug.WriteLine($"PORT OPENED: {portName} @ {baudRate}");

                // Port kontrol döngüsünü başlat (Fiziksel kablo kopmasını yakalamak için Polling)
                StartConnectionMonitor(portName);
            }
        }

        private System.Management.ManagementEventWatcher _usbWatcher;

        private void StartConnectionMonitor(string currentPort)
        {
            StopConnectionMonitor(); // Önceki watcher'ı temizle
            
            try
            {
                // WMI (Windows Management Instrumentation) ile donanım çıkarılmasını O.S. seviyesinde dinle
                var query = new System.Management.WqlEventQuery(
                    "SELECT * FROM __InstanceDeletionEvent WITHIN 1 WHERE TargetInstance ISA 'Win32_PnPEntity'"
                );

                _usbWatcher = new System.Management.ManagementEventWatcher(query);
                _usbWatcher.EventArrived += (s, e) =>
                {
                    if (!IsConnected) return;

                    var instance = (System.Management.ManagementBaseObject)e.NewEvent["TargetInstance"];
                    var name = instance["Name"]?.ToString() ?? "";
                    
                    // Örn: "USB Serial Device (COM5)" içinden "(COM5)" ararız
                    if (name.Contains($"({currentPort})", StringComparison.OrdinalIgnoreCase))
                    {
                        Debug.WriteLine($"[SerialManager] WMI: USB DONANIM KOPMASI ALGILANDI! Aygıt çıkarıldı: {name}");
                        
                        Application.Current?.Dispatcher.InvokeAsync(() =>
                        {
                            ForceDisconnect();
                        });
                    }
                };

                _usbWatcher.Start();
                Debug.WriteLine($"[SerialManager] WMI Watcher başlatıldı: {currentPort} dinleniyor.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SerialManager] WMI Watcher başlatılamadı: {ex.Message}");
                // Alternatif fallback: Geriye dönük polling (Eğer WMI servisi kapalıysa)
                StartFallbackPolling(currentPort);
            }
        }

        private void StartFallbackPolling(string currentPort)
        {
            _monitorCts?.Cancel();
            _monitorCts = new System.Threading.CancellationTokenSource();

            _connectionMonitorTask = System.Threading.Tasks.Task.Run(async () =>
            {
                while (!_monitorCts.Token.IsCancellationRequested)
                {
                    await System.Threading.Tasks.Task.Delay(1000, _monitorCts.Token);
                    if (!IsConnected) break;

                    try
                    {
                        if (_serialPort != null && _serialPort.IsOpen)
                        {
                            bool dsr = _serialPort.DsrHolding; // Bu okuma koptuğunda hata fırlatabilir
                        }
                    }
                    catch (Exception)
                    {
                        Application.Current?.Dispatcher.InvokeAsync(() => ForceDisconnect());
                        break;
                    }
                }
            }, _monitorCts.Token);
        }

        private void StopConnectionMonitor()
        {
            try { _monitorCts?.Cancel(); } catch { }
            try 
            {
                if (_usbWatcher != null)
                {
                    _usbWatcher.Stop();
                    _usbWatcher.Dispose();
                    _usbWatcher = null;
                }
            } 
            catch { }
        }


        // Yalnızca kullanıcı "Disconnect" butonuna bastığında çağrılmalı.
        public void Disconnect()
        {
            if (IsConnected)
            {
                IsUserRequestedDisconnect = true; 
                InternalDisconnect();
            }
        }

        // Hata durumunda veya iç sistemden kablo koptuğunda çağrılır
        private void ForceDisconnect()
        {
            if (IsConnected)
            {
                IsUserRequestedDisconnect = false; // Kullanıcı isteği değil!
                InternalDisconnect();
            }
        }

        private void InternalDisconnect()
        {
            IsConnected = false;
            _isDisconnecting = true; // Tüm veri alımını (DataReceived) o saniye durdur
            try { _monitorCts?.Cancel(); } catch { } // İzlemeyi durdur

            // Deadlock önlemek için port kapatma işlemini arka planda yapıyoruz.
            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    if (_serialPort != null && _serialPort.IsOpen)
                    {
                        // Port okuma döngüsünü (BaseStream) kırmaya zorla
                        _serialPort.DiscardInBuffer();
                        _serialPort.DiscardOutBuffer();
                        _serialPort.Close();
                    }
                    _buffer.Clear(); // Kuyrukta kalan son paketleri çöpe at
                }
                catch { }
                finally
                {
                    Application.Current?.Dispatcher.InvokeAsync(() =>
                        OnConnectionChanged?.Invoke(false));
                }
            });
            IsSynchronized = false;
        }

        public void Send(byte[] data)
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                try
                {
                    _serialPort.Write(data, 0, data.Length);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Veri gönderme hatası: {ex.Message}");
                    if (ex is System.IO.IOException || ex is UnauthorizedAccessException)
                    {
                        ForceDisconnect();
                    }
                }
            }
        }

        private void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            Debug.WriteLine($"Serial Port Error: {e.EventType}");
            ForceDisconnect();
        }

        private List<byte> _buffer = new List<byte>();

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (_isDisconnecting || _serialPort == null || !_serialPort.IsOpen)
                return;

            try
            {
                int bytesToRead = _serialPort.BytesToRead;
                byte[] tempBuffer = new byte[bytesToRead];
                _serialPort.Read(tempBuffer, 0, bytesToRead);

                _buffer.AddRange(tempBuffer);

                while (_buffer.Count >= 20)
                {
                    // Paket başlık kontrolü (0x46 = 'F', 0x43 = 'C')
                    if (_buffer[0] == 0x46 && _buffer[1] == 0x43)
                    {
                        byte[] packet = _buffer.GetRange(0, 20).ToArray();
                        _buffer.RemoveRange(0, 20);

                        byte checksum = 0xFF;
                        for (int i = 0; i < 19; i++)
                            checksum = (byte)(checksum - packet[i]);

                        if (packet[19] == checksum)
                        {
                            byte packetType = packet[2];
                            
                            // string hexDump = string.Join(" ", packet.Select(b => b.ToString("X2")));
                            // Debug.WriteLine($"PAKET YAKALANDI: Type={packetType} | Hex: {hexDump}");

                            // Deadlock önlemek için Invoke yerine BeginInvoke (Asenkron kuyruklama) kullanıyoruz.
                            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                if (packetType == 16) // Telemetry (Eskiden 0'dı, cihaz 16 gönderiyor)
                                {
                                    TelemetryData telem = _telemetryParser.ParseTelemetry(packet);

                                    DataDesc.Instance.currentRoll = telem.Roll;
                                    DataDesc.Instance.currentPitch = telem.Pitch;
                                    DataDesc.Instance.currentAltitude = telem.Altitude;
                                    DataDesc.Instance.currentYaw = telem.Yaw;
                                    DataDesc.Instance.currentRH = telem.RH;
                                    DataDesc.Instance.currentRV = telem.RV;
                                    DataDesc.Instance.currentLH = telem.LH;
                                    DataDesc.Instance.currentLV = telem.LV;

                                    DataDesc.Instance.rollSetpoint = (telem.RH - 1000) / 12f;
                                    DataDesc.Instance.pitchSetpoint = (telem.RV - 1000) / 12f;
                                    DataDesc.Instance.yawSetpoint = (telem.LH - 1000) / 12f;
                                    
                                    // Eğer sistem eşleşmediyse, ilk geçerli veriyle sistemi senkronize edelim
                                    if (!IsSynchronized)
                                    {
                                        IsSynchronized = true;
                                        ConnectionSynchronized?.Invoke();
                                    }
                                }
                                else if (packetType == 17) // Gps (Eskiden 1'di, cihaz 17 gönderiyor)
                                {
                                    GpsData gps = _telemetryParser.ParseGps(packet);

                                    DataDesc.Instance.currentLat = gps.Latitude;
                                    DataDesc.Instance.currentLon = gps.Longitude;
                                    DataDesc.Instance.currentSatellites = gps.Satellites;
                                    DataDesc.Instance.currentBattery = gps.Battery;
                                    DataDesc.Instance.currentChannel5 = gps.Channel5;
                                    DataDesc.Instance.currentChannel6 = gps.Channel6;
                                    DataDesc.Instance.currentHorizontalSpeed = gps.HorizontalSpeed;
                                    DataDesc.Instance.currentVerticalSpeed = gps.VerticalSpeed;
                                }
                                else if (packetType == 18) // ModelInfo
                                {
                                    ModelData model = _telemetryParser.ParseModelInfo(packet);
                                    
                                    DataDesc.Instance.currentModelID = model.ModelID;
                                    DataDesc.Instance.currentFree1 = model.Free1;
                                    DataDesc.Instance.currentChannel7 = model.Channel7;
                                    DataDesc.Instance.currentChannel8 = model.Channel8;
                                }
                                else if (packetType >= 0x00 && packetType <= 0x05)
                                {
                                    // PID Cevap Paketi: Firmware Encode_Msg_PID_Gain
                                    //   packet[2] = axis_id (0..5) -- doğrudan axis ID!
                                    //   packet[3..6]  = P  (float)
                                    //   packet[7..10] = I  (float)
                                    //   packet[11..14]= D  (float)
                                    //   packet[15..18]= Extra (float)
                                    PID pidParser = new PID();
                                    byte axisId = packetType; // packet[2] = axis_id
                                    
                                    try
                                    {
                                        PID.PIDController gains = pidParser.ParsePid(packet, axisId);
                                        Debug.WriteLine($"[SerialManager] PID Alındı: axis={axisId} P={gains.P} I={gains.I} D={gains.D} E={gains.Extra}");
                                        OnPidReceived?.Invoke((PID.PidAxis)axisId, gains);
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.WriteLine($"[SerialManager] PID Parse Hatası: {ex.Message}");
                                    }
                                }
                                // Eski ID'ler temizlendi.
                            }));
                        }
                    }
                    else
                    {
                        _buffer.RemoveAt(0);
                    }
                }
            }
            catch (TimeoutException)
            {
                // Timeout normaldir, kablo çıkarılmış ve okuma takılmış olabilir.
            }
            catch (Exception ex)
            {
                if (ex is System.IO.IOException || ex is UnauthorizedAccessException || ex is InvalidOperationException)
                {
                    Application.Current?.Dispatcher.InvokeAsync(() => ForceDisconnect());
                }

                // Ekrana veya konsola debug bas.
                Debug.WriteLine($"Veri alınırken hata: {ex.Message}");
            }
        }
    }
}

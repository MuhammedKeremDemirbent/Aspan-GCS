using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Aspan_Ground.cardconnection;

namespace Aspan_Ground.pages
{
    public partial class PIDView : UserControl
    {
        private PID.PIDController[] pidGains = new PID.PIDController[10];
        private bool[] pidValid = new bool[10];

        public PIDView()
        {
            InitializeComponent();
            this.Loaded   += PIDView_Loaded;
            this.Unloaded += PIDView_Unloaded;
        }

        private void PIDView_Loaded(object sender, RoutedEventArgs e)
        {
            SerialManager.Instance.OnPidReceived += SerialManager_OnPidReceived;
        }

        private void PIDView_Unloaded(object sender, RoutedEventArgs e)
        {
            SerialManager.Instance.OnPidReceived -= SerialManager_OnPidReceived;
        }

        private void SerialManager_OnPidReceived(PID.PidAxis axis, PID.PIDController gains)
        {
            Dispatcher.InvokeAsync(() =>
            {
                pidGains[(int)axis] = gains;
                pidValid[(int)axis] = true;
                UpdatePidUI(axis, gains);
            });
        }

        // ── Bağlantı uyarısı (projenin custom alert'i) ────────
        private void ShowConnectionAlert()
        {
            (Application.Current.MainWindow as MainWindow)?.ShowError(
                "Bağlantı Yok",
                "Seri port kapalı!\nLütfen önce üst panelden bağlanın.");
        }

        // ── WRITE: bool döner, mesaj GÖSTERMEZ ────────────────
        private bool WritePid(PID.PidAxis axis, PID.PIDController gains)
        {
            if (!SerialManager.Instance.IsConnected)
                return false;

            PID pidTool = new PID();
            byte[] packet = pidTool.EncodePidWrite((byte)axis, gains);
            SerialManager.Instance.Send(packet);
            pidGains[(int)axis] = gains;
            pidValid[(int)axis] = true;
            Debug.WriteLine($"[PID] YAZILDI → {axis} P={gains.P} I={gains.I} D={gains.D} E={gains.Extra}");
            return true;
        }

        // ── READ: bool döner, mesaj GÖSTERMEZ ─────────────────
        private bool ReadPid(PID.PidAxis axis)
        {
            if (!SerialManager.Instance.IsConnected)
                return false;

            PID pidTool = new PID();
            byte[] packet = pidTool.EncodePidReadRequest((byte)axis);
            SerialManager.Instance.Send(packet);
            Debug.WriteLine($"[PID] READ REQUEST → {axis}");
            return true;
        }

        // ── UI UPDATE ──────────────────────────────────────────
        private void UpdatePidUI(PID.PidAxis axis, PID.PIDController pid)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.InvokeAsync(() => UpdatePidUI(axis, pid));
                return;
            }
            try
            {
                switch (axis)
                {
                    case PID.PidAxis.Roll:
                        NumericPR.Value     = (double)pid.P;
                        NumericIR.Value     = (double)pid.I;
                        NumericDR.Value     = (double)pid.D;
                        NumericEXTRAR.Value = (double)pid.Extra;
                        break;
                    case PID.PidAxis.Pitch:
                        NumericPP.Value      = (double)pid.P;
                        NumericIP.Value      = (double)pid.I;
                        NumericDP.Value      = (double)pid.D;
                        NumericIEXTRAP.Value = (double)pid.Extra;
                        break;
                    case PID.PidAxis.Yaw:
                        NumericPY.Value     = (double)pid.P;
                        NumericIY.Value     = (double)pid.I;
                        NumericDY.Value     = (double)pid.D;
                        NumericEXTRAY.Value = (double)pid.Extra;
                        break;
                    case PID.PidAxis.Height:
                        NumericPH.Value     = (double)pid.P;
                        NumericIH.Value     = (double)pid.I;
                        NumericDH.Value     = (double)pid.D;
                        NumericEXTRAH.Value = (double)pid.Extra;
                        break;
                    case PID.PidAxis.East:
                        NumericPE.Value     = (double)pid.P;
                        NumericIE.Value     = (double)pid.I;
                        NumericDE.Value     = (double)pid.D;
                        NumericEXTRAE.Value = (double)pid.Extra;
                        break;
                    case PID.PidAxis.North:
                        NumericPN.Value     = (double)pid.P;
                        NumericIN.Value     = (double)pid.I;
                        NumericDN.Value     = (double)pid.D;
                        NumericEXTRAN.Value = (double)pid.Extra;
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PID] UI güncelleme hatası ({axis}): {ex.Message}");
            }
        }

        // ── FLASH ──────────────────────────────────────────────
        private async void FlashButton(Button btn, string tmpText, string origText)
        {
            if (btn == null) return;
            var origBg = btn.Background;
            btn.Content    = tmpText;
            btn.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(26, 127, 55));
            await System.Threading.Tasks.Task.Delay(900);
            btn.Content    = origText;
            btn.Background = origBg;
        }

        // ── ROLL ───────────────────────────────────────────────
        private void ButtonRollRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (!ReadPid(PID.PidAxis.Roll)) { ShowConnectionAlert(); return; }
            FlashButton(sender as Button, "Reading...", "Read");
        }
        private void ButtonRollWrite_Click(object sender, RoutedEventArgs e)
        {
            if (!WritePid(PID.PidAxis.Roll, new PID.PIDController(
                (float)(NumericPR.Value    ?? 0), (float)(NumericIR.Value    ?? 0),
                (float)(NumericDR.Value    ?? 0), (float)(NumericEXTRAR.Value ?? 0))))
            { ShowConnectionAlert(); return; }
            FlashButton(sender as Button, "Sent ✓", "Write");
        }

        // ── PITCH ──────────────────────────────────────────────
        private void ButtonPitchRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (!ReadPid(PID.PidAxis.Pitch)) { ShowConnectionAlert(); return; }
            FlashButton(sender as Button, "Reading...", "Read");
        }
        private void ButtonPitchWrite_Click(object sender, RoutedEventArgs e)
        {
            if (!WritePid(PID.PidAxis.Pitch, new PID.PIDController(
                (float)(NumericPP.Value      ?? 0), (float)(NumericIP.Value      ?? 0),
                (float)(NumericDP.Value      ?? 0), (float)(NumericIEXTRAP.Value ?? 0))))
            { ShowConnectionAlert(); return; }
            FlashButton(sender as Button, "Sent ✓", "Write");
        }

        // ── YAW ────────────────────────────────────────────────
        private void ButtonYawRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (!ReadPid(PID.PidAxis.Yaw)) { ShowConnectionAlert(); return; }
            FlashButton(sender as Button, "Reading...", "Read");
        }
        private void ButtonYawWrite_Click(object sender, RoutedEventArgs e)
        {
            if (!WritePid(PID.PidAxis.Yaw, new PID.PIDController(
                (float)(NumericPY.Value     ?? 0), (float)(NumericIY.Value     ?? 0),
                (float)(NumericDY.Value     ?? 0), (float)(NumericEXTRAY.Value ?? 0))))
            { ShowConnectionAlert(); return; }
            FlashButton(sender as Button, "Sent ✓", "Write");
        }

        // ── HEIGHT ─────────────────────────────────────────────
        private void ButtonHeightRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (!ReadPid(PID.PidAxis.Height)) { ShowConnectionAlert(); return; }
            FlashButton(sender as Button, "Reading...", "Read");
        }
        private void ButtonHeightWrite_Click(object sender, RoutedEventArgs e)
        {
            if (!WritePid(PID.PidAxis.Height, new PID.PIDController(
                (float)(NumericPH.Value     ?? 0), (float)(NumericIH.Value     ?? 0),
                (float)(NumericDH.Value     ?? 0), (float)(NumericEXTRAH.Value ?? 0))))
            { ShowConnectionAlert(); return; }
            FlashButton(sender as Button, "Sent ✓", "Write");
        }

        // ── EAST ───────────────────────────────────────────────
        private void ButtonEastRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (!ReadPid(PID.PidAxis.East)) { ShowConnectionAlert(); return; }
            FlashButton(sender as Button, "Reading...", "Read");
        }
        private void ButtonEastWrite_Click(object sender, RoutedEventArgs e)
        {
            if (!WritePid(PID.PidAxis.East, new PID.PIDController(
                (float)(NumericPE.Value     ?? 0), (float)(NumericIE.Value     ?? 0),
                (float)(NumericDE.Value     ?? 0), (float)(NumericEXTRAE.Value ?? 0))))
            { ShowConnectionAlert(); return; }
            FlashButton(sender as Button, "Sent ✓", "Write");
        }

        // ── NORTH ──────────────────────────────────────────────
        private void ButtonNorthRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (!ReadPid(PID.PidAxis.North)) { ShowConnectionAlert(); return; }
            FlashButton(sender as Button, "Reading...", "Read");
        }
        private void ButtonNorthWrite_Click(object sender, RoutedEventArgs e)
        {
            if (!WritePid(PID.PidAxis.North, new PID.PIDController(
                (float)(NumericPN.Value     ?? 0), (float)(NumericIN.Value     ?? 0),
                (float)(NumericDN.Value     ?? 0), (float)(NumericEXTRAN.Value ?? 0))))
            { ShowConnectionAlert(); return; }
            FlashButton(sender as Button, "Sent ✓", "Write");
        }

        // ── READ ALL ───────────────────────────────────────────
        // Bağlantı 1 kere kontrol edilir, 1 uyarı gösterilir
        private void ButtonReadAll_Click(object sender, RoutedEventArgs e)
        {
            if (!SerialManager.Instance.IsConnected)
            {
                ShowConnectionAlert();
                return;
            }

            // Firmware (telemetry.c) üzerindeki case 6 (Read All) komutunu tetikler.
            // Tek bir paket gider, cihaz tüm PID'leri sırayla yanıtlayarak döner.
            ReadPid(PID.PidAxis.All);
            
            FlashButton(sender as Button, "Reading All...", "Read All");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspan_Ground.cardconnection
{
    public class PID
    {
        public struct PIDController
        {
            public float P;
            public float I;
            public float D;
            public float Extra;

            public PIDController(float p = 0, float i = 0, float d = 0, float extra = 0)
            {
                P = p;
                I = i;
                D = d;
                Extra = extra;
            }
        }

        public enum PidAxis : byte
        {
            Roll   = 0,
            Pitch  = 1,
            Yaw    = 2,
            Height = 3,
            East   = 4,
            North  = 5
        }

        // ──────────────────────────────────────────────────
        //  ParsePid  →  Firmware: Encode_Msg_PID_Gain
        //
        //  Firmware paketi:
        //    [0..1] = 0x46 0x43
        //    [2]    = axis_id  (0-5)
        //    [3..6] = P  (float, little-endian)
        //    [7..10]= I  (float)
        //    [11..14]= D (float)
        //    [15..18]= Extra (float)
        //    [19]   = checksum
        // ──────────────────────────────────────────────────
        public PIDController ParsePid(byte[] packet, byte id)
        {
            if (packet == null || packet.Length < 20)
                throw new Exception("Geçersiz PID paketi (uzunluk < 20)");

            float p     = BitConverter.ToSingle(packet, 3);   // offset 3 (firmware: *(float*)&buf[3])
            float i     = BitConverter.ToSingle(packet, 7);   // offset 7
            float d     = BitConverter.ToSingle(packet, 11);  // offset 11
            float extra = BitConverter.ToSingle(packet, 15);  // offset 15

            return new PIDController { P = p, I = i, D = d, Extra = extra };
        }

        // ──────────────────────────────────────────────────
        //  EncodePidReadRequest  →  Firmware case 0x10
        //
        //  Firmware trigger:
        //    telemetry_rx_buf[2] = 0x10
        //    telemetry_rx_buf[3] = axis_id
        // ──────────────────────────────────────────────────
        public byte[] EncodePidReadRequest(byte axisId)
        {
            byte[] packet = new byte[20];
            packet[0] = 0x46;
            packet[1] = 0x43;
            packet[2] = 0x10;    // Read-request komutu (firmware case 0x10)
            packet[3] = axisId;  // Hangi eksenin verisi istensin
            for (int i = 4; i < 19; i++) packet[i] = 0x00;

            byte chk = 0xFF;
            for (int i = 0; i < 19; i++) chk -= packet[i];
            packet[19] = chk;

            return packet;
        }

        // ──────────────────────────────────────────────────
        //  EncodePidWrite  →  Firmware case 0..5
        //
        //  Firmware trigger:
        //    telemetry_rx_buf[2] = axis_id (0-5) — doğrudan!
        //    telemetry_rx_buf[3..6]  = P  (float)
        //    telemetry_rx_buf[7..10] = I
        //    telemetry_rx_buf[11..14]= D
        //    telemetry_rx_buf[15..18]= Extra
        // ──────────────────────────────────────────────────
        public byte[] EncodePidWrite(byte axisId, PIDController gains)
        {
            byte[] packet = new byte[20];
            packet[0] = 0x46;
            packet[1] = 0x43;
            packet[2] = axisId;  // Axis ID doğrudan (0x21 değil!)

            BitConverter.GetBytes(gains.P).CopyTo(packet, 3);     // offset 3
            BitConverter.GetBytes(gains.I).CopyTo(packet, 7);     // offset 7
            BitConverter.GetBytes(gains.D).CopyTo(packet, 11);    // offset 11
            BitConverter.GetBytes(gains.Extra).CopyTo(packet, 15); // offset 15

            byte chk = 0xFF;
            for (int i = 0; i < 19; i++) chk -= packet[i];
            packet[19] = chk;

            return packet;
        }
    }
}

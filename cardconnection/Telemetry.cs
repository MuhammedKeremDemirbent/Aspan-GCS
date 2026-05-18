using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspan_Ground.cardconnection
{
    internal class Telemetry
    {
        public TelemetryData ParseTelemetry(byte[] packet)
        {
            short roll = BitConverter.ToInt16(packet, 3);
            short pitch = BitConverter.ToInt16(packet, 5);
            ushort yaw = BitConverter.ToUInt16(packet, 7);
            short baroAltFilt = BitConverter.ToInt16(packet, 9);
            short RH = BitConverter.ToInt16(packet, 11);
            short RV = BitConverter.ToInt16(packet, 13);
            short LH = BitConverter.ToInt16(packet, 15);
            short LV = BitConverter.ToInt16(packet, 17);

            return new TelemetryData
            {
                Roll = roll / 100.0f,
                Pitch = pitch / 100.0f,
                Yaw = yaw / 100.0f,
                Altitude = baroAltFilt / 10.0f,
                RH = RH,
                RV = RV,
                LH = LH,
                LV = LV
            };
        }

        public GpsData ParseGps(byte[] packet)
        {
            int lat = BitConverter.ToInt32(packet, 3);
            int lon = BitConverter.ToInt32(packet, 7);

            return new GpsData
            {
                Latitude = lat / 1e7, // Assuming scaled by 1e7 externally
                Longitude = lon / 1e7,
                Satellites = packet[11],
                Battery = packet[12],
                Channel5 = packet[13],
                Channel6 = packet[14],
                HorizontalSpeed = BitConverter.ToInt16(packet, 15),
                VerticalSpeed = BitConverter.ToInt16(packet, 17)
            };
        }

        public ModelData ParseModelInfo(byte[] packet)
        {
            return new ModelData
            {
                ModelID = BitConverter.ToInt16(packet, 3),
                Free1 = BitConverter.ToInt16(packet, 5),
                Channel7 = packet[7],
                Channel8 = packet[8],
                FlightMode = packet[9],
                ArmStatus = packet[10]
            };
        }

        public MissionData ParseMissionInfo(byte[] packet)
        {
            // packet[0] = 'B', packet[1] = 'G'
            // packet[2] = GOREV_CEVAP_ID
            // packet[3] = gorev verisi
            // packet[4] = checksum
            
            // Checksum doğrulaması:
            byte expectedChecksum = (byte)(packet[2] + packet[3]);
            bool isChecksumValid = (expectedChecksum == packet[4]);

            return new MissionData
            {
                PaketID = packet[2],
                GorevStatus = packet[3],
                IsChecksumValid = isChecksumValid
            };
        }
    }

    public struct ModelData
    {
        public short ModelID;
        public short Free1;
        public byte Channel7;
        public byte Channel8;
        public byte FlightMode;
        public byte ArmStatus;
    }

    public struct TelemetryData
    {
        public float Roll;
        public float Pitch;
        public float Yaw;
        public float Altitude;
        public short RH;
        public short RV;
        public short LH;
        public short LV;
    }

    public struct GpsData
    {
        public double Latitude;
        public double Longitude;
        public byte Satellites;
        public byte Battery;
        public byte Channel5;
        public byte Channel6;
        public short HorizontalSpeed;
        public short VerticalSpeed;
    }

    public struct MissionData
    {
        public byte PaketID;
        public byte GorevStatus;
        public bool IsChecksumValid; // İletişim hatası var mı diye kontrol etmek için
    }
}

    

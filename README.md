<<<<<<< HEAD
# Aspan Ground Control Station

A professional, feature-rich Windows-based Ground Control Station (GCS) software for drone and UAV operations. Built with C# and WPF, this application provides real-time telemetry monitoring, mission planning, and PID tuning capabilities.

## Sustainable Development Goals

- **SDG 9 (Industry, Innovation and Infrastructure):** Fosters innovation in unmanned aerial vehicle (UAV) technology by providing robust, accessible ground control infrastructure.
- **SDG 11 (Sustainable Cities and Communities):** Supports the deployment of drones for urban planning, environmental monitoring, and emergency response operations.
- **SDG 4 (Quality Education):** Serves as an educational tool for aerospace and robotics engineering students to understand flight dynamics, PID tuning, and telemetry systems.

---

## Project Overview

Aspan Ground is designed to be the central command hub for UAV systems, offering:

1. **Serial Connection Management** - Reliable communication with the drone flight controller.
2. **Real-Time Telemetry & Instruments** - Live visualization of altitude, speed, attitude, and heading.
3. **PID Tuning Interface** - On-the-fly adjustment of control loops for optimal flight performance.
4. **Mission Planning** - Map-based route creation and tracking.
5. **Data Visualization** - Live graphs and logging of sensor data.

---

## Project Structure

```
Aspan_Ground/
├── cardconnection/          # Serial communication and data parsing
│   ├── SerialManager.cs     # Manages COM port connections
│   ├── Telemetry.cs         # Telemetry data structures
│   └── PID.cs               # PID tuning data handling
├── components/              # Reusable UI components
│   ├── atoms/               # Basic UI elements (e.g., ComboBox)
│   ├── molecules/           # Composite UI elements (Alerts, Top Panels)
│   └── instruments/         # WPF Flight Instruments
│       ├── AltimeterInstrumentControl.cs
│       ├── AttitudeIndicatorInstrumentControl.cs
│       ├── HeadingIndicatorInstrumentControl.cs
│       └── ...
├── pages/                   # Main application views
│   ├── MissionView.xaml     # Map and waypoint planning
│   ├── PIDView.xaml         # Control loop tuning interface
│   ├── GraphView.xaml       # Real-time sensor data charting
│   ├── WatchView.xaml       # Dashboard for critical flight data
│   ├── LogsView.xaml        # Raw sensor logs
│   └── DataView.xaml        # Comprehensive data overview
├── MainWindow.xaml          # Main application window and navigation
├── App.xaml                 # Application entry point and global styles
├── AspanGround.iss          # Inno Setup installer script
└── README.md                # Project documentation
```

---

## Key Features

### Flight Instruments
Fully interactive, avionics-style WPF controls including:
- **Attitude Indicator (Artificial Horizon)**
- **Altimeter & Vertical Speed Indicator**
- **Heading Indicator (Compass)**
- **Airspeed Indicator**
- **Turn Coordinator**

### Telemetry & Logging
- Parses raw serial data into structured telemetry updates.
- Robust error handling for unexpected USB disconnections.
- Raw sensor logging capabilities.

### User Interface
- Modern, dark-themed UI built with MahApps.Metro.
- Modular page navigation (Watch, Data, Graph, PID, Mission, Logs).
- Custom alert and notification system.

---

## Quick Start

### Prerequisites

- Windows OS
- .NET 8.0 SDK (or compatible .NET Core version)
- Visual Studio 2022 (Recommended)

### Installation & Build

1. **Clone the repository**
```bash
git clone https://github.com/MuhammedKeremDemirbent/Aspan_Ground.git
cd Aspan_Ground
```

2. **Open the Solution**
Double-click `Aspan_Ground.sln` to open the project in Visual Studio.

3. **Build and Run**
- Restore NuGet packages (MahApps.Metro, etc.).
- Press `F5` or click **Start** in Visual Studio to run the application.

4. **Building Installer**
Use Inno Setup with the provided `AspanGround.iss` file to compile a standalone Windows installer.

---

## Usage Guide

1. **Connection:** Select the appropriate COM port and Baud Rate from the top navigation bar and click **Connect**.
2. **Dashboard:** Navigate to the **Watch** view to observe live flight instruments.
3. **Tuning:** Use the **PID** view to adjust Proportional, Integral, and Derivative gains in real-time.
4. **Monitoring:** Switch to the **Graph** view for live charting of specific telemetry metrics.
5. **Planning:** Use the **Mission** view to plan and monitor autonomous flight paths.

---

## Author

**Muhammed Kerem Demirbent**

- GitHub: [MuhammedKeremDemirbent](https://github.com/MuhammedKeremDemirbent)

---

## Disclaimer

This software is intended for developmental and experimental use. Always adhere to local aviation regulations and safety guidelines when operating Unmanned Aerial Vehicles.
=======
Ground Control Station for UAV's.
>>>>>>> 1e7d3758d7251d7f9496d0b500c09ebecca6382b

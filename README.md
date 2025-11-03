# 🚗 Vehicle Tracking Data Sender (TCP Client Desktop App)

A **desktop application** designed to **send real-time vehicle tracking data** to a remote **TCP server** or central tracking system.  
This app simulates or transmits GPS data (latitude, longitude, speed, timestamp, etc.) from vehicles over a **TCP Client** connection.

---

## 🚀 Features

- 📡 **TCP Client Communication**
  - Establishes a stable connection with a remote TCP listener/server.
  - Supports continuous or interval-based data transmission.
  - Handles automatic reconnects on connection loss.

- 🛰️ **Real-Time GPS Data Simulation**
  - Sends dynamic vehicle data including:
    - Latitude / Longitude
    - Speed
    - Direction
    - Timestamp
    - IMEI / Device ID
  - Supports both **simulated** and **live GPS** data sources.

- ⚙️ **Customizable Configuration**
  - User-defined TCP server IP and port.
  - Adjustable data send interval.
  - Custom data format templates.

- 🧾 **Logging and Monitoring**
  - Displays live logs of connection status and data packets.
  - Records all sent data to local log files for auditing.

- 🔐 **Reliable Transmission**
  - Includes error handling, retry mechanism, and connection validation.
  - Supports encryption (optional future upgrade).

---

## 🛠️ Technologies Used

| Component | Technology |
|-----------|------------|
| **Language** | C# |
| **Framework** | .NET Framework / .NET Core |
| **Networking** | TCP Client (System.Net.Sockets) |
| **UI Framework** | Windows Forms / WPF |
| **Database (optional)** | SQLite for log persistence |
| **Logging** | NLog / Serilog |

---

## 💻 Installation & Setup

1. Clone the repository:
   ```bash
   git clone https://github.com/your-username/TcpClientVehicleTrackingApp.git

----------------------------------

🧰 Example Data Packet

Sample data format sent to the TCP server:

IMEI:359710055555555, Lat:23.780573, Lng:90.279239, Speed:45, Time:2025-11-03 12:45:30


Customizable data templates allow adapting to specific GPS tracker protocols.

----------------------------------

📡 Example Workflow

Application connects to remote TCP server using specified IP/Port.

Vehicle data (real or simulated) is generated at each interval.

TCP client sends formatted data to the server.

Server receives and processes location data for tracking or storage.

-------------------------------

🧭 Example UI Overview

Connection Settings: Configure server IP, port, and send frequency.

Live Log Panel: Displays real-time connection and data transfer logs.

Control Buttons: Connect / Disconnect / Start / Stop sending.

Status Indicator: Connection status (Connected / Disconnected).

--------------------------

📚 Future Enhancements

SSL/TLS encrypted TCP communication.

Integration with GPS devices via COM port.

Support for UDP mode.

Multi-vehicle simulation.

WebSocket/REST integration for hybrid tracking systems.

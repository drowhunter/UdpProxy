# UdpProxy

## Overview
UdpProxy is a lightweight and efficient proxy server for UDP traffic. It is designed to handle high-throughput and low-latency applications.

## Features
- High performance
- Low latency
- Easy to configure
- Supports multiple clients

## Installation
To install UdpProxy, clone the repository and build the project:
```sh
git clone https://github.com/yourusername/UdpProxy.git
cd UdpProxy
make
```

## Usage

To start the UdpProxy server, here are some examples:

---

Listen for traffic on port 20778 and forward to 20777
```
udp-proxy.exe --listen-port 20778 --fwd-ports 20777
```

Listen for traffic on port 20778 and forward to 20777 and 20779
```
udp-proxy.exe --listen-port 20778 --fwd-ports 20777,20779
```

Listen for traffic on port 20778 and forward to 20777 and launch a game
```
udp-proxy.exe --listen-port 20778 --fwd-ports 20777,20779 --run-command "C:\path\to\game.exe"
```

### Program Launching
The following examples show how you can use udpproxy as a launcher ,  (note any processes launched or monitored in this way will cause udpproxy.exe to automatically quit when the process ends )

---
Listen for traffic on port 20778 and forward to 20777 and launch a game and wait for a different process to launch.  Useful if your game has a launcher thats different than the main exe
```
udp-proxy.exe --listen-port 20778 --fwd-ports 20777,20779 --run-command "C:\path\to\launcher.exe" --process-name "game.exe"
```

Listen for traffic on port 20778 and forward to 20777 launch dirt rally 2 on steam and wait for the process
```
udp-proxy.exe --listen-port 20778 --fwd-ports 20777,20779 --run-command "steam://rungameid/690790" --process-name "dirtrally2.exe"
```
## Configuration
The follwing arguments are supported:

```
--listen-port  <int>         (required) The port to bind (listen) on for incoming data
--fwd-ports    <int,int,...> (required) The ports to forward data to
--run-command  <string>                 The command to run the application
--process-name <string>                 The name of the proccess to monitor
--debug-mode                            If this is passed you will see characters printed representing the inbound and outbound packets. "." = in and "x" = out
```

## Contributing
Contributions are welcome! Please fork the repository and submit a pull request.

## License
This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Contact
For any questions or issues, please open an issue on GitHub or contact the maintainer at your.email@example.com.
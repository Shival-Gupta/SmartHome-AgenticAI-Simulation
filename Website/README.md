# SmartHome Website

This web interface controls the `SmartHome-AgenticAI-Simulation`. It provides a dashboard for manual control and AI-driven commands via text or voice.

## Directory Structure

```
Website/
├── index.html          # Main landing page
├── README.md           # This guide
├── assets/
│   ├── css/
│   │   └── main.css    # All styles
│   ├── js/
│   │   └── main.js     # All JavaScript logic
│   └── favicon.ico     # Site icon
└── controller/
    ├── index.html    # Simplified chooser page
    └── v1 # Main control interface (Advanced AI)
```

## Setup

1.  **Run Simulation**: Start `../UnitySimulation/Environment Samsung.exe` (see [UnitySimulation README](../UnitySimulation/README.md)). The simulation must be running for the website to connect via WebSocket.
2.  **Serve Website**:
    Navigate to the `Website` directory in your terminal and start a simple web server (requires Python 3):
    ```bash
    cd Website
    python -m http.server 8000
    ```
    *(If you don't have Python, use another local server like Node.js `http-server`)*
3.  **Open Browser**: Visit [http://localhost:8000](http://localhost:8000) in a compatible browser (Chrome/Edge recommended for voice features).

## Interface

| Version     | Path             | Features                                | Devices                                        |
| ----------- | ---------------- | --------------------------------------- | ---------------------------------------------- |
| Advanced AI | `/controller/v1` | Manual controls, text/voice AI commands | Lights, TV, AC, Fan, Fridge, Induction, Washer |

## Usage

1.  **Start**: Go to the landing page (`/index.html`).
2.  **Navigate**: Click the "Control the Simulation" link, which takes you to `/controller/index.html`.
3.  **Launch Controller**: Click the link on the chooser page to open the main `/controller/v1`.
4.  **Control**:
    * Use buttons, sliders, and inputs for direct manual control of devices.
    * Type commands into the text input (e.g., "Turn on the living room light") and press Enter or click Send.
    * Click the microphone icon (🎤) to issue voice commands (requires microphone permission).
5.  **Check Status**: The connection status ("Connected" / "Disconnected") is shown at the top right.

## Core Features

* **Real-time Control**: Connects to the simulation via WebSocket (`ws://localhost:8080/iot`).
* **Voice Commands**: Utilizes browser's Speech Recognition API (best support in Chrome/Edge). Ensure microphone access is granted.
* **Text Commands**: Send natural language commands for AI processing.
* **Manual Overrides**: Directly manipulate device states via UI controls.
* **Keyboard Shortcuts**:
    * `Ctrl + /`: Focus the text command input field.
    * `Alt + V`: Start/stop voice command recognition.
    * `Ctrl + R`: Attempt to reconnect WebSocket (prevents full page reload).

**Sample Manual Command (Internal)**:
```javascript
// Example: Toggle light 0 on (This logic is handled by ui interactions in main.js)
// sendCommand({ device: "light", action: "toggle", deviceIndex: 0, parameters: { state: true } });
```

## Troubleshooting

| Issue                 | Fix                                                                        |
| --------------------- | -------------------------------------------------------------------------- |
| Controls unresponsive | Ensure the Unity simulation is running and WebSocket server is active.     |
| "Disconnected" status | Check simulation status. Check WebSocket URL (`ws://localhost:8080/iot`).  |
| Voice not working     | Use Chrome/Edge. Allow microphone permission when prompted. Check console. |
| WebSocket errors      | Check browser console (F12) for detailed errors. Verify server URL.        |
| Layout issues         | Hard refresh (`Ctrl+F5` or `Cmd+Shift+R`), clear browser cache.            |
| Command errors        | Check browser console for errors from `main.js` or WebSocket messages.     |

## Development

* **Styles**: Modify `assets/css/main.css`.
* **Logic**: Update `assets/js/main.js` (includes WebSocket, UI interactions, device controls).
* **HTML Structure**: Edit controller in the `controller/` directory or the main `index.html`.
* **API**: Refer to the [UnitySimulation API](../UnitySimulation/API.md) for details on WebSocket command structure.
* **Contribute**: Follow guidelines in [CONTRIBUTING.md](../CONTRIBUTING.md).

## Next Steps After Running the Website

To fully integrate with the `SmartHome-AgenticAI-Simulation` project:
* **[UnitySimulation README](../UnitySimulation/README.md):** Details on running the 3D simulation that the website controls via WebSocket.
* **[AgenticAI README](../AgenticAI/README.md):** Guide to setting up AI agents for automated control through the website's command input.
* **[Root README](../README.md):** Overview and quick start for the entire project.

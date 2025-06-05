# SmartHome-AgenticAI-Simulation

## Demo Video
Watch the system in action:

<div align="center">
  <a href="https://www.youtube.com/watch?v=f703A_g5J04">
    <img src="https://img.youtube.com/vi/f703A_g5J04/hqdefault.jpg" alt="Watch the video">
  </a>
</div>

[Download the demo video](./Demo_Screen_recording.mp4).

---

A unified platform for smart home automation, integrating:
- **Unity Simulation**: 3D environment for IoT device control
- **AI Automation**: Natural language processing for device management
- **Web Interface**: Multi-tier control interfaces (Basic, AI, Advanced)

Components communicate via a WebSocket server at `ws://localhost:8080/iot` for real-time control and automation.

## Prerequisites
- **Python 3.10+**: For AI system and web server
- **Unity**: Optional, for simulation development (pre-built executable provided)
- **CrewAI CLI**: Install via `pip install crewai`
- **UV**: Optional, for dependency management (`pip install uv`)
- **Node.js**: v14.0.0 or higher
- **npm**: v6.0.0 or higher

## Project Structure
| Component         | Description                                  | Documentation                          |
|-------------------|----------------------------------------------|----------------------------------------|
| `UnitySimulation` | 3D smart home simulation environment         | [Setup Guide](./UnitySimulation/README.md) |
| `AgenticAI`       | AI agents for automated device control       | [AI Configuration](./AgenticAI/README.md) |
| `Website`         | Web interfaces for manual and AI control     | [Web UI Guide](./Website/README.md) |
| `MCP`             | Master Control Program for IoT devices       | [MCP Setup](./mcp/README.md) |

## Quick Start
1. **Launch Simulation**
   ```bash
   ./UnitySimulation/Environment_Samsung.exe
   ```

2. **Start AI System**
   ```bash
   cd AgenticAI
   crewai run
   ```

3. **Run Web Interface**
   ```bash
   cd Website
   python -m http.server 8000
   ```
   Open `http://localhost:8000` in your browser.

4. **Start MCP Server**
   ```bash
   cd mcp
   npm start
   ```

## System Integration
Components communicate via WebSocket (`ws://localhost:8080/iot`):
- **Web Interface ↔ Unity**: Sends device control commands
- **Web Interface ↔ AI**: Processes natural language inputs
- **AI Agents ↔ Unity**: Executes automated commands
- **MCP Server ↔ IoT Devices**: Manages low-level device control

See [UnitySimulation API](./UnitySimulation/API.md) for WebSocket API specifications.

## Component Documentation
- [UnitySimulation README](./UnitySimulation/README.md): Run the 3D simulation.
- [AgenticAI README](./AgenticAI/README.md): Configure AI agents.
- [Website README](./Website/README.md): Set up the web interface.
- [MCP README](./mcp/README.md): Configure the Master Control Program.

## MCP Home Control Server
The Master Control Program (MCP) Server enables real-time IoT device control via WebSocket at `ws://localhost:8080/iot`.

### Prerequisites
- Node.js (v14.0.0+)
- npm (v6.0.0+)

### Setup
1. Clone the repository:
   ```bash
   git clone https://github.ecodesamsung.com/SRIB-PRISM/VITC_24LAI09VITC_GenAI_for_Context_Aware_Smart_Home_Intelligence
   cd mcp
   ```

2. Configure the MCP server using `mcp_server_config.json` (see below).
3. Start the server:
   ```bash
   npm start
   ```

### MCP Server Configuration
Edit `mcp_server_config.json` to set up the server. Example:
```json
{
  "mcpServers": {
    "home_automation_123": {
      "command": "uv",
      "args": [
        "--directory",
        "/absolute/path/to/mcp",
        "run",
        "ws_home.py"
      ],
      "env": {
        "GEMINI_API_KEY": "your-gemini-api-key"
      }
    }
  }
}
```

#### Configuration Steps
1. Replace `/absolute/path/to/mcp` with the absolute path to the `mcp` directory.
2. Replace `your-gemini-api-key` with your Gemini API key:
   - Visit [Google AI Studio](https://aistudio.google.com/apikey).
   - Log in, generate an API key, and update `GEMINI_API_KEY`.

### Connecting to the MCP Server
Use any WebSocket client to connect at `ws://localhost:8080/iot`.

#### Browser Example
```javascript
const ws = new WebSocket('ws://localhost:8080/iot');
ws.onopen = () => console.log('Connected to MCP Server');
ws.onmessage = (event) => console.log('Received:', JSON.parse(event.data));
```

#### Node.js Example
```javascript
const WebSocket = require('ws');
const ws = new WebSocket('ws://localhost:8080/iot');
ws.on('open', () => console.log('Connected to MCP Server'));
ws.on('message', (data) => console.log('Received:', JSON.parse(data)));
```

### Basic Commands
#### Control a Light
```javascript
// Turn on light #0
ws.send(JSON.stringify({
  device: 'light',
  action: 'toggle',
  deviceIndex: 0,
  parameters: { state: true }
}));

// Set light intensity
ws.send(JSON.stringify({
  device: 'light',
  action: 'setintensity',
  deviceIndex: 0,
  parameters: { intensity: 1.5 }
}));
```

#### Control a TV
```javascript
// Turn on TV
ws.send(JSON.stringify({
  device: 'tv',
  action: 'toggle',
  parameters: { state: true }
}));

// Change channel
ws.send(JSON.stringify({
  device: 'tv',
  action: 'setchannel',
  parameters: { channel: 5 }
}));
```

### Supported Devices
- `light`: Brightness, color
- `tv`: Power, volume, channel, source
- `ac`: Temperature, fan speed, eco mode
- `fridge`: Temperature, door status
- `induction`: Heat levels
- `washingmachine`: Cycle control
- `fan`: Power, speed
- `general`: Text/voice commands

See the [API documentation](./mcp/API.md) for full command details.

### Troubleshooting
- Ensure port `8080` is free.
- Check firewall settings for port access.
- Review server logs for errors.

## Contribution
See [CONTRIBUTING.md](./CONTRIBUTING.md) for guidelines.  
License: [MIT](./LICENSE)

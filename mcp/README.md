# MCP Home Control Server

A simple guide on how to run the MCP (Master Control Program) Server for IoT device control.

## Running the MCP Server

### Prerequisites

- Node.js (v14.0.0 or higher)
- npm (v6.0.0 or higher)

### Quick Start

1. Clone this repository:

   ```bash
   git clone this-repo-url
   cd mcp
   ```

2. Set up the MCP server configuration with your MCP client (e.g., Claude Desktop, VS Code, Cursor, etc.) or create your own MCP client. Use the `mcp_server_config.json` file for configuration.

3. Start the server:

   ```bash
   npm start
   ```

The server will start on port 8080 by default and is accessible via WebSocket at `ws://localhost:8080/iot`.

## MCP Server Configuration

The MCP server configuration is managed through the `mcp_server_config.json` file. This file contains details about the server's command, arguments, and environment variables.

### Configuration Details

Here is an example configuration:

```json
{
  "mcpServers": {
    "home_automation_123": {
      "command": "uv",
      "args": [
        "--directory",
        "PROJECT_LOCAL_PATH/mcp",
        "run",
        "ws_home.py"
      ],
      "env": {
        "GEMINI_API_KEY": "YOUR_GEMINI_API_KEY"
      }
    }
  }
}
```

### Replacing Placeholders

1. **`PROJECT_LOCAL_PATH`**: Replace this with the absolute path to the cloned directory of this repository. For example, if you cloned the repository to `C:\Users\YourName\Projects\mcp-server`, replace `PROJECT_LOCAL_PATH` with that path.

2. **`YOUR_GEMINI_API_KEY`**: Replace this with your actual Gemini API key. Follow the steps below to obtain your Gemini API key.

### Steps to Get Your Gemini API Key

1. Visit the Gemini API portal at [https://gemini.example.com](https://gemini.example.com).
2. Log in with your credentials or create an account if you don't have one.
3. Navigate to the "API Keys" section in your account dashboard.
4. Click on "Generate New API Key" and follow the instructions.
5. Copy the generated API key and replace `YOUR_GEMINI_API_KEY` in the configuration file with this value.

Ensure that the `mcp_server_config.json` file is correctly updated before starting the server.

## Connecting to the Server

The MCP server uses WebSockets to communicate with clients. You can connect to it using any WebSocket client.

### Browser Example

```javascript
const ws = new WebSocket('ws://localhost:8080/iot');

ws.onopen = () => {
  console.log('Connected to MCP Server');
};

ws.onmessage = (event) => {
  const data = JSON.parse(event.data);
  console.log('Received:', data);
};
```

### Node.js Example

```javascript
const WebSocket = require('ws');
const ws = new WebSocket('ws://localhost:8080/iot');

ws.on('open', () => {
  console.log('Connected to MCP Server');
});

ws.on('message', (data) => {
  const message = JSON.parse(data);
  console.log('Received:', message);
});
```

## Basic Commands

### Controlling a Light

```javascript
// Turn on light #0
ws.send(JSON.stringify({
  "device": "light",
  "action": "toggle",
  "deviceIndex": 0,
  "parameters": { "state": true }
}));

// Set light intensity
ws.send(JSON.stringify({
  "device": "light",
  "action": "setintensity",
  "deviceIndex": 0,
  "parameters": { "intensity": 1.5 }
}));
```

### Controlling the TV

```javascript
// Turn on the TV
ws.send(JSON.stringify({
  "device": "tv",
  "action": "toggle",
  "parameters": { "state": true }
}));

// Change the channel
ws.send(JSON.stringify({
  "device": "tv",
  "action": "setchannel",
  "parameters": { "channel": 5 }
}));
```

## Troubleshooting

- If the server fails to start, check if the port `8080` is already in use.
- Ensure your firewall allows connections on the configured port.
- Check the logs for detailed error messages.

## API Reference

The MCP server supports the following device types:

- `light` - Control smart lights (brightness, color)
- `tv` - Control TVs (power, volume, channel, source)
- `ac` - Control air conditioners (temperature, fan speed, eco mode)
- `fridge` - Control refrigerators (temperature settings, door status)
- `induction` - Control induction cooktops (heat levels)
- `washingmachine` - Control washing machines
- `fan` - Control fans (power, speed)
- `general` - Process general text/voice commands

For a complete API reference with all supported commands and parameters, please refer to the API documentation included with the server.

## License

[MIT](LICENSE)

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

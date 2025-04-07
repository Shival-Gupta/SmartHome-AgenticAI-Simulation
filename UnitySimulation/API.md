# IoT WebSocket API Documentation

## Overview

This documentation describes the WebSocket API for controlling IoT devices in the smart home system.

- **Endpoint:** `ws://localhost:8080/iot`
- **Protocol:** WebSocket
- **Message Format:** JSON
- **API Version:** 1.0.0

All commands are exchanged as JSON objects over the WebSocket connection. The server processes each command and returns a JSON response with the current state of the targeted device.

---

## Connection Management

### Establishing a Connection

To start interacting with the IoT system, establish a WebSocket connection to the endpoint:

```javascript
const socket = new WebSocket('ws://localhost:8080/iot');

socket.onopen = () => {
  console.log('Connected to IoT WebSocket server');
};

socket.onclose = (event) => {
  console.log('Disconnected from IoT WebSocket server', event.code, event.reason);
};

socket.onerror = (error) => {
  console.error('WebSocket error:', error);
};
```

### Initial State

Upon successful connection, the server will automatically send the initial state of all devices:

```javascript
socket.onmessage = (event) => {
  const response = JSON.parse(event.data);
  
  if (response.type === 'deviceList') {
    console.log('Received initial device state:', response.data);
    
    // Process the list of devices
    const devices = response.data.devices;
    devices.forEach(device => {
      console.log(`Device: ${device.deviceType} in ${device.location}`);
    });
  }
};
```

### Handling Disconnections

The client should implement reconnection logic to handle disconnections:

```javascript
let reconnectAttempts = 0;
const maxReconnectAttempts = 5;
const reconnectInterval = 3000; // 3 seconds

function setupWebSocket() {
  const socket = new WebSocket('ws://localhost:8080/iot');
  
  socket.onopen = () => {
    console.log('Connected to IoT WebSocket server');
    reconnectAttempts = 0;
  };
  
  socket.onclose = (event) => {
    console.log('Disconnected from IoT WebSocket server', event.code, event.reason);
    
    if (reconnectAttempts < maxReconnectAttempts) {
      reconnectAttempts++;
      console.log(`Attempting to reconnect (${reconnectAttempts}/${maxReconnectAttempts})...`);
      setTimeout(setupWebSocket, reconnectInterval);
    } else {
      console.error('Maximum reconnection attempts reached');
    }
  };
  
  // Rest of socket setup...
  
  return socket;
}

const socket = setupWebSocket();
```

---

## Message Structure

### Request Format

Requests to control devices must adhere to the following structure:

```json
{
  "device": "device_type",
  "action": "action_name",
  "deviceIndex": optional_integer,
  "parameters": { 
    /* action-specific parameters */ 
  }
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `device` | string | Yes | The type of device to control (e.g., "light", "tv") |
| `action` | string | Yes | The action to perform on the device |
| `deviceIndex` | integer | For Arrays | The index of the device when multiple instances exist (e.g., lights) |
| `parameters` | object | Yes | Parameters specific to the action |

### Response Format

Responses from the server follow this structure:

```json
{
  "success": boolean,
  "message": "status message",
  "type": "response_type",
  "data": {
    "deviceId": "unique_device_id",
    "deviceType": "device_type",
    "roomNumber": "room_name",
    "power": boolean,
    /* other device-specific properties */
  }
}
```

| Field | Type | Description |
|-------|------|-------------|
| `success` | boolean | Whether the command was processed successfully |
| `message` | string | A human-readable message about the command status |
| `type` | string | The type of response (e.g., "deviceState", "commandResponse", "deviceList") |
| `data` | object | Contains the current state of the device |

---

## Error Handling

The server uses standard HTTP status codes and error messages to indicate issues:

```json
{
  "success": false,
  "message": "Error message explaining the problem",
  "type": "commandResponse"
}
```

### Common Error Scenarios

| Error Case | Message |
|------------|---------|
| Invalid device type | "Unknown device: [device]" |
| Invalid action | "Unknown action: [action] for device [device]" |
| Missing parameters | "Missing '[parameter]' parameter for [action] action" |
| Invalid device index | "Invalid [device] index: [index]" |
| Invalid parameter value | "Invalid [parameter] format: [value]" |
| Device unavailable | "[device] is not available" |

### Example Error Response

```json
{
  "success": false,
  "message": "Missing 'state' parameter for toggle action",
  "type": "commandResponse"
}
```

---

## Supported Devices and Actions

### 1. Light Control

Control smart lights with various actions for power, brightness, and color.

#### Device Type: `"light"`

#### Actions:

##### Toggle Light (`"toggle"`)

Turn a light on or off.

**Parameters:**
- `state` (Boolean): `true` for ON, `false` for OFF.

**Example Request:**
```json
{
  "device": "light",
  "action": "toggle",
  "deviceIndex": 0,
  "parameters": { "state": true }
}
```

**Example Response:**
```json
{
  "success": true,
  "message": "Light command processed successfully.",
  "type": "deviceState",
  "data": {
    "deviceId": "LightController_3A9F",
    "roomNumber": "Living Room",
    "deviceType": "Light",
    "power": true,
    "intensity": 1.5,
    "color": "FFAA00",
    "deviceIndex": 0
  }
}
```

**Edge Cases:**
- If no `state` parameter is provided, the light will toggle from its current state.
- If the light is already in the requested state, no change occurs.

##### Set Light Intensity (`"setintensity"`)

Adjust the brightness of a light.

**Parameters:**
- `intensity` (Float): A value between 0.0 and 2.0.

**Example Request:**
```json
{
  "device": "light",
  "action": "setintensity",
  "deviceIndex": 0,
  "parameters": { "intensity": 1.5 }
}
```

**Edge Cases:**
- Values below 0 will be clamped to 0.
- Values above 2.0 will be clamped to 2.0.

##### Set Light Color (`"setcolor"`)

Change the color of a light.

**Parameters:**
- `color` (String): Hexadecimal color code (without the `#`), e.g., `"FFAA00"`.

**Example Request:**
```json
{
  "device": "light",
  "action": "setcolor",
  "deviceIndex": 0,
  "parameters": { "color": "FFAA00" }
}
```

**Edge Cases:**
- If an invalid color format is provided, the server will return an error.
- The `#` prefix is optional; both `"FFAA00"` and `"#FFAA00"` are valid.

---

### 2. TV Control

Control smart TVs with actions for power, volume, channel, and input source.

#### Device Type: `"tv"`

#### Actions:

##### Toggle TV (`"toggle"`)

Turn a TV on or off.

**Parameters:**
- `state` (Boolean): `true` for ON, `false` for OFF.

**Example Request:**
```json
{
  "device": "tv",
  "action": "toggle",
  "parameters": { "state": true }
}
```

**Example Response:**
```json
{
  "success": true,
  "message": "TV command processed successfully.",
  "type": "deviceState",
  "data": {
    "deviceId": "TVController_B42E",
    "roomNumber": "Living Room",
    "deviceType": "TV",
    "power": true,
    "volume": 25,
    "channel": 5,
    "source": "HDMI1"
  }
}
```

**Edge Cases:**
- If no `state` parameter is provided, the TV will toggle from its current state.

##### Set Volume (`"setvolume"`)

Adjust the TV volume.

**Parameters:**
- `volume` (Integer): A value from 0 to 100.

**Example Request:**
```json
{
  "device": "tv",
  "action": "setvolume",
  "parameters": { "volume": 25 }
}
```

**Edge Cases:**
- Values below 0 will be clamped to 0.
- Values above 100 will be clamped to 100.

##### Set Channel (`"setchannel"`)

Change the TV channel.

**Parameters:**
- `channel` (Integer): The channel number to tune to.

**Example Request:**
```json
{
  "device": "tv",
  "action": "setchannel",
  "parameters": { "channel": 5 }
}
```

**Edge Cases:**
- Channel numbers below 1 will be set to 1.

##### Set Source (`"setsource"`)

Change the TV input source.

**Parameters:**
- `source` (String): e.g., `"HDMI1"`, `"HDMI2"`, `"TV"`, or `"AV"`.

**Example Request:**
```json
{
  "device": "tv",
  "action": "setsource",
  "parameters": { "source": "HDMI1" }
}
```

**Edge Cases:**
- If an invalid source is provided, the server will return an error.
- Valid sources: `"HDMI1"`, `"HDMI2"`, `"TV"`, `"AV"`.

---

### 3. AC Control

Control air conditioners with actions for power, temperature, fan speed, and eco mode.

#### Device Type: `"ac"`

#### Actions:

##### Toggle AC (`"toggle"`)

Turn an air conditioner on or off.

**Parameters:**
- `state` (Boolean): `true` for ON, `false` for OFF.

**Example Request:**
```json
{
  "device": "ac",
  "action": "toggle",
  "parameters": { "state": true }
}
```

**Example Response:**
```json
{
  "success": true,
  "message": "AC command processed successfully.",
  "type": "deviceState",
  "data": {
    "deviceId": "ACController_C72D",
    "roomNumber": "Bedroom",
    "deviceType": "AC",
    "power": true,
    "temperature": 22,
    "fanSpeed": 2,
    "ecoMode": false
  }
}
```

**Edge Cases:**
- If no `state` parameter is provided, the AC will toggle from its current state.

##### Set Temperature (`"settemperature"`)

Set the target temperature for an air conditioner.

**Parameters:**
- `temperature` (Integer): Temperature in Celsius, typically between 16 and 30.

**Example Request:**
```json
{
  "device": "ac",
  "action": "settemperature",
  "parameters": { "temperature": 22 }
}
```

**Edge Cases:**
- Values below 16 will be clamped to 16.
- Values above 30 will be clamped to 30.

##### Set Fan Speed (`"setfanspeed"`)

Adjust the fan speed of an air conditioner.

**Parameters:**
- `speed` (Integer): Range from 0 to 3.

**Example Request:**
```json
{
  "device": "ac",
  "action": "setfanspeed",
  "parameters": { "speed": 2 }
}
```

**Edge Cases:**
- Values below 0 will be clamped to 0.
- Values above 3 will be clamped to 3.

##### Toggle Eco Mode (`"toggleeco"`)

Enable or disable the eco mode of an air conditioner.

**Parameters:**
- `eco` (Boolean): `true` to enable, `false` to disable.

**Example Request:**
```json
{
  "device": "ac",
  "action": "toggleeco",
  "parameters": { "eco": true }
}
```

**Edge Cases:**
- If no `eco` parameter is provided, the eco mode will toggle from its current state.
- Enabling eco mode may automatically adjust fan speed to save energy.

---

### 4. Fan Control

Control smart fans with actions for power and speed.

#### Device Type: `"fan"`

#### Actions:

##### Toggle Fan (`"toggle"`)

Turn a fan on or off.

**Parameters:**
- `state` (Boolean): `true` for ON, `false` for OFF.

**Example Request:**
```json
{
  "device": "fan",
  "action": "toggle",
  "parameters": { "state": true }
}
```

**Example Response:**
```json
{
  "success": true,
  "message": "Fan command processed successfully.",
  "type": "deviceState",
  "data": {
    "deviceId": "FanController_D81F",
    "roomNumber": "Living Room",
    "deviceType": "Fan",
    "power": true,
    "rpm": 1200
  }
}
```

**Edge Cases:**
- If no `state` parameter is provided, the fan will toggle from its current state.

##### Set RPM (`"setrpm"`)

Adjust the rotation speed of a fan.

**Parameters:**
- `rpm` (Integer): Revolutions per minute, between 100 and 2000.

**Example Request:**
```json
{
  "device": "fan",
  "action": "setrpm",
  "parameters": { "rpm": 1200 }
}
```

**Edge Cases:**
- Values below 100 will be clamped to 100.
- Values above 2000 will be clamped to 2000.

---

### 5. Fridge Control

Control smart refrigerators with actions for power, temperature, and door status.

#### Device Type: `"fridge"`

#### Actions:

##### Toggle Fridge (`"toggle"`)

Turn a refrigerator on or off.

**Parameters:**
- `state` (Boolean): `true` for ON, `false` for OFF.

**Example Request:**
```json
{
  "device": "fridge",
  "action": "toggle",
  "parameters": { "state": true }
}
```

**Example Response:**
```json
{
  "success": true,
  "message": "Fridge command processed successfully.",
  "type": "deviceState",
  "data": {
    "deviceId": "FridgeController_E63C",
    "roomNumber": "Kitchen",
    "deviceType": "Fridge",
    "power": true,
    "mainTemperature": 4,
    "freezeTemperature": -18,
    "mainDoorOpen": false,
    "freezeDoorOpen": false
  }
}
```

**Edge Cases:**
- When turned off, temperatures will rise to ambient (typically 20°C).
- When turned back on, default temperatures will be restored.

##### Set Main Temperature (`"settemperature"`)

Set the temperature for the main compartment.

**Parameters:**
- `temperature` (Integer): Temperature in Celsius.

**Example Request:**
```json
{
  "device": "fridge",
  "action": "settemperature",
  "parameters": { "temperature": 4 }
}
```

**Edge Cases:**
- Values below -10 will be clamped to -10.
- Values above 10 will be clamped to 10.
- Setting temperature while the fridge is off will return an error.

##### Set Freezer Temperature (`"setfreezetemperature"`)

Set the temperature for the freezer compartment.

**Parameters:**
- `temperature` (Integer): Temperature in Celsius.

**Example Request:**
```json
{
  "device": "fridge",
  "action": "setfreezetemperature",
  "parameters": { "temperature": -18 }
}
```

**Edge Cases:**
- Values below -30 will be clamped to -30.
- Values above -10 will be clamped to -10.
- Setting freezer temperature while the fridge is off will return an error.

##### Set Door Status (`"setdoorstatus"`)

Simulate opening or closing doors.

**Parameters:**
- `fridgeDoor` (Boolean, Optional): `true` if the main door is open.
- `freezeDoor` (Boolean, Optional): `true` if the freezer door is open.

**Example Request (Toggle main door):**
```json
{
  "device": "fridge",
  "action": "setdoorstatus",
  "parameters": { "fridgeDoor": true }
}
```

**Example Request (Toggle both doors):**
```json
{
  "device": "fridge",
  "action": "setdoorstatus",
  "parameters": { 
    "fridgeDoor": true,
    "freezeDoor": true
  }
}
```

**Edge Cases:**
- Opening doors for too long will trigger warnings about temperature rise.
- At least one parameter must be provided.

---

### 6. Induction Control

Control induction cooktops with heat level settings.

#### Device Type: `"induction"`

#### Actions:

##### Set Heat Level (`"setheat"`)

Set the heat level of an induction cooktop.

**Parameters:**
- `level` (Integer): Heat level between 0 and 3.

**Example Request:**
```json
{
  "device": "induction",
  "action": "setheat",
  "parameters": { "level": 2 }
}
```

**Example Response:**
```json
{
  "success": true,
  "message": "Induction command processed successfully.",
  "type": "deviceState",
  "data": {
    "deviceId": "InductionController_F52A",
    "roomNumber": "Kitchen",
    "deviceType": "Induction",
    "heatLevel": 2,
    "power": true
  }
}
```

**Edge Cases:**
- Values below 0 will be clamped to 0.
- Values above 3 will be clamped to 3.
- Setting level to 0 effectively turns off the cooktop.

##### Increase Heat (`"increaseheat"`)

Increase the heat level by one step.

**Example Request:**
```json
{
  "device": "induction",
  "action": "increaseheat",
  "parameters": {}
}
```

**Edge Cases:**
- If already at maximum level (3), no change occurs.

##### Decrease Heat (`"decreaseheat"`)

Decrease the heat level by one step.

**Example Request:**
```json
{
  "device": "induction",
  "action": "decreaseheat",
  "parameters": {}
}
```

**Edge Cases:**
- If already at minimum level (0), no change occurs.

---

### 7. Washing Machine Control

Control washing machines with power, cycle, and temperature settings.

#### Device Type: `"washingmachine"`

#### Actions:

##### Toggle Washing Machine (`"toggle"`)

Turn a washing machine on or off.

**Parameters:**
- `state` (Boolean): `true` for ON, `false` for OFF.

**Example Request:**
```json
{
  "device": "washingmachine",
  "action": "toggle",
  "parameters": { "state": true }
}
```

**Example Response:**
```json
{
  "success": true,
  "message": "Washing machine command processed successfully.",
  "type": "deviceState",
  "data": {
    "deviceId": "WashingMachineController_G41B",
    "roomNumber": "Bathroom",
    "deviceType": "WashingMachine",
    "power": true,
    "cycle": "Normal",
    "temperature": 40,
    "remainingTime": 60,
    "isRunning": false
  }
}
```

**Edge Cases:**
- If turned off while a cycle is running, the cycle will be paused.

##### Set Cycle (`"setcycle"`)

Set the washing cycle.

**Parameters:**
- `cycle` (String): The washing cycle to use.

**Example Request:**
```json
{
  "device": "washingmachine",
  "action": "setcycle",
  "parameters": { "cycle": "Quick" }
}
```

**Edge Cases:**
- Cannot change cycle while a wash is in progress.
- Valid cycles: `"Normal"`, `"Quick"`, `"Intensive"`, `"Delicate"`, `"EcoFriendly"`.
- Each cycle may automatically set appropriate temperature and time.

##### Set Temperature (`"settemperature"`)

Set the washing temperature.

**Parameters:**
- `temperature` (Integer): Temperature in Celsius.

**Example Request:**
```json
{
  "device": "washingmachine",
  "action": "settemperature",
  "parameters": { "temperature": 40 }
}
```

**Edge Cases:**
- Values below 20 will be clamped to 20.
- Values above 90 will be clamped to 90.
- Cannot change temperature while a wash is in progress.

##### Start Cycle (`"startcycle"`)

Start the washing cycle.

**Example Request:**
```json
{
  "device": "washingmachine",
  "action": "startcycle",
  "parameters": {}
}
```

**Edge Cases:**
- Cannot start if the machine is off.
- Cannot start if a cycle is already running.

##### Pause Cycle (`"pausecycle"`)

Pause the current washing cycle.

**Example Request:**
```json
{
  "device": "washingmachine",
  "action": "pausecycle",
  "parameters": {}
}
```

**Edge Cases:**
- Has no effect if no cycle is running.

##### Resume Cycle (`"resumecycle"`)

Resume a paused washing cycle.

**Example Request:**
```json
{
  "device": "washingmachine",
  "action": "resumecycle",
  "parameters": {}
}
```

**Edge Cases:**
- Cannot resume if the machine is off.
- Cannot resume if a cycle is already running.
- Cannot resume a completed cycle.

##### Cancel Cycle (`"cancelcycle"`)

Cancel the current washing cycle.

**Example Request:**
```json
{
  "device": "washingmachine",
  "action": "cancelcycle",
  "parameters": {}
}
```

**Edge Cases:**
- Has no effect if no cycle is running.

---

### 8. General Command (Text/Voice Processing)

Process natural language commands for any device.

#### Device Type: `"general"`

#### Actions:

##### Process General Command (`"processCommand"`)

Process a general command in natural language.

**Parameters:**
- `input` (String): The command text.

**Example Request:**
```json
{
  "device": "general",
  "action": "processCommand",
  "parameters": { "input": "Turn on the living room lights" }
}
```

**Example Response:**
```json
{
  "success": true,
  "message": "Command processed successfully",
  "type": "commandResponse",
  "data": {
    "interpreted": {
      "device": "light",
      "location": "Living Room",
      "action": "toggle",
      "parameters": { "state": true }
    },
    "executed": true
  }
}
```

**Edge Cases:**
- If the system cannot understand the command, it will return an error.
- If multiple devices match the description, the system may ask for clarification.
- Complex commands may be broken down into multiple actions.

---

## Advanced Usage

### Batch Commands

Execute multiple commands in a single request:

```json
{
  "device": "batch",
  "action": "execute",
  "parameters": {
    "commands": [
      {
        "device": "light",
        "action": "toggle",
        "deviceIndex": 0,
        "parameters": { "state": true }
      },
      {
        "device": "tv",
        "action": "toggle",
        "parameters": { "state": true }
      }
    ]
  }
}
```

### Scenes

Activate predefined scenes that control multiple devices:

```json
{
  "device": "scene",
  "action": "activate",
  "parameters": {
    "scene": "movie_night"
  }
}
```

### Scheduling

Schedule commands to execute at a specific time:

```json
{
  "device": "scheduler",
  "action": "schedule",
  "parameters": {
    "time": "2023-05-20T19:30:00",
    "command": {
      "device": "light",
      "action": "toggle",
      "deviceIndex": 0,
      "parameters": { "state": false }
    }
  }
}
```

---

## Performance Considerations

1. **Throttling**: The server limits requests to 10 per second per client.
2. **Timeouts**: Requests timeout after 5 seconds if no response is received.
3. **Concurrency**: The server handles multiple client connections efficiently.

---

## Security Considerations

1. **Authentication**: Currently not implemented. In production, use WebSocket over WSS with token-based authentication.
2. **Access Control**: Implementation varies by deployment. Define roles and permissions as needed.
3. **Input Validation**: All inputs are validated server-side to prevent injection attacks.

---

## Change Log

### v1.0.0 (Current)
- Initial release with support for 7 device types.
- WebSocket API with JSON message format.
- Real-time state updates.

---

## Appendix

### Testing Tools

1. **WebSocket Client**: [https://websocketking.com/](https://websocketking.com/)
2. **Postman**: Supports WebSocket testing in newer versions

### Example Client Implementations

#### JavaScript

```javascript
class IoTClient {
  constructor(url = 'ws://localhost:8080/iot') {
    this.url = url;
    this.socket = null;
    this.messageHandlers = new Map();
    this.connect();
  }
  
  connect() {
    this.socket = new WebSocket(this.url);
    
    this.socket.onopen = () => {
      console.log('Connected to IoT server');
      this.onConnected();
    };
    
    this.socket.onclose = () => {
      console.log('Disconnected from IoT server');
      setTimeout(() => this.connect(), 5000);
    };
    
    this.socket.onmessage = (event) => {
      const response = JSON.parse(event.data);
      
      if (this.messageHandlers.has(response.type)) {
        this.messageHandlers.get(response.type)(response);
      }
      
      this.onMessage(response);
    };
  }
  
  onConnected() {
    // Override in subclass
  }
  
  onMessage(response) {
    // Override in subclass
  }
  
  on(messageType, handler) {
    this.messageHandlers.set(messageType, handler);
  }
  
  send(device, action, parameters, deviceIndex = null) {
    const message = {
      device,
      action,
      parameters
    };
    
    if (deviceIndex !== null) {
      message.deviceIndex = deviceIndex;
    }
    
    this.socket.send(JSON.stringify(message));
  }
  
  toggleLight(index, state) {
    this.send('light', 'toggle', { state }, index);
  }
  
  setLightIntensity(index, intensity) {
    this.send('light', 'setintensity', { intensity }, index);
  }
  
  setLightColor(index, color) {
    this.send('light', 'setcolor', { color }, index);
  }
  
  // Add more helper methods for other devices...
}
```

#### Python

```python
import json
import threading
import time
import websocket

class IoTClient:
    def __init__(self, url='ws://localhost:8080/iot'):
        self.url = url
        self.ws = None
        self.message_handlers = {}
        self.connect()
    
    def connect(self):
        websocket.enableTrace(False)
        self.ws = websocket.WebSocketApp(
            self.url,
            on_open=self.on_open,
            on_message=self.on_message,
            on_error=self.on_error,
            on_close=self.on_close
        )
        
        thread = threading.Thread(target=self.ws.run_forever)
        thread.daemon = True
        thread.start()
    
    def on_open(self, ws):
        print("Connected to IoT server")
    
    def on_message(self, ws, message):
        response = json.loads(message)
        message_type = response.get('type')
        
        if message_type in self.message_handlers:
            self.message_handlers[message_type](response)
    
    def on_error(self, ws, error):
        print(f"Error: {error}")
    
    def on_close(self, ws, close_status_code, close_msg):
        print("Disconnected from IoT server")
        time.sleep(5)
        self.connect()
    
    def on(self, message_type, handler):
        self.message_handlers[message_type] = handler
    
    def send(self, device, action, parameters, device_index=None):
        message = {
            'device': device,
            'action': action,
            'parameters': parameters
        }
        
        if device_index is not None:
            message['deviceIndex'] = device_index
        
        self.ws.send(json.dumps(message))
    
    def toggle_light(self, index, state):
        self.send('light', 'toggle', {'state': state}, index)
    
    def set_light_intensity(self, index, intensity):
        self.send('light', 'setintensity', {'intensity': intensity}, index)
    
    def set_light_color(self, index, color):
        self.send('light', 'setcolor', {'color': color}, index)
    
    # Add more helper methods for other devices...
```

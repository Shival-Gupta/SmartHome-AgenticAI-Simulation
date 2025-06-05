# IoT WebSocket API Documentation

## Overview

- **Endpoint:** `ws://localhost:8080/iot`
- **Protocol:** WebSocket
- **Message Format:** JSON

All commands are exchanged as JSON objects over the WebSocket connection. The server processes each command and returns a JSON response.

---

## Message Structure

### Request Format

```json
{
  "device": "device_type",
  "action": "action_name",
  "deviceIndex": optional_integer,  // Only needed for devices with multiple instances (e.g., lights) (there are total 6 lights(0-5), 0 and 1 are in hall, 2 is in kitchen, rest are in rooms)
  "parameters": { /* action-specific parameters */ }
}
```

- **device:** Specifies the device type. Valid values include:
  - `"light"`
  - `"tv"`
  - `"ac"`
  - `"fridge"`
  - `"induction"`
  - `"washingmachine"`
  - `"fan"`
  - `"general"` (for generic commands, such as processing text/voice input)
- **action:** The specific command to execute (see the sections below).
- **deviceIndex:** *(Optional)* The index of the device instance (e.g., `0` for Light 1, `1` for Light 2).
- **parameters:** An object containing any parameters required by the command.

### Response Format

```json
{
  "success": boolean,
  "message": "status message",
  "data": {
    "deviceId": "unique_device_id",
    "deviceType": "device_type",
    "roomNumber": "room_name",
    "power": boolean,           // Most devices have this (ON/OFF status)
    ... // other device-specific properties
  }
}
```

- **success:** Boolean indicating whether the command was processed successfully.
- **message:** A text message describing the command status.
- **data:** An object containing the current state of the device. This includes standard properties like `deviceId`, `deviceType`, and `roomNumber`, along with device-specific properties.

### Initial State Response

When a client connects, the server automatically sends an initial state message:

```json
{
  "success": true,
  "message": "Initial state",
  "data": {
    "lights": [
      {
        "deviceId": "LightController_3A9F",
        "roomNumber": "Living Room",
        "deviceType": "Light",
        "power": false,
        "intensity": 1.0,
        "color": "FFFFFF",
        "deviceIndex": 0
      },
      // ... more lights
    ],
    "tv": {
      "deviceId": "TVController_B42E",
      "roomNumber": "Living Room",
      "deviceType": "TV",
      "power": false,
      "volume": 10,
      "channel": 1,
      "source": "HDMI1"
    },
    // ... other devices
  }
}
```

### Error Response Format

If an error occurs while processing a command, the server responds with:

```json
{
  "success": false,
  "message": "Error description"
}
```

---

## Supported Devices and Actions

### 1. Light Control

- **Toggle Light**
  - **Action:** `"toggle"`
  - **Parameters:**
    - `state` (Boolean): `true` for ON, `false` for OFF.
  - **Example Request:**
    ```json
    {
      "device": "light",
      "action": "toggle",
      "deviceIndex": 0,
      "parameters": { "state": true }
    }
    ```
  - **Example Response:**
    ```json
    {
      "success": true,
      "message": "Light command processed successfully.",
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

- **Set Light Intensity**
  - **Action:** `"setintensity"`
  - **Parameters:**
    - `intensity` (Float): A value between 0.0 and 2.0.
  - **Example Request:**
    ```json
    {
      "device": "light",
      "action": "setintensity",
      "deviceIndex": 0,
      "parameters": { "intensity": 1.5 }
    }
    ```

- **Set Light Color**
  - **Action:** `"setcolor"`
  - **Parameters:**
    - `color` (String): Hexadecimal color code (without the `#`), e.g., `"FFAA00"`.
  - **Example Request:**
    ```json
    {
      "device": "light",
      "action": "setcolor",
      "deviceIndex": 0,
      "parameters": { "color": "FFAA00" }
    }
    ```

---

### 2. TV Control

- **Toggle TV**
  - **Action:** `"toggle"`
  - **Parameters:**
    - `state` (Boolean)
  - **Example Request:**
    ```json
    {
      "device": "tv",
      "action": "toggle",
      "parameters": { "state": true }
    }
    ```
  - **Example Response:**
    ```json
    {
      "success": true,
      "message": "TV command processed successfully.",
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

- **Set Volume**
  - **Action:** `"setvolume"`
  - **Parameters:**
    - `volume` (Integer): A value from 0 to 100.
  - **Example Request:**
    ```json
    {
      "device": "tv",
      "action": "setvolume",
      "parameters": { "volume": 25 }
    }
    ```

- **Set Channel**
  - **Action:** `"setchannel"`
  - **Parameters:**
    - `channel` (Integer)
  - **Example Request:**
    ```json
    {
      "device": "tv",
      "action": "setchannel",
      "parameters": { "channel": 5 }
    }
    ```

- **Set Source**
  - **Action:** `"setsource"`
  - **Parameters:**
    - `source` (String): e.g., `"HDMI1"`, `"HDMI2"`, or `"TV"`.
  - **Example Request:**
    ```json
    {
      "device": "tv",
      "action": "setsource",
      "parameters": { "source": "HDMI1" }
    }
    ```

---

### 3. AC Control

- **Toggle AC**
  - **Action:** `"toggle"`
  - **Parameters:**
    - `state` (Boolean)
  - **Example Request:**
    ```json
    {
      "device": "ac",
      "action": "toggle",
      "parameters": { "state": true }
    }
    ```
  - **Example Response:**
    ```json
    {
      "success": true,
      "message": "AC command processed successfully.",
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

- **Set Temperature**
  - **Action:** `"settemperature"`
  - **Parameters:**
    - `temperature` (Integer): Typically between 16 and 30 (°C).
  - **Example Request:**
    ```json
    {
      "device": "ac",
      "action": "settemperature",
      "parameters": { "temperature": 22 }
    }
    ```

- **Set Fan Speed**
  - **Action:** `"setfanspeed"`
  - **Parameters:**
    - `speed` (Integer): Range from 0 to 3.
  - **Example Request:**
    ```json
    {
      "device": "ac",
      "action": "setfanspeed",
      "parameters": { "speed": 2 }
    }
    ```

- **Toggle Eco Mode**
  - **Action:** `"toggleeco"`
  - **Parameters:**
    - `eco` (Boolean)
  - **Example Request:**
    ```json
    {
      "device": "ac",
      "action": "toggleeco",
      "parameters": { "eco": true }
    }
    ```

---

### 4. Fan Control

- **Toggle Fan**
  - **Action:** `"toggle"`
  - **Parameters:**
    - `state` (Boolean)
  - **Example Request:**
    ```json
    {
      "device": "fan",
      "action": "toggle",
      "parameters": { "state": true }
    }
    ```
  - **Example Response:**
    ```json
    {
      "success": true,
      "message": "Fan command processed successfully.",
      "data": {
        "deviceId": "FanController_D81F",
        "roomNumber": "Living Room",
        "deviceType": "Fan",
        "power": true,
        "rpm": 1200
      }
    }
    ```

- **Set RPM**
  - **Action:** `"setrpm"`
  - **Parameters:**
    - `rpm` (Integer): For example, a value between 100 and 2000.
  - **Example Request:**
    ```json
    {
      "device": "fan",
      "action": "setrpm",
      "parameters": { "rpm": 1200 }
    }
    ```

---

### 5. Fridge Control

- **Toggle Fridge**
  - **Action:** `"toggle"`
  - **Parameters:**
    - `state` (Boolean)
  - **Example Request:**
    ```json
    {
      "device": "fridge",
      "action": "toggle",
      "parameters": { "state": true }
    }
    ```
  - **Example Response:**
    ```json
    {
      "success": true,
      "message": "Fridge command processed successfully.",
      "data": {
        "deviceId": "FridgeController_E57B",
        "roomNumber": "Kitchen",
        "deviceType": "Fridge",
        "power": true,
        "temperature": 4,
        "freezeTemperature": -18,
        "fridgeDoor": false,
        "freezeDoor": false
      }
    }
    ```

- **Set Main Temperature**
  - **Action:** `"settemperature"`
  - **Parameters:**
    - `temperature` (Integer)
  - **Example Request:**
    ```json
    {
      "device": "fridge",
      "action": "settemperature",
      "parameters": { "temperature": 4 }
    }
    ```

- **Set Freezer Temperature**
  - **Action:** `"setfreezetemperature"`
  - **Parameters:**
    - `temperature` (Integer)
  - **Example Request:**
    ```json
    {
      "device": "fridge",
      "action": "setfreezetemperature",
      "parameters": { "temperature": -18 }
    }
    ```

- **Set Door Status**
  - **Action:** `"setdoorstatus"`
  - **Parameters:**
    - `fridgeDoor` (Boolean): `true` if the main door is open.
    - `freezeDoor` (Boolean): `true` if the freezer door is open.
  - **Example Request (Toggle main door):**
    ```json
    {
      "device": "fridge",
      "action": "setdoorstatus",
      "parameters": { "fridgeDoor": true }
    }
    ```

---

### 6. Induction Control

- **Set Heat Level**
  - **Action:** `"setheat"`
  - **Parameters:**
    - `level` (Integer): Typically between 0 and 3.
  - **Example Request:**
    ```json
    {
      "device": "induction",
      "action": "setheat",
      "parameters": { "level": 2 }
    }
    ```
  - **Example Response:**
    ```json
    {
      "success": true,
      "message": "Induction command processed successfully.",
      "data": {
        "deviceId": "InductionController_F19C",
        "roomNumber": "Kitchen",
        "deviceType": "Induction",
        "level": 2
      }
    }
    ```

---

### 7. Washing Machine Control

- **Toggle Washing Machine**
  - **Action:** `"toggle"`
  - **Parameters:**
    - `state` (Boolean)
  - **Example Request:**
    ```json
    {
      "device": "washingmachine",
      "action": "toggle",
      "parameters": { "state": true }
    }
    ```
  - **Example Response:**
    ```json
    {
      "success": true,
      "message": "Washing machine command processed successfully.",
      "data": {
        "deviceId": "WashingMachineController_G34D",
        "roomNumber": "Utility Room",
        "deviceType": "WashingMachine",
        "power": true
      }
    }
    ```

---

### 8. General Command (Text/Voice Processing)

- **Process General Command**
  - **Action:** `"processCommand"`
  - **Parameters:**
    - `input` (String): The command text.
  - **Example Request:**
    ```json
    {
      "device": "general",
      "action": "processCommand",
      "parameters": { "input": "Turn on the living room lights" }
    }
    ```
  - **Example Response:**
    ```json
    {
      "success": true,
      "message": "Command processed"
    }
    ```

---

## Status Handling

All commands return a status object with the following fields:

- `success`: Boolean indicating if the operation completed successfully
- `message`: String describing the status or error message
- `data`: Object containing the updated device state (when applicable)

### Status Codes

The `success` field determines whether the operation succeeded:
- `true`: The command was processed successfully
- `false`: There was an error processing the command

### Error Handling

Common error messages include:
- "Missing parameter" - When a required parameter is not provided
- "Invalid parameter" - When a parameter value is outside its allowed range
- "Unknown device" - When the specified device doesn't exist
- "Unknown action" - When the action isn't supported for the specified device

Example error response:
```json
{
  "success": false,
  "message": "Missing 'intensity' parameter for setintensity action."
}
```

---

## Client Implementation Guide

### Connecting to the WebSocket

```javascript
const ws = new WebSocket('ws://localhost:8080/iot');

ws.onopen = () => {
  console.log('Connected to IoT WebSocket');
};

ws.onclose = () => {
  console.log('Disconnected from IoT WebSocket');
};

ws.onerror = (error) => {
  console.error('WebSocket error:', error);
};

ws.onmessage = (event) => {
  try {
    const response = JSON.parse(event.data);
    console.log('Received:', response);
    
    if (response.message === "Initial state") {
      // Handle initial state update
      updateAllDevices(response.data);
    } else if (response.success && response.data) {
      // Handle individual device update
      updateDeviceState(response.data);
    } else {
      console.error('Command failed:', response.message);
    }
  } catch (e) {
    console.error('Error parsing message:', e);
  }
};
```

### Sending Commands

```javascript
function sendCommand(device, action, parameters, deviceIndex = null) {
  if (ws.readyState === WebSocket.OPEN) {
    const command = {
      device: device,
      action: action,
      parameters: parameters
    };
    
    if (deviceIndex !== null) {
      command.deviceIndex = deviceIndex;
    }
    
    ws.send(JSON.stringify(command));
  } else {
    console.error('WebSocket is not connected');
  }
}

// Example usage:
sendCommand('light', 'toggle', { state: true }, 0);
sendCommand('ac', 'settemperature', { temperature: 22 });
```

---

Developers can use this documentation as a guide to build custom frontends that interact with the IoT Device Controller via WebSocket. Simply connect to `ws://localhost:8080/iot` and exchange JSON messages as demonstrated.

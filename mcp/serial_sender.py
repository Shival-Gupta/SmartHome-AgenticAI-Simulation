"""Control the light. True = ON, False = OFF"""
device_states = [0, 0, 0, 0]

def send_device_states():
    """Sends the current device state array to Arduino."""
    data = ','.join(str(s) for s in device_states) + '\n'
    print(f"Sent: {device_states}")

def control_fan(state: bool):
    """Control the fan. True = ON, False = OFF"""
    device_states[0] = int(state)
    send_device_states()
    return f"Fan turned {'ON' if state else 'OFF'}."

def control_light(state: bool):
    """Control the light. True = ON, False = OFF"""
    device_states[1] = int(state)
    send_device_states()
    return f"Light turned {'ON' if state else 'OFF'}."

def control_uv_sensor(state: bool):
    """Enable or disable UV sensor."""
    device_states[2] = int(state)
    send_device_states()
    return f"UV sensor turned {'ON' if state else 'OFF'}."

def set_display_digit(digit: int):
    """Set the digit on the single-digit display."""
    if 0 <= digit <= 9:
        device_states[3] = digit
        send_device_states()
        return f"Display set to {digit}."
    else:
        return "Invalid digit! Must be between 0 and 9."

available_functions = [
    {
        "name": "control_fan",
        "description": "Control the fan. True = ON, False = OFF",
        "parameters": {
            "type": "object",
            "properties": {
                "state": {"type": "boolean", "description": "The desired state of the fan."},
            },
            "required": ["state"],
        },
    },
    {
        "name": "control_light",
        "description": "Control the light. True = ON, False = OFF",
        "parameters": {
            "type": "object",
            "properties": {
                "state": {"type": "boolean", "description": "The desired state of the light."},
            },
            "required": ["state"],
        },
    },
    {
        "name": "control_uv_sensor",
        "description": "Enable or disable UV sensor.",
        "parameters": {
            "type": "object",
            "properties": {
                "state": {"type": "boolean", "description": "The desired state of the UV sensor."},
            },
            "required": ["state"],
        },
    },
    {
        "name": "set_display_digit",
        "description": "Set the digit on the single-digit display.",
        "parameters": {
            "type": "object",
            "properties": {
                "digit": {"type": "number", "description": "The digit to display."},
            },
            "required": ["digit"],
        },
    },
]
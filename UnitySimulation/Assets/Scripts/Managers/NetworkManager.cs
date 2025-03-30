using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net;
using WebSocketSharp;
using WebSocketSharp.Server;
using Newtonsoft.Json;

[Serializable]
public class DeviceCommand
{
    public string device;     // e.g., "light", "tv", "ac"
    public string action;     // e.g., "toggle", "setTemperature"
    public int deviceIndex;   // For arrays like lights
    public Dictionary<string, object> parameters;

    /// <summary>
    /// Gets the device type as an enum for type safety.
    /// </summary>
    public DeviceType DeviceType
    {
        get
        {
            if (Enum.TryParse(device, true, out DeviceType type))
                return type;
            throw new ArgumentException($"Unknown device type: {device}");
        }
    }

    /// <summary>
    /// Gets the action as a specific enum type.
    /// </summary>
    public T GetAction<T>() where T : Enum
    {
        if (Enum.TryParse(action, true, out T act))
            return act;
        throw new ArgumentException($"Unknown action: {action} for device {device}");
    }
}

[Serializable]
public class WebSocketResponse
{
    public bool success;
    public string message;
    public object data;
}

// Enums for device types and actions to improve type safety
public enum DeviceType
{
    Light,
    TV,
    AC,
    Fridge,
    Induction,
    WashingMachine,
    Fan
}

public enum LightAction
{
    Toggle,
    SetIntensity,
    SetColor
}

public enum TVAction
{
    Toggle,
    SetVolume,
    SetChannel,
    SetSource
}

public enum ACAction
{
    Toggle,
    SetTemperature,
    SetFanSpeed,
    ToggleEco
}

public enum FridgeAction
{
    Toggle,
    SetTemperature,
    SetFreezeTemperature,
    SetDoorStatus
}

public enum InductionAction
{
    SetHeat
}

public enum WashingMachineAction
{
    Toggle
}

public enum FanAction
{
    Toggle,
    SetRPM
}

/// <summary>
/// Manages a WebSocket server to handle IoT device control within a Unity application.
/// </summary>
public class NetworkManager : MonoBehaviour
{
    private WebSocketServer wssv;
    private DeviceController deviceController;

    [SerializeField, Tooltip("The port on which the WebSocket server will listen (default: 8080).")]
    private int port = 8080;

    [SerializeField, Tooltip("If true, starts the WebSocket server automatically when the script awakes.")]
    private bool startServerOnAwake = true;

    private void Awake()
    {
        deviceController = FindObjectOfType<DeviceController>();
        if (deviceController == null)
        {
            Debug.LogError("DeviceController not found in the scene! WebSocket server cannot operate.");
            return;
        }

        if (startServerOnAwake)
            StartServer();
    }

    /// <summary>
    /// Starts the WebSocket server to listen for IoT device commands.
    /// </summary>
    public void StartServer()
    {
        try
        {
            wssv = new WebSocketServer(IPAddress.Any, port);
            wssv.AddWebSocketService<IoTWebSocketService>("/iot",
                () => new IoTWebSocketService(deviceController));
            wssv.Start();
            Debug.Log($"WebSocket server started on port {port}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to start WebSocket server: {e.Message}");
        }
    }

    private void OnDestroy()
    {
        if (wssv != null && wssv.IsListening)
        {
            wssv.Stop();
            Debug.Log("WebSocket server stopped");
        }
    }
}

public interface ICommandHandler
{
    WebSocketResponse Handle(DeviceCommand command);
}

/// <summary>
/// WebSocket service that processes incoming IoT device commands and dispatches them to appropriate handlers.
/// </summary>
public class IoTWebSocketService : WebSocketBehavior
{
    private readonly DeviceController deviceController;
    private readonly Dictionary<DeviceType, ICommandHandler> handlers;

    public IoTWebSocketService(DeviceController controller)
    {
        deviceController = controller ?? throw new ArgumentNullException(nameof(controller), "DeviceController cannot be null.");
        handlers = new Dictionary<DeviceType, ICommandHandler>
        {
            { DeviceType.Light, new LightCommandHandler(deviceController) },
            { DeviceType.TV, new TVCommandHandler(deviceController) },
            { DeviceType.AC, new ACCommandHandler(deviceController) },
            { DeviceType.Fridge, new FridgeCommandHandler(deviceController) },
            { DeviceType.Induction, new InductionCommandHandler(deviceController) },
            { DeviceType.WashingMachine, new WashingMachineCommandHandler(deviceController) },
            { DeviceType.Fan, new FanCommandHandler(deviceController) }
        };
    }

    protected override void OnMessage(MessageEventArgs e)
    {
        try
        {
            var command = JsonConvert.DeserializeObject<DeviceCommand>(e.Data);
            if (command == null || string.IsNullOrEmpty(command.device) || string.IsNullOrEmpty(command.action))
                throw new ArgumentException("Invalid command format: device and action are required.");

            var response = ProcessCommand(command);
            Send(JsonConvert.SerializeObject(response));
        }
        catch (Exception ex)
        {
            var errorResponse = new WebSocketResponse
            {
                success = false,
                message = $"Error processing command: {ex.Message}"
            };
            Send(JsonConvert.SerializeObject(errorResponse));
        }
    }

    private WebSocketResponse ProcessCommand(DeviceCommand command)
    {
        if (handlers.TryGetValue(command.DeviceType, out var handler))
        {
            return handler.Handle(command);
        }
        return new WebSocketResponse { success = false, message = $"Unknown device: {command.device}" };
    }
}

#region Command Handlers

public class LightCommandHandler : ICommandHandler
{
    private readonly DeviceController deviceController;

    public LightCommandHandler(DeviceController controller)
    {
        deviceController = controller;
    }

    public WebSocketResponse Handle(DeviceCommand command)
    {
        try
        {
            var action = command.GetAction<LightAction>();
            switch (action)
            {
                case LightAction.Toggle:
                    bool state = command.parameters != null && command.parameters.ContainsKey("state")
                        ? Convert.ToBoolean(command.parameters["state"])
                        : !deviceController.GetLightSettings(command.deviceIndex).isOn;
                    deviceController.ToggleLight(command.deviceIndex, state);
                    break;
                case LightAction.SetIntensity:
                    if (command.parameters == null || !command.parameters.ContainsKey("intensity"))
                        throw new ArgumentException("Missing 'intensity' parameter for setintensity action.");
                    float intensity = Convert.ToSingle(command.parameters["intensity"]);
                    deviceController.SetLightIntensity(command.deviceIndex, intensity);
                    break;
                case LightAction.SetColor:
                    if (command.parameters == null || !command.parameters.ContainsKey("color"))
                        throw new ArgumentException("Missing 'color' parameter for setcolor action.");
                    string hexColor = command.parameters["color"].ToString();
                    deviceController.SetLightColor(command.deviceIndex, hexColor);
                    break;
            }
            return new WebSocketResponse { success = true, message = "Light command processed successfully." };
        }
        catch (Exception ex)
        {
            return new WebSocketResponse { success = false, message = $"Light command failed: {ex.Message}" };
        }
    }
}

public class TVCommandHandler : ICommandHandler
{
    private readonly DeviceController deviceController;

    public TVCommandHandler(DeviceController controller)
    {
        deviceController = controller;
    }

    public WebSocketResponse Handle(DeviceCommand command)
    {
        try
        {
            var action = command.GetAction<TVAction>();
            switch (action)
            {
                case TVAction.Toggle:
                    bool state = command.parameters != null && command.parameters.ContainsKey("state")
                        ? Convert.ToBoolean(command.parameters["state"])
                        : !deviceController.tvOn;
                    deviceController.ToggleTV(state);
                    break;
                case TVAction.SetVolume:
                    if (command.parameters == null || !command.parameters.ContainsKey("volume"))
                        throw new ArgumentException("Missing 'volume' parameter for setvolume action.");
                    int volume = Convert.ToInt32(command.parameters["volume"]);
                    deviceController.SetTVVolume(volume);
                    break;
                case TVAction.SetChannel:
                    if (command.parameters == null || !command.parameters.ContainsKey("channel"))
                        throw new ArgumentException("Missing 'channel' parameter for setchannel action.");
                    int channel = Convert.ToInt32(command.parameters["channel"]);
                    deviceController.SetTVChannel(channel);
                    break;
                case TVAction.SetSource:
                    if (command.parameters == null || !command.parameters.ContainsKey("source"))
                        throw new ArgumentException("Missing 'source' parameter for setsource action.");
                    string source = command.parameters["source"].ToString();
                    deviceController.SetTVSource(source);
                    break;
            }
            return new WebSocketResponse { success = true, message = "TV command processed successfully." };
        }
        catch (Exception ex)
        {
            return new WebSocketResponse { success = false, message = $"TV command failed: {ex.Message}" };
        }
    }
}

public class ACCommandHandler : ICommandHandler
{
    private readonly DeviceController deviceController;

    public ACCommandHandler(DeviceController controller)
    {
        deviceController = controller;
    }

    public WebSocketResponse Handle(DeviceCommand command)
    {
        try
        {
            var action = command.GetAction<ACAction>();
            switch (action)
            {
                case ACAction.Toggle:
                    bool state = command.parameters != null && command.parameters.ContainsKey("state")
                        ? Convert.ToBoolean(command.parameters["state"])
                        : !deviceController.acOn;
                    deviceController.ToggleAC(state);
                    break;
                case ACAction.SetTemperature:
                    if (command.parameters == null || !command.parameters.ContainsKey("temperature"))
                        throw new ArgumentException("Missing 'temperature' parameter for settemperature action.");
                    int temperature = Convert.ToInt32(command.parameters["temperature"]);
                    deviceController.SetACTemperature(temperature);
                    break;
                case ACAction.SetFanSpeed:
                    if (command.parameters == null || !command.parameters.ContainsKey("speed"))
                        throw new ArgumentException("Missing 'speed' parameter for setfanspeed action.");
                    int speed = Convert.ToInt32(command.parameters["speed"]);
                    deviceController.SetACFanSpeed(speed);
                    break;
                case ACAction.ToggleEco:
                    bool ecoState = command.parameters != null && command.parameters.ContainsKey("eco")
                        ? Convert.ToBoolean(command.parameters["eco"])
                        : !deviceController.acEcoMode;
                    deviceController.ToggleACEcoMode(ecoState);
                    break;
            }
            return new WebSocketResponse { success = true, message = "AC command processed successfully." };
        }
        catch (Exception ex)
        {
            return new WebSocketResponse { success = false, message = $"AC command failed: {ex.Message}" };
        }
    }
}

public class FridgeCommandHandler : ICommandHandler
{
    private readonly DeviceController deviceController;

    public FridgeCommandHandler(DeviceController controller)
    {
        deviceController = controller;
    }

    public WebSocketResponse Handle(DeviceCommand command)
    {
        try
        {
            var action = command.GetAction<FridgeAction>();
            switch (action)
            {
                case FridgeAction.Toggle:
                    bool state = command.parameters != null && command.parameters.ContainsKey("state")
                        ? Convert.ToBoolean(command.parameters["state"])
                        : !deviceController.fridgeOn;
                    deviceController.ToggleFridge(state);
                    break;
                case FridgeAction.SetTemperature:
                    if (command.parameters == null || !command.parameters.ContainsKey("temperature"))
                        throw new ArgumentException("Missing 'temperature' parameter for settemperature action.");
                    int temp = Convert.ToInt32(command.parameters["temperature"]);
                    deviceController.SetFridgeTemperature(temp);
                    break;
                case FridgeAction.SetFreezeTemperature:
                    if (command.parameters == null || !command.parameters.ContainsKey("temperature"))
                        throw new ArgumentException("Missing 'temperature' parameter for setfreezetemperature action.");
                    int freezeTemp = Convert.ToInt32(command.parameters["temperature"]);
                    deviceController.SetFreezeTemperature(freezeTemp);
                    break;
                case FridgeAction.SetDoorStatus:
                    if (command.parameters != null)
                    {
                        if (command.parameters.ContainsKey("fridgeDoor"))
                            deviceController.SetFridgeDoorOpen(Convert.ToBoolean(command.parameters["fridgeDoor"]));
                        if (command.parameters.ContainsKey("freezeDoor"))
                            deviceController.SetFreezeDoorOpen(Convert.ToBoolean(command.parameters["freezeDoor"]));
                    }
                    break;
            }
            return new WebSocketResponse { success = true, message = "Fridge command processed successfully." };
        }
        catch (Exception ex)
        {
            return new WebSocketResponse { success = false, message = $"Fridge command failed: {ex.Message}" };
        }
    }
}

public class InductionCommandHandler : ICommandHandler
{
    private readonly DeviceController deviceController;

    public InductionCommandHandler(DeviceController controller)
    {
        deviceController = controller;
    }

    public WebSocketResponse Handle(DeviceCommand command)
    {
        try
        {
            var action = command.GetAction<InductionAction>();
            switch (action)
            {
                case InductionAction.SetHeat:
                    if (command.parameters == null || !command.parameters.ContainsKey("level"))
                        throw new ArgumentException("Missing 'level' parameter for setheat action.");
                    int level = Convert.ToInt32(command.parameters["level"]);
                    deviceController.SetInductionHeat(level);
                    break;
            }
            return new WebSocketResponse { success = true, message = "Induction command processed successfully." };
        }
        catch (Exception ex)
        {
            return new WebSocketResponse { success = false, message = $"Induction command failed: {ex.Message}" };
        }
    }
}

public class WashingMachineCommandHandler : ICommandHandler
{
    private readonly DeviceController deviceController;

    public WashingMachineCommandHandler(DeviceController controller)
    {
        deviceController = controller;
    }

    public WebSocketResponse Handle(DeviceCommand command)
    {
        try
        {
            var action = command.GetAction<WashingMachineAction>();
            switch (action)
            {
                case WashingMachineAction.Toggle:
                    bool state = command.parameters != null && command.parameters.ContainsKey("state")
                        ? Convert.ToBoolean(command.parameters["state"])
                        : !deviceController.washingMachineOn;
                    deviceController.ToggleWashingMachine(state);
                    break;
            }
            return new WebSocketResponse { success = true, message = "Washing machine command processed successfully." };
        }
        catch (Exception ex)
        {
            return new WebSocketResponse { success = false, message = $"Washing machine command failed: {ex.Message}" };
        }
    }
}

public class FanCommandHandler : ICommandHandler
{
    private readonly DeviceController deviceController;

    public FanCommandHandler(DeviceController controller)
    {
        deviceController = controller;
    }

    public WebSocketResponse Handle(DeviceCommand command)
    {
        try
        {
            var action = command.GetAction<FanAction>();
            switch (action)
            {
                case FanAction.Toggle:
                    bool state = command.parameters != null && command.parameters.ContainsKey("state")
                        ? Convert.ToBoolean(command.parameters["state"])
                        : !deviceController.fanOn;
                    deviceController.ToggleFan(state);
                    break;
                case FanAction.SetRPM:
                    if (command.parameters == null || !command.parameters.ContainsKey("rpm"))
                        throw new ArgumentException("Missing 'rpm' parameter for setrpm action.");
                    int rpm = Convert.ToInt32(command.parameters["rpm"]);
                    deviceController.SetFanRPM(rpm);
                    break;
            }
            return new WebSocketResponse { success = true, message = "Fan command processed successfully." };
        }
        catch (Exception ex)
        {
            return new WebSocketResponse { success = false, message = $"Fan command failed: {ex.Message}" };
        }
    }
}

#endregion

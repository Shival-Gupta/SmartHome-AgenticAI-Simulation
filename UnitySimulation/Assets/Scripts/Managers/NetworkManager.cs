using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net;
using WebSocketSharp;
using WebSocketSharp.Server;
using Newtonsoft.Json;

#region Data Models

/// <summary>
/// Represents a command sent via WebSocket to control an IoT device.
/// </summary>
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
    /// <typeparam name="T">The enum type corresponding to the device's actions.</typeparam>
    /// <returns>The parsed action enum.</returns>
    public T GetAction<T>() where T : struct
    {
        if (!typeof(T).IsEnum)
            throw new ArgumentException("T must be an enum type.");
        if (Enum.TryParse(action, true, out T act))
            return act;
        throw new ArgumentException($"Unknown action: {action} for device {device}");
    }
}

/// <summary>
/// Represents a response sent back to the WebSocket client.
/// </summary>
[Serializable]
public class WebSocketResponse
{
    public bool success;
    public string message;
    public object data;
    public string type;
}

#endregion

#region Enums

/// <summary>
/// Enumerates the supported IoT device types.
/// </summary>
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

/// <summary>
/// Actions available for lights.
/// </summary>
public enum LightAction
{
    Toggle,
    SetIntensity,
    SetColor
}

/// <summary>
/// Actions available for TVs.
/// </summary>
public enum TVAction
{
    Toggle,
    SetVolume,
    SetChannel,
    SetSource
}

/// <summary>
/// Actions available for air conditioners.
/// </summary>
public enum ACAction
{
    Toggle,
    SetTemperature,
    SetFanSpeed,
    ToggleEco
}

/// <summary>
/// Actions available for fridges.
/// </summary>
public enum FridgeAction
{
    Toggle,
    SetTemperature,
    SetFreezeTemperature,
    SetDoorStatus
}

/// <summary>
/// Actions available for induction cooktops.
/// </summary>
public enum InductionAction
{
    SetHeat
}

/// <summary>
/// Actions available for washing machines.
/// </summary>
public enum WashingMachineAction
{
    Toggle
}

/// <summary>
/// Actions available for fans.
/// </summary>
public enum FanAction
{
    Toggle,
    SetRPM
}

#endregion

/// <summary>
/// Manages a WebSocket server to handle IoT device control within a Unity application.
/// </summary>
public class NetworkManager : MonoBehaviour
{
    #region Fields

    private WebSocketServer wssv;
    private DeviceController deviceController;

    [SerializeField, Tooltip("The port on which the WebSocket server will listen (default: 8080).")]
    private int port = 8080;

    [SerializeField, Tooltip("If true, starts the WebSocket server automatically when the script awakes.")]
    private bool startServerOnAwake = true;

    #endregion

    #region Unity Lifecycle

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

    private void OnDestroy()
    {
        if (wssv != null && wssv.IsListening)
        {
            wssv.Stop();
            Debug.Log("WebSocket server stopped");
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Starts the WebSocket server to listen for IoT device commands.
    /// </summary>
    public void StartServer()
    {
        try
        {
            wssv = new WebSocketServer(IPAddress.Any, port);
#pragma warning disable CS0618 // Suppress obsolete warning
            wssv.AddWebSocketService<IoTWebSocketService>("/iot", () => new IoTWebSocketService(deviceController));
#pragma warning restore CS0618
            wssv.Start();
            Debug.Log($"WebSocket server started on port {port}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to start WebSocket server: {e.Message}");
        }
    }

    #endregion
}

/// <summary>
/// Defines the interface for handling device-specific commands.
/// </summary>
public interface ICommandHandler
{
    /// <summary>
    /// Handles the given device command and returns a response.
    /// </summary>
    /// <param name="command">The command to process.</param>
    /// <returns>A WebSocketResponse indicating the result.</returns>
    WebSocketResponse Handle(DeviceCommand command);
}

/// <summary>
/// WebSocket service that processes incoming IoT device commands and dispatches them to appropriate handlers.
/// </summary>
public class IoTWebSocketService : WebSocketBehavior
{
    #region Fields

    private readonly DeviceController deviceController;
    private readonly Dictionary<DeviceType, ICommandHandler> handlers;

    #endregion

    #region Constructor

    /// <summary>
    /// Initializes the WebSocket service with a DeviceController and sets up command handlers.
    /// </summary>
    /// <param name="controller">The DeviceController instance to use.</param>
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

    #endregion

    #region WebSocket Events

    protected override void OnOpen()
    {
        base.OnOpen();
        SendInitialState();
    }

    protected override void OnMessage(MessageEventArgs e)
    {
        try
        {
            var data = e.Data;
            if (data == "requestInitialState")
            {
                SendInitialState();
                return;
            }
            
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

    #endregion

    #region Command Processing

    /// <summary>
    /// Processes an incoming device command by routing it to the appropriate handler.
    /// </summary>
    /// <param name="command">The device command to process.</param>
    /// <returns>A response indicating the result of the command execution.</returns>
    private WebSocketResponse ProcessCommand(DeviceCommand command)
    {
        if (handlers.TryGetValue(command.DeviceType, out var handler))
        {
            var response = handler.Handle(command);
            response.type = "deviceState"; // Add type for client compatibility
            return response;
        }
        return new WebSocketResponse { success = false, message = $"Unknown device: {command.device}", type = "commandResponse" };
    }

    #endregion

    #region Helper Function

    private void SendInitialState()
    {
        var initialState = new Dictionary<string, object>();
        var devices = new List<object>();

        // Get all device controllers
        var deviceControllers = UnityEngine.Object.FindObjectsOfType<SmartDevice>();
        
        foreach (var device in deviceControllers)
        {
            var deviceInfo = device.GetStatus();
            deviceInfo["location"] = device.RoomNumber;
            deviceInfo["deviceId"] = device.DeviceID;
            deviceInfo["deviceType"] = device.GetType().Name.Replace("Controller", "");
            devices.Add(deviceInfo);
        }

        initialState["devices"] = devices;
        initialState["timestamp"] = DateTime.UtcNow.ToString("o");
        initialState["messageType"] = "initialState";

        var response = new WebSocketResponse
        {
            success = true,
            message = "Initial state",
            data = initialState,
            type = "deviceList"
        };
        Send(JsonConvert.SerializeObject(response));
    }

    private void SendLegacyInitialState()
    {
        var initialState = new Dictionary<string, object>();

        // Lights
        var lights = new List<object>();
        for (int i = 0; i < deviceController.lightSettings.Count; i++)
        {
            var settings = deviceController.GetLightSettings(i);
            lights.Add(new
            {
                device = "light",
                deviceIndex = i,
                state = settings.isOn,
                intensity = settings.intensity,
                color = settings.hexColor
            });
        }
        initialState["lights"] = lights;

        // TV
        initialState["tv"] = new
        {
            device = "tv",
            state = deviceController.tvOn,
            volume = deviceController.tvVolume,
            channel = deviceController.tvChannel,
            source = deviceController.tvSource
        };

        // AC
        initialState["ac"] = new
        {
            device = "ac",
            state = deviceController.acOn,
            temperature = deviceController.acTemperature,
            fanSpeed = deviceController.acFanSpeed,
            ecoMode = deviceController.acEcoMode
        };

        // Other devices...

        var response = new WebSocketResponse
        {
            success = true,
            message = "Initial state",
            data = initialState
        };
        Send(JsonConvert.SerializeObject(response));
    }

    #endregion
}

#region Command Handlers

/// <summary>
/// Handles commands for light devices.
/// </summary>
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
            int lightIndex = command.deviceIndex;
            if (lightIndex < 0 || lightIndex >= deviceController.lightControllers.Length || deviceController.lightControllers[lightIndex] == null)
                throw new ArgumentException($"Invalid light index: {lightIndex}");

            var lightController = deviceController.lightControllers[lightIndex];
            var action = command.GetAction<LightAction>();
            
            switch (action)
            {
                case LightAction.Toggle:
                    bool state = command.parameters != null && command.parameters.ContainsKey("state")
                        ? Convert.ToBoolean(command.parameters["state"])
                        : !deviceController.lightSettings[lightIndex].isOn;
                    deviceController.ToggleLight(lightIndex, state);
                    // Also update the light controller directly for immediate feedback
                    lightController.ToggleLight(state);
                    break;
                case LightAction.SetIntensity:
                    if (command.parameters == null || !command.parameters.ContainsKey("intensity"))
                        throw new ArgumentException("Missing 'intensity' parameter for setintensity action.");
                    float intensity = Convert.ToSingle(command.parameters["intensity"]);
                    deviceController.SetLightIntensity(lightIndex, intensity);
                    // Also update the light controller directly for immediate feedback
                    lightController.SetLightIntensity(intensity);
                    break;
                case LightAction.SetColor:
                    if (command.parameters == null || !command.parameters.ContainsKey("color"))
                        throw new ArgumentException("Missing 'color' parameter for setcolor action.");
                    string colorStr = command.parameters["color"].ToString();
                    if (!colorStr.StartsWith("#"))
                        colorStr = "#" + colorStr;
                    
                    Color color = Color.white;
                    if (ColorUtility.TryParseHtmlString(colorStr, out color))
                    {
                        deviceController.SetLightColor(lightIndex, colorStr.Replace("#", ""));
                        // Also update the light controller directly for immediate feedback
                        lightController.SetLightColor(color);
                    }
                    else
                    {
                        throw new ArgumentException($"Invalid color format: {colorStr}");
                    }
                    break;
            }

            // Get updated status directly from the light controller
            Dictionary<string, object> status = lightController.GetStatus();
            status["deviceIndex"] = lightIndex;
            
            return new WebSocketResponse
            {
                success = true,
                message = "Light command processed successfully.",
                data = status,
                type = "deviceState"
            };
        }
        catch (Exception ex)
        {
            return new WebSocketResponse { success = false, message = $"Light command failed: {ex.Message}", type = "commandResponse" };
        }
    }
}

/// <summary>
/// Handles commands for TV devices.
/// </summary>
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
                        throw new ArgumentException("Missing 'volume' parameter.");
                    int volume = Convert.ToInt32(command.parameters["volume"]);
                    deviceController.SetTVVolume(volume);
                    break;
                case TVAction.SetChannel:
                    if (command.parameters == null || !command.parameters.ContainsKey("channel"))
                        throw new ArgumentException("Missing 'channel' parameter.");
                    int channel = Convert.ToInt32(command.parameters["channel"]);
                    deviceController.SetTVChannel(channel);
                    break;
                case TVAction.SetSource:
                    if (command.parameters == null || !command.parameters.ContainsKey("source"))
                        throw new ArgumentException("Missing 'source' parameter.");
                    string source = command.parameters["source"].ToString();
                    deviceController.SetTVSource(source);
                    break;
            }
            return new WebSocketResponse
            {
                success = true,
                message = "TV command processed successfully.",
                data = new
                {
                    device = "tv",
                    state = deviceController.tvOn,
                    volume = deviceController.tvVolume,
                    channel = deviceController.tvChannel,
                    source = deviceController.tvSource
                }
            };
        }
        catch (Exception ex)
        {
            return new WebSocketResponse { success = false, message = $"TV command failed: {ex.Message}" };
        }
    }
}

/// <summary>
/// Handles commands for AC devices.
/// </summary>
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
            return new WebSocketResponse
            {
                success = true,
                message = "AC command processed successfully.",
                data = new
                {
                    device = "ac",
                    state = deviceController.acOn,
                    temperature = deviceController.acTemperature,
                    fanSpeed = deviceController.acFanSpeed,
                    ecoMode = deviceController.acEcoMode
                }
            };
        }
        catch (Exception ex)
        {
            return new WebSocketResponse { success = false, message = $"AC command failed: {ex.Message}" };
        }
    }
}

/// <summary>
/// Handles commands for fridge devices.
/// </summary>
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
            return new WebSocketResponse
            {
                success = true,
                message = "Fridge command processed successfully.",
                data = new
                {
                    device = "fridge",
                    state = deviceController.fridgeOn,
                    temperature = deviceController.fridgeTemperature,
                    freezeTemperature = deviceController.freezeTemperature,
                    fridgeDoor = deviceController.fridgeDoorOpen,
                    freezeDoor = deviceController.freezeDoorOpen
                }
            };
        }
        catch (Exception ex)
        {
            return new WebSocketResponse { success = false, message = $"Fridge command failed: {ex.Message}" };
        }
    }
}

/// <summary>
/// Handles commands for induction cooktops.
/// </summary>
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
            return new WebSocketResponse
            {
                success = true,
                message = "Induction command processed successfully.",
                data = new
                {
                    device = "induction",
                    level = deviceController.inductionHeat
                }
            };
        }
        catch (Exception ex)
        {
            return new WebSocketResponse { success = false, message = $"Induction command failed: {ex.Message}" };
        }
    }
}

/// <summary>
/// Handles commands for washing machines.
/// </summary>
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
            return new WebSocketResponse
            {
                success = true,
                message = "Washing machine command processed successfully.",
                data = new
                {
                    device = "washingmachine",
                    state = deviceController.washingMachineOn
                }
            };
        }
        catch (Exception ex)
        {
            return new WebSocketResponse { success = false, message = $"Washing machine command failed: {ex.Message}" };
        }
    }
}

/// <summary>
/// Handles commands for fans.
/// </summary>
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
            return new WebSocketResponse
            {
                success = true,
                message = "Fan command processed successfully.",
                data = new
                {
                    device = "fan",
                    state = deviceController.fanOn,
                    rpm = deviceController.fanRPM
                }
            };
        }
        catch (Exception ex)
        {
            return new WebSocketResponse { success = false, message = $"Fan command failed: {ex.Message}" };
        }
    }
}

#endregion

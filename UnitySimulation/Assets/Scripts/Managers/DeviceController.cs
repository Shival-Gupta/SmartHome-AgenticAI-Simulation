using UnityEngine;
using System.Collections.Generic;

#region Data Models

/// <summary>
/// Settings for a single light device.
/// </summary>
[System.Serializable]
public class LightSettings
{
    [Tooltip("Whether the light is on or off.")]
    public bool isOn = true;

    [Tooltip("Light intensity (0 to 2).")]
    [Range(0, 2)] public float intensity = 1.0f;

    [Tooltip("Light color in hexadecimal format (e.g., FFFFFF for white).")]
    public string hexColor = "FFFFFF";
}

#endregion

/// <summary>
/// Central controller for managing IoT device states and updating their respective controllers.
/// </summary>
public class DeviceController : MonoBehaviour
{
    #region Fields - Device Controllers

    [Header("Device Controllers")]
    [SerializeField, Tooltip("Array of light controllers in the scene.")]
    private LightController[] lightControllers;

    [SerializeField, Tooltip("TV controller in the scene.")]
    private TVController tvController;

    [SerializeField, Tooltip("Induction cooktop controller in the scene.")]
    private InductionController inductionController;

    [SerializeField, Tooltip("Fridge controller in the scene.")]
    private FridgeController fridgeController;

    [SerializeField, Tooltip("Air conditioner controller in the scene.")]
    private ACController acController;

    [SerializeField, Tooltip("Washing machine controller in the scene.")]
    private WashingMachineController washingMachineController;

    [SerializeField, Tooltip("Fan controller in the scene.")]
    private FanController fanController;

    #endregion

    #region Fields - Device Settings

    [Header("Light Settings")]
    [Tooltip("List of settings for each light in the scene.")]
    public List<LightSettings> lightSettings = new List<LightSettings>();

    [Header("TV Settings")]
    [Tooltip("Whether the TV is on or off.")]
    public bool tvOn;

    [Tooltip("TV volume level (0 to 100).")]
    [Range(0, 100)] public int tvVolume = 10;

    [Tooltip("Current TV channel.")]
    public int tvChannel = 1;

    [Tooltip("Current TV input source (e.g., HDMI1).")]
    public string tvSource = "HDMI1";

    [Header("Induction Settings")]
    [Tooltip("Heat level of the induction cooktop (0 to 3).")]
    [Range(0, 3)] public int inductionHeat;

    [Header("Fridge Settings")]
    [Tooltip("Whether the fridge is on or off.")]
    public bool fridgeOn;

    [Tooltip("Fridge temperature in degrees Celsius (-10 to 10).")]
    [Range(-10, 10)] public int fridgeTemperature = 4;

    [Tooltip("Freezer temperature in degrees Celsius (-30 to -10).")]
    [Range(-30, -10)] public int freezeTemperature = -18;

    [Header("Fridge Door Status")]
    [Tooltip("Set manually for testing: whether the fridge door is open.")]
    public bool fridgeDoorOpen;

    [Tooltip("Set manually for testing: whether the freezer door is open.")]
    public bool freezeDoorOpen;

    [Header("AC Settings")]
    [Tooltip("Air conditioner temperature in degrees Celsius (16 to 30).")]
    [Range(16, 30)] public int acTemperature = 24;

    [Tooltip("Fan speed of the air conditioner (0 to 3).")]
    [Range(0, 3)] public int acFanSpeed = 1;

    [Tooltip("Whether the air conditioner is on or off.")]
    public bool acOn;

    [Tooltip("Whether eco mode is enabled on the air conditioner.")]
    public bool acEcoMode;

    [Header("Washing Machine Settings")]
    [Tooltip("Whether the washing machine is on or off.")]
    public bool washingMachineOn;

    [Header("Fan Settings")]
    [Tooltip("Whether the fan is on or off.")]
    public bool fanOn;

    [Tooltip("Fan speed in RPM.")]
    public int fanRPM = 400;

    #endregion

    #region Fields - Change Tracking

    // Flags to track which devices need updates
    private bool lightsChanged;
    private bool tvChanged;
    private bool inductionChanged;
    private bool fridgeChanged;
    private bool acChanged;
    private bool washingMachineChanged;
    private bool fanChanged;

    #endregion

    #region Unity Lifecycle

    private void Update()
    {
        // Update only the devices that have changed
        if (lightsChanged)
        {
            UpdateLightControllers();
            lightsChanged = false;
        }
        if (tvChanged)
        {
            UpdateTVController();
            tvChanged = false;
        }
        if (inductionChanged)
        {
            UpdateInductionController();
            inductionChanged = false;
        }
        if (fridgeChanged)
        {
            UpdateFridgeController();
            fridgeChanged = false;
        }
        if (acChanged)
        {
            UpdateACController();
            acChanged = false;
        }
        if (washingMachineChanged)
        {
            UpdateWashingMachineController();
            washingMachineChanged = false;
        }
        if (fanChanged)
        {
            UpdateFanController();
            fanChanged = false;
        }
    }

    private void OnValidate()
    {
        // Set all change flags when values are modified in the Inspector
        lightsChanged = true;
        tvChanged = true;
        inductionChanged = true;
        fridgeChanged = true;
        acChanged = true;
        washingMachineChanged = true;
        fanChanged = true;
    }

    #endregion

    #region Public Methods - Command Handlers

    /// <summary>
    /// Gets the settings for a specific light by index.
    /// </summary>
    /// <param name="index">The index of the light.</param>
    /// <returns>The light settings.</returns>
    public LightSettings GetLightSettings(int index)
    {
        if (index < 0 || index >= lightSettings.Count)
            throw new System.ArgumentException($"Light index {index} out of range.", nameof(index));
        return lightSettings[index];
    }

    /// <summary>
    /// Toggles a light on or off.
    /// </summary>
    /// <param name="index">The index of the light.</param>
    /// <param name="state">The desired state (true for on, false for off).</param>
    public void ToggleLight(int index, bool state)
    {
        if (index < 0 || index >= lightSettings.Count) return;
        lightSettings[index].isOn = state;
        lightsChanged = true;
    }

    /// <summary>
    /// Sets the intensity of a light.
    /// </summary>
    /// <param name="index">The index of the light.</param>
    /// <param name="intensity">The intensity value (clamped between 0 and 2).</param>
    public void SetLightIntensity(int index, float intensity)
    {
        if (index < 0 || index >= lightSettings.Count) return;
        lightSettings[index].intensity = Mathf.Clamp(intensity, 0, 2);
        lightsChanged = true;
    }

    /// <summary>
    /// Sets the color of a light.
    /// </summary>
    /// <param name="index">The index of the light.</param>
    /// <param name="hexColor">The hex color code (e.g., "FFFFFF").</param>
    public void SetLightColor(int index, string hexColor)
    {
        if (index < 0 || index >= lightSettings.Count) return;
        lightSettings[index].hexColor = hexColor;
        lightsChanged = true;
    }

    /// <summary>
    /// Toggles the TV on or off.
    /// </summary>
    /// <param name="state">The desired state.</param>
    public void ToggleTV(bool state)
    {
        tvOn = state;
        tvChanged = true;
    }

    /// <summary>
    /// Sets the TV volume.
    /// </summary>
    /// <param name="volume">The volume level (clamped between 0 and 100).</param>
    public void SetTVVolume(int volume)
    {
        tvVolume = Mathf.Clamp(volume, 0, 100);
        tvChanged = true;
    }

    /// <summary>
    /// Sets the TV channel.
    /// </summary>
    /// <param name="channel">The channel number.</param>
    public void SetTVChannel(int channel)
    {
        tvChannel = channel;
        tvChanged = true;
    }

    /// <summary>
    /// Sets the TV input source.
    /// </summary>
    /// <param name="source">The input source (e.g., "HDMI1").</param>
    public void SetTVSource(string source)
    {
        tvSource = source;
        tvChanged = true;
    }

    /// <summary>
    /// Sets the induction cooktop heat level.
    /// </summary>
    /// <param name="level">The heat level (clamped between 0 and 3).</param>
    public void SetInductionHeat(int level)
    {
        inductionHeat = Mathf.Clamp(level, 0, 3);
        inductionChanged = true;
    }

    /// <summary>
    /// Toggles the fridge on or off.
    /// </summary>
    /// <param name="state">The desired state.</param>
    public void ToggleFridge(bool state)
    {
        fridgeOn = state;
        fridgeChanged = true;
    }

    /// <summary>
    /// Sets the fridge temperature.
    /// </summary>
    /// <param name="temp">The temperature (clamped between -10 and 10).</param>
    public void SetFridgeTemperature(int temp)
    {
        fridgeTemperature = Mathf.Clamp(temp, -10, 10);
        fridgeChanged = true;
    }

    /// <summary>
    /// Sets the freezer temperature.
    /// </summary>
    /// <param name="temp">The temperature (clamped between -30 and -10).</param>
    public void SetFreezeTemperature(int temp)
    {
        freezeTemperature = Mathf.Clamp(temp, -30, -10);
        fridgeChanged = true;
    }

    /// <summary>
    /// Sets the fridge door state.
    /// </summary>
    /// <param name="state">The desired state.</param>
    public void SetFridgeDoorOpen(bool state)
    {
        fridgeDoorOpen = state;
        fridgeChanged = true;
    }

    /// <summary>
    /// Sets the freezer door state.
    /// </summary>
    /// <param name="state">The desired state.</param>
    public void SetFreezeDoorOpen(bool state)
    {
        freezeDoorOpen = state;
        fridgeChanged = true;
    }

    /// <summary>
    /// Toggles the air conditioner on or off.
    /// </summary>
    /// <param name="state">The desired state.</param>
    public void ToggleAC(bool state)
    {
        acOn = state;
        acChanged = true;
    }

    /// <summary>
    /// Sets the air conditioner temperature.
    /// </summary>
    /// <param name="temp">The temperature (clamped between 16 and 30).</param>
    public void SetACTemperature(int temp)
    {
        acTemperature = Mathf.Clamp(temp, 16, 30);
        acChanged = true;
    }

    /// <summary>
    /// Sets the air conditioner fan speed.
    /// </summary>
    /// <param name="speed">The fan speed (clamped between 0 and 3).</param>
    public void SetACFanSpeed(int speed)
    {
        acFanSpeed = Mathf.Clamp(speed, 0, 3);
        acChanged = true;
    }

    /// <summary>
    /// Toggles the air conditioner eco mode.
    /// </summary>
    /// <param name="state">The desired state.</param>
    public void ToggleACEcoMode(bool state)
    {
        acEcoMode = state;
        acChanged = true;
    }

    /// <summary>
    /// Toggles the washing machine on or off.
    /// </summary>
    /// <param name="state">The desired state.</param>
    public void ToggleWashingMachine(bool state)
    {
        washingMachineOn = state;
        washingMachineChanged = true;
    }

    /// <summary>
    /// Toggles the fan on or off.
    /// </summary>
    /// <param name="state">The desired state.</param>
    public void ToggleFan(bool state)
    {
        fanOn = state;
        fanChanged = true;
    }

    /// <summary>
    /// Sets the fan RPM.
    /// </summary>
    /// <param name="rpm">The RPM value.</param>
    public void SetFanRPM(int rpm)
    {
        fanRPM = rpm;
        fanChanged = true;
    }

    #endregion

    #region Controller Update Methods

    /// <summary>
    /// Updates all light controllers based on current settings.
    /// </summary>
    private void UpdateLightControllers()
    {
        if (lightControllers == null || lightSettings.Count != lightControllers.Length) return;

        for (int i = 0; i < lightControllers.Length; i++)
        {
            if (lightControllers[i] == null) continue;

            var controller = lightControllers[i];
            var settings = lightSettings[i];

            controller.ToggleLight(settings.isOn);
            controller.SetLightIntensity(settings.intensity);

            if (ColorUtility.TryParseHtmlString("#" + settings.hexColor, out Color color))
            {
                controller.SetLightColor(color);
            }
            else
            {
                Debug.LogWarning($"Invalid hex color: {settings.hexColor}");
            }
        }
    }

    /// <summary>
    /// Updates the TV controller based on current settings.
    /// </summary>
    private void UpdateTVController()
    {
        if (tvController == null) return;

        tvController.ToggleTV(tvOn);
        tvController.SetVolume(tvVolume);
        tvController.SetChannel(tvChannel);
        tvController.SetSource(tvSource);
    }

    /// <summary>
    /// Updates the induction controller based on current settings.
    /// </summary>
    private void UpdateInductionController()
    {
        if (inductionController == null) return;

        inductionController.heatLevel = inductionHeat;
    }

    /// <summary>
    /// Updates the fridge controller based on current settings.
    /// </summary>
    private void UpdateFridgeController()
    {
        if (fridgeController == null) return;

        fridgeController.ToggleFridge(fridgeOn);
        fridgeController.SetTemperature(fridgeTemperature);
        fridgeController.SetFreezeTemperature(freezeTemperature);
        fridgeController.mainDoorOpen = fridgeDoorOpen;
        fridgeController.freezeDoorOpen = freezeDoorOpen;
    }

    /// <summary>
    /// Updates the AC controller based on current settings.
    /// </summary>
    private void UpdateACController()
    {
        if (acController == null) return;

        acController.ToggleAC(acOn);
        acController.SetFanSpeed(acFanSpeed);
        acController.SetTemperature(acTemperature);
        acController.ToggleEcoMode(acEcoMode);
    }

    /// <summary>
    /// Updates the washing machine controller based on current settings.
    /// </summary>
    private void UpdateWashingMachineController()
    {
        if (washingMachineController == null) return;

        washingMachineController.ToggleWashingMachine(washingMachineOn);
    }

    /// <summary>
    /// Updates the fan controller based on current settings.
    /// </summary>
    private void UpdateFanController()
    {
        if (fanController == null) return;

        fanController.ToggleFan(fanOn);
        fanController.SetRPM(fanRPM);
    }

    #endregion

    #region Public Utility Methods

    /// <summary>
    /// Logs the status of all devices to the Unity console.
    /// </summary>
    public void LogAllDeviceStatus()
    {
        for (int i = 0; i < lightControllers.Length; i++)
        {
            if (lightControllers[i] != null)
            {
                string[] status = lightControllers[i].GetStatusArray();
                Debug.Log($"Light {i} Status:\n{string.Join("\n", status)}");
            }
        }

        if (tvController != null)
        {
            string[] status = tvController.GetStatusArray();
            Debug.Log($"TV Status:\n{string.Join("\n", status)}");
        }

        if (acController != null)
        {
            string[] status = acController.GetStatusArray();
            Debug.Log($"AC Status:\n{string.Join("\n", status)}");
        }

        if (fridgeController != null)
        {
            string[] status = fridgeController.GetStatusArray();
            Debug.Log($"Fridge Status:\n{string.Join("\n", status)}");
        }

        if (inductionController != null)
        {
            string[] status = inductionController.GetStatusArray();
            Debug.Log($"Induction Status:\n{string.Join("\n", status)}");
        }

        if (washingMachineController != null)
        {
            string[] status = washingMachineController.GetStatusArray();
            Debug.Log($"Washing Machine Status:\n{string.Join("\n", status)}");
        }

        if (fanController != null)
        {
            string[] status = fanController.GetStatusArray();
            Debug.Log($"Fan Status:\n{string.Join("\n", status)}");
        }
    }

    #endregion
}

using UnityEngine;
using TMPro;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// Controller for smart air conditioner devices in the IoT system.
/// Manages temperature, fan speed, power state, and eco mode with UI feedback.
/// </summary>
public class ACController : SmartDevice
{
    [Header("Device Settings")]
    [SerializeField, Tooltip("Whether the AC is currently on")]
    private bool isOn = false;
    
    [SerializeField, Tooltip("Current fan speed (0-3)"), Range(0, 3)]
    private int fanSpeed = 1;
    
    [SerializeField, Tooltip("Target temperature in Celsius"), Range(16, 30)]
    private int temperature = 24;
    
    [SerializeField, Tooltip("Whether eco mode is enabled")]
    private bool ecoMode = false;
    
    [SerializeField, Tooltip("Room where this AC is located")]
    private string roomLocation = "Bedroom";

    [Header("UI References")]
    [SerializeField, Tooltip("Text component for displaying AC status")]
    private TMP_Text acStatusText;
    
    [SerializeField, Tooltip("Text component for displaying temperature")]
    private TMP_Text acTemperatureText;
    
    [SerializeField, Tooltip("Text component for displaying fan speed")]
    private TMP_Text acFanSpeedText;
    
    [SerializeField, Tooltip("Text component for displaying eco mode")]
    private TMP_Text acEcoModeText;

    [Header("Optional Components")]
    [SerializeField, Tooltip("Particle system for cool air simulation")]
    private ParticleSystem airParticles;

    /// <summary>
    /// Minimum allowed temperature setting
    /// </summary>
    public const int MIN_TEMPERATURE = 16;
    
    /// <summary>
    /// Maximum allowed temperature setting
    /// </summary>
    public const int MAX_TEMPERATURE = 30;
    
    /// <summary>
    /// Maximum fan speed level
    /// </summary>
    public const int MAX_FAN_SPEED = 3;

    /// <summary>
    /// Gets or sets whether the AC is powered on
    /// </summary>
    public bool IsPoweredOn
    {
        get => isOn;
        private set => isOn = value;
    }

    /// <summary>
    /// Gets or sets the current fan speed level
    /// </summary>
    public int FanSpeed
    {
        get => fanSpeed;
        private set => fanSpeed = Mathf.Clamp(value, 0, MAX_FAN_SPEED);
    }

    /// <summary>
    /// Gets or sets the target temperature
    /// </summary>
    public int Temperature
    {
        get => temperature;
        private set => temperature = Mathf.Clamp(value, MIN_TEMPERATURE, MAX_TEMPERATURE);
    }

    /// <summary>
    /// Gets or sets whether eco mode is enabled
    /// </summary>
    public bool EcoMode
    {
        get => ecoMode;
        private set => ecoMode = value;
    }

    /// <summary>
    /// Initialize the AC controller
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        SetRoomNumber(roomLocation);
    }

    private void Start()
    {
        UpdateACUI();
        UpdateACEffects();
    }

    /// <summary>
    /// Toggle the AC power state
    /// </summary>
    /// <param name="state">True to turn on, false to turn off</param>
    public void ToggleAC(bool state)
    {
        isOn = state;
        Debug.Log($"[{DeviceID}] AC is now {(isOn ? "ON" : "OFF")}");
        UpdateACUI();
        UpdateACEffects();
    }

    /// <summary>
    /// Set the AC fan speed
    /// </summary>
    /// <param name="speed">Fan speed level (0-3)</param>
    /// <returns>The actual fan speed set after validation</returns>
    public int SetFanSpeed(int speed)
    {
        if (speed < 0 || speed > MAX_FAN_SPEED)
        {
            Debug.LogWarning($"[{DeviceID}] Invalid fan speed: {speed}. Valid range is 0-{MAX_FAN_SPEED}.");
            return fanSpeed;
        }

        fanSpeed = speed;
        Debug.Log($"[{DeviceID}] AC Fan Speed set to {fanSpeed}");
        UpdateACUI();
        UpdateACEffects();
        return fanSpeed;
    }

    /// <summary>
    /// Set the target temperature
    /// </summary>
    /// <param name="temp">Temperature in Celsius</param>
    /// <returns>The actual temperature set after validation</returns>
    public int SetTemperature(int temp)
    {
        temperature = Mathf.Clamp(temp, MIN_TEMPERATURE, MAX_TEMPERATURE);
        Debug.Log($"[{DeviceID}] AC Temperature set to {temperature}°C");
        UpdateACUI();
        return temperature;
    }

    /// <summary>
    /// Toggle eco mode
    /// </summary>
    /// <param name="state">True to enable eco mode, false to disable</param>
    public void ToggleEcoMode(bool state)
    {
        ecoMode = state;
        Debug.Log($"[{DeviceID}] Eco Mode is now {(ecoMode ? "ON" : "OFF")}");
        UpdateACUI();
        
        // In eco mode, automatically adjust fan speed to save energy
        if (ecoMode && fanSpeed > 1)
        {
            SetFanSpeed(1);
        }
    }

    /// <summary>
    /// Update the UI elements with current AC status
    /// </summary>
    private void UpdateACUI()
    {
        if (acStatusText != null)
            acStatusText.text = $"AC: {(isOn ? "ON" : "OFF")}";
        if (acTemperatureText != null)
            acTemperatureText.text = $"Temperature: {temperature}°C";
        if (acFanSpeedText != null)
            acFanSpeedText.text = $"Fan Speed: {fanSpeed}";
        if (acEcoModeText != null)
            acEcoModeText.text = $"Eco Mode: {(ecoMode ? "ON" : "OFF")}";
    }
    
    /// <summary>
    /// Update visual effects based on AC state
    /// </summary>
    private void UpdateACEffects()
    {
        if (airParticles == null) return;
        
        var emission = airParticles.emission;
        var main = airParticles.main;
        
        if (isOn)
        {
            emission.enabled = true;
            emission.rateOverTime = fanSpeed * 5f;
            main.startSpeed = 0.5f + (fanSpeed * 0.5f);
        }
        else
        {
            emission.enabled = false;
        }
    }

    /// <summary>
    /// Gets the current device status
    /// </summary>
    /// <returns>A dictionary with the current device status</returns>
    public override Dictionary<string, object> GetStatus()
    {
        // Get the base status from the parent class
        Dictionary<string, object> status = base.GetStatus();
        
        // Add AC-specific status
        status["power"] = isOn;
        status["temperature"] = temperature;
        status["fanSpeed"] = fanSpeed;
        status["ecoMode"] = ecoMode;
        
        return status;
    }

    /// <summary>
    /// Legacy method for backward compatibility.
    /// Gets the AC status as an array of strings.
    /// </summary>
    /// <returns>String array with status information</returns>
    public override string[] GetStatusArray()
    {
        string[] baseStatus = base.GetStatusArray();
        string[] acStatus = new string[]
        {
            $"Power: {(isOn ? "ON" : "OFF")}",
            $"Temperature: {temperature}°C",
            $"Fan Speed: {fanSpeed}",
            $"Eco Mode: {(ecoMode ? "ON" : "OFF")}"
        };
        return baseStatus.Concat(acStatus).ToArray();
    }
    
    /// <summary>
    /// Validates values when changed in the inspector
    /// </summary>
    private void OnValidate()
    {
        temperature = Mathf.Clamp(temperature, MIN_TEMPERATURE, MAX_TEMPERATURE);
        fanSpeed = Mathf.Clamp(fanSpeed, 0, MAX_FAN_SPEED);
    }
}

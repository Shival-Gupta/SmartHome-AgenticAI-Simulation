using UnityEngine;
using TMPro;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// Controller for smart fan devices in the IoT system.
/// Manages fan power state and RPM settings with UI feedback.
/// </summary>
public class FanController : SmartDevice
{
    [Header("Device Settings")]
    [SerializeField, Tooltip("Room where this fan is located")]
    private string roomLocation = "Living Room";

    [SerializeField, Tooltip("Whether the fan is currently on")]
    private bool isOn = false;

    [SerializeField, Tooltip("Fan speed in RPM (100-2000)"), Range(100, 2000)]
    private int rpm = 400;

    [Header("UI References")]
    [SerializeField, Tooltip("Text component for displaying fan status")]
    private TMP_Text fanStatusText;

    [SerializeField, Tooltip("Text component for displaying fan RPM")]
    private TMP_Text fanRPMText;

    [Header("Optional Components")]
    [SerializeField, Tooltip("Gameobject representing the fan blades (for rotation)")]
    private Transform fanBlades;

    [SerializeField, Tooltip("Rotation speed multiplier")]
    private float rotationMultiplier = 0.1f;

    /// <summary>
    /// Minimum allowed fan RPM
    /// </summary>
    public const int MIN_RPM = 100;

    /// <summary>
    /// Maximum allowed fan RPM
    /// </summary>
    public const int MAX_RPM = 2000;

    /// <summary>
    /// Gets or sets whether the fan is powered on
    /// </summary>
    public bool IsPoweredOn 
    { 
        get => isOn;
        private set => isOn = value;
    }

    /// <summary>
    /// Gets or sets the current fan RPM
    /// </summary>
    public int CurrentRPM
    {
        get => rpm;
        private set => rpm = Mathf.Clamp(value, MIN_RPM, MAX_RPM);
    }

    /// <summary>
    /// Initialize the fan controller
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        SetRoomNumber(roomLocation);
    }

    private void Start()
    {
        UpdateFanUI();
    }

    private void Update()
    {
        // Rotate fan blades if powered on and we have a reference
        if (isOn && fanBlades != null)
        {
            float rotationSpeed = rpm * rotationMultiplier * Time.deltaTime;
            fanBlades.Rotate(0, 0, rotationSpeed);
        }
    }

    /// <summary>
    /// Toggles the fan's power state
    /// </summary>
    /// <param name="state">True to turn on, false to turn off</param>
    public void ToggleFan(bool state)
    {
        isOn = state;
        Debug.Log($"[{DeviceID}] Fan is now {(isOn ? "ON" : "OFF")}");
        UpdateFanUI();
    }

    /// <summary>
    /// Sets the fan's rotation speed in RPM
    /// </summary>
    /// <param name="newRPM">The target RPM (will be clamped between MIN_RPM and MAX_RPM)</param>
    /// <returns>The actual RPM set after clamping</returns>
    public int SetRPM(int newRPM)
    {
        rpm = Mathf.Clamp(newRPM, MIN_RPM, MAX_RPM);
        Debug.Log($"[{DeviceID}] Fan speed set to {rpm} RPM");
        UpdateFanUI();
        return rpm;
    }

    /// <summary>
    /// Updates the UI elements with current fan status
    /// </summary>
    private void UpdateFanUI()
    {
        if (fanStatusText != null)
            fanStatusText.text = $"Fan: {(isOn ? "ON" : "OFF")}";

        if (fanRPMText != null)
            fanRPMText.text = $"RPM: {rpm}";
    }

    /// <summary>
    /// Gets the current device status
    /// </summary>
    /// <returns>A dictionary with the current device status</returns>
    public override Dictionary<string, object> GetStatus()
    {
        // Get the base status from the parent class
        Dictionary<string, object> status = base.GetStatus();
        
        // Add Fan-specific status
        status["power"] = isOn;
        status["rpm"] = rpm;
        
        return status;
    }

    /// <summary>
    /// Legacy method for backward compatibility.
    /// Gets the fan status as an array of strings.
    /// </summary>
    /// <returns>String array with status information</returns>
    public override string[] GetStatusArray()
    {
        string[] baseStatus = base.GetStatusArray();
        string[] fanStatus = new string[]
        {
            $"Power: {(isOn ? "ON" : "OFF")}",
            $"RPM: {rpm}"
        };
        return baseStatus.Concat(fanStatus).ToArray();
    }

    /// <summary>
    /// Validates values when changed in the inspector
    /// </summary>
    private void OnValidate()
    {
        rpm = Mathf.Clamp(rpm, MIN_RPM, MAX_RPM);
    }
}

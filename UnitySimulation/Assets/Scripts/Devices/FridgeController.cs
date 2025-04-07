using UnityEngine;
using TMPro;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// Controller for smart refrigerator devices in the IoT system.
/// Manages fridge power, temperature, freezer temperature, and door status with UI feedback.
/// </summary>
public class FridgeController : SmartDevice
{
    [Header("Device Settings")]
    [SerializeField, Tooltip("Whether the fridge is currently on")]
    private bool isOn = false;
    
    [SerializeField, Tooltip("Main compartment temperature (°C)"), Range(-10, 10)]
    private int mainTemperature = 4;
    
    [SerializeField, Tooltip("Freezer compartment temperature (°C)"), Range(-30, -10)]
    private int freezeTemperature = -18;
    
    /// <summary>
    /// Whether the main compartment door is open
    /// </summary>
    public bool mainDoorOpen = false;
    
    /// <summary>
    /// Whether the freezer compartment door is open
    /// </summary>
    public bool freezeDoorOpen = false;
    
    [SerializeField, Tooltip("Room where this fridge is located")]
    private string roomLocation = "Kitchen";

    [Header("UI References")]
    [SerializeField, Tooltip("Text component for displaying fridge status")]
    private TMP_Text fridgeStatusText;
    
    [SerializeField, Tooltip("Text component for displaying main temperature")]
    private TMP_Text fridgeTempText;
    
    [SerializeField, Tooltip("Text component for displaying freezer temperature")]
    private TMP_Text freezeTempText;
    
    [SerializeField, Tooltip("Text component for displaying main door status")]
    private TMP_Text fridgeDoorStatusText;
    
    [SerializeField, Tooltip("Text component for displaying freezer door status")]
    private TMP_Text freezeDoorStatusText;

    [Header("Optional Components")]
    [SerializeField, Tooltip("Light that turns on when fridge door is open")]
    private Light fridgeInternalLight;
    
    [SerializeField, Tooltip("GameObject representing the main door")]
    private Transform mainDoorTransform;
    
    [SerializeField, Tooltip("GameObject representing the freezer door")]
    private Transform freezerDoorTransform;
    
    [SerializeField, Tooltip("Maximum door opening angle")]
    private float doorOpenAngle = 120f;

    /// <summary>
    /// Minimum allowed main compartment temperature
    /// </summary>
    public const int MIN_MAIN_TEMP = -10;
    
    /// <summary>
    /// Maximum allowed main compartment temperature
    /// </summary>
    public const int MAX_MAIN_TEMP = 10;
    
    /// <summary>
    /// Minimum allowed freezer compartment temperature
    /// </summary>
    public const int MIN_FREEZE_TEMP = -30;
    
    /// <summary>
    /// Maximum allowed freezer compartment temperature
    /// </summary>
    public const int MAX_FREEZE_TEMP = -10;

    /// <summary>
    /// Gets or sets whether the fridge is powered on
    /// </summary>
    public bool IsPoweredOn
    {
        get => isOn;
        private set => isOn = value;
    }

    /// <summary>
    /// Gets or sets the main compartment temperature
    /// </summary>
    public int MainTemperature
    {
        get => mainTemperature;
        private set => mainTemperature = Mathf.Clamp(value, MIN_MAIN_TEMP, MAX_MAIN_TEMP);
    }

    /// <summary>
    /// Gets or sets the freezer compartment temperature
    /// </summary>
    public int FreezeTemperature
    {
        get => freezeTemperature;
        private set => freezeTemperature = Mathf.Clamp(value, MIN_FREEZE_TEMP, MAX_FREEZE_TEMP);
    }

    /// <summary>
    /// Initialize the fridge controller
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        SetRoomNumber(roomLocation);
    }

    private void Start()
    {
        UpdateFridgeUI();
        UpdateDoorVisuals();
    }

    /// <summary>
    /// Update door visuals when values change in the editor or at runtime
    /// </summary>
    private void Update()
    {
        // This ensures door visuals are updated whenever the fields are changed directly
        UpdateDoorVisuals();
    }

    /// <summary>
    /// Toggle the fridge power state
    /// </summary>
    /// <param name="state">True to turn on, false to turn off</param>
    public void ToggleFridge(bool state)
    {
        isOn = state;
        Debug.Log($"[{DeviceID}] Fridge is now {(isOn ? "ON" : "OFF")}");
        
        // If turning off, set temperatures to ambient
        if (!isOn)
        {
            mainTemperature = 20; // Room temperature
            freezeTemperature = 20;
        }
        else
        {
            // Restore default temperatures
            mainTemperature = 4;
            freezeTemperature = -18;
        }
        
        UpdateFridgeUI();
    }

    /// <summary>
    /// Set the main compartment temperature
    /// </summary>
    /// <param name="temp">Temperature in Celsius</param>
    /// <returns>The actual temperature set after validation</returns>
    public int SetTemperature(int temp)
    {
        if (!isOn)
        {
            Debug.LogWarning($"[{DeviceID}] Cannot set temperature while fridge is off");
            return mainTemperature;
        }
        
        mainTemperature = Mathf.Clamp(temp, MIN_MAIN_TEMP, MAX_MAIN_TEMP);
        Debug.Log($"[{DeviceID}] Fridge temperature set to {mainTemperature}°C");
        UpdateFridgeUI();
        return mainTemperature;
    }

    /// <summary>
    /// Set the freezer compartment temperature
    /// </summary>
    /// <param name="temp">Temperature in Celsius</param>
    /// <returns>The actual temperature set after validation</returns>
    public int SetFreezeTemperature(int temp)
    {
        if (!isOn)
        {
            Debug.LogWarning($"[{DeviceID}] Cannot set freezer temperature while fridge is off");
            return freezeTemperature;
        }
        
        freezeTemperature = Mathf.Clamp(temp, MIN_FREEZE_TEMP, MAX_FREEZE_TEMP);
        Debug.Log($"[{DeviceID}] Freezer temperature set to {freezeTemperature}°C");
        UpdateFridgeUI();
        return freezeTemperature;
    }

    /// <summary>
    /// Set the main compartment door state
    /// </summary>
    /// <param name="isOpen">True if the door should be open, false otherwise</param>
    public void SetMainDoorOpen(bool isOpen)
    {
        mainDoorOpen = isOpen;
        Debug.Log($"[{DeviceID}] Main door is now {(mainDoorOpen ? "open" : "closed")}");
        
        // If door is open for too long, raise temperature
        if (isOpen && isOn)
        {
            // This would be implemented with a coroutine in a real system
            // For now, we'll just log a warning
            Debug.LogWarning($"[{DeviceID}] Main door is open, temperature may rise");
        }
        
        UpdateFridgeUI();
        UpdateDoorVisuals();
    }

    /// <summary>
    /// Set the freezer compartment door state
    /// </summary>
    /// <param name="isOpen">True if the door should be open, false otherwise</param>
    public void SetFreezeDoorOpen(bool isOpen)
    {
        freezeDoorOpen = isOpen;
        Debug.Log($"[{DeviceID}] Freezer door is now {(freezeDoorOpen ? "open" : "closed")}");
        
        // If door is open for too long, raise temperature
        if (isOpen && isOn)
        {
            // This would be implemented with a coroutine in a real system
            // For now, we'll just log a warning
            Debug.LogWarning($"[{DeviceID}] Freezer door is open, temperature may rise");
        }
        
        UpdateFridgeUI();
        UpdateDoorVisuals();
    }

    /// <summary>
    /// Update the UI elements with current fridge status
    /// </summary>
    private void UpdateFridgeUI()
    {
        if (fridgeStatusText != null)
            fridgeStatusText.text = $"Fridge: {(isOn ? "ON" : "OFF")}";
        if (fridgeTempText != null)
            fridgeTempText.text = $"Main Temp: {mainTemperature}°C";
        if (freezeTempText != null)
            freezeTempText.text = $"Freezer Temp: {freezeTemperature}°C";
        if (fridgeDoorStatusText != null)
            fridgeDoorStatusText.text = $"Main Door: {(mainDoorOpen ? "Open" : "Shut")}";
        if (freezeDoorStatusText != null)
            freezeDoorStatusText.text = $"Freezer Door: {(freezeDoorOpen ? "Open" : "Shut")}";
    }
    
    /// <summary>
    /// Update the visual representation of the doors
    /// </summary>
    private void UpdateDoorVisuals()
    {
        // Update main door transform if available
        if (mainDoorTransform != null)
        {
            Vector3 mainDoorRotation = mainDoorTransform.localEulerAngles;
            mainDoorRotation.y = mainDoorOpen ? doorOpenAngle : 0;
            mainDoorTransform.localEulerAngles = mainDoorRotation;
        }
        
        // Update freezer door transform if available
        if (freezerDoorTransform != null)
        {
            Vector3 freezerDoorRotation = freezerDoorTransform.localEulerAngles;
            freezerDoorRotation.y = freezeDoorOpen ? doorOpenAngle : 0;
            freezerDoorTransform.localEulerAngles = freezerDoorRotation;
        }
        
        // Control internal light
        if (fridgeInternalLight != null)
        {
            fridgeInternalLight.enabled = (mainDoorOpen || freezeDoorOpen) && isOn;
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
        
        // Add fridge-specific status
        status["power"] = isOn;
        status["mainTemperature"] = mainTemperature;
        status["freezeTemperature"] = freezeTemperature;
        status["mainDoorOpen"] = mainDoorOpen;
        status["freezeDoorOpen"] = freezeDoorOpen;
        
        return status;
    }

    /// <summary>
    /// Legacy method for backward compatibility.
    /// Gets the fridge status as an array of strings.
    /// </summary>
    /// <returns>String array with status information</returns>
    public override string[] GetStatusArray()
    {
        string[] baseStatus = base.GetStatusArray();
        string[] fridgeStatus = new string[]
        {
            $"Power: {(isOn ? "ON" : "OFF")}",
            $"Main Temp: {mainTemperature}°C",
            $"Freeze Temp: {freezeTemperature}°C",
            $"Main Door: {(mainDoorOpen ? "Open" : "Shut")}",
            $"Freezer Door: {(freezeDoorOpen ? "Open" : "Shut")}"
        };
        return baseStatus.Concat(fridgeStatus).ToArray();
    }
    
    /// <summary>
    /// Validates values when changed in the inspector
    /// </summary>
    private void OnValidate()
    {
        mainTemperature = Mathf.Clamp(mainTemperature, MIN_MAIN_TEMP, MAX_MAIN_TEMP);
        freezeTemperature = Mathf.Clamp(freezeTemperature, MIN_FREEZE_TEMP, MAX_FREEZE_TEMP);
        
        // Update visuals if in editor
        if (!Application.isPlaying)
        {
            UpdateDoorVisuals();
        }
    }
}

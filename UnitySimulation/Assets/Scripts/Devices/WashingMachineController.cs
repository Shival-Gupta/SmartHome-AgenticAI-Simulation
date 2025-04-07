using UnityEngine;
using TMPro;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// Controller for smart washing machine devices in the IoT system.
/// Manages power state and wash cycles with visual feedback.
/// </summary>
public class WashingMachineController : SmartDevice
{
    [Header("Device Settings")]
    [SerializeField, Tooltip("Whether the washing machine is currently on")]
    private bool isOn = false;

    [SerializeField, Tooltip("Current wash cycle")]
    private WashCycle currentCycle = WashCycle.Normal;
    
    [SerializeField, Tooltip("Current wash temperature in Celsius"), Range(20, 90)]
    private int washTemperature = 40;
    
    [SerializeField, Tooltip("Remaining time in minutes")]
    private int remainingMinutes = 60;
    
    [SerializeField, Tooltip("Room where this washing machine is located")]
    private string roomLocation = "Bathroom";

    [Header("Visual Elements")]
    [SerializeField, Tooltip("GameObject representing the active state")]
    private GameObject onVisual;
    
    [SerializeField, Tooltip("GameObject representing the inactive state")]
    private GameObject offVisual;
    
    [SerializeField, Tooltip("Optional particle system for water")]
    private ParticleSystem waterParticles;
    
    [Header("UI Elements")]
    [SerializeField, Tooltip("Text component for displaying washing machine status")]
    private TMP_Text statusText;
    
    [SerializeField, Tooltip("Text component for displaying current cycle")]
    private TMP_Text cycleText;
    
    [SerializeField, Tooltip("Text component for displaying temperature")]
    private TMP_Text temperatureText;
    
    [SerializeField, Tooltip("Text component for displaying remaining time")]
    private TMP_Text timeText;

    /// <summary>
    /// Enum representing available wash cycles
    /// </summary>
    public enum WashCycle
    {
        Normal,
        Quick,
        Intensive,
        Delicate,
        EcoFriendly
    }

    /// <summary>
    /// Gets or sets whether the washing machine is powered on
    /// </summary>
    public bool IsPoweredOn
    {
        get => isOn;
        private set => isOn = value;
    }

    /// <summary>
    /// Gets or sets the current wash cycle
    /// </summary>
    public WashCycle CurrentCycle
    {
        get => currentCycle;
        private set => currentCycle = value;
    }

    /// <summary>
    /// Gets or sets the wash temperature
    /// </summary>
    public int WashTemperature
    {
        get => washTemperature;
        private set => washTemperature = Mathf.Clamp(value, 20, 90);
    }

    /// <summary>
    /// Gets the remaining time in minutes
    /// </summary>
    public int RemainingMinutes => remainingMinutes;

    /// <summary>
    /// Whether a wash cycle is currently in progress
    /// </summary>
    private bool isCycleRunning = false;
    
    /// <summary>
    /// Timer for counting down minutes
    /// </summary>
    private float minuteTimer = 0f;

    /// <summary>
    /// Initialize the washing machine controller
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        SetRoomNumber(roomLocation);
    }

    private void Start()
    {
        UpdateVisualState();
        UpdateUIElements();
    }

    private void Update()
    {
        // If cycle is running, update timer
        if (isCycleRunning && isOn)
        {
            minuteTimer += Time.deltaTime;
            
            // Every minute, decrease the remaining time
            if (minuteTimer >= 60f)
            {
                minuteTimer = 0f;
                remainingMinutes = Mathf.Max(0, remainingMinutes - 1);
                UpdateUIElements();
                
                // If cycle is complete
                if (remainingMinutes <= 0)
                {
                    CompleteCycle();
                }
            }
        }
    }

    /// <summary>
    /// Toggle the washing machine power state
    /// </summary>
    /// <param name="state">True to turn on, false to turn off</param>
    public void ToggleWashingMachine(bool state)
    {
        isOn = state;
        
        // If turning off, pause any running cycle
        if (!isOn && isCycleRunning)
        {
            PauseCycle();
        }
        
        UpdateVisualState();
        UpdateUIElements();
        Debug.Log($"[{DeviceID}] Washing machine is now {(isOn ? "ON" : "OFF")}");
    }

    /// <summary>
    /// Set the wash cycle
    /// </summary>
    /// <param name="cycle">The wash cycle to set</param>
    public void SetWashCycle(WashCycle cycle)
    {
        if (isCycleRunning)
        {
            Debug.LogWarning($"[{DeviceID}] Cannot change cycle while washing is in progress");
            return;
        }
        
        currentCycle = cycle;
        
        // Set appropriate default values based on cycle
        switch (cycle)
        {
            case WashCycle.Quick:
                remainingMinutes = 30;
                washTemperature = 30;
                break;
            case WashCycle.Intensive:
                remainingMinutes = 90;
                washTemperature = 60;
                break;
            case WashCycle.Delicate:
                remainingMinutes = 45;
                washTemperature = 20;
                break;
            case WashCycle.EcoFriendly:
                remainingMinutes = 75;
                washTemperature = 40;
                break;
            default: // Normal
                remainingMinutes = 60;
                washTemperature = 40;
                break;
        }
        
        UpdateUIElements();
        Debug.Log($"[{DeviceID}] Wash cycle set to {cycle}");
    }

    /// <summary>
    /// Set the wash temperature
    /// </summary>
    /// <param name="temperature">Temperature in Celsius (20-90)</param>
    /// <returns>The actual temperature set after validation</returns>
    public int SetTemperature(int temperature)
    {
        if (isCycleRunning)
        {
            Debug.LogWarning($"[{DeviceID}] Cannot change temperature while washing is in progress");
            return washTemperature;
        }
        
        washTemperature = Mathf.Clamp(temperature, 20, 90);
        UpdateUIElements();
        Debug.Log($"[{DeviceID}] Wash temperature set to {washTemperature}°C");
        return washTemperature;
    }

    /// <summary>
    /// Start the wash cycle
    /// </summary>
    /// <returns>True if the cycle was started successfully, false otherwise</returns>
    public bool StartCycle()
    {
        if (!isOn)
        {
            Debug.LogWarning($"[{DeviceID}] Cannot start cycle while machine is off");
            return false;
        }
        
        if (isCycleRunning)
        {
            Debug.LogWarning($"[{DeviceID}] Cycle already in progress");
            return false;
        }
        
        isCycleRunning = true;
        minuteTimer = 0f;
        
        // Start water particles if available
        if (waterParticles != null)
        {
            waterParticles.Play();
        }
        
        UpdateVisualState();
        UpdateUIElements();
        Debug.Log($"[{DeviceID}] Wash cycle started: {currentCycle}, {washTemperature}°C, {remainingMinutes} minutes remaining");
        return true;
    }

    /// <summary>
    /// Pause the current wash cycle
    /// </summary>
    public void PauseCycle()
    {
        if (!isCycleRunning) return;
        
        isCycleRunning = false;
        
        // Stop water particles if available
        if (waterParticles != null)
        {
            waterParticles.Pause();
        }
        
        UpdateVisualState();
        UpdateUIElements();
        Debug.Log($"[{DeviceID}] Wash cycle paused");
    }

    /// <summary>
    /// Resume a paused wash cycle
    /// </summary>
    /// <returns>True if the cycle was resumed successfully, false otherwise</returns>
    public bool ResumeCycle()
    {
        if (!isOn)
        {
            Debug.LogWarning($"[{DeviceID}] Cannot resume cycle while machine is off");
            return false;
        }
        
        if (isCycleRunning)
        {
            Debug.LogWarning($"[{DeviceID}] Cycle already running");
            return false;
        }
        
        if (remainingMinutes <= 0)
        {
            Debug.LogWarning($"[{DeviceID}] Cannot resume completed cycle");
            return false;
        }
        
        isCycleRunning = true;
        
        // Resume water particles if available
        if (waterParticles != null)
        {
            waterParticles.Play();
        }
        
        UpdateVisualState();
        UpdateUIElements();
        Debug.Log($"[{DeviceID}] Wash cycle resumed");
        return true;
    }

    /// <summary>
    /// Cancel the current wash cycle
    /// </summary>
    public void CancelCycle()
    {
        if (!isCycleRunning) return;
        
        isCycleRunning = false;
        remainingMinutes = 0;
        
        // Stop water particles if available
        if (waterParticles != null)
        {
            waterParticles.Stop();
        }
        
        UpdateVisualState();
        UpdateUIElements();
        Debug.Log($"[{DeviceID}] Wash cycle cancelled");
    }

    /// <summary>
    /// Handle completion of a wash cycle
    /// </summary>
    private void CompleteCycle()
    {
        isCycleRunning = false;
        remainingMinutes = 0;
        
        // Stop water particles if available
        if (waterParticles != null)
        {
            waterParticles.Stop();
        }
        
        UpdateVisualState();
        UpdateUIElements();
        Debug.Log($"[{DeviceID}] Wash cycle completed");
    }

    /// <summary>
    /// Update the visual representation of the washing machine based on state
    /// </summary>
    private void UpdateVisualState()
    {
        if (onVisual != null)
            onVisual.SetActive(isOn);
            
        if (offVisual != null)
            offVisual.SetActive(!isOn);
            
        // More complex visual updates could be added here
    }

    /// <summary>
    /// Update the UI elements with current washing machine status
    /// </summary>
    private void UpdateUIElements()
    {
        if (statusText != null)
        {
            if (!isOn)
                statusText.text = "Status: OFF";
            else if (isCycleRunning)
                statusText.text = "Status: RUNNING";
            else if (remainingMinutes <= 0)
                statusText.text = "Status: COMPLETE";
            else
                statusText.text = "Status: READY";
        }
        
        if (cycleText != null)
            cycleText.text = $"Cycle: {currentCycle}";
            
        if (temperatureText != null)
            temperatureText.text = $"Temperature: {washTemperature}°C";
            
        if (timeText != null)
            timeText.text = $"Time: {remainingMinutes} min";
    }

    /// <summary>
    /// Gets the current device status
    /// </summary>
    /// <returns>A dictionary with the current device status</returns>
    public override Dictionary<string, object> GetStatus()
    {
        // Get the base status from the parent class
        Dictionary<string, object> status = base.GetStatus();
        
        // Add washing machine-specific status
        status["power"] = isOn;
        status["cycle"] = currentCycle.ToString();
        status["temperature"] = washTemperature;
        status["remainingTime"] = remainingMinutes;
        status["isRunning"] = isCycleRunning;
        
        return status;
    }

    /// <summary>
    /// Legacy method for backward compatibility.
    /// Gets the washing machine status as an array of strings.
    /// </summary>
    /// <returns>String array with status information</returns>
    public override string[] GetStatusArray()
    {
        string[] baseStatus = base.GetStatusArray();
        string[] washingMachineStatus = new string[]
        {
            $"Power: {(isOn ? "ON" : "OFF")}",
            $"Cycle: {currentCycle}",
            $"Temperature: {washTemperature}°C",
            $"Remaining Time: {remainingMinutes} min",
            $"Status: {(isCycleRunning ? "Running" : "Idle")}"
        };
        return baseStatus.Concat(washingMachineStatus).ToArray();
    }
    
    /// <summary>
    /// Validates values when changed in the inspector
    /// </summary>
    private void OnValidate()
    {
        washTemperature = Mathf.Clamp(washTemperature, 20, 90);
        remainingMinutes = Mathf.Max(0, remainingMinutes);
    }
}

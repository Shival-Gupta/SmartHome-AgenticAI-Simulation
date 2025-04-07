using UnityEngine;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// Controller for smart induction cooktop devices in the IoT system.
/// Manages heat level with visual feedback.
/// </summary>
public class InductionController : SmartDevice
{
    [Header("Device Settings")]
    /// <summary>
    /// Current heat level of the induction cooktop (0-10)
    /// </summary>
    public int heatLevel = 0;
    
    [SerializeField, Tooltip("Room where this induction cooktop is located")]
    private string roomLocation = "Kitchen";

    [Header("Visual Elements")]
    [SerializeField, Tooltip("Array of GameObjects representing heat levels")]
    private GameObject[] heatIndicators;
    
    [SerializeField, Tooltip("Optional glow material for higher heat levels")]
    private Material glowMaterial;
    
    [SerializeField, Tooltip("Color for low heat")]
    private Color lowHeatColor = new Color(1f, 0.5f, 0f, 1f); // Orange
    
    [SerializeField, Tooltip("Color for high heat")]
    private Color highHeatColor = new Color(1f, 0f, 0f, 1f); // Red

    /// <summary>
    /// Maximum heat level
    /// </summary>
    public const int MAX_HEAT_LEVEL = 3;

    /// <summary>
    /// Initialize the induction controller
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        SetRoomNumber(roomLocation);
    }

    private void Start()
    {
        UpdateInductionState();
    }

    /// <summary>
    /// Update visuals when heat level changes
    /// </summary>
    private void Update()
    {
        // This ensures induction visuals are updated whenever the heatLevel is changed directly
        UpdateInductionState();
    }

    /// <summary>
    /// Set the heat level for the induction cooktop
    /// </summary>
    /// <param name="level">Heat level (0-3)</param>
    /// <returns>The actual heat level set after validation</returns>
    public int SetHeatLevel(int level)
    {
        heatLevel = Mathf.Clamp(level, 0, MAX_HEAT_LEVEL);
        UpdateInductionState();
        Debug.Log($"[{DeviceID}] Induction heat level set to {heatLevel}");
        return heatLevel;
    }

    /// <summary>
    /// Increase the heat level by one step
    /// </summary>
    /// <returns>The new heat level</returns>
    public int IncreaseHeat()
    {
        return SetHeatLevel(heatLevel + 1);
    }

    /// <summary>
    /// Decrease the heat level by one step
    /// </summary>
    /// <returns>The new heat level</returns>
    public int DecreaseHeat()
    {
        return SetHeatLevel(heatLevel - 1);
    }

    /// <summary>
    /// Turn off the induction cooktop
    /// </summary>
    public void TurnOff()
    {
        heatLevel = 0;
        UpdateInductionState();
        Debug.Log($"[{DeviceID}] Induction cooktop turned off");
    }

    /// <summary>
    /// Update the visual representation of the induction cooktop based on heat level
    /// </summary>
    private void UpdateInductionState()
    {
        // Update heat indicators
        if (heatIndicators != null && heatIndicators.Length > 0)
        {
            for (int i = 0; i < heatIndicators.Length; i++)
            {
                if (heatIndicators[i] != null)
                {
                    bool isActive = i < heatLevel;
                    heatIndicators[i].SetActive(isActive);
                    
                    // Update material color based on heat level
                    if (isActive && glowMaterial != null)
                    {
                        // Calculate color between low and high heat based on position in the sequence
                        float t = (float)i / (heatIndicators.Length - 1);
                        Color heatColor = Color.Lerp(lowHeatColor, highHeatColor, t);
                        
                        // Apply to material
                        Renderer renderer = heatIndicators[i].GetComponent<Renderer>();
                        if (renderer != null)
                        {
                            renderer.material.color = heatColor;
                        }
                    }
                }
            }
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
        
        // Add induction-specific status
        status["heatLevel"] = heatLevel;
        status["power"] = heatLevel > 0; // Derived power state
        
        return status;
    }

    /// <summary>
    /// Legacy method for backward compatibility.
    /// Gets the induction status as an array of strings.
    /// </summary>
    /// <returns>String array with status information</returns>
    public override string[] GetStatusArray()
    {
        string[] baseStatus = base.GetStatusArray();
        string[] inductionStatus = new string[]
        {
            $"Heat Level: {heatLevel}",
            $"Power: {(heatLevel > 0 ? "ON" : "OFF")}"
        };
        return baseStatus.Concat(inductionStatus).ToArray();
    }
    
    /// <summary>
    /// Validates values when changed in the inspector
    /// </summary>
    private void OnValidate()
    {
        heatLevel = Mathf.Clamp(heatLevel, 0, MAX_HEAT_LEVEL);
        
        // Update visuals immediately in editor
        if (!Application.isPlaying && heatIndicators != null)
        {
            UpdateInductionState();
        }
    }
}

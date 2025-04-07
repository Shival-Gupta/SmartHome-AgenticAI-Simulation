using UnityEngine;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// Controller for smart light devices in the IoT system.
/// Manages light power state, intensity, and color with Unity Light component feedback.
/// </summary>
public class LightController : SmartDevice
{
    [Header("Device Settings")]
    [SerializeField, Tooltip("Room where this light is located")]
    private string roomLocation = "Living Room";

    [SerializeField, Tooltip("Whether the light is currently on")]
    private bool isOn = true;

    [SerializeField, Tooltip("Light intensity (0-2)"), Range(0f, 2f)]
    private float intensity = 1.0f;

    [SerializeField, Tooltip("Light color")]
    private Color lightColor = Color.white;

    [Header("Optional Settings")]
    [SerializeField, Tooltip("Emission material when light is on")]
    private Material emissionMaterial;
    
    [SerializeField, Tooltip("Emission color property name")]
    private string emissionColorProperty = "_EmissionColor";

    [SerializeField, Tooltip("Emission intensity multiplier")]
    private float emissionIntensity = 2f;

    /// <summary>
    /// Reference to the Unity Light component
    /// </summary>
    private Light lightComponent;

    /// <summary>
    /// Minimum allowed light intensity
    /// </summary>
    public const float MIN_INTENSITY = 0f;

    /// <summary>
    /// Maximum allowed light intensity
    /// </summary>
    public const float MAX_INTENSITY = 2f;

    /// <summary>
    /// Gets or sets whether the light is powered on
    /// </summary>
    public bool IsPoweredOn
    {
        get => isOn;
        private set => isOn = value;
    }

    /// <summary>
    /// Gets or sets the current light intensity
    /// </summary>
    public float Intensity
    {
        get => intensity;
        private set => intensity = Mathf.Clamp(value, MIN_INTENSITY, MAX_INTENSITY);
    }

    /// <summary>
    /// Gets or sets the light color
    /// </summary>
    public Color LightColor
    {
        get => lightColor;
        private set => lightColor = value;
    }

    /// <summary>
    /// Initialize the light controller
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        SetRoomNumber(roomLocation);
        
        // Get the light component
        lightComponent = GetComponent<Light>();
        if (lightComponent == null)
        {
            Debug.LogError($"[{DeviceID}] Light component missing on {gameObject.name}. Please add a Light component.", this);
            // Add a light component to avoid null references
            lightComponent = gameObject.AddComponent<Light>();
            lightComponent.type = LightType.Point;
        }

        ApplyLightSettings();
    }

    /// <summary>
    /// Validates values when changed in the inspector
    /// </summary>
    private void OnValidate()
    {
        if (lightComponent == null)
            lightComponent = GetComponent<Light>();

        intensity = Mathf.Clamp(intensity, MIN_INTENSITY, MAX_INTENSITY);
        ApplyLightSettings();
    }

    /// <summary>
    /// Toggle the light's power state
    /// </summary>
    /// <param name="state">True to turn on, false to turn off</param>
    public void ToggleLight(bool state)
    {
        isOn = state;
        if (lightComponent != null)
        {
            lightComponent.enabled = isOn;
            UpdateEmission();
            Debug.Log($"[{DeviceID}] Light {gameObject.name} toggled {(isOn ? "ON" : "OFF")}");
        }
    }

    /// <summary>
    /// Set the light's intensity
    /// </summary>
    /// <param name="newIntensity">The target intensity (will be clamped between MIN_INTENSITY and MAX_INTENSITY)</param>
    /// <returns>The actual intensity set after clamping</returns>
    public float SetLightIntensity(float newIntensity)
    {
        intensity = Mathf.Clamp(newIntensity, MIN_INTENSITY, MAX_INTENSITY);
        if (lightComponent != null)
        {
            lightComponent.intensity = intensity;
            UpdateEmission();
            Debug.Log($"[{DeviceID}] Light {gameObject.name} intensity set to {intensity}");
        }
        return intensity;
    }

    /// <summary>
    /// Set the light's color
    /// </summary>
    /// <param name="color">The color to set</param>
    public void SetLightColor(Color color)
    {
        lightColor = color;
        if (lightComponent != null)
        {
            lightComponent.color = color;
            UpdateEmission();
            Debug.Log($"[{DeviceID}] Light {gameObject.name} color set to {ColorUtility.ToHtmlStringRGB(color)}");
        }
    }

    /// <summary>
    /// Apply all light settings to the Light component
    /// </summary>
    private void ApplyLightSettings()
    {
        if (lightComponent == null) return;
        
        lightComponent.enabled = isOn;
        lightComponent.intensity = intensity;
        lightComponent.color = lightColor;
        UpdateEmission();
    }
    
    /// <summary>
    /// Update the emission material if available
    /// </summary>
    private void UpdateEmission()
    {
        if (emissionMaterial == null) return;
        
        if (isOn)
        {
            // Calculate emission color based on light color and intensity
            Color emissionColor = lightColor * intensity * emissionIntensity;
            emissionMaterial.SetColor(emissionColorProperty, emissionColor);
            emissionMaterial.EnableKeyword("_EMISSION");
        }
        else
        {
            // Turn off emission when light is off
            emissionMaterial.SetColor(emissionColorProperty, Color.black);
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
        
        // Add light-specific status
        status["power"] = isOn;
        status["intensity"] = intensity;
        status["color"] = ColorUtility.ToHtmlStringRGB(lightColor);
        
        return status;
    }

    /// <summary>
    /// Legacy method for backward compatibility.
    /// Gets the light status as an array of strings.
    /// </summary>
    /// <returns>String array with status information</returns>
    public override string[] GetStatusArray()
    {
        string[] baseStatus = base.GetStatusArray();
        string[] lightStatus = new string[]
        {
            $"Power: {(isOn ? "ON" : "OFF")}",
            $"Intensity: {intensity}",
            $"Color: {ColorUtility.ToHtmlStringRGB(lightColor)}"
        };
        return baseStatus.Concat(lightStatus).ToArray();
    }
}

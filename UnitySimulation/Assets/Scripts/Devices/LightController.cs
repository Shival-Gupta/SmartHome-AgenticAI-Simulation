using UnityEngine;
using System.Linq; // For Concat

public class LightController : SmartDevice
{
    [SerializeField] private string roomNumberPublic = "Living Room";

    private Light lightComponent;

    [Tooltip("Turn the light on or off")]
    public bool isOn = true;

    [Range(0f, 2f)]
    [Tooltip("Adjust the light's intensity")]
    public float intensity = 1.0f;

    [Tooltip("Select the light's color")]
    public Color lightColor = Color.white; // Changed from string to Color

    protected override void Awake()
    {
        base.Awake();
        SetRoomNumber(roomNumberPublic); // Assign SmartDevice's room number from public variable
        lightComponent = GetComponent<Light>();

        if (lightComponent == null)
        {
            Debug.LogError($"Light component missing on {gameObject.name}. Please add a Light component.", this);
            return;
        }

        ApplyLightSettings();
    }

    private void OnValidate()
    {
        if (lightComponent == null)
            lightComponent = GetComponent<Light>();

        ApplyLightSettings();
    }

    public void ToggleLight(bool state)
    {
        isOn = state;
        if (lightComponent != null)
        {
            lightComponent.enabled = isOn;
            Debug.Log($"Light {gameObject.name} toggled {(isOn ? "ON" : "OFF")}");
        }
    }

    public void SetLightIntensity(float newIntensity)
    {
        intensity = Mathf.Clamp(newIntensity, 0f, 2f);
        if (lightComponent != null)
        {
            lightComponent.intensity = intensity;
        }
    }

    public void SetLightColor(Color color)
    {
        lightColor = color;
        if (lightComponent != null)
        {
            lightComponent.color = color;
            Debug.Log($"Light {gameObject.name} color set to {ColorUtility.ToHtmlStringRGB(color)}");
        }
    }

    private void ApplyLightSettings()
    {
        ToggleLight(isOn);
        SetLightIntensity(intensity);
        SetLightColor(lightColor);
    }

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

/*using UnityEngine;

public class LightController : MonoBehaviour
{
    private Light lightSource;

    private void Awake()
    {
        lightSource = GetComponent<Light>();
    }

    public void SetLightIntensity(float intensity)
    {
        if (lightSource != null)
            lightSource.intensity = intensity;
    }

    public void SetLightColor(Color color)
    {
        if (lightSource != null)
            lightSource.color = color;
    }

    public void ToggleLight(bool state)
    {
        if (lightSource != null)
            lightSource.enabled = state;
    }
}
*/

using UnityEngine;

public class LightController : MonoBehaviour
{
    private Light lightComponent;
    public bool isOn = true;
    public float intensity = 1.0f;
    public string hexColor = "#FFFFFF";

    private void Awake()
    {
        lightComponent = GetComponent<Light>();

        if (lightComponent == null)
        {
            Debug.LogError($"Light component missing on {gameObject.name}. Please add a Light component.", this);
            return;
        }

        
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

    public void SetHue(string hex)
    {
        if (!hex.StartsWith("#"))
        {
            hex = "#" + hex;  
        }

        if (ColorUtility.TryParseHtmlString(hex, out Color color))
        {
            hexColor = hex;
            if (lightComponent != null)
            {
                lightComponent.color = color;
                Debug.Log($"Light {gameObject.name} color set to {hex}");
            }
        }
        else
        {
            Debug.LogError($"Invalid hex color: {hex}. Use format #RRGGBB.");
        }
    }

    private void ApplyLightSettings()
    {
        ToggleLight(isOn);
        SetLightIntensity(intensity);
        SetHue(hexColor);
    }
}

/*using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class ACController : MonoBehaviour
{
    [SerializeField] private TMP_Text acTempText; 
    private int currentTemperature = 24; // Default AC temperature

    public void SetTemperature(int temperature)
    {
        currentTemperature = temperature;
        if (acTempText != null)
            acTempText.text = "AC Temp: " + currentTemperature + "°C";
    }
}
*/

using UnityEngine;
using TMPro;

public class ACController : MonoBehaviour
{
    public bool isOn = false;
    public int fanSpeed = 1;
    public int temperature = 24;
    public bool ecoMode = false;

    [SerializeField] private TMP_Text ACStatusText;
    [SerializeField] private TMP_Text ACTemperatureText;
    [SerializeField] private TMP_Text ACFanSpeedText;
    [SerializeField] private TMP_Text ACEcoModeText;

    private void Start()
    {
        UpdateACUI();
    }

    public void ToggleAC(bool state)
    {
        isOn = state;
        Debug.Log($"AC is now {(isOn ? "ON" : "OFF")}");
        UpdateACUI();
    }

    public void SetFanSpeed(int speed)
    {
        if (speed < 0 || speed > 3)
        {
            Debug.LogError("Invalid fan speed. Choose 0-3.");
            return;
        }

        fanSpeed = speed;
        Debug.Log($"AC Fan Speed set to {fanSpeed}");
        UpdateACUI();
    }

    public void SetTemperature(int temp)
    {
        temperature = temp;
        Debug.Log($"AC Temperature set to {temperature}°C");
        UpdateACUI();
    }

    public void ToggleEcoMode(bool state)
    {
        ecoMode = state;
        Debug.Log($"Eco Mode is now {(ecoMode ? "ON" : "OFF")}");
        UpdateACUI();
    }

    private void UpdateACUI()
    {
        if (ACStatusText != null)
            ACStatusText.text = $"AC: {(isOn ? "ON" : "OFF")}";

        if (ACTemperatureText != null)
            ACTemperatureText.text = $"Temperature: {temperature}°C";

        if (ACFanSpeedText != null)
            ACFanSpeedText.text = $"Fan Speed: {fanSpeed}";

        if (ACEcoModeText != null)
            ACEcoModeText.text = $"Eco Mode: {(ecoMode ? "ON" : "OFF")}";
    }
}



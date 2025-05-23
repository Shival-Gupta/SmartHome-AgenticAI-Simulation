using UnityEngine;
using TMPro;
using System.Linq; // For Concat
using System.Collections.Generic;

public class ACController : SmartDevice
{
    public bool isOn = false;
    public int fanSpeed = 1;
    public int temperature = 24;
    public bool ecoMode = false;

    [SerializeField] private TMP_Text ACStatusText;
    [SerializeField] private TMP_Text ACTemperatureText;
    [SerializeField] private TMP_Text ACFanSpeedText;
    [SerializeField] private TMP_Text ACEcoModeText;
    [SerializeField] private string roomNumberPublic = "Bedroom";

    protected override void Awake()
    {
        base.Awake();
        SetRoomNumber(roomNumberPublic); // Assign SmartDevice's room number from public variable
    }

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
}

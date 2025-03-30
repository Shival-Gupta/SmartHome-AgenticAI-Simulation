using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class LightSettings
{
    public bool isOn = true;
    [Range(0, 2)] public float intensity = 1.0f;
    public string hexColor = "FFFFFF";
}

public class DeviceController : MonoBehaviour
{
    #region Device Controllers
    [Header("Device Controllers")]
    [SerializeField] private LightController[] lightControllers;
    [SerializeField] private TVController tvController;
    [SerializeField] private InductionController inductionController;
    [SerializeField] private FridgeController fridgeController;
    [SerializeField] private ACController acController;
    [SerializeField] private WashingMachineController washingMachineController;
    [SerializeField] private FanController fanController;
    #endregion

    #region Light Settings
    [Header("Light Settings")]
    public List<LightSettings> lightSettings = new List<LightSettings>();
    #endregion

    #region TV Settings
    [Header("TV Settings")]
    public bool tvOn;
    [Range(0, 100)] public int tvVolume = 10;
    public int tvChannel = 1;
    public string tvSource = "HDMI1";
    #endregion

    #region Induction Settings
    [Header("Induction Settings")]
    [Range(0, 3)] public int inductionHeat;
    #endregion

    #region Fridge Settings
    [Header("Fridge Settings")]
    public bool fridgeOn;
    [Range(-10, 10)] public int fridgeTemperature = 4;
    [Range(-30, -10)] public int freezeTemperature = -18;

    [Header("Fridge Door Status")]
    [Tooltip("Set manually for testing")]
    public bool fridgeDoorOpen;
    public bool freezeDoorOpen;
    #endregion

    #region AC Settings
    [Header("AC Settings")]
    [Range(16, 30)] public int acTemperature = 24;
    [Range(0, 3)] public int acFanSpeed = 1;
    public bool acOn;
    public bool acEcoMode;
    #endregion

    #region Washing Machine Settings
    [Header("Washing Machine Settings")]
    public bool washingMachineOn;
    #endregion

    #region Fan Settings
    [Header("Fan Settings")]
    public bool fanOn;
    public int fanRPM = 400;
    #endregion

    // Change flags to optimize updates
    private bool lightsChanged;
    private bool tvChanged;
    private bool inductionChanged;
    private bool fridgeChanged;
    private bool acChanged;
    private bool washingMachineChanged;
    private bool fanChanged;

    private void Update()
    {
        if (lightsChanged)
        {
            UpdateLightControllers();
            lightsChanged = false;
        }
        if (tvChanged)
        {
            UpdateTVController();
            tvChanged = false;
        }
        if (inductionChanged)
        {
            UpdateInductionController();
            inductionChanged = false;
        }
        if (fridgeChanged)
        {
            UpdateFridgeController();
            fridgeChanged = false;
        }
        if (acChanged)
        {
            UpdateACController();
            acChanged = false;
        }
        if (washingMachineChanged)
        {
            UpdateWashingMachineController();
            washingMachineChanged = false;
        }
        if (fanChanged)
        {
            UpdateFanController();
            fanChanged = false;
        }
    }

    private void OnValidate()
    {
        // Set flags when values change in the inspector
        lightsChanged = true;
        tvChanged = true;
        inductionChanged = true;
        fridgeChanged = true;
        acChanged = true;
        washingMachineChanged = true;
        fanChanged = true;
    }

    #region Public Methods for Command Handlers

    public LightSettings GetLightSettings(int index)
    {
        if (index < 0 || index >= lightSettings.Count)
            throw new IndexOutOfRangeException($"Light index {index} out of range.");
        return lightSettings[index];
    }

    public void ToggleLight(int index, bool state)
    {
        if (index < 0 || index >= lightSettings.Count) return;
        lightSettings[index].isOn = state;
        lightsChanged = true;
    }

    public void SetLightIntensity(int index, float intensity)
    {
        if (index < 0 || index >= lightSettings.Count) return;
        lightSettings[index].intensity = Mathf.Clamp(intensity, 0, 2);
        lightsChanged = true;
    }

    public void SetLightColor(int index, string hexColor)
    {
        if (index < 0 || index >= lightSettings.Count) return;
        lightSettings[index].hexColor = hexColor;
        lightsChanged = true;
    }

    public void ToggleTV(bool state)
    {
        tvOn = state;
        tvChanged = true;
    }

    public void SetTVVolume(int volume)
    {
        tvVolume = Mathf.Clamp(volume, 0, 100);
        tvChanged = true;
    }

    public void SetTVChannel(int channel)
    {
        tvChannel = channel;
        tvChanged = true;
    }

    public void SetTVSource(string source)
    {
        tvSource = source;
        tvChanged = true;
    }

    public void SetInductionHeat(int level)
    {
        inductionHeat = Mathf.Clamp(level, 0, 3);
        inductionChanged = true;
    }

    public void ToggleFridge(bool state)
    {
        fridgeOn = state;
        fridgeChanged = true;
    }

    public void SetFridgeTemperature(int temp)
    {
        fridgeTemperature = Mathf.Clamp(temp, -10, 10);
        fridgeChanged = true;
    }

    public void SetFreezeTemperature(int temp)
    {
        freezeTemperature = Mathf.Clamp(temp, -30, -10);
        fridgeChanged = true;
    }

    public void SetFridgeDoorOpen(bool state)
    {
        fridgeDoorOpen = state;
        fridgeChanged = true;
    }

    public void SetFreezeDoorOpen(bool state)
    {
        freezeDoorOpen = state;
        fridgeChanged = true;
    }

    public void ToggleAC(bool state)
    {
        acOn = state;
        acChanged = true;
    }

    public void SetACTemperature(int temp)
    {
        acTemperature = Mathf.Clamp(temp, 16, 30);
        acChanged = true;
    }

    public void SetACFanSpeed(int speed)
    {
        acFanSpeed = Mathf.Clamp(speed, 0, 3);
        acChanged = true;
    }

    public void ToggleACEcoMode(bool state)
    {
        acEcoMode = state;
        acChanged = true;
    }

    public void ToggleWashingMachine(bool state)
    {
        washingMachineOn = state;
        washingMachineChanged = true;
    }

    public void ToggleFan(bool state)
    {
        fanOn = state;
        fanChanged = true;
    }

    public void SetFanRPM(int rpm)
    {
        fanRPM = rpm;
        fanChanged = true;
    }

    #endregion

    #region Controller Update Methods

    private void UpdateLightControllers()
    {
        if (lightControllers == null || lightSettings.Count != lightControllers.Length) return;

        for (int i = 0; i < lightControllers.Length; i++)
        {
            if (lightControllers[i] == null) continue;

            var controller = lightControllers[i];
            var settings = lightSettings[i];

            controller.ToggleLight(settings.isOn);
            controller.SetLightIntensity(settings.intensity);

            if (ColorUtility.TryParseHtmlString("#" + settings.hexColor, out Color color))
            {
                controller.SetLightColor(color);
            }
            else
            {
                Debug.LogWarning($"Invalid hex color: {settings.hexColor}");
            }
        }
    }

    private void UpdateTVController()
    {
        if (tvController == null) return;

        tvController.ToggleTV(tvOn);
        tvController.SetVolume(tvVolume);
        tvController.SetChannel(tvChannel);
        tvController.SetSource(tvSource);
    }

    private void UpdateInductionController()
    {
        if (inductionController == null) return;

        inductionController.heatLevel = inductionHeat;
    }

    private void UpdateFridgeController()
    {
        if (fridgeController == null) return;

        fridgeController.ToggleFridge(fridgeOn);
        fridgeController.SetTemperature(fridgeTemperature);
        fridgeController.SetFreezeTemperature(freezeTemperature);
        fridgeController.mainDoorOpen = fridgeDoorOpen;
        fridgeController.freezeDoorOpen = freezeDoorOpen;
    }

    private void UpdateACController()
    {
        if (acController == null) return;

        acController.ToggleAC(acOn);
        acController.SetFanSpeed(acFanSpeed);
        acController.SetTemperature(acTemperature);
        acController.ToggleEcoMode(acEcoMode);
    }

    private void UpdateWashingMachineController()
    {
        if (washingMachineController == null) return;

        washingMachineController.ToggleWashingMachine(washingMachineOn);
    }

    private void UpdateFanController()
    {
        if (fanController == null) return;

        fanController.ToggleFan(fanOn);
        fanController.SetRPM(fanRPM);
    }

    #endregion

    public void LogAllDeviceStatus()
    {
        for (int i = 0; i < lightControllers.Length; i++)
        {
            if (lightControllers[i] != null)
            {
                string[] status = lightControllers[i].GetStatusArray();
                Debug.Log($"Light {i} Status:\n{string.Join("\n", status)}");
            }
        }

        if (tvController != null)
        {
            string[] status = tvController.GetStatusArray();
            Debug.Log($"TV Status:\n{string.Join("\n", status)}");
        }

        if (acController != null)
        {
            string[] status = acController.GetStatusArray();
            Debug.Log($"AC Status:\n{string.Join("\n", status)}");
        }

        if (fridgeController != null)
        {
            string[] status = fridgeController.GetStatusArray();
            Debug.Log($"Fridge Status:\n{string.Join("\n", status)}");
        }

        if (inductionController != null)
        {
            string[] status = inductionController.GetStatusArray();
            Debug.Log($"Induction Status:\n{string.Join("\n", status)}");
        }

        if (washingMachineController != null)
        {
            string[] status = washingMachineController.GetStatusArray();
            Debug.Log($"Washing Machine Status:\n{string.Join("\n", status)}");
        }

        if (fanController != null)
        {
            string[] status = fanController.GetStatusArray();
            Debug.Log($"Fan Status:\n{string.Join("\n", status)}");
        }
    }
}

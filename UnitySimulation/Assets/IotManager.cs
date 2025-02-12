using UnityEngine;
using System.Collections.Generic;
using static UnityEditor.PlayerSettings;

[ExecuteAlways]  // Runs in Edit Mode
public class IotManager : MonoBehaviour
{
    [SerializeField] private LightController[] lightControllers;
    [SerializeField] private TVController tvController;
    [SerializeField] private InductionController inductionController;
    [SerializeField] private FridgeController fridgeController;
    [SerializeField] private ACController acController;
    [SerializeField] private WashingMachineController washingMachineController;

    public bool tvOn = false;
    [Range(0, 3)] public int inductionHeat = 0;

    [Range(-10, 10)] public int fridgeTemperature = 4;  // Adjustable in Inspector

    [Range(16, 30)] public int acTemperature = 24;
    [Range(0, 3)] public int acFanSpeed = 1;
    public bool acOn = false;
    public bool acEcoMode = false;
    
    
    public bool washingMachineOn = false;               // Toggle in Inspector

  


    /*[System.Serializable]
    public class LightSettings
    {
        public float intensity = 1.0f;
        public Color color = Color.white;
        public bool isOn = true;
    }

    public List<LightSettings> lightSettings = new List<LightSettings>();*/

    [System.Serializable]
    public class LightSettings
    {
        public bool isOn = true;
        [Range(0, 2)] public float intensity = 1.0f;
        public string hexColor = "FFFFFF";
    }

    public List<LightSettings> lightSettings = new List<LightSettings>();

    private void Update()
    {
        ApplyInspectorChanges();
    }

    private void ApplyInspectorChanges()
    {
        /*if (lightControllers != null && lightSettings.Count == lightControllers.Length)
        {
            for (int i = 0; i < lightControllers.Length; i++)
            {
                if (lightControllers[i] != null)
                {
                    lightControllers[i].SetLightIntensity(lightSettings[i].intensity);
                    lightControllers[i].SetLightColor(lightSettings[i].color);
                    lightControllers[i].ToggleLight(lightSettings[i].isOn);
                }
            }
        }*/

    

        if (lightControllers != null && lightSettings.Count == lightControllers.Length)
        {
            for (int i = 0; i < lightControllers.Length; i++)
            {
                if (lightControllers[i] != null)
                {
                    lightControllers[i].ToggleLight(lightSettings[i].isOn);
                    lightControllers[i].SetLightIntensity(lightSettings[i].intensity);
                    lightControllers[i].SetHue(lightSettings[i].hexColor);
                }
            }
        }
        if (tvController != null)
            tvController.ToggleTV(tvOn);

        if (inductionController != null)
            inductionController.heatLevel = inductionHeat;

        if (fridgeController != null)
            fridgeController.SetTemperature(fridgeTemperature);

        /*if (acController != null)
           acController.SetTemperature(acTemperature);*/
        if (acController != null)
        {
            acController.ToggleAC(acOn);
            acController.SetFanSpeed(acFanSpeed);
            acController.SetTemperature(acTemperature);
            acController.ToggleEcoMode(acEcoMode);
        }

        if (washingMachineController != null)
            washingMachineController.ToggleWashingMachine(washingMachineOn);
    }
}

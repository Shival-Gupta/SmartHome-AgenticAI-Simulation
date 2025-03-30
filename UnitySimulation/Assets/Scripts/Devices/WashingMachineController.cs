using UnityEngine;
using System.Linq; // For Concat

public class WashingMachineController : SmartDevice
{
    [SerializeField] private GameObject onCylinder;
    [SerializeField] private GameObject offCylinder;
    [SerializeField] private string roomNumberPublic = "Bathroom";

    private bool isOn = false;

    protected override void Awake()
    {
        base.Awake();
        SetRoomNumber(roomNumberPublic);
    }

    public void ToggleWashingMachine(bool state)
    {
        isOn = state;
        onCylinder.SetActive(isOn);
        offCylinder.SetActive(!isOn);
    }

    public override string[] GetStatusArray()
    {
        string[] baseStatus = base.GetStatusArray();
        string[] washingMachineStatus = new string[]
        {
            $"Power: {(isOn ? "ON" : "OFF")}" // Changed label to "Power" for consistency
        };
        return baseStatus.Concat(washingMachineStatus).ToArray();
    }
}

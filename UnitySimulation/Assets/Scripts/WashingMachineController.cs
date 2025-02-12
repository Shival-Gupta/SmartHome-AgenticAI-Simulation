using UnityEngine;

public class WashingMachineController : MonoBehaviour
{
    [SerializeField] private GameObject onCylinder;  
    [SerializeField] private GameObject offCylinder;

    private bool isOn = false;

    public void ToggleWashingMachine(bool state)
    {
        isOn = state;
        onCylinder.SetActive(isOn);
        offCylinder.SetActive(!isOn);
    }
}

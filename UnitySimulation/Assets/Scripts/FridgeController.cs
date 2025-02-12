using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class FridgeController : MonoBehaviour
{
    [SerializeField] private TMP_Text fridgeTempText;

    private int currentTemperature = 4; // Default fridge temperature

    public void SetTemperature(int temperature)
    {
        currentTemperature = temperature;
        if (fridgeTempText != null)
            fridgeTempText.text = "Fridge Temp: " + currentTemperature + "°C";
    }
}

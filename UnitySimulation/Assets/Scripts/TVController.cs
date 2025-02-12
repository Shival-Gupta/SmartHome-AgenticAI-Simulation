using UnityEngine;
using UnityEngine.UI;

public class TVController : MonoBehaviour
{
    [SerializeField] private Image tvScreen; 
    [SerializeField] private Sprite tvOnSprite; // Image when TV is ON
    [SerializeField] private Sprite tvOffSprite; // Image when TV is OFF

    private bool isOn = false;  // Default TV state is OFF

    public void ToggleTV(bool state)
    {
        isOn = state;

        if (tvScreen != null)
            tvScreen.sprite = isOn ? tvOnSprite : tvOffSprite;
    }
}

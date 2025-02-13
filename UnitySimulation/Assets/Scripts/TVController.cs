/*using UnityEngine;
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
*/

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TVController : MonoBehaviour
{
    public bool isOn = false;
    public int volume = 10;
    public int channel = 1;
    public string source = "HDMI1";

    [SerializeField] private TMP_Text TVStatusText;
    [SerializeField] private TMP_Text TVVolumeText;
    [SerializeField] private TMP_Text TVChannelText;
    [SerializeField] private TMP_Text TVSourceText;
    [SerializeField] private Image tvImage;   // Image that will change
    [SerializeField] private Sprite tvOnSprite;  // TV ON image
    [SerializeField] private Sprite tvOffSprite; // TV OFF image

    private void Start()
    {
        UpdateTVUI();
    }

    public void ToggleTV(bool state)
    {
        isOn = state;
        Debug.Log($"TV is now {(isOn ? "ON" : "OFF")}");
        UpdateTVUI();
    }

    public void SetVolume(int vol)
    {
        volume = Mathf.Clamp(vol, 0, 100);
        Debug.Log($"TV volume set to {volume}");
        UpdateTVUI();
    }

    public void SetChannel(int ch)
    {
        channel = ch;
        Debug.Log($"TV channel set to {channel}");
        UpdateTVUI();
    }

    public void SetSource(string newSource)
    {
        if (newSource == "HDMI1" || newSource == "HDMI2")
        {
            source = newSource;
            Debug.Log($"TV source set to {source}");
            UpdateTVUI();
        }
        else
        {
            Debug.LogError("Invalid TV source! Choose HDMI1 or HDMI2.");
        }
    }

    private void UpdateTVUI()
    {
        if (TVStatusText != null)
            TVStatusText.text = $"TV: {(isOn ? "ON" : "OFF")}";

        if (TVVolumeText != null)
            TVVolumeText.text = $"Volume: {volume}";

        if (TVChannelText != null)
            TVChannelText.text = $"Channel: {channel}";

        if (TVSourceText != null)
            TVSourceText.text = $"Source: {source}";

        if (tvImage != null)
            tvImage.sprite = isOn ? tvOnSprite : tvOffSprite;
    }
}

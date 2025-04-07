using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// Controller for smart TV devices in the IoT system.
/// Manages power, volume, channel, source, and display with UI feedback.
/// </summary>
public class TVController : SmartDevice
{
    [Header("Device Settings")]
    [SerializeField, Tooltip("Whether the TV is currently on")]
    private bool isOn = false;
    
    [SerializeField, Tooltip("Current volume level"), Range(0, 100)]
    private int volume = 10;
    
    [SerializeField, Tooltip("Current channel number"), Min(0)]
    private int channel = 1;
    
    [SerializeField, Tooltip("Current input source")]
    private string source = "HDMI1";
    
    [SerializeField, Tooltip("Room where this TV is located")]
    private string roomLocation = "Living Room";

    [Header("UI References")]
    [SerializeField, Tooltip("Text component for displaying TV status")]
    private TMP_Text tvStatusText;
    
    [SerializeField, Tooltip("Text component for displaying volume")]
    private TMP_Text tvVolumeText;
    
    [SerializeField, Tooltip("Text component for displaying channel")]
    private TMP_Text tvChannelText;
    
    [SerializeField, Tooltip("Text component for displaying source")]
    private TMP_Text tvSourceText;
    
    [SerializeField, Tooltip("Image component for off state")]
    private Image tvImage;
    
    [SerializeField, Tooltip("Raw image component for video display")]
    private RawImage tvVideo;
    
    [SerializeField, Tooltip("Sprite to show when TV is off")]
    private Sprite tvOffSprite;

    [Header("Video Player")]
    [SerializeField, Tooltip("Video player component")]
    private VideoPlayer tvVideoPlayer;

    /// <summary>
    /// Maximum volume level
    /// </summary>
    public const int MAX_VOLUME = 100;
    
    /// <summary>
    /// Valid input sources
    /// </summary>
    public static readonly string[] VALID_SOURCES = { "HDMI1", "HDMI2", "TV", "AV" };

    /// <summary>
    /// Gets or sets whether the TV is powered on
    /// </summary>
    public bool IsPoweredOn
    {
        get => isOn;
        private set => isOn = value;
    }

    /// <summary>
    /// Gets or sets the current volume level
    /// </summary>
    public int Volume
    {
        get => volume;
        private set => volume = Mathf.Clamp(value, 0, MAX_VOLUME);
    }

    /// <summary>
    /// Gets or sets the current channel
    /// </summary>
    public int Channel
    {
        get => channel;
        private set => channel = Mathf.Max(1, value);
    }

    /// <summary>
    /// Gets or sets the current input source
    /// </summary>
    public string Source
    {
        get => source;
        private set => source = ValidateSource(value) ? value : source;
    }

    /// <summary>
    /// Initialize the TV controller
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        SetRoomNumber(roomLocation);
    }

    private void Start()
    {
        InitializeVideoPlayer();
        UpdateTVUI();
    }

    /// <summary>
    /// Initialize the video player component
    /// </summary>
    private void InitializeVideoPlayer()
    {
        if (tvVideoPlayer != null)
        {
            tvVideoPlayer.playOnAwake = false;
            tvVideoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
            tvVideoPlayer.SetDirectAudioVolume(0, volume / (float)MAX_VOLUME);
            tvVideoPlayer.isLooping = true;
        }
    }

    /// <summary>
    /// Toggle the TV power state
    /// </summary>
    /// <param name="state">True to turn on, false to turn off</param>
    public void ToggleTV(bool state)
    {
        isOn = state;
        UpdateTVUI();
        Debug.Log($"[{DeviceID}] TV is now {(isOn ? "ON" : "OFF")}");
    }

    /// <summary>
    /// Set the TV volume
    /// </summary>
    /// <param name="vol">Volume level (0-100)</param>
    /// <returns>The actual volume set after validation</returns>
    public int SetVolume(int vol)
    {
        volume = Mathf.Clamp(vol, 0, MAX_VOLUME);
        if (tvVideoPlayer != null)
        {
            tvVideoPlayer.SetDirectAudioVolume(0, volume / (float)MAX_VOLUME);
        }
        UpdateTVUI();
        Debug.Log($"[{DeviceID}] TV volume set to {volume}");
        return volume;
    }

    /// <summary>
    /// Set the TV channel
    /// </summary>
    /// <param name="ch">Channel number (minimum 1)</param>
    /// <returns>The actual channel set after validation</returns>
    public int SetChannel(int ch)
    {
        channel = Mathf.Max(1, ch);
        UpdateTVUI();
        Debug.Log($"[{DeviceID}] TV channel set to {channel}");
        return channel;
    }

    /// <summary>
    /// Set the TV input source
    /// </summary>
    /// <param name="newSource">Source name (e.g., "HDMI1", "HDMI2", "TV", "AV")</param>
    /// <returns>True if the source was valid and set successfully, otherwise false</returns>
    public bool SetSource(string newSource)
    {
        if (ValidateSource(newSource))
        {
            source = newSource;
            UpdateTVUI();
            Debug.Log($"[{DeviceID}] TV source set to {source}");
            return true;
        }
        
        Debug.LogWarning($"[{DeviceID}] Invalid TV source: {newSource}. Valid sources: {string.Join(", ", VALID_SOURCES)}");
        return false;
    }

    /// <summary>
    /// Validate if a source name is valid
    /// </summary>
    /// <param name="sourceName">Source name to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    private bool ValidateSource(string sourceName)
    {
        return System.Array.Exists(VALID_SOURCES, s => s == sourceName);
    }

    /// <summary>
    /// Update the UI elements with current TV status
    /// </summary>
    private void UpdateTVUI()
    {
        // Update text displays
        if (tvStatusText != null)
            tvStatusText.text = $"TV: {(isOn ? "ON" : "OFF")}";
        if (tvVolumeText != null)
            tvVolumeText.text = $"Volume: {volume}";
        if (tvChannelText != null)
            tvChannelText.text = $"Channel: {channel}";
        if (tvSourceText != null)
            tvSourceText.text = $"Source: {source}";

        // Update visual state
        UpdateVisualState();

        // Control video playback
        UpdateVideoPlayback();
    }

    /// <summary>
    /// Update the visual state of the TV (on/off display)
    /// </summary>
    private void UpdateVisualState()
    {
        if (tvImage != null && tvVideo != null)
        {
            tvImage.gameObject.SetActive(!isOn);
            tvVideo.gameObject.SetActive(isOn);
            
            if (!isOn && tvOffSprite != null)
            {
                tvImage.sprite = tvOffSprite;
            }
        }
    }

    /// <summary>
    /// Update the video playback based on TV state
    /// </summary>
    private void UpdateVideoPlayback()
    {
        if (tvVideoPlayer == null) return;
        
        if (isOn && !tvVideoPlayer.isPlaying)
        {
            tvVideoPlayer.Play();
        }
        else if (!isOn && tvVideoPlayer.isPlaying)
        {
            tvVideoPlayer.Pause();
        }
    }

    /// <summary>
    /// Gets the current device status
    /// </summary>
    /// <returns>A dictionary with the current device status</returns>
    public override Dictionary<string, object> GetStatus()
    {
        // Get the base status from the parent class
        Dictionary<string, object> status = base.GetStatus();
        
        // Add TV-specific status
        status["power"] = isOn;
        status["volume"] = volume;
        status["channel"] = channel;
        status["source"] = source;
        
        return status;
    }

    /// <summary>
    /// Legacy method for backward compatibility.
    /// Gets the TV status as an array of strings.
    /// </summary>
    /// <returns>String array with status information</returns>
    public override string[] GetStatusArray()
    {
        string[] baseStatus = base.GetStatusArray();
        string[] tvStatus = new string[]
        {
            $"Power: {(isOn ? "ON" : "OFF")}",
            $"Volume: {volume}",
            $"Channel: {channel}",
            $"Source: {source}"
        };
        return baseStatus.Concat(tvStatus).ToArray();
    }
    
    /// <summary>
    /// Validates values when changed in the inspector
    /// </summary>
    private void OnValidate()
    {
        volume = Mathf.Clamp(volume, 0, MAX_VOLUME);
        channel = Mathf.Max(1, channel);
        
        // Ensure source is valid
        bool sourceValid = false;
        foreach (var validSource in VALID_SOURCES)
        {
            if (source == validSource)
            {
                sourceValid = true;
                break;
            }
        }
        
        if (!sourceValid && VALID_SOURCES.Length > 0)
        {
            source = VALID_SOURCES[0];
            Debug.LogWarning($"[{DeviceID}] Invalid source set in Inspector. Defaulting to {source}");
        }
    }
}

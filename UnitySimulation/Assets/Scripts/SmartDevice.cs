using UnityEngine;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

/// <summary>
/// Base abstract class for all smart devices in the IoT system.
/// Provides common functionality and interface for device identification, status reporting,
/// and room assignment.
/// </summary>
public abstract class SmartDevice : MonoBehaviour
{
    [SerializeField, Tooltip("Unique device identifier (auto-generated if empty)")]
    private string deviceID = string.Empty;
    
    [SerializeField, Tooltip("Room where the device is located")]
    private string roomNumber = "Home";
    
    /// <summary>
    /// Gets the unique device identifier
    /// </summary>
    public string DeviceID => string.IsNullOrEmpty(deviceID) ? GenerateDeviceID() : deviceID;
    
    /// <summary>
    /// Gets the room where the device is located
    /// </summary>
    public string RoomNumber => roomNumber;
    
    /// <summary>
    /// Initializes the device when the component awakens
    /// </summary>
    protected virtual void Awake()
    {
        if (string.IsNullOrEmpty(deviceID))
        {
            deviceID = GenerateDeviceID();
            Debug.Log($"Generated device ID: {deviceID}");
        }
    }

    /// <summary>
    /// Generates a unique device ID based on the class name and a unique identifier
    /// </summary>
    /// <returns>A formatted device ID string</returns>
    private string GenerateDeviceID()
    {
        try 
        {
            string typeName = GetType().Name; // Get class name (e.g., TVController)
            string uniqueID = Guid.NewGuid().ToString("N").Substring(0, 4).ToUpper(); // Generate short unique ID in uppercase
            return $"{typeName}_{uniqueID}"; // Format: TV_3A9F
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error generating device ID: {ex.Message}");
            return $"{GetType().Name}_{DateTime.Now.Ticks.ToString("X").Substring(0, 4)}"; // Fallback ID
        }
    }

    /// <summary>
    /// Sets the room number for the device
    /// </summary>
    /// <param name="room">The room name to set</param>
    /// <returns>True if the room was successfully set, false otherwise</returns>
    protected bool SetRoomNumber(string room)
    {
        if (string.IsNullOrWhiteSpace(room))
        {
            Debug.LogWarning($"[{DeviceID}] Attempted to set empty room name. Using default: {roomNumber}");
            return false;
        }
        
        roomNumber = room;
        return true;
    }

    /// <summary>
    /// Gets the current device status as a dictionary of properties
    /// </summary>
    /// <returns>A dictionary containing device status information</returns>
    public virtual Dictionary<string, object> GetStatus()
    {
        return new Dictionary<string, object>
        {
            { "deviceId", DeviceID },
            { "roomNumber", RoomNumber },
            { "deviceType", GetType().Name.Replace("Controller", "") }
        };
    }

    /// <summary>
    /// Converts the device status to a JSON string
    /// </summary>
    /// <returns>A JSON string representing the device status</returns>
    public string GetStatusJson()
    {
        try
        {
            return JsonConvert.SerializeObject(GetStatus(), Formatting.Indented);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[{DeviceID}] Failed to serialize device status to JSON: {ex.Message}");
            return "{}";
        }
    }

    /// <summary>
    /// Legacy method for backward compatibility. 
    /// Returns device status as an array of strings.
    /// </summary>
    /// <returns>An array of strings representing the device status</returns>
    [Obsolete("Use GetStatus() instead for JSON-compatible object")]
    public virtual string[] GetStatusArray()
    {
        return new string[]
        {
            $"Device ID: {DeviceID}",
            $"Room: {RoomNumber}",
            $"Type: {GetType().Name.Replace("Controller", "")}"
        };
    }
    
    /// <summary>
    /// Called when the device is destroyed
    /// </summary>
    protected virtual void OnDestroy()
    {
        Debug.Log($"[{DeviceID}] Device destroyed");
    }
}

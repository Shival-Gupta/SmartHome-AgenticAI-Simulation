using UnityEngine;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

public abstract class SmartDevice : MonoBehaviour
{
    [SerializeField, ReadOnly] private string deviceID;   // Unique ID
    [SerializeField, ReadOnly] private string roomNumber = "Home"; // Room (assigned from derived class)

    public string DeviceID => deviceID;   // Accessible from outside
    public string RoomNumber => roomNumber; // Accessible from outside

    protected virtual void Awake()
    {
        if (string.IsNullOrEmpty(deviceID))
        {
            GenerateDeviceID();
        }
    }

    private void GenerateDeviceID()
    {
        try 
        {
            string typeName = GetType().Name; // Get class name (e.g., TVController)
            string uniqueID = Guid.NewGuid().ToString("N").Substring(0, 4); // Generate short unique ID
            deviceID = $"{typeName}_{uniqueID}"; // Format: TV_3A9F
            Debug.Log($"Generated device ID: {deviceID}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error generating device ID: {ex.Message}");
            throw;
        }
    }

    // Method to set room number from child classes
    protected void SetRoomNumber(string room)
    {
        if (string.IsNullOrEmpty(room))
        {
            Debug.LogWarning("Attempting to set empty room name. Using default.");
            return;
        }
        roomNumber = room; // Assign value from derived class
    }

    // Base method to get device status as a dictionary
    public virtual Dictionary<string, object> GetStatus()
    {
        return new Dictionary<string, object>
        {
            { "deviceId", DeviceID },
            { "roomNumber", RoomNumber },
            { "deviceType", GetType().Name.Replace("Controller", "") }
        };
    }

    // Utility method to convert status to JSON string
    public string GetStatusJson()
    {
        return JsonConvert.SerializeObject(GetStatus());
    }

    // Backwards compatibility with string array status (will be deprecated)
    [Obsolete("Use GetStatus() instead for JSON-compatible object")]
    public virtual string[] GetStatusArray()
    {
        return new string[]
        {
            $"Device ID: {DeviceID}",
            $"Room: {RoomNumber}"
        };
    }
}

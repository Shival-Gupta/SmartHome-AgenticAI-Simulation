using UnityEngine;
using System;

public class SmartDevice : MonoBehaviour
{
    [SerializeField, ReadOnly] private string deviceID;   // Unique ID
    [SerializeField, ReadOnly] private string roomNumber = "Home"; // Room (assigned from derived class)

    public string DeviceID => deviceID;   // Accessible from outside
    public string RoomNumber => roomNumber; // Accessible from outside

    protected virtual void Awake()
    {
        GenerateDeviceID();
    }

    private void GenerateDeviceID()
    {
        string typeName = GetType().Name; // Get class name (e.g., TVController)
        string uniqueID = Guid.NewGuid().ToString("N").Substring(0, 4); // Generate short unique ID
        deviceID = $"{typeName}_{uniqueID}"; // Format: TV_3A9F
    }

    // Method to set room number from child classes
    protected void SetRoomNumber(string room)
    {
        roomNumber = room; // Assign value from derived class
    }

    // Virtual method to get the base status
    public virtual string[] GetStatusArray()
    {
        return new string[]
        {
            $"Device ID: {DeviceID}",
            $"Room: {RoomNumber}"
        };
    }
}

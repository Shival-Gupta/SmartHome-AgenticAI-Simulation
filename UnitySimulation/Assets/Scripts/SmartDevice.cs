/*using UnityEngine;

public class SmartDevice : MonoBehaviour
{
    [SerializeField, ReadOnly] private string deviceID;   // Unique ID (visible, not editable)
    [SerializeField, ReadOnly] private string roomNumber; // Room name (visible, not editable)

    public string DeviceID => deviceID;      
    public string RoomNumber => roomNumber;  

    protected virtual void Awake()
    {
        deviceID = gameObject.name;  // Assign device ID automatically

        if (transform.parent != null)
            roomNumber = transform.parent.name;  // Assign room based on parent name
        else
            roomNumber = "UnknownRoom";
    }
}
*/
/*

using UnityEngine;
using System;

public class SmartDevice : MonoBehaviour
{
    [SerializeField, ReadOnly] private string deviceID;   // Unique ID
    [SerializeField, ReadOnly] private string roomNumber; // Room

    public string DeviceID => deviceID;
    public string RoomNumber => roomNumber;

    protected virtual void Awake()
    {
        AssignRoomName();
        GenerateDeviceID();
    }

    private void AssignRoomName()
    {
        if (transform.parent != null)
            roomNumber = transform.parent.gameObject.name; // Assign based on parent (room)
        else
            roomNumber = "UnknownRoom";
    }

    private void GenerateDeviceID()
    {
        string typeName = GetType().Name; // Get class name (e.g., LightController)
        string uniqueID = Guid.NewGuid().ToString("N").Substring(0, 4); // Generate a short unique ID
        deviceID = $"{typeName}_{uniqueID}"; // Format: Light_3A9F
    }
}
*/

/*using UnityEngine;
using System;

public class SmartDevice : MonoBehaviour
{
    [SerializeField, ReadOnly] private string deviceID;   // Unique ID
    [SerializeField, ReadOnly] private string roomNumber; // Room (assigned from derived class)

    public string DeviceID => deviceID;
    public string RoomNumber => roomNumber;

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
}
*/

using UnityEngine;
using System;

public class SmartDevice : MonoBehaviour
{
    [SerializeField, ReadOnly] private string deviceID;   // Unique ID
    [SerializeField, ReadOnly] private string roomNumber; // Room (assigned from derived class)

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
}

using UnityEngine;
using System.Linq; // For Concat

public class InductionController : SmartDevice
{
    [SerializeField] private GameObject[] cylinders;
    [Range(0, 3)] public int heatLevel = 0;  // Intensity from 0 to 3
    [SerializeField] private string roomNumberPublic = "Kitchen";

    protected override void Awake()
    {
        base.Awake();
        SetRoomNumber(roomNumberPublic); // Assign SmartDevice's room number from public variable
    }

    private void Update()
    {
        UpdateInductionState();
    }

    private void UpdateInductionState()
    {
        for (int i = 0; i < cylinders.Length; i++)
        {
            if (cylinders[i] != null)
                cylinders[i].SetActive(i < heatLevel);
        }
    }

    public override string[] GetStatusArray()
    {
        string[] baseStatus = base.GetStatusArray();
        string[] inductionStatus = new string[]
        {
            $"Heat Level: {heatLevel}"
        };
        return baseStatus.Concat(inductionStatus).ToArray();
    }
}

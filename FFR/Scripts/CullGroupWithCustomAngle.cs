
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class CullGroupWithCustomAngle : UdonSharpBehaviour
{
    public float RenderDistance = 200000;
    public float cullAngle = 180;
    public GameObject[] CullObjects;
    private int index = 0;
    private int maxLength = 0;
    private VRCPlayerApi localPlayer;

    void Start()
    {
        localPlayer = Networking.LocalPlayer;
        maxLength = CullObjects.Length; 
    }
    void Update()
    {
        if (CullObjects == null)
        {
            return;
        }

        LookLogic();
    }

    public void LookLogic()
    {
        if (index < maxLength)
        {
            var distance = Vector3.Distance(Networking.LocalPlayer != null ? Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position : Vector3.zero, CullObjects[index].transform.position);
            var ObjectToTargetVector = CullObjects[index].transform.position - (Networking.LocalPlayer != null ? Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position : Vector3.zero);
            var AIForward = Networking.LocalPlayer != null ? Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation * Vector3.forward : Vector3.zero;
            var targetDirection = ObjectToTargetVector.normalized;
            var tempAngleCheck = Vector3.Angle(targetDirection, AIForward);

            if (distance > RenderDistance || tempAngleCheck > cullAngle)
            {
                CullObjects[index].SetActive(false);
            }
            else
            {
                CullObjects[index].SetActive(true);
            }

            if (index + 1 < maxLength)
            {
                index = index + 1;
            }
            else
            {
                index = 0;
            }
        }
    }


}

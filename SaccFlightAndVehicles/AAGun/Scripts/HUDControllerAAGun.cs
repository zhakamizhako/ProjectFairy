
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

public class HUDControllerAAGun : UdonSharpBehaviour
{
    public AAGunController AAGunControl;
    public Transform ElevationIndicator;
    public Transform HeadingIndicator;
    Vector3 temprot;
    private void Start()
    {
        Assert(AAGunControl != null, "Start: AAGunControl != null");
        Assert(ElevationIndicator != null, "Start: ElevationIndicator != null");
        Assert(HeadingIndicator != null, "Start: HeadingIndicator != null");
    }
    private void Update()
    {
        //Heading indicator
        temprot = AAGunControl.Rotator.transform.rotation.eulerAngles;
        temprot.x = 0;
        temprot.z = 0;
        HeadingIndicator.localRotation = Quaternion.Euler(-temprot);
        /////////////////

        //Elevation indicator
        temprot = AAGunControl.Rotator.transform.localRotation.eulerAngles;
        temprot.y = 0;
        temprot.z = 0;
        ElevationIndicator.localRotation = Quaternion.Euler(-temprot);
        /////////////////
    }
    private void Assert(bool condition, string message)
    {
        if (!condition)
        {
            Debug.LogError("Assertion failed : '" + GetType() + " : " + message + "'", this);
        }
    }
}
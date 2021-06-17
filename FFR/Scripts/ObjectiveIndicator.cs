
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using UnityEngine.Animations;
using VRC.Udon;

public class ObjectiveIndicator : UdonSharpBehaviour
{
    public bool destroy = false;
    public MissileTrackerAndResponse ParentClass;
    public PlayerUIScript UIScript;
    public Transform TargetClass;
    public Transform Pointer;
    public Text indicator;
    public Text distanceText;
    private Vector3 startScale;
    public bool DisplayText = true;
    public bool DisplayDistance = true;

    void Start()
    {
        startScale = transform.localScale;
    }

    void LateUpdate()
    {
        if (destroy)
        {
            DestroyImmediate(gameObject);
            return;
        }

        if(UIScript.PlayerAircraft ==null){
            destroy = true;
        }

        if (UIScript.PlayerAircraft != null && !destroy)
        {
            if (UIScript.PlayerAircraft.Piloting || UIScript.PlayerAircraft.Passenger)
            {
                var PrePos = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
                var playerRotData = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation;
                gameObject.transform.position = PrePos;
                gameObject.transform.position += gameObject.transform.forward * UIScript.vrDistance;
                Pointer.LookAt(TargetClass.position, UIScript.parentHolderTexts.position);
                Pointer.localRotation = Quaternion.Euler(0, 0, -Pointer.transform.localRotation.eulerAngles.y);
                gameObject.transform.rotation = playerRotData;
                gameObject.transform.localScale = startScale * UIScript.vrSize;
                

                if(DisplayDistance && distanceText!=null){
                     float dist = Vector3.Distance(UIScript.PlayerAircraft.VehicleMainObj.transform.position, TargetClass.gameObject.transform.position);
                     distanceText.text = dist.ToString("F0");
                }else{
                    if(distanceText!=null && distanceText.gameObject.activeSelf){ distanceText.gameObject.SetActive(false); }
                }
                if(DisplayText && indicator!=null){
                    if(!indicator.gameObject.activeSelf) indicator.gameObject.SetActive(true);
                }else{
                    if(indicator!=null && indicator.gameObject.activeSelf){ indicator.gameObject.SetActive(false); }
                }
            }
            if(!ParentClass.isRenderedMarker || !ParentClass.isRendered){
                ParentClass.RemoveObjectiveTracker(this);
            }
        }
    }

    public void toDestroy()
    {
        destroy = true;
        // Will be called just prior to destruction of the gameobject to which this script is attached

    }

}

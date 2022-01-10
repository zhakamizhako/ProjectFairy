
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.Animations;

public class LandingSystemController : UdonSharpBehaviour
{
    // public Transform CatchArmDetector;
    // public Transform offSetGear;
    public EngineController EngineControl;
    public CatchArmController CAC;
    public FHudController fhud;
    
    // public ParentConstraint constraint;
    // [System.NonSerializedAttribute] public ConstraintSource source;
    public bool isSnagged;

    void Start()
    {
        // Assert (CatchArmDetector != null, "Start: AnimationControl != null");
        // Assert (EngineControl != null, "Start: Detector != null");
    }

    void Update(){
        if(CAC==null && isSnagged){
            isSnagged = false;
            if(fhud!=null && fhud.CatchArmLocked!=null && fhud.CatchArmReady!=null & fhud.CatapultReady!=null){
                if(fhud.CatchArmLocked.activeSelf)
                    fhud.CatchArmLocked.SetActive(false);
                if(fhud.CatapultReady.activeSelf)
                    fhud.CatapultReady.SetActive(false);
                if(fhud.CatchArmReady.activeSelf)
                    fhud.CatchArmReady.SetActive(false);
            }
        }else if(CAC!=null && EngineControl.Piloting)
        {
            if (!Input.GetKeyDown(KeyCode.C) || !(CAC.holdTimer > CAC.holdTime)) return;
            if(EngineControl.localPlayer==null){ CAC.Launch(); }
            CAC.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Launch");
        }
    }

    public void Launch()
    {
        if (CAC == null || !EngineControl.Piloting || !(CAC.holdTimer > CAC.holdTime)) return;
        if(EngineControl.localPlayer==null){ CAC.Launch(); }
        CAC.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Launch");
    }
}

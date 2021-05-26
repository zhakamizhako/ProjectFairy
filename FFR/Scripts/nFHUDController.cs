
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class nFHUDController : UdonSharpBehaviour
{
    public EngineController EngineControl;
    public EffectsController EController;
    public Animator FhudAnimator;
    public OpenWorldMovementLogic OWML;
    public float AltimeterDivider = 1000;
    public float SpeedDivider = 1000;
    public Text[] textObjectsSpeeds;
    public Text[] textObjectsAltimeterL;
    public Text[] textObjectsAltimeterR;
    public Text[] textObjectsHeader;
    public Text AltimeterText;
    public Text SpeedometerText;
    public Text HeadingText;
    public Text GText;
    public Text AlphaText;
    public Text MachText;
    public bool isHeadTracked;
    private VRCPlayerApi localPlayer;
    public Transform parentTransform;
    public PlayerUIScript UIScript;
    public bool onTesting;


    void Start()
    {
        localPlayer = Networking.LocalPlayer;
    }

    void Update(){
        if(FhudAnimator!=null && EngineControl==null && EngineControl.VehicleRigidbody==null && (!EngineControl.Passenger || !EngineControl.Piloting)){
            return;
        }

        doSpeed();
        doAltitude();
        doElements();
        doHeading();
        doLevel();
        doTexts();
        doVelocity();
        doCheckPullUp();
    }

    void doColor(){

    }

    void LateUpdate(){

    }

    void doSpeed(){
        float currentSpeed = EngineControl.CurrentVel.magnitude* 1.9438445f;
        FhudAnimator.SetFloat("speedometer",(currentSpeed) / SpeedDivider );

    }
    
    void doAltitude(){
        FhudAnimator.SetFloat("altitude", ((EngineControl.CenterOfMass.position.y + -EngineControl.SeaLevel) * 3.28084f) / 10000);
    }
    
    void doElements(){
        FhudAnimator.SetBool("flaps", EngineControl.EffectsControl.Flaps);
        FhudAnimator.SetBool("gears", !EngineControl.EffectsControl.GearUp);
        FhudAnimator.SetBool("airbrake", EngineControl.BrakeInput==1);
        FhudAnimator.SetBool("limits", EngineControl.FlightLimitsEnabled);
        FhudAnimator.SetBool("danger", EngineControl.Health < (EngineControl.FullHealth *.25));
        FhudAnimator.SetBool("overg", EngineControl.Gs > (EngineControl.MaxGs * .60f));
    }
    
    void doHeading(){

    }

    void doLevel(){

    }

    void doTexts(){

    }

    void doVelocity(){

    }

    void doCheckPullUp(){

    }

    void doCheckCollisionAlert(){

    }
}

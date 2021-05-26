using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class CanvasHud : UdonSharpBehaviour
{
    public Text AltimeterText;
    public EngineController EngineControl;
    private EffectsController EffectsControl;
    public HitboxControllerAndEffects hbcontroller;
    public Animator CanvasHUDAnimator;
    public float divisibleValue = 1500f;
    public float divisibleValue2 = 100f;
    public ThirdPersonPlayerCamera third;
    public GameObject thirdHUD;
    // public FHudController fhud;
    // public LayerMask UILayer;
    public GameObject cockpitcam;
    public bool debugIsEnabled = false;
    public bool isVR = false;
    public Transform parentTransform;
    public PlayerUIScript UIScript;
    private VRCPlayerApi localPlayer;

    private int TempAltimeterVal = 0;

    private Vector3 startSize;
    void Start()
    {
        localPlayer = Networking.LocalPlayer;
        startSize = gameObject.transform.localScale;
        EffectsControl = EngineControl.EffectsControl;
    }

    void LateUpdate()
    {
        if (isVR && UIScript != null)
        {
            if (parentTransform != null)
            {
                float offset = UIScript.vrDistance;
                float size = UIScript.vrSize;
                if (localPlayer != null)
                {
                    var PrePos = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;
                    parentTransform.position = PrePos;
                    parentTransform.position += parentTransform.forward * offset;
                    parentTransform.rotation = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation;
                    gameObject.transform.localScale = startSize * size;
                }
                else
                {
                    parentTransform.position = parentTransform.forward * offset;
                    //  parentTransform.rotation = new Vector3(0,0,0);
                }
            }
        }
    }
    void Update()
    {
        if (EngineControl != null)
        {
            //REQUIRES HBCONTROLLER
            if (!isVR && thirdHUD != null)
            {
                if (third.enabledCam == true)
                {
                    cockpitcam.SetActive(true);
                    thirdHUD.SetActive(true);
                    debugIsEnabled = true;
                }
                else
                {
                    thirdHUD.SetActive(false);
                    cockpitcam.SetActive(false);
                    debugIsEnabled = false;
                }
            }
            if (AltimeterText != null)
            {
                int testAltimeterVal = Mathf.RoundToInt((EngineControl.CenterOfMass.position.y + -EngineControl.SeaLevel) * 3.28084f);
                if (testAltimeterVal != TempAltimeterVal)
                {
                    TempAltimeterVal = testAltimeterVal;
                    AltimeterText.text = string.Format("{0}ft", testAltimeterVal);
                }
                // AltimeterText.text = ((EngineControl.CenterOfMass.position.y + -EngineControl.SeaLevel) * 3.28084f).ToString ("F0") + "ft";

            }
            if (CanvasHUDAnimator != null)
            {
                if (!hbcontroller.isLEngineDead)
                {
                    CanvasHUDAnimator.SetFloat("RPML", EngineControl.Throttle);
                    CanvasHUDAnimator.SetFloat("speedL", ((EngineControl.CurrentVel.magnitude) * 1.9438445f) / 360);
                    CanvasHUDAnimator.SetBool("ABL", EffectsControl.AfterburnerOn);
                }
                else
                {
                    CanvasHUDAnimator.SetFloat("RPML", 0);
                    CanvasHUDAnimator.SetFloat("speedL", 0);
                    CanvasHUDAnimator.SetBool("ABL", false);
                }
                if (!hbcontroller.isREngineDead)
                {
                    CanvasHUDAnimator.SetFloat("RPMR", EngineControl.Throttle);
                    CanvasHUDAnimator.SetBool("ABR", EffectsControl.AfterburnerOn);
                    CanvasHUDAnimator.SetFloat("speedR", ((EngineControl.CurrentVel.magnitude) * 1.9438445f) / 360);
                }
                else
                {
                    CanvasHUDAnimator.SetFloat("RPMR", 0);
                    CanvasHUDAnimator.SetFloat("speedR", 0);
                    CanvasHUDAnimator.SetBool("ABR", false);
                }
                CanvasHUDAnimator.SetFloat("altneedle", ((EngineControl.CenterOfMass.position.y + -EngineControl.SeaLevel) * 3.28084f) / 1000);
                CanvasHUDAnimator.SetFloat("altw", ((EngineControl.CenterOfMass.position.y + -EngineControl.SeaLevel) * 3.28084f) / 10000);
                CanvasHUDAnimator.SetFloat("liftneedle", (EngineControl.CurrentVel.y * 60 * 3.28084f) / divisibleValue + .5f);
            }
            if (hbcontroller != null)
            {
                CanvasHUDAnimator.SetFloat("health_wingL", (hbcontroller.initHealthLWing - hbcontroller.HealthLWing) / hbcontroller.initHealthLWing * 100f * 0.01f);
                CanvasHUDAnimator.SetFloat("health_wingR", (hbcontroller.initHealthRWing - hbcontroller.HealthRWing) / hbcontroller.initHealthRWing * 100f * 0.01f);
                CanvasHUDAnimator.SetFloat("health_rudderL", (hbcontroller.initHealthLRudder - hbcontroller.HealthLRudder) / hbcontroller.initHealthLRudder * 100f * 0.01f);
                CanvasHUDAnimator.SetFloat("health_rudderR", (hbcontroller.initHealthRRudder - hbcontroller.HealthRRudder) / hbcontroller.initHealthRRudder * 100f * 0.01f);
                CanvasHUDAnimator.SetFloat("health_aileronL", (hbcontroller.initHealthLAileron - hbcontroller.HealthLAileron) / hbcontroller.initHealthLAileron * 100f * 0.01f);
                CanvasHUDAnimator.SetFloat("health_aileronR", (hbcontroller.initHealthRAileron - hbcontroller.HealthRAileron) / hbcontroller.initHealthRAileron * 100f * 0.01f);
                CanvasHUDAnimator.SetFloat("health_engineL", (hbcontroller.initHealthLEngine - hbcontroller.HealthLEngine) / hbcontroller.initHealthLEngine * 100f * 0.01f);
                CanvasHUDAnimator.SetFloat("health_engineR", (hbcontroller.initHealthREngine - hbcontroller.HealthREngine) / hbcontroller.initHealthREngine * 100f * 0.01f);
                CanvasHUDAnimator.SetFloat("health_flapsL", (hbcontroller.initHealthLFlap - hbcontroller.HealthLFlap) / hbcontroller.initHealthLFlap * 100f * 0.01f);
                CanvasHUDAnimator.SetFloat("health_flapsR", (hbcontroller.initHealthRFlap - hbcontroller.HealthRFlap) / hbcontroller.initHealthRFlap * 100f * 0.01f);
                CanvasHUDAnimator.SetFloat("health_elevatorL", (hbcontroller.initHealthLElevator - hbcontroller.HealthLElevator) / hbcontroller.initHealthLElevator * 100f * 0.01f);
                CanvasHUDAnimator.SetFloat("health_elevatorR", (hbcontroller.initHealthRElevator - hbcontroller.HealthRElevator) / hbcontroller.initHealthRElevator * 100f * 0.01f);
                CanvasHUDAnimator.SetFloat("health_body", (hbcontroller.initHealthBody - hbcontroller.HealthBody) / hbcontroller.initHealthBody * 100f * 0.01f);
                CanvasHUDAnimator.SetFloat("health", (hbcontroller.EngineControl.Health - hbcontroller.EngineControl.FullHealth) / hbcontroller.EngineControl.FullHealth * 100f * 0.01f);
            }
        }

    }
}
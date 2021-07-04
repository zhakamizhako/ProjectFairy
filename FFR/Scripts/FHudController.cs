using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class FHudController : UdonSharpBehaviour
{
    public EngineController EngineController;
    // public EffectsController EController;
    public GameObject PlaneBody;
    public Transform HeadingTool;
    // public Transform Altimeter;
    // public Transform Speedometer;
    public Transform GunHud;
    // public Transform HorizonTool;
    // public Transform LevelTool;
    // public GameObject FlapsUI;
    // public GameObject GearUpUI;
    // public GameObject GearDownUI;
    // public GameObject OverGUI;
    // public GameObject DangerUI;
    // public GameObject ChangeTargetUI;
    // public GameObject NoTargetUI;
    public GameObject PullupUI;
    // public GameObject LimiterUI;
    // public GameObject AirBrakeUI;
    // public AudioSource AB;
    // public AudioSource MissileAlertSound;
    // public GameObject ABUI;
    public AudioSource Pullupaudio;
    public Text AltimeterText;
    public Text SpeedometerText;
    public Text GsText;
    public Text SpeedMachText;
    // public Text HealthText;
    public Text AngleOfAttack;
    // public GameObject MaveFlipMode;
    // public float divisibleValue = 5;
    // private bool isPlayedAB = false;
    private bool isPlayedPullup = false;

    public bool isHeadTracked = true;
    public float distance_from_head = 1.333f;

    // public float SeaLevel = -200000;
    // private Vector3 altimeterLerp = new Vector3(0, 0, 0);
    // private Vector3 speedometerLerp = new Vector3(0, 0, 0);
    // private Vector3 headingTurn = new Vector3(0, 0, 0);
    // private Vector3 gunhudLerp = new Vector3(0, 0, 0);
    // public float distance_from_head = 1.333f;
    public Transform parentTransform;
    // public bool isVR;
    public bool onTesting = false;
    public PlayerUIScript UIScript;
    private VRCPlayerApi localPlayer;
    private Vector3 startSize;

    // private int HealthTemp = 0;
    private int altimetertemp = 0;
    private int speedometerTextTemp = 0;
    private float machTemp = 0f;
    private float gsTemp = 0f;
    private float aoatemp = 0f;

    public GameObject CatchArmLocked;
    public GameObject CatchArmReady;
    public GameObject CatapultReady;
    public GameObject MissObject;
    public GameObject HitObject;
    public GameObject MissileAlertObject;
    public GameObject CautionObject;
    private Rigidbody MainBodyPlane;

    //Animator centre
    public Animator HUDAnimator;
    // public float altitudeMultiplier;
    // public float speedMultiplier;
    public bool isHeadingConstrainted = false;
    public bool isHeadingLocalRotation = true;

    public LayerMask pullupdetector;
    public float checkPullupEvery = 1f;
    private float checkpulluptimer = 0f;
    public bool doPullUpCheck = false;
    public float AltDivisor1;
    public float AltDivisor2;
    public float LiftDivisor;
    public float SpeedDivisor1;
    public Vector3 offsets = new Vector3(0.5f, 0, 0.5f);

    void Start()
    {
        localPlayer = Networking.LocalPlayer;
        startSize = gameObject.transform.localScale;
        MainBodyPlane = PlaneBody != null ? PlaneBody.GetComponent<Rigidbody>() : null;
        // if (EngineController!=null && !EngineController.InEditor) {
        //     gameObject.SetActive (false);
        // }
        // EngineController.localPlayer = Networking.LocalPlayer;
        if (CatchArmLocked != null) { CatchArmLocked.SetActive(false); }
        if (CatchArmReady != null) { CatchArmReady.SetActive(false); }
        if (CatapultReady != null) { CatapultReady.SetActive(false); }
        if (MissObject != null) { MissObject.SetActive(false); }
        if (HitObject != null) { HitObject.SetActive(false); }
        if (MissileAlertObject != null) { MissileAlertObject.SetActive(false); }
        if (CautionObject != null) { CautionObject.SetActive(false); }
    }

    void LateUpdate()//???????????????????????????
    {
        if (localPlayer != null && (onTesting || EngineController != null) && UIScript != null && isHeadTracked && (EngineController.Piloting || EngineController.Passenger))
        {
            if (parentTransform != null)
            {
                float offset = UIScript.vrDistance2;
                float size = UIScript.vrSize2;
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


    void doAnimator()
    {
        float value = EngineController.VehicleMainObj.transform.position.y +
       (EngineController.OWML != null && EngineController.OWML.ScriptEnabled ?
                (EngineController.OWML.AnchorCoordsPosition.y - EngineController.OWML.Map.position.y) + EngineController.SeaLevel * 3.28084f
                : (EngineController.VehicleMainObj.transform.position.y + EngineController.SeaLevel * 3.28084f));

        //Uncomment this line when not using OWML.
        // float value = EngineController.VehicleMainObj.transform.position.y + EngineController.SeaLevel * 3.28084f;

        HUDAnimator.SetFloat("altimeter1", (value / AltDivisor1));
        HUDAnimator.SetFloat("altimeter2", (value / AltDivisor2));
        HUDAnimator.SetFloat("liftneedle", (EngineController.CurrentVel.y * 60 * 3.28084f) / LiftDivisor + .5f); // Note: Lift on every 60 seconds / Total value  ?. 

        //Level
        var body = EngineController.VehicleMainObj.transform.rotation.eulerAngles;
        HUDAnimator.SetFloat("level", (body.z / 360) + offsets.z);
        HUDAnimator.SetFloat("pitch", (body.x / 180) + offsets.x);

        //should be fine in the main animator
        HUDAnimator.SetBool("flaps", EngineController.EffectsControl.Flaps);
        HUDAnimator.SetBool("gears", !EngineController.EffectsControl.GearUp);

        HUDAnimator.SetBool("over_g", EngineController.Gs > ((EngineController.MaxGs) * (.60f)));
        HUDAnimator.SetBool("danger", EngineController.Health < (EngineController.FullHealth * .25));

        HUDAnimator.SetBool("pullup", doPullUpCheck);

        HUDAnimator.SetBool("brake", EngineController.BrakeInput == 1);
        HUDAnimator.SetBool("limiter",EngineController.FlightLimitsEnabled);
        //heading
        float angle = -EngineController.VehicleMainObj.transform.rotation.eulerAngles.y;
        HUDAnimator.SetFloat("heading", (angle / 360) + offsets.y);

        //Speedometer
        HUDAnimator.SetFloat("speed", ((EngineController.CurrentVel.magnitude) * 1.9438445f) / SpeedDivisor1);

    }

    void doTexts()
    {

        if (AltimeterText != null)
        {
            float value = EngineController.VehicleMainObj.transform.position.y +
                   (EngineController.OWML != null && EngineController.OWML.ScriptEnabled ?
                            (EngineController.OWML.AnchorCoordsPosition.y - EngineController.OWML.Map.position.y) + EngineController.SeaLevel * 3.28084f
                            : (EngineController.VehicleMainObj.transform.position.y + EngineController.SeaLevel * 3.28084f));

            if (value != altimetertemp)
            {
                altimetertemp = Mathf.RoundToInt(value);
                AltimeterText.text = string.Format("{0}ft", altimetertemp);
            }

            // if (PullupUI != null)
            // {
                
            // }
        }
        if (SpeedometerText != null)
        {
            if (EngineController.VehicleRigidbody != null)
            {
                int testSpeedometer = Mathf.RoundToInt(EngineController.CurrentVel.magnitude * 1.9438445f);
                if (testSpeedometer != speedometerTextTemp)
                {
                    speedometerTextTemp = testSpeedometer;
                    SpeedometerText.text = string.Format("{0}kt", speedometerTextTemp);
                }

                // SpeedometerText.text = (((EngineController.CurrentVel.magnitude) * 1.9438445f).ToString("F0")) + "kt";
            }
        }

        if (SpeedMachText != null)
        {
            float TestSpeedMach = (EngineController.CurrentVel.magnitude) / 343f;
            if (TestSpeedMach != machTemp)
            {
                machTemp = TestSpeedMach;
                SpeedMachText.text = string.Format("{0:0.00}", machTemp);
            }
        }
        if (GsText != null)
        {
            float gstest = EngineController.Gs;
            if (gstest != gsTemp)
            {
                gsTemp = gstest;
                GsText.text = string.Format("{0:0.00}", gsTemp);
            }
        }
        if (AngleOfAttack != null)
        {
            float aoatest = EngineController.AngleOfAttack;
            if (aoatest != aoatemp)
            {
                aoatemp = aoatest;
                AngleOfAttack.text = string.Format("{0:0.00}", aoatemp);
            }
        }
    }

    void Update()
    {
        if (EngineController != null && EngineController.VehicleRigidbody != null && (EngineController.Passenger || EngineController.Piloting))
        {
            //??
            if (HUDAnimator != null)
            {
                doAnimator();
                // float value = EngineController.VehicleMainObj.transform.position.y +
                //        (EngineController.OWML != null && EngineController.OWML.ScriptEnabled ?
                //                 (EngineController.OWML.AnchorCoordsPosition.y - EngineController.OWML.Map.position.y) + EngineController.SeaLevel * 3.28084f
                //                 : (EngineController.VehicleMainObj.transform.position.y + EngineController.SeaLevel * 3.28084f));

                // HUDAnimator.SetFloat("altimeter", (value));
                // HUDAnimator.SetFloat("speedometer", (EngineController.CurrentVel.magnitude * 1.9438445f) * speedMultiplier);
            }
            doTexts();
            doPullup();
            // if (Speedometer != null)
            // {
            //     // if (EngineController.VehicleRigidbody)
            //     // {
            //     //     speedometerLerp = Vector3.Lerp(speedometerLerp, new Vector3(-((EngineController.CurrentVel.magnitude * 1.9438445f) / 5.5f), 0, 0), 4f * Time.deltaTime);
            //     //     Speedometer.localRotation = Quaternion.Euler(speedometerLerp);
            //     // }
            // }
            // if (MaveFlipMode != null)
            // {
            //     if (EngineController != null)
            //     {
            //         if (EngineController.EffectsControl.Flaps)
            //         {
            //             if (MaveFlipMode.activeSelf)
            //                 MaveFlipMode.SetActive(false);
            //         }
            //         else
            //         {
            //             if (!MaveFlipMode.activeSelf)
            //                 MaveFlipMode.SetActive(true);
            //         }
            //     }
            // }
            // if (AirBrakeUI != null)
            // {
            //     if (EngineController.BrakeInput == 1)
            //     {
            //         if (!AirBrakeUI.activeSelf)
            //             AirBrakeUI.SetActive(true);
            //     }
            //     else
            //     {
            //         if (AirBrakeUI.activeSelf)
            //             AirBrakeUI.SetActive(false);
            //     }
            // }
            // if (Altimeter != null)
            // {
            //     float value = EngineController.VehicleMainObj.transform.position.y +
            //                            (EngineController.OWML != null && EngineController.OWML.ScriptEnabled ?
            //                                     (EngineController.OWML.AnchorCoordsPosition.y - EngineController.OWML.Map.position.y) + EngineController.SeaLevel * 3.28084f
            //                                     : (EngineController.VehicleMainObj.transform.position.y + EngineController.SeaLevel * 3.28084f));

            //     altimeterLerp = Vector3.Lerp(altimeterLerp, new Vector3(-(value) / divisibleValue, 0, 0), 4.5f * Time.deltaTime);
            //     Altimeter.localRotation = Quaternion.Euler(altimeterLerp);
            // }
            // if (HorizonTool != null)
            // {
            //     var hrotate = EngineController.VehicleMainObj.transform.rotation.eulerAngles;
            //     // float new_z = hrotate.z;
            //     // hrotate.z = 0;
            //     hrotate.y = 0;
            //     hrotate.z = -hrotate.z;
            //     HorizonTool.localRotation = Quaternion.Euler(-hrotate);
            //     HorizonTool.localPosition = Vector3.zero;
            // }
            // if (LevelTool != null)
            // {
            //     var hrotate = EngineController.VehicleMainObj.transform.rotation.eulerAngles;
            //     // float new_z = hrotate.z;
            //     hrotate.z = 0;
            //     hrotate.x = 0;
            //     hrotate.y = EngineController.VehicleMainObj.transform.rotation.eulerAngles.z;
            //     LevelTool.localRotation = Quaternion.Euler(-hrotate);
            //     LevelTool.localPosition = Vector3.zero;
            // }
            // if (FlapsUI != null)
            // {
            //     if (EngineController.EffectsControl.Flaps)
            //     {
            //         if (!FlapsUI.activeSelf)
            //             FlapsUI.SetActive(true);
            //     }
            //     else
            //     {
            //         if (FlapsUI.activeSelf)
            //             FlapsUI.SetActive(false);
            //     }
            // }
            // if (HealthText != null)
            // {
            //     int TestHealth = Mathf.RoundToInt(EngineController.Health);
            //     if (TestHealth != HealthTemp)
            //     {
            //         HealthTemp = TestHealth;
            //         HealthText.text = string.Format("HP:{0}", HealthTemp);
            //     }
            // }
            // if (CopilotHealthText != null) { CopilotHealthText.text = "HP: " + EngineController.Health.ToString("F0"); }

            // if (LimiterUI != null)
            // {
            //     if (EngineController.FlightLimitsEnabled)
            //     {
            //         if (!LimiterUI.activeSelf)
            //             LimiterUI.SetActive(true);
            //     }
            //     else
            //     {
            //         if (LimiterUI.activeSelf)
            //             LimiterUI.SetActive(false);
            //     }
            // }


            // if (GearDownUI != null)
            // {
            //     if (!EngineController.EffectsControl.GearUp)
            //     {
            //         if (!GearDownUI.activeSelf)
            //             GearDownUI.SetActive(true);
            //     }
            //     else
            //     {
            //         if (GearDownUI.activeSelf)
            //             GearDownUI.SetActive(false);
            //     }
            // }
            // if (DangerUI != null)
            // {
            //     if (EngineController.Health < (EngineController.FullHealth * .25))
            //     {
            //         if (!DangerUI.activeSelf)
            //             DangerUI.SetActive(true);
            //     }
            //     else
            //     {
            //         if (DangerUI.activeSelf)
            //             DangerUI.SetActive(false);
            //     }
            // }
            // if (AB != null)
            // {
            //     if (EngineController.EffectsControl.AfterburnerOn)
            //     {
            //         if (isPlayedAB == false)
            //         {
            //             AB.Play();
            //             isPlayedAB = true;
            //             if (ABUI != null)
            //                 ABUI.SetActive(true);
            //         }
            //     }
            //     else
            //     {
            //         isPlayedAB = false;
            //         if (ABUI != null)
            //             ABUI.SetActive(false);
            //     }
            // }
            // if (OverGUI != null && EController != null)
            // {
            //     if (EngineController.Gs > ((EngineController.MaxGs) * (.60f)))
            //     {
            //         if (!OverGUI.activeSelf)
            //             OverGUI.SetActive(true);
            //     }
            //     else
            //     {
            //         if (OverGUI.activeSelf)
            //             OverGUI.SetActive(false);
            //     }
            // }
            if (GunHud != null)
            {
                Vector3 tempvel;
                if (EngineController.CurrentVel.magnitude < 2)
                {
                    tempvel = -Vector3.up * 2;//straight down instead of spazzing out when moving very slow
                }
                else
                {
                    tempvel = EngineController.CurrentVel;
                }

                GunHud.position = transform.position + tempvel;
                GunHud.localPosition = GunHud.localPosition.normalized * distance_from_head;
                // var localVelocity = PlaneBody.transform.InverseTransformDirection(EngineController.CurrentVel);
                // gunhudLerp = Vector3.Lerp(gunhudLerp, new Vector3(-localVelocity.x / 2.5f, 0, localVelocity.y / 2.5f), 4.5f * Time.deltaTime);
                // GunHud.localPosition = gunhudLerp;
            }

            if (PlaneBody != null && HeadingTool != null)
            {
                float angle = (Mathf.Atan2(MainBodyPlane.velocity.x, MainBodyPlane.velocity.z) * Mathf.Rad2Deg);
                angle = (angle + 360f) % 360f;
                Vector3 headingTurn = EngineController.VehicleMainObj.transform.rotation.eulerAngles;
                headingTurn.z = !isHeadingConstrainted ? EngineController.VehicleMainObj.transform.rotation.eulerAngles.y : 0;
                headingTurn.x = 0;
                headingTurn.y = 0;
                if (isHeadingLocalRotation)
                {
                    HeadingTool.localRotation = Quaternion.Euler(headingTurn);
                }
                else
                {
                    HeadingTool.rotation = Quaternion.Euler(headingTurn);
                }
            }
        }
        if (EngineController != null && !EngineController.Piloting)
        {
            doPullUpCheck = false;
            // if (Pullupaudio != null && isPlayedPullup)
            // {
            //     Pullupaudio.Stop();
            //     isPlayedPullup = false;
            // }
        }

    }

    public void doPullup()
    {
        if (checkpulluptimer < checkPullupEvery)
        {
            checkpulluptimer = checkpulluptimer + Time.deltaTime;

            return;
        }
        else
        {
            checkpulluptimer = 0f;
        }

        if (EngineController.EffectsControl.GearUp)
        {
            var hit = Physics.Raycast(EngineController.GroundDetector.position, -(EngineController.GroundDetector.up), 500f, pullupdetector, QueryTriggerInteraction.UseGlobal); //in case of shit happens like multiple rayhitted objects

            if (hit)
            {
                doPullUpCheck = true;
                // PullupUI.SetActive(true);
                // if (Pullupaudio != null && isPlayedPullup == false)
                // {
                //     Pullupaudio.Play();
                //     isPlayedPullup = true;
                // }
            }
            else
            {
                doPullUpCheck = false;
                // PullupUI.SetActive(false);
                // if (Pullupaudio != null && isPlayedPullup == true)
                // {
                //     Pullupaudio.Stop();
                //     isPlayedPullup = false;
                // }
            }

        }
        else
        {
            doPullUpCheck = false;
            // PullupUI.SetActive(false);
            // if (Pullupaudio != null && isPlayedPullup == true)
            // {
            //     Pullupaudio.Stop();
            //     isPlayedPullup = false;
            // }
        }
    }
}
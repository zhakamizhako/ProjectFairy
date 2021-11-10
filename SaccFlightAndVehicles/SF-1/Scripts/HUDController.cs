
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

public class HUDController : UdonSharpBehaviour
{
    public EngineController EngineControl;
    // public Material SmokeColorIndicator;
    public Text HUDText_G;
    public Text HUDText_mach;
    public Text HUDText_altitude;
    public Text HUDText_knotstarget;
    public Text HUDText_knots;
    public Text HUDText_knotsairspeed;
    public Text HUDText_angleofattack;
    // public Text HUDText_AAM_ammo;
    // public Text HUDText_AGM_ammo;
    // public Text HUDText_Bomb_ammo;
    public GameObject HudCrosshairGun;
    public GameObject HudCrosshair;
    public GameObject HudHold;
    public GameObject HudLimit;
    public GameObject HudAB;
    public Transform DownIndicator;
    public Transform ElevationIndicator;
    public Transform HeadingIndicator;
    public Transform VelocityIndicator;
    public Transform AAMTargetIndicator;
    // public Transform GUNLeadIndicator;
    public float BulletSpeed = 1050;
    public Transform PitchRoll;
    public Transform Yaw;
    // public Transform LStickDisplayHighlighter;
    // public Transform RStickDisplayHighlighter;
    /*     public Transform TrimPitch;
        public Transform TrimYaw; */
    public GameObject AtGScreen;
    // public GameObject LStick_funcon1;
    // public GameObject LStick_funcon2;
    // public GameObject LStick_funcon4;
    // public GameObject LStick_funcon6;
    // public GameObject LStick_funcon7;
    // public GameObject LStick_funcon8;
    // public GameObject RStick_funcon3;
    // public GameObject RStick_funcon5;
    // public GameObject RStick_funcon6;
    // public GameObject RStick_funcon7;
    // public GameObject RStick_funcon8;
    private Animator PlaneAnimator;
    public float distance_from_head = 1.333f;
    private float maxGs = 0f;
    private Vector3 InputsZeroPos;
    private Vector3 startingpos;
    private float check = 0;
    [System.NonSerializedAttribute] public float MenuSoundCheckLast = 0;
    private int showvel;
    const float InputSquareSize = 0.0284317f;
    [System.NonSerializedAttribute] public Vector3 GUN_TargetDirOld;
    [System.NonSerializedAttribute] public float GUN_TargetSpeedLerper;
    [System.NonSerializedAttribute] public Vector3 RelativeTargetVelLastFrame;
    private Vector3 TargetDir = Vector3.zero;
    private Vector3 TargetSpeed;
    // private bool HasAAMTargets = false;
    private float FullFuelDivider;
    private float FullGunAmmoDivider;
    private Vector3 RelativeTargetVel;
    private Vector3 AAMCurrentTargetPositionLastFrame;
    private Transform VehicleTransform;
    private EffectsController EffectsControl;
    private Camera AtGCam;
    float debuglerper;
    float debugcurrentframe;
    private void Start()
    {
        Assert(EngineControl != null, "Start: EngineControl != null");
        // // Assert(SmokeColorIndicator != null, "Start: SmokeColorIndicator != null");
        Assert(HUDText_G != null, "Start: HUDText_G != null");
        Assert(HUDText_mach != null, "Start: HUDText_mach != null");
        Assert(HUDText_altitude != null, "Start: HUDText_altitude != null");
        Assert(HUDText_knotstarget != null, "Start: HUDText_knotstarget != null");
        Assert(HUDText_knots != null, "Start: HUDText_knots != null");
        Assert(HUDText_knotsairspeed != null, "Start: HUDText_knotsairspeed != null");
        Assert(HUDText_angleofattack != null, "Start: HUDText_angleofattack != null");
        // Assert(HudCrosshairGun != null, "Start: HudCrosshairGun != null");
        // Assert(HudCrosshair != null, "Start: HudCrosshair != null");
        Assert(HudHold != null, "Start: HudHold != null");
        Assert(HudLimit != null, "Start: HudLimit != null");
        Assert(HudAB != null, "Start: HudAB != null");
        Assert(DownIndicator != null, "Start: DownIndicator != null");
        Assert(ElevationIndicator != null, "Start: ElevationIndicator != null");
        Assert(HeadingIndicator != null, "Start: HeadingIndicator != null");
        Assert(VelocityIndicator != null, "Start: VelocityIndicator != null");
        // Assert(AAMTargetIndicator != null, "Start: AAMTargetIndicator != null");
        // // Assert(GUNLeadIndicator != null, "Start: GUNLeadIndicator != null");
        // Assert(LStickDisplayHighlighter != null, "Start: LStickDisplayHighlighter != null");
        // Assert(RStickDisplayHighlighter != null, "Start: RStickDisplayHighlighter != null");
        Assert(PitchRoll != null, "Start: PitchRoll != null");
        Assert(Yaw != null, "Start: Yaw != null");
        /*         Assert(TrimPitch != null, "Start: TrimPitch != null");
                Assert(TrimYaw != null, "Start: TrimYaw != null"); */
        // Assert(AtGScreen != null, "Start: AGMScreen != null");
        // // Assert(LStick_funcon1 != null, "Start: LStick_funcon1 != null");
        // // Assert(LStick_funcon2 != null, "Start: LStick_funcon2 != null");
        // // Assert(LStick_funcon4 != null, "Start: LStick_funcon4 != null");
        // // Assert(LStick_funcon6 != null, "Start: LStick_funcon6 != null");
        // // Assert(LStick_funcon7 != null, "Start: LStick_funcon7 != null");
        // // Assert(LStick_funcon8 != null, "Start: LStick_funcon8 != null");
        // Assert(RStick_funcon3 != null, "Start: LStick_funcon3 != null");
        // Assert(RStick_funcon5 != null, "Start: LStick_funcon5 != null");
        // Assert(RStick_funcon6 != null, "Start: LStick_funcon6 != null");
        // Assert(RStick_funcon7 != null, "Start: LStick_funcon7 != null");
        // Assert(RStick_funcon8 != null, "Start: LStick_funcon8 != null");

	PlaneAnimator = EngineControl.VehicleMainObj.GetComponent<Animator>();
        InputsZeroPos = PitchRoll.localPosition;
        VehicleTransform = EngineControl.VehicleMainObj.transform;
        EffectsControl = EngineControl.EffectsControl;
        // AtGCam = EngineControl.AtGCam;

        float fuel = EngineControl.Fuel;
        FullFuelDivider = 1f / (fuel > 0 ? fuel : 10000000);
        // float gunammo = EngineControl.GunAmmoInSeconds;
        // FullGunAmmoDivider = 1f / (gunammo > 0 ? gunammo : 10000000);
    }
    private void OnEnable()
    {
        maxGs = 0f;
    }
    private void LateUpdate()
    {
        float SmoothDeltaTime = Time.smoothDeltaTime;
        //RollPitch Indicator
        PitchRoll.localPosition = InputsZeroPos + (new Vector3(-EngineControl.RotationInputs.z, EngineControl.RotationInputs.x, 0)) * InputSquareSize;

        //Yaw Indicator
        Yaw.localPosition = InputsZeroPos + (new Vector3(EngineControl.RotationInputs.y, 0, 0)) * InputSquareSize;

        /*         //Yaw Trim Indicator
                TrimYaw.localPosition = InputsZeroPos + (new Vector3(EngineControl.Trim.y, 0, 0)) * InputSquareSize;

                //Pitch Trim Indicator
                TrimPitch.localPosition = InputsZeroPos + (new Vector3(0, EngineControl.Trim.x, 0)) * InputSquareSize; */

        //Velocity indicator
        Vector3 tempvel;
        if (EngineControl.CurrentVel.magnitude < 2)
        {
            tempvel = -Vector3.up * 2;//straight down instead of spazzing out when moving very slow
        }
        else
        {
            tempvel = EngineControl.CurrentVel;
        }

        VelocityIndicator.position = transform.position + tempvel;
        VelocityIndicator.localPosition = VelocityIndicator.localPosition.normalized * distance_from_head;
        /////////////////

        // if (EngineControl.RStickSelection == 1)
        // {
        //     HudCrosshairGun.SetActive(true);
        //     HudCrosshair.SetActive(false);
        // }
        // else
        // {
        //     HudCrosshairGun.SetActive(false);
        //     HudCrosshair.SetActive(true);
        // }

        //AAM Target Indicator
        // if (EngineControl.AAMHasTarget && (EngineControl.RStickSelection == 1 || EngineControl.RStickSelection == 2))//GUN or AAM
        // {
        //     AAMTargetIndicator.localScale = new Vector3(1, 1, 1);
        //     AAMTargetIndicator.position = transform.position + EngineControl.AAMCurrentTargetDirection;
        //     AAMTargetIndicator.localPosition = AAMTargetIndicator.localPosition.normalized * distance_from_head;
        //     if (EngineControl.AAMLocked)
        //     {
        //         AAMTargetIndicator.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));//back of mesh is locked version
        //     }
        //     else
        //     {
        //         AAMTargetIndicator.localRotation = Quaternion.identity;
        //     }
        // }
        // else AAMTargetIndicator.localScale = Vector3.zero;
        /////////////////

        ////GUN Lead Indicator
        //if (EngineControl.AAMHasTarget && EngineControl.RStickSelection == 1)
        //{
        //    GUNLeadIndicator.gameObject.SetActive(true);
        //    Vector3 TargetDir;
        //    if (EngineControl.AAMCurrentTargetEngineControl == null)//target is a dummy target
        //    { TargetDir = EngineControl.AAMTargets[EngineControl.AAMTarget].transform.position - transform.position; }
        //    else
        //    { TargetDir = EngineControl.AAMCurrentTargetEngineControl.CenterOfMass.position - transform.position; }
        //    GUN_TargetDirOld = Vector3.Lerp(GUN_TargetDirOld, TargetDir, .2f);
	//
        //    Vector3 RelativeTargetVel = TargetDir - GUN_TargetDirOld;
        //    float BulletPlusPlaneSpeed = (EngineControl.CurrentVel + (VehicleTransform.forward * BulletSpeed) - (RelativeTargetVel * .25f)).magnitude;
        //   Vector3 TargetAccel = RelativeTargetVel - RelativeTargetVelLastFrame;
        //    //GUN_TargetDirOld is around 4 frames worth of distance behind a moving target (lerped by .2) in order to smooth out the calculation for unsmooth netcode
        //    //multiplying the result by .25(to get back to 1 frames worth) seems to actually give an accurate enough result to use in prediction
        //    GUN_TargetSpeedLerper = Mathf.Lerp(GUN_TargetSpeedLerper, (RelativeTargetVel.magnitude * .25f) / SmoothDeltaTime, 15 * SmoothDeltaTime);
        //    float BulletHitTime = TargetDir.magnitude / BulletPlusPlaneSpeed;
        //    //normalize lerped relative target velocity vector and multiply by lerped speed
        //   Vector3 RelTargVelNormalized = RelativeTargetVel.normalized;
        //    Vector3 PredictedPos = (TargetDir
        //        + ((RelTargVelNormalized * GUN_TargetSpeedLerper)/* Linear */
        //            //the .125 in the next line is combined .25 for undoing the lerp, and .5 for the acceleration formula
        //            + (TargetAccel * .125f * BulletHitTime)
        //                + new Vector3(0, 9.81f * .5f * BulletHitTime, 0))//Bulletdrop
        //                    * BulletHitTime);
        //    GUNLeadIndicator.position = transform.position + PredictedPos;
        //    GUNLeadIndicator.localPosition = GUNLeadIndicator.localPosition.normalized * distance_from_head;
        //    RelativeTargetVelLastFrame = RelativeTargetVel;
        //}
        //else GUNLeadIndicator.gameObject.SetActive(false);
        /////////////////

        //Smoke Color Indicator
        // SmokeColorIndicator.color = EngineControl.SmokeColor_Color;

        //Heading indicator
        Vector3 VehicleEuler = EngineControl.VehicleMainObj.transform.rotation.eulerAngles;
        HeadingIndicator.localRotation = Quaternion.Euler(new Vector3(0, -VehicleEuler.y, 0));
        /////////////////

	//Elevation indicator
        ElevationIndicator.rotation = Quaternion.Euler(new Vector3(0, VehicleEuler.y, 0));
        /////////////////

        //Down indicator
        DownIndicator.localRotation = Quaternion.Euler(new Vector3(0, 0, -VehicleEuler.z));
        /////////////////

        //LIMITS indicator
        if (EngineControl.FlightLimitsEnabled)
        {
            HudLimit.SetActive(true);
        }
        else { HudLimit.SetActive(false); }
        //Alt. HOLD indicator
        if (EngineControl.AltHold)
        {
            HudHold.SetActive(true);
        }
        else { HudHold.SetActive(false); }

        //Left Stick Selector
        // switch (EngineControl.LStickSelection)
        // {
        //     case 0:
        //         // LStickDisplayHighlighter.localRotation = Quaternion.Euler(0, 0, 180);//invisible, backfacing
        //         break;
        //     case 1:
        //         // LStickDisplayHighlighter.localRotation = Quaternion.Euler(0, 0, 0);
        //         break;
        //     case 2:
        //         // LStickDisplayHighlighter.localRotation = Quaternion.Euler(0, 45, 0);
        //         break;
        //     case 3:
        //         // LStickDisplayHighlighter.localRotation = Quaternion.Euler(0, 90, 0);
        //         break;
        //     case 4:
        //         // LStickDisplayHighlighter.localRotation = Quaternion.Euler(0, 135, 0);
        //         break;
        //     case 5:
        //         // LStickDisplayHighlighter.localRotation = Quaternion.Euler(0, 180, 0);
        //         break;
        //     case 6:
        //         // LStickDisplayHighlighter.localRotation = Quaternion.Euler(0, 225, 0);
        //         break;
        //     case 7:
        //         // LStickDisplayHighlighter.localRotation = Quaternion.Euler(0, 270, 0);
        //         break;
        //     case 8:
        //         // LStickDisplayHighlighter.localRotation = Quaternion.Euler(0, 315, 0);
        //         break;
        // }

        // //Right Stick Selector
        // switch (EngineControl.RStickSelection)
        // {
        //     case 0:
        //         // RStickDisplayHighlighter.localRotation = Quaternion.Euler(0, 0, 180);//invisible, backfacing
        //         break;
        //     case 1:
        //         // RStickDisplayHighlighter.localRotation = Quaternion.Euler(0, 0, 0);
        //         break;
        //     case 2:
        //         // RStickDisplayHighlighter.localRotation = Quaternion.Euler(0, 45, 0);
        //         break;
        //     case 3:
        //         // RStickDisplayHighlighter.localRotation = Quaternion.Euler(0, 90, 0);
        //         break;
        //     case 4:
        //         // RStickDisplayHighlighter.localRotation = Quaternion.Euler(0, 135, 0);
        //         break;
        //     case 5:
        //         // RStickDisplayHighlighter.localRotation = Quaternion.Euler(0, 180, 0);
        //         break;
        //     case 6:
        //         // RStickDisplayHighlighter.localRotation = Quaternion.Euler(0, 225, 0);
        //         break;
        //     case 7:
        //         // RStickDisplayHighlighter.localRotation = Quaternion.Euler(0, 270, 0);
        //         break;
        //     case 8:
        //         // RStickDisplayHighlighter.localRotation = Quaternion.Euler(0, 315, 0);
        //         break;
        // }


        //AB
        if (HudAB != null)
        {
            if (EngineControl.EffectsControl.AfterburnerOn) { HudAB.SetActive(true); }
            else { HudAB.SetActive(false); }
        }

        //Cruise Control target knots
        if (EngineControl.Cruise)
        {
            HUDText_knotstarget.text = ((EngineControl.SetSpeed) * 1.9438445f).ToString("F0");
        }
        else { HUDText_knotstarget.text = string.Empty; }

        //left stick toggles/functions on?
	//if (EngineControl.VTOLAngle > 0) { LStick_funcon1.SetActive(true); }
        //else { LStick_funcon1.SetActive(false); }

        // if (EngineControl.FlightLimitsEnabled) { LStick_funcon2.SetActive(true); }
        // else { LStick_funcon2.SetActive(false); }

        // if (EngineControl.CatapultStatus == 1) { LStick_funcon4.SetActive(true); }
        // else { LStick_funcon4.SetActive(false); }

        // if (EngineControl.AltHold) { LStick_funcon6.SetActive(true); }
        // else { LStick_funcon6.SetActive(false); }

        // /*         if (EngineControl.Trim.x != 0) { LStick_funcon6.SetActive(true); }
        // else { LStick_funcon6.SetActive(false); } */

        // if (EngineControl.EffectsControl.CanopyOpen) { LStick_funcon7.SetActive(true); }
        // else { LStick_funcon7.SetActive(false); }

        // if (EngineControl.Cruise) { LStick_funcon8.SetActive(true); }
        // else { LStick_funcon8.SetActive(false); }


        //right stick toggles/functions on?
        // if (EngineControl.AGMLocked) { RStick_funcon3.SetActive(true); }
        // else { RStick_funcon3.SetActive(false); }

        // if (!EngineControl.EffectsControl.GearUp) { RStick_funcon5.SetActive(true); }
        // else { RStick_funcon5.SetActive(false); }

        // if (EngineControl.EffectsControl.Flaps) { RStick_funcon6.SetActive(true); }
        // else { RStick_funcon6.SetActive(false); }

        // if (EngineControl.EffectsControl.HookDown) { RStick_funcon7.SetActive(true); }
        // else { RStick_funcon7.SetActive(false); }

        // if (EngineControl.EffectsControl.Smoking) { RStick_funcon8.SetActive(true); }
        // else { RStick_funcon8.SetActive(false); }

        //play menu sound if selection changed since last frame
        float MenuSoundCheck = EngineControl.RStickSelection + EngineControl.LStickSelection;
        if (!EngineControl.SoundControl.MenuSelectNull && MenuSoundCheck != MenuSoundCheckLast)
        {
            EngineControl.SoundControl.MenuSelect.Play();
        }
        MenuSoundCheckLast = MenuSoundCheck;

        //AGMScreen
        // if (EngineControl.RStickSelection == 3 && !EngineControl.AGMLocked)
        // {
        //     if (!EngineControl.AtGCamNull)
        //     {
        //         AtGScreen.SetActive(true);
        //         EngineControl.AtGCam.gameObject.SetActive(true);
        //         float newzoom = 0;
        //         //if turning camera fast, zoom out
        //         if (EngineControl.AGMRotDif < .8f)
        //         {
        //             RaycastHit camhit;
        //             Physics.Raycast(EngineControl.AtGCam.transform.position, EngineControl.AtGCam.transform.forward, out camhit, Mathf.Infinity, 1);
        //             if (camhit.point != null)
        //             {
        //                 //dolly zoom //Mathf.Atan(100 <--the 100 is the height of the camera frustrum at the target distance
        //                 newzoom = Mathf.Clamp(2.0f * Mathf.Atan(100 * 0.5f / Vector3.Distance(gameObject.transform.position, camhit.point)) * Mathf.Rad2Deg, 1.5f, 90);
        //                 EngineControl.AtGCam.fieldOfView = Mathf.Clamp(Mathf.Lerp(EngineControl.AtGCam.fieldOfView, newzoom, 1.5f * DeltaTime), 0.3f, 90);
        //             }
        //         }
        //         else
        //         {
        //             newzoom = 80;
        //             EngineControl.AtGCam.fieldOfView = Mathf.Clamp(Mathf.Lerp(EngineControl.AtGCam.fieldOfView, newzoom, 3.5f * DeltaTime), 0.3f, 90); //zooming in is a bit slower than zooming out                       
        //         }
        //     }
        // }
        if (EngineControl.RStickSelection == 4)
        {
            //     // if (!EngineControl.AtGCamNull)
            //     // {
            //     //     EngineControl.AtGCam.gameObject.SetActive(true);
            //     //     AtGScreen.SetActive(true);
            //     //     EngineControl.AtGCam.fieldOfView = 60;
            //     //     EngineControl.AtGCam.transform.localRotation = Quaternion.Euler(110, 0, 0);
            //     // }
            // }
            // else if (EngineControl.AGMLocked)
            // {
            //     // if (!EngineControl.AtGCamNull)
            //     // {
            //     //     EngineControl.AtGCam.gameObject.SetActive(true);
            //     //     EngineControl.AtGCam.transform.LookAt(EngineControl.AGMTarget, EngineControl.VehicleMainObj.transform.up);
            //     // }
            //     // RaycastHit camhit;
            //     // Physics.Raycast(EngineControl.AtGCam.transform.position, EngineControl.AtGCam.transform.forward, out camhit, Mathf.Infinity, 1);
            //     // if (camhit.point != null)
            //     // {
            //     //     //dolly zoom //Mathf.Atan(40 <--the 40 is the height of the camera frustrum at the target distance
            //     //     EngineControl.AtGCam.fieldOfView = Mathf.Max(Mathf.Lerp(EngineControl.AtGCam.fieldOfView, 2.0f * Mathf.Atan(60 * 0.5f / Vector3.Distance(gameObject.transform.position, camhit.point)) * Mathf.Rad2Deg, 5 * DeltaTime), 0.3f);
            //     // }
            // }
            // else
            // {
            //     if (!EngineControl.AtGCamNull)
            //     {
            //         AtGScreen.SetActive(false);
            //         EngineControl.AtGCam.gameObject.SetActive(false);
            //     }
        }



        //updating numbers 3~ times a second
        if (check > .3)//update text
        {
            if (EngineControl.Gs > maxGs) { maxGs = EngineControl.Gs; }
            HUDText_G.text = string.Concat(EngineControl.Gs.ToString("F1"), "\n", maxGs.ToString("F1"));
            HUDText_mach.text = ((EngineControl.Speed) / 343f).ToString("F2");
            HUDText_altitude.text = string.Concat((EngineControl.CurrentVel.y * 60 * 3.28084f).ToString("F0"), "\n", ((EngineControl.CenterOfMass.position.y + -EngineControl.SeaLevel) * 3.28084f).ToString("F0"));
            HUDText_knots.text = ((EngineControl.Speed) * 1.9438445f).ToString("F0");
            HUDText_knotsairspeed.text = ((EngineControl.AirSpeed) * 1.9438445f).ToString("F0");

            if (EngineControl.Speed < 2)
            {
                HUDText_angleofattack.text = System.String.Empty;
            }
            else
            {
                HUDText_angleofattack.text = EngineControl.AngleOfAttack.ToString("F0");
            }
            check = 0;
        }
        check += SmoothDeltaTime;

        // if (EngineControl.HasAAM) HUDText_AAM_ammo.text = EngineControl.NumAAM.ToString("F0");
        // else HUDText_AAM_ammo.text = string.Empty;
        // if (EngineControl.HasAGM) HUDText_AGM_ammo.text = EngineControl.NumAGM.ToString("F0");
        // else HUDText_AGM_ammo.text = string.Empty;
        // if (EngineControl.HasBomb) HUDText_Bomb_ammo.text = EngineControl.NumBomb.ToString("F0");
        // else HUDText_Bomb_ammo.text = string.Empty;

        PlaneAnimator.SetFloat("fuel", EngineControl.Fuel * FullFuelDivider);
        // PlaneAnimator.SetFloat("gunammo", EngineControl.GunAmmoInSeconds * FullGunAmmoDivider);
    }
    private void Assert(bool condition, string message)
    {
        if (!condition)
        {
            Debug.LogError("Assertion failed : '" + GetType() + " : " + message + "'", this);
        }
    }
}

using UnityEngine;
using System.Collections;
using VRC.SDKBase;
using VRC.Udon;
using UdonSharp;
using UnityEngine.UI;

public class RadarRender : UdonSharpBehaviour
{

    public MissileTargets RadarObjects;
    public Transform ReferenceArea;
    public float range;
    public float maxRange;
    public bool use3d = false;

    public float scale = 0.5f;
    public float scaleStep = 0.1f;
    public RadarObject[] List;
    public LayerMask LayerDetection;

    public RadarObject Fab;

    public int counter = 0;
    public int maxLength = 0;

    public float TimeToFetch = 1f;
    public float TimerToFetch = 0f;

    public int counterList = 0;

    public bool isAssigning = false;
    public WeaponSelector wp;
    public MissileScript ToAssignMissile;
    public MissileTrackerAndResponse myTracker;
    public MissileTrackerAndResponse ToAssignTracker;
    public MissileTrackerAndResponse parsing;
    public TarpsArea ToAssignTarpsArea;
    public MissileScript parsingMis;
    public TarpsArea parsingTarp;
    public Transform spawnRadarObjects;
    public bool found = false;
    public RaycastHit[] hits;
    public bool reset = false;
    public bool waitForCallback = false;
    public bool renderIcons = false;
    public bool ForceTransform = false;
    public bool parsemode = false;
    public bool removing = false;
    public bool ignoreWP = true;
    public bool UseOnBoardText = false;
    public bool useOnlyOne = true;
    public Text OnBoardText;
    public Text[] ScaleTexts;
    public Text[] HalfScaleTexts;
    public Text[] FourthScaleTexts;
    public bool inverse = false;
    public bool RotateRadar = false;
    public Transform RadarBase;

    public float IconScale = 1f;
    public float IconScaleStep = 0.1f;

    public Text iconScaleText;
    private float initScale;
    private float initRange;
    private Vector3 initIconScale;
    public float WaitForCallBackTimer = 4f;
    private float WaitForCallBackTimer_current = 0f;


    void Start()
    {
        if (ScaleTexts != null && ScaleTexts.Length > 0)
        {
            foreach (Text g in ScaleTexts)
            {
                g.text = "" + range.ToString("F0") + "m";
            }
        }

        if (HalfScaleTexts != null && HalfScaleTexts.Length > 0)
        {
            foreach (Text g in HalfScaleTexts)
            {
                g.text = "" + (range / 2).ToString("F0") + "m";
            }
        }

        if (FourthScaleTexts != null && FourthScaleTexts.Length > 0)
        {
            foreach (Text g in FourthScaleTexts)
            {
                g.text = "" + (range / 4).ToString("F0") + "m";
            }
        }

        initRange = range;
        initScale = scale;

        // maxLength = RadarObjects.Targets.Length;
    }

    public void AddScale()
    {
        if (initRange / (scale / initScale) > maxRange)
        {
            return;
        }
        scale = scale + scaleStep;
        range = initRange / (scale / initScale);
        if (ScaleTexts != null && ScaleTexts.Length > 0)
        {
            foreach (Text g in ScaleTexts)
            {
                g.text = "" + range.ToString("F0") + "m";
            }
        }

        if (HalfScaleTexts != null && HalfScaleTexts.Length > 0)
        {
            foreach (Text g in HalfScaleTexts)
            {
                g.text = "" + (range / 2).ToString("F0") + "m";
            }
        }

        if (FourthScaleTexts != null && FourthScaleTexts.Length > 0)
        {
            foreach (Text g in FourthScaleTexts)
            {
                g.text = "" + (range / 4).ToString("F0") + "m";
            }
        }
    }

    public void DecreaseScale()
    {
        if (initRange / (scale / initScale) < 0)
        {
            return;
        }
        scale = scale - scaleStep;
        range = initRange / (scale / initScale);
        if (ScaleTexts != null && ScaleTexts.Length > 0)
        {
            foreach (Text g in ScaleTexts)
            {
                g.text = "" + range.ToString("F0") + "m";
            }
        }

        if (HalfScaleTexts != null && HalfScaleTexts.Length > 0)
        {
            foreach (Text g in HalfScaleTexts)
            {
                g.text = "" + (range / 2).ToString("F0") + "m";
            }
        }

        if (FourthScaleTexts != null && FourthScaleTexts.Length > 0)
        {
            foreach (Text g in FourthScaleTexts)
            {
                g.text = "" + (range / 4).ToString("F0") + "m";
            }
        }
    }

    public void AddIconScale()
    {
        IconScale = IconScale + IconScaleStep;
        iconScaleText.text = IconScale + "";
    }

    public void DecreaseIconScale()
    {
        IconScale = IconScale - IconScaleStep;
        iconScaleText.text = IconScale + "";
    }

    void Update()
    {
        if (RotateRadar)
        {
            rotatePos();
        }
        if (waitForCallback)
        {
            if (WaitForCallBackTimer_current > WaitForCallBackTimer)
            {
                WaitForCallBackTimer_current = 0;
                waitForCallback = false;
            }
            else
            {
                WaitForCallBackTimer_current = WaitForCallBackTimer_current + Time.deltaTime;
                return;
            }
        }
        if (!reset)
        {
            doTimer();
        }
        else
        {
            ParseRadarObjects();
            if (!parsemode)
                doCounter();
        }

    }

    public void rotatePos()
    {
        if (RadarBase != null)
        {
            Vector3 x = new Vector3(0, ReferenceArea.rotation.eulerAngles.y, 0);
            RadarBase.localRotation = !inverse ? Quaternion.Euler(x) : Quaternion.Inverse((Quaternion.Euler(x)));
        }
    }

    public void doCounter()
    {
        if (counter + 1 < maxLength)
        {
            counter = counter + 1;
        }
        else
        {
            counter = 0;
            reset = false;
            found = false;
        }
    }

    public void doTimer()
    {
        if (TimerToFetch + 1 < TimeToFetch)
        {
            TimerToFetch = TimerToFetch + 1;
        }
        else
        {
            FetchRadarObjects();
            TimerToFetch = 0;
        }
    }

    // public void ForceDeleteRadarObjects()
    // {
    //     parsing = null;
    //     parsingMis = null;
    //     parsingTarp = null;
    //     parsemode = false;
    //     counterList = 0;
    //     List = null;
    //     int x = spawnRadarObjects.transform.childCount;
    //     for (int y = 0; y < x; y++)
    //     {
    //         Utilities.IsValid(Destroy(spawnRadarObjects.transform.GetChild(y).gameObject));
    //     }
    // }

    public void AddRadarObject(MissileTrackerAndResponse tg, MissileScript ms, TarpsArea ts)
    {
        Debug.Log("Attempting to add");

        var runtime2 = VRCInstantiate(Fab.gameObject);
        runtime2.transform.SetParent(spawnRadarObjects);
        runtime2.transform.position = spawnRadarObjects.position;
        runtime2.transform.localScale = Fab.transform.localScale;

        waitForCallback = true;

        ToAssignTracker = tg;

        ToAssignMissile = ms;
        ToAssignTarpsArea = ts;


        runtime2.SetActive(true);

    }

    void ParseRadarObjects()
    {
        if (counter >= hits.Length)
        {
            reset = false;
            parsing = null;
            parsingMis = null;
            parsingTarp = null;
            parsemode = false;
            return;
        }

        if (!Utilities.IsValid(parsing) && !Utilities.IsValid(parsingMis) && !Utilities.IsValid(parsingTarp))
        {
            var g = hits[counter];
            if (g.transform == null)
            {
                return;
            }
            GameObject bb = g.transform.gameObject;
            var MTR = bb.GetComponent<MissileTrackerAndResponse>();
            var MIS = bb.GetComponent<MissileScript>();
            var TARP = ignoreWP ? bb.GetComponent<TarpsArea>() : (wp != null && wp.tarps != null && wp.tarps.isSelected ? bb.GetComponent<TarpsArea>() : null);
            if ((MTR == null && MIS == null && TARP == null) || (MTR != null && MTR == myTracker))
            {
                return;
            }

            if (MTR != null && MTR.isRendered)
            {
                if (
                    (MTR != myTracker) &&
                    ((MTR.AI != null && !MTR.AI.dead) ||
                    (MTR.AI == null) ||
                    (MTR.EngineController != null && MTR.EngineController.Health > 0))
                    )
                {
                    parsing = MTR;
                    parsemode = true;
                }

            }
            else if (MIS != null && !MIS.isExploded)
            {
                parsingMis = MIS;
                parsemode = true;
            }

            else if (TARP != null && TARP.belongsTo != null && TARP.belongsTo.isEnabed)
            {
                parsingTarp = TARP;
                parsemode = true;
            }

        }
        else if (parsing != null && parsingMis == null && parsingTarp == null)
        {
            // Debug.Log("ppaparse");

            if (counterList < List.Length && List[counterList].Tracking == parsing)
            {
                parsemode = false;
                parsing = null;
                // Debug.Log("found");
                found = true;
            }

            if (counterList >= List.Length && !found)
            {
                // Debug.Log("Attempt to add");
                AddRadarObject(parsing, null, null);
                parsing = null;
                counterList = 0;
                parsemode = false;
            }

            if (counterList < List.Length && !found)
            {
                // Debug.Log("counter");
                counterList = counterList + 1;
            }

            if (found)
            {
                counterList = 0;
                found = false;
                parsemode = false;
            }

        }
        else if (parsing == null && parsingMis != null && parsingTarp == null)
        {

            if (counterList < List.Length && List[counterList].TrackingMissile == parsingMis)
            {
                parsemode = false;
                parsingMis = null;
                Debug.Log("found");
                found = true;
            }

            if (counterList >= List.Length && !found)
            {
                Debug.Log("Attempt to add");
                AddRadarObject(null, parsingMis, null);
                parsingMis = null;
                counterList = 0;
                parsemode = false;
            }

            if (counterList < List.Length && !found)
            {
                Debug.Log("counter");
                counterList = counterList + 1;
            }

            if (found)
            {
                counterList = 0;
                found = false;
                parsemode = false;
            }

        }
        else if (parsing == null && parsingMis == null && parsingTarp != null)
        {

            if (counterList < List.Length && List[counterList].TrackingTarps == parsingTarp)
            {
                parsemode = false;
                parsingMis = null;
                parsingTarp = null;
                Debug.Log("found");
                found = true;
            }

            if (counterList >= List.Length && !found)
            {
                Debug.Log("Attempt to add");
                AddRadarObject(null, null, parsingTarp);
                parsingTarp = null;
                counterList = 0;
                parsemode = false;
            }

            if (counterList < List.Length && !found)
            {
                Debug.Log("counter");
                counterList = counterList + 1;
            }

            if (found)
            {
                counterList = 0;
                found = false;
                parsemode = false;
            }

        }

    }

    void FetchRadarObjects()
    {
        hits = Physics.SphereCastAll(ReferenceArea.position, range, ReferenceArea.up, range, LayerDetection, QueryTriggerInteraction.Collide);
        maxLength = hits.Length;

        if (hits.Length == 0)
        {
            reset = false;
            counter = 0;
            TimerToFetch = 0;
        }
        else
        {
            reset = true;
        }
        parsing = null;
        parsingMis = null;
        parsingTarp = null;
    }

    public void Assigned(RadarObject bb)
    {
        isAssigning = false;
        RadarObject[] temp = new RadarObject[List.Length + 1];
        List.CopyTo(temp, 0);

        temp[temp.Length - 1] = bb;
        List = temp;
        Debug.Log("AAdded");
        waitForCallback = false;
        WaitForCallBackTimer_current = 0f;
        ToAssignTracker = null;
        ToAssignMissile = null;
        ToAssignTarpsArea = null;
    }

    public void CallBackReceived()
    {
        waitForCallback = false;
        WaitForCallBackTimer_current = 0f;
    }

    public void Remove(RadarObject bb)
    {
        parsing = null;
        parsingMis = null;
        parsingTarp = null;
        parsemode = false;

        waitForCallback = true;
        int index = 0;
        if (List.Length == 0)
        {
            return;
        }
        for (int x = 0; x < List.Length; x++)
        {
            if (List[x] != null && List[x] == bb)
            {
                index = x;
                found = true;
            }
        }
        if (found)
        {
            bb.toDestroy();
            waitForCallback = false;
            WaitForCallBackTimer_current = 0f;
            if (List.Length - 1 > 0)
            {
                RadarObject[] temp = new RadarObject[List.Length - 1];
                // Debug.Log("B"); 
                int y = 0;
                for (int x = 0; x < List.Length; x++)
                {
                    if (x != index)
                    {
                        temp[y] = List[x];
                        y = y + 1;
                    }
                }
                // Debug.Log("C");
                List = temp;
                // Debug.Log("DONE");
            }
            else
            {
                List = new RadarObject[0];
                counterList = 0;
            }
        }
    }

}

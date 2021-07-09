using UnityEngine;
using System.Collections;
using VRC.SDKBase;
using VRC.Udon;
using UdonSharp;

public class RadarRender : UdonSharpBehaviour
{

    public MissileTargets RadarObjects;
    public Transform ReferenceArea;
    public float range;
    public bool use3d = false;

    public float scale = 0.5f;
    public RadarObject[] List;
    public LayerMask LayerDetection;

    public RadarObject Fab;

    public int counter = 0;
    public int maxLength = 0;

    public float TimeToFetch = 1f;
    public float TimerToFetch = 0f;

    public int counterList = 0;

    public bool isAssigning = false;
    public MissileScript ToAssignMissile;
    public MissileTrackerAndResponse myTracker;
    public MissileTrackerAndResponse ToAssignTracker;

    public MissileTrackerAndResponse parsing;
    public MissileScript parsingMis;
    public Transform spawnRadarObjects;
    public bool found = false;
    public RaycastHit[] hits;
    public bool reset = false;
    public bool waitForCallback = false;

    public bool parsemode = false;
    public bool removing = false;
    void Start()
    {
        // maxLength = RadarObjects.Targets.Length;
    }

    void Update()
    {
        if (waitForCallback)
        {
            return;
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

    public void ForceDeleteRadarObjects()
    {
        foreach (RadarObject l in List)
        {
            l.toDestroy();
        }
        List = null;

    }

    public void AddRadarObject(MissileTrackerAndResponse tg, MissileScript ms)
    {
        Debug.Log("Attempting to add");

        var runtime2 = VRCInstantiate(Fab.gameObject);
        runtime2.transform.SetParent(spawnRadarObjects);
        runtime2.transform.position = spawnRadarObjects.position;
        runtime2.transform.localScale = Fab.transform.localScale;

        waitForCallback = true;

        ToAssignTracker = tg;

        ToAssignMissile = ms;
        runtime2.SetActive(true);

    }

    void ParseRadarObjects()
    {
        if (counter >= hits.Length)
        {
            reset = false;
            parsing = null;
            parsingMis = null;
            parsemode = false;
            return;
        }

        if (!Utilities.IsValid(parsing) && !Utilities.IsValid(parsingMis))
        {
            var g = hits[counter];
            if (g.transform == null)
            {
                return;
            }
            GameObject bb = g.transform.gameObject;
            var MTR = bb.GetComponent<MissileTrackerAndResponse>();
            var MIS = bb.GetComponent<MissileScript>();
            if (MTR == null && MIS == null)
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

        }
        else if (parsing != null && parsingMis == null)
        {
            Debug.Log("ppaparse");
            // if (List.Length == 0)
            // {
            //     AddRadarObject(parsing, null);
            //     parsing = null;
            //     counterList = 0;
            // }
            if (counterList < List.Length && List[counterList].Tracking == parsing)
            {
                parsemode = false;
                parsing = null;
                Debug.Log("found");
                found = true;
            }

            if (counterList >= List.Length && !found)
            {
                Debug.Log("Attempt to add");
                AddRadarObject(parsing, null);
                parsing = null;
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
        else if (parsing == null && parsingMis != null)
        {
            Debug.Log("ppaparse");
            // if (List.Length == 0)
            // {
            //     AddRadarObject(parsing, null);
            //     parsing = null;
            //     counterList = 0;
            // }
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
                AddRadarObject(null, parsingMis);
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
        // else if (parsingMis != null && parsing == null)
        // {
        //     if (counterList < List.Length && !found)
        //     {
        //         found = List[counterList].TrackingMissile == parsingMis;
        //     }
        //     else if (counterList >= List.Length && !found)
        //     {
        //         AddRadarObject(null, parsingMis);
        //         parsingMis = null;
        //         counterList = 0;
        //     }
        //     else if (found)
        //     {
        //         parsingMis = null;
        //         counterList = 0;
        //     }
        //     if (List.Length == 0)
        //     {
        //         AddRadarObject(null, parsingMis);
        //         parsingMis = null;
        //         counterList = 0;
        //     }
        // }

    }

    void FetchRadarObjects()
    {
        hits = Physics.SphereCastAll(ReferenceArea.position, range, ReferenceArea.up, range, LayerDetection, QueryTriggerInteraction.UseGlobal);
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
        ToAssignTracker = null;
        ToAssignMissile = null;
    }

    public void CallBackReceived()
    {
        waitForCallback = false;
    }

    public void Remove(RadarObject bb)
    {
        parsing = null;
        parsingMis = null;
        parsemode = false;
        // removing = true;

        // if(parsing == bb.Tracking){
        //     parsing = null;
        // }else if(parsingMis == bb.TrackingMissile){
        //     parsingMis = null;
        // }
        waitForCallback = true;
        for (int x = 0; x < List.Length; x++)
        {
            if (List[x] != null && List[x] == bb)
            {
                found = true;
            }
        }
        if (found)
        {
            RadarObject[] temp = new RadarObject[List.Length - 1];
            // Debug.Log("B"); 
            int y = 0;
            for (int x = 0; x < List.Length; x++)
            {
                // Debug.Log("SCAN");
                if (!removing)
                {
                    if (x < List.Length && List[x] != bb)
                    {
                        removing = true;
                        temp[y] = List[x];
                        y = y + 1;
                        removing=false;
                        // Debug.Log("INCREMENT");
                    }
                    else if (x < List.Length && List[x] == bb)
                    {
                        List[x].toDestroy();
                        // Destroy(Indicators[x]);
                        Debug.Log("DESTROY");
                    }
                    else
                    {
                        Debug.Log("Array error");
                        ForceDeleteRadarObjects();
                    }
                }
            }
            // Debug.Log("C");
            List = temp;
            // Debug.Log("DONE");
        }
    }

}

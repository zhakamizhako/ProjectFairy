
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class TarpsArea : UdonSharpBehaviour
{
    public TarpsTarget belongsTo;
    public GameObject CheckedA;
    public bool isNear = false;
    public bool isFar = false;
    // public string type = "close"; //close, far

    void OnTriggerEnter(Collider c)
    {
        CheckedA = c.gameObject;
        MissileTrackerAndResponse parent = HandleChecker(c);
        if(parent==null){
            return;
        }

        Debug.Log("MTR Present");
        if(parent.tarps!=null){
            parent.tarps.HandleEnter(this);
        }
        // if(parent)
    }

    // void OnTriggerStay(Collider c)
    // {
    //       MissileTrackerAndResponse parent = HandleChecker(c);
    //     if(parent==null){
    //         return;
    //     }
    //      if(parent.tarps!=null){
    //         parent.tarps.HandleStay(belongsTo);
    //     }
    // }

    void OnTriggerExit(Collider c)
    {
          MissileTrackerAndResponse parent = HandleChecker(c);
        if(parent==null){
            return;
        }
         if(parent.tarps!=null){
            parent.tarps.HandleExit(this);
        }
    }

    private MissileTrackerAndResponse HandleChecker (Collider c){
        Debug.Log("HandleChecker Called");
        HitDetector check = c.gameObject.GetComponent<HitDetector>();
        if(check!=null && check.Tracker!=null){
            return (MissileTrackerAndResponse) check.Tracker;
        }
        Debug.Log("Missing MTR");
            return null;
    }
}

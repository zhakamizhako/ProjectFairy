using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class RestartTriggerScripts : UdonSharpBehaviour {
    public TriggerScript[] TriggerScripts;
    public ConditionalTriggerScript[] ConditionalTriggers;
    public AIObject[] AIs;
    public bool ran = false;
    public bool run = false;
    public bool runInSync = false;
    [UdonSynced (UdonSyncMode.None)] public bool ranSync = false;
    public TriggerScript onReset;
    public MissileTrackerAndResponse[] WaypointsToShow;
    void Start () {

    }

    public void Interact(){
        run = true;
    }

    void Update () {
        if (run && runInSync == false && ran == false) { // If global event..
            ran = true;
            // runScript ();
        } else if (run && runInSync && ran == false) { // If LocalPlayer event only...
            ranSync = true;
            // ran = true;
            // SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "runScript");
        }

        if(ran || ranSync){
            foreach(AIObject g in AIs){
                g.revive = true;
            }
            foreach(TriggerScript c in TriggerScripts){
                c.ran = false;
                c.ranSync = false;
                c.stopped = false;
                c.currentX = 0;
                for(int x=0;x<c.isRunning.Length;x++){
                    c.isRunning[x] = false;
                }
                foreach(ConditionalTriggerScript z in ConditionalTriggers){
                    z.disabled = false;
                }
            }
            foreach(MissileTrackerAndResponse m in WaypointsToShow){
                m.isRendered = true;
            }
            run = false;
            if(onReset!=null){
                onReset.run = true;
            }
            ran = false;
            ranSync = false;
        }
    }
}
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
    // [UdonSynced (UdonSyncMode.None)] public bool ranSync = false;
    public TriggerScript onReset;
    public MissileTrackerAndResponse[] WaypointsToShow;
    void Start () {

    }

    void runSync () {
        ran = true;
    }
    public void Interact () {
        run = true;
    }

    void Update () {
        if (run && !runInSync && ran == false) { // If LocalPlayer event only...
            ran = true;
            // runScript ();
        } else if (run && runInSync && ran == false) { // If global event.. 
            // ranSync = true;
            // ran = true;
            SendCustomNetworkEvent (VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "runSync");
            ran = true;
        }

        if (ran) {
            foreach (AIObject g in AIs) {
                g.revive = true;
            }
            foreach (MissileTrackerAndResponse m in WaypointsToShow) {
                m.isRendered = true;
                m.isWaypointEnabled = true;
            }
            foreach (ConditionalTriggerScript z in ConditionalTriggers) {
                z.disabled = false;
            }
            run = false;
            if (onReset != null) {
                onReset.run = true;
            }
            ran = false;
            // ranSync = false;
            foreach (TriggerScript c in TriggerScripts) {
                c.ran = false;
                // c.ranSync = false;
                c.stopped = false;
                c.currentX = 0;
                for (int x = 0; x < c.isRunning.Length; x++) {
                    c.isRunning[x] = false;
                }
            }
        }
    }
}
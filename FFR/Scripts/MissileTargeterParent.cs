using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

//So i am a script that's responsible to assign a target with the missile launcher.
//Since we cannot assign the missile's target directly, we assign it here first then launch the hell off that missile script.
//Better since we dont have to ask GetComponent<T> and crash the script.
public class MissileTargeterParent : UdonSharpBehaviour {
    public MissileTrackerAndResponse Target;
    [UdonSynced (UdonSyncMode.None)] public bool noTarget = true;
    void Start () {

    }
}
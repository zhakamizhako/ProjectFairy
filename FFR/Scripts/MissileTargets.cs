using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class MissileTargets : UdonSharpBehaviour
{
    //Purpose of this Script is to list down the targets that you can lock on with the missile.
    //Nothing else.
    //I think there's a better solution than making an empty script but... Oh well.
    public MissileTrackerAndResponse[] Targets;
    void Start()
    {
        
    }
}

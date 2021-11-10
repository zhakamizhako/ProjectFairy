
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

public class CopilotController : UdonSharpBehaviour
{
    public EngineController EngineControl;
    public MissileTrackerAndResponse selectedTracker;
    public Text Infobox;
    public Transform TargetDetector;

    void Start()
    {
        
    }
}

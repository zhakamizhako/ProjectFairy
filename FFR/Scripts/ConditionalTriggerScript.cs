using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ConditionalTriggerScript : UdonSharpBehaviour
{
    public TriggerScript[] TriggerScriptsToFullfill;
    public AIObject[] AIObjectsToDefeat;
    public AITurretScript[] AITurretsToDefeat;
    public TriggerScript ScriptToRun;
    public bool runInSync = false;
    private int countTriggersGood = 0;
    private int countAITurretGood = 0;
    private int countAIObjectsGood = 0;
    public bool runIfTurretGood = false;
    public bool runIfTriggerGood = false;
    public bool runIfAIObjectsGood = true;
    public bool disabled = false;
    [System.NonSerializedAttribute] [HideInInspector] public VRCPlayerApi localPlayer;
    // private int countAITurretsGood;
    void Start()
    {
        localPlayer = Networking.LocalPlayer;
    }

    public void runScript()
    {
        if (ScriptToRun != null)
            ScriptToRun.run = true;
    }

    void Update()
    {
        if (!disabled)
        {
            countAIObjectsGood = 0;
            countTriggersGood = 0;
            if (runIfAIObjectsGood)
            {
                for (int x = 0; x < AIObjectsToDefeat.Length; x++)
                {
                    if (AIObjectsToDefeat[x].dead)
                    {
                        countAIObjectsGood = countAIObjectsGood + 1;
                    }
                }
                if (countAIObjectsGood == AIObjectsToDefeat.Length)
                {
                    Debug.Log("Launch Script");
                    if (runInSync && localPlayer != null)
                    {
                        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "runScript");
                    }
                    else
                    {
                        runScript();
                    }
                    disabled = true;
                }
            }
            if (runIfTriggerGood)
            {
                for (int x = 0; x < TriggerScriptsToFullfill.Length; x++)
                {
                    if (TriggerScriptsToFullfill[x].ran)
                    {
                        countTriggersGood = countTriggersGood + 1;
                    }
                }
                if (countTriggersGood >= TriggerScriptsToFullfill.Length)
                {
                    Debug.Log("Launch Script");
                    if (runInSync && localPlayer != null)
                    {
                        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "runScript");
                    }
                    else
                    {
                        runScript();
                    }
                    disabled = true;
                }
            }
            if (runIfTurretGood)
            {
                for (int x = 0; x < AITurretsToDefeat.Length; x++)
                {
                    if (AITurretsToDefeat[x].dead)
                    {
                        countTriggersGood = countTriggersGood + 1;
                    }
                }
                if (countTriggersGood >= AITurretsToDefeat.Length)
                {
                    Debug.Log("Launch Script");
                    if (runInSync && localPlayer != null)
                    {
                        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "runScript");
                    }
                    else
                    {
                        runScript();
                    }
                    disabled = true;
                }
            }
        }

    }
}
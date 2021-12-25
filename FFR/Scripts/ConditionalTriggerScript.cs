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
    public float delay = 0f;
    private float timer = 0f;
    private bool scriptRun = false;
    public PlayerUIScript UIScript;
    [System.NonSerializedAttribute] [HideInInspector] public VRCPlayerApi localPlayer;
    // private int countAITurretsGood;
    void Start()
    {
        localPlayer = Networking.LocalPlayer;
        if (UIScript == null)
        {
            UIScript = ScriptToRun.UIScript !=null ? ScriptToRun.UIScript : null;

            if (UIScript == null)
                Debug.LogError("[MISSING UI SCRIPT] WARNING! MISSING UI SCRIPT!");
        }
    }

    public void runScript()
    {
        if (ScriptToRun != null)
        {
            // ScriptToRun.run = true;
            UIScript.AddToQueueScript(ScriptToRun);

            scriptRun = false;
            timer = 0f;
        }

    }

    void Update()
    {
        if (scriptRun)
        {
            if (timer < delay)
            {
                timer += Time.deltaTime;
            }
            else
            {
                if (runInSync && localPlayer != null)
                {
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "runScript");
                }
                else
                {
                    runScript();
                }
            }
        }
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
                    scriptRun = true;
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
                    scriptRun = true;
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
                    scriptRun = true;
                    disabled = true;
                }
            }
        }

    }
}
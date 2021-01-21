using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class EventManager : UdonSharpBehaviour {
    public string[] eventsIds; //for use in sync
    public string[] eventNames; //For use in Screen monitor
    private bool[] isRunning;
    public TriggerScript[] MainTriggers;
    public GameObject[][] eventObjects;
    //1st dimension : Event #
    //2nd Dimension : the objects that belong to that event
    [UdonSynced (UdonSyncMode.None)] public string syncString;
    public Text TextBoard;
    private bool changed = false;
    private bool update = false;
    private string currentString = null;
    //So here's how it works:
    //{id}={1,0};{id}={1,0}
    //where id is the e vent id, 0(false) or 1(true) if event is active
    //eg: NV=0;BM=1;BT=0;
    //Where event BM is currently active
    public int wtf = 1;

    void Start () {
        isRunning = new bool[MainTriggers.Length];
        for (int x = 0; x < MainTriggers.Length; x++) {
            isRunning[x] = false;
        }
        foreach (GameObject[] s in eventObjects) {
            foreach (GameObject x in s) {
                x.SetActive (false);
            }
        }
    }

    public void activeCall (TriggerScript g) {
        Debug.Log (g);
        for (int x = 0; x < MainTriggers.Length; x++) {
            if (MainTriggers[x] == g) {
                isRunning[x] = true;
                updateString ();
                break;
            }
        }
        Debug.Log ("Active Call");
    }

    public void closeEventCall (string id) {
        for (int x = 0; x < MainTriggers.Length; x++) {
            string index = eventsIds[x];
            if (index == id) {
                isRunning[x] = false;
                updateString ();
                break;
            }
        }
    }

    void updateString () {
        Debug.Log ("String Update");
        string temp = "";
        for (int x = 0; x < MainTriggers.Length; x++) {
            string index = eventsIds[x];
            if (isRunning[x]) {
                index = index + "=1;";
            } else {
                index = index + "=0;";
            }
            temp = temp + index;
        }
        syncString = temp;
        changed = true;
    }

    void Update () {
        if (currentString != syncString) {
            changed = true;
        }

        if (changed) {
            string[] b = syncString.Split (';');
            string activeEvents = "";
            for (int x = 0; x < b.Length; x++) {
                string[] a = b[x].Split ('=');
                for (int bx = 0; bx < eventsIds.Length; bx++) {
                    if (a[0] == eventsIds[bx] && a[1] == "1") {
                        foreach (GameObject g in eventObjects[bx]) {
                            g.SetActive (true);
                        }
                        activeEvents = activeEvents + eventNames[bx] + "\n";
                    }
                    else if (a[0] == eventsIds[bx] && a[1] == "0") {
                        foreach (GameObject g in eventObjects[bx]) {
                            g.SetActive (false);
                        }
                        activeEvents = activeEvents + eventNames[bx] + "\n";
                    }
                }
                // if (a[0] == eventsIds) {
                //     currentTargetIndex = int.Parse (a[1]);
                // } 
            }
            TextBoard.text = activeEvents;
            changed = false;
        }
    }
}
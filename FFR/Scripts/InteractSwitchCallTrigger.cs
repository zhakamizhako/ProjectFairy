
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class InteractSwitchCallTrigger : UdonSharpBehaviour
{
    public string EventName;
    public UdonBehaviour CallBehaviour;
    public bool callGlobal = false;

    public void Interact(){
        if(CallBehaviour==null || EventName==null){
            return;
        }

        if(callGlobal){
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, EventName);
        }else{
            ExecuteEvent();
        }
    }

    public void ExecuteEvent(){
        CallBehaviour.SendCustomEvent(EventName);
    }
}

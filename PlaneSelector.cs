using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class PlaneSelector : UdonSharpBehaviour {
    public Button Next;
    public Button Previous;
    [UdonSynced (UdonSyncMode.None)] public int SelectedPlane;
    public GameObject[] Planes;
    public EngineController activePlane;
    private VRCPlayerApi localPlayer;
    void Start () {
        localPlayer = Networking.LocalPlayer;
    }

    public void NextPlane () {
        if (!activePlane.Occupied) {
            if (SelectedPlane + 1 > Planes.Length) {
                SelectedPlane = 0;
            } else {
                SelectedPlane = SelectedPlane + 1;
            }
            Networking.SetOwner(localPlayer,Planes[SelectedPlane]); 
        }
    }

    public void PrevPlane () {
        if (!activePlane.Occupied) {
            if (SelectedPlane - 1 < 0) {
                SelectedPlane = 0;
            } else {
                SelectedPlane = SelectedPlane - 1;
            }
            Networking.SetOwner(localPlayer,Planes[SelectedPlane]); 
        }
    }

    public void Update(){
        for(int x=0;x<Planes.Length;x++){
            if(x==SelectedPlane){
                Planes[x].SetActive(true);
            }else{
                Planes[x].SetActive(false);
            }
        }
    }

}
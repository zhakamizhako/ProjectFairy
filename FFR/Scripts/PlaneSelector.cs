using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class PlaneSelector : UdonSharpBehaviour
{
    // public Button Next;
    // public Button Previous;
    [UdonSynced(UdonSyncMode.None)] public int SelectedPlane;
    public AircraftList[] Planes;
    public VehicleRespawnButton activePlane;
    public Text PlaneSelectedText;
    public Text DescriptionText;
    private VRCPlayerApi localPlayer;
    private float waitTime = 10;
    private float waitingtimer = 0;
    private bool timerwait = false;
    private bool hasStarted = false;
    public bool updated = false;
    public int x = 0;
    private int y = 0;
    public int skipFrames = 3;
    private int currentFrame = 0;

    void Start()
    {
        localPlayer = Networking.LocalPlayer;
        // for (int x = 0; x < Planes.Length; x++)
        // {
        //     if (x != SelectedPlane) { Planes[x].PlaneParent.SetActive(false); }
        // }
        PlaneSelectedText.text = Planes[SelectedPlane].AircraftName;
        DescriptionText.text = Planes[SelectedPlane].VehicleDescription;
        activePlane = Planes[SelectedPlane].VehicleButton;
        x = 0;
    }

    public void NextPlane()
    {
        if (!activePlane.EngineControl.dead && (!activePlane.EngineControl.Occupied && (Networking.IsOwner(gameObject) || !timerwait)))
        {
            if (SelectedPlane + 1 < Planes.Length)
            {
                SelectedPlane = SelectedPlane + 1;
            }
            else
            {
                SelectedPlane = 0;
            }
            // Debug.Log("[PlaneSelector]:" + SelectedPlane);
            Networking.SetOwner(localPlayer, Planes[SelectedPlane].PlaneParent);
            activePlane = Planes[SelectedPlane].VehicleButton;
            activePlane.RespawnPlane();
            Networking.SetOwner(localPlayer, gameObject);
        }
    }

    private void Interact()
    {
        activePlane.RespawnPlane();
    }

    public void PrevPlane()
    {
        if (!activePlane.EngineControl.dead && (!activePlane.EngineControl.Occupied && (Networking.IsOwner(gameObject) || !timerwait)))
        {
            if (SelectedPlane - 1 > -1)
            {
                SelectedPlane = SelectedPlane - 1;
            }
            else
            {
                SelectedPlane = Planes.Length - 1;
            }
            // Debug.Log("[PlaneSelector]:" + SelectedPlane);
            Networking.SetOwner(localPlayer, Planes[SelectedPlane].PlaneParent);
            activePlane = Planes[SelectedPlane].VehicleButton;
            activePlane.RespawnPlane();
            Networking.SetOwner(localPlayer, gameObject);
        }
    }

    public void Update()
    {
        if (hasStarted)
        {
            // for(int x=0;x<Planes.Length;x++){
            if (timerwait)
            {
                waitingtimer = waitingtimer + Time.deltaTime;
                if (waitingtimer > waitTime)
                {
                    timerwait = false;
                }
            }

            if (!Planes[SelectedPlane].PlaneParent.activeSelf)
            {
                Planes[SelectedPlane].PlaneParent.SetActive(true);
                PlaneSelectedText.text = Planes[SelectedPlane].AircraftName;
                DescriptionText.text = Planes[SelectedPlane].VehicleDescription != null ? Planes[SelectedPlane].VehicleDescription : "No Information";
                activePlane = Planes[SelectedPlane].VehicleButton;
                timerwait = true;
                waitingtimer = 0;

                for (int x = 0; x < Planes.Length; x++)
                {
                    if (x != SelectedPlane) { Planes[x].PlaneParent.SetActive(false); }
                }
                // updated = false;
                // x = 0;
            }

        }
        else //If it hasn't started yet..?
        {
            if (currentFrame < skipFrames) //frame counter just to make sure that the rest of the planes has initialized. 
            {
                currentFrame = currentFrame + 1;
            }
            else
            {
                if (x < Planes.Length)
                {
                    if (x != SelectedPlane)
                    {
                        if (Planes[x].EngineController.ScriptHasStarted)
                        {
                            Planes[x].PlaneParent.SetActive(false);
                            x = x + 1;
                        }
                    }
                    else
                    {
                        x = x + 1;
                    }
                }
                else
                {
                    hasStarted = true;
                    x = 0;
                    if(Networking.IsOwner(Planes[SelectedPlane].EngineController.gameObject)){
                        Planes[SelectedPlane].VehicleButton.RespawnPlane();
                    }
                    Debug.LogError("PlaneSelectorHasStarted.");
                }
            }
        }
    }
}
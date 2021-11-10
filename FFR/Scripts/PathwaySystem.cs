
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class PathwaySystem : UdonSharpBehaviour
{
    public GameObject[] PathwayObjects;
    public bool isRendered = false;
    private bool hidden = false;
    private int index = 0;
    private int maxLength = 0;
    private VRCPlayerApi localPlayer;
    public PlayerUIScript UIScript;
    public float distance = 200;
    void Start()
    {
        localPlayer = Networking.LocalPlayer;
        maxLength = PathwayObjects.Length;
        index = 0;
    }

    void Update()
    {
        if (PathwayObjects == null || UIScript == null)
        {
            return;
        }

        if (isRendered)
        {
            LookLogic();
        }
        else
        {
            if (!hidden)
            {
                HideLogic();
            }
        }
    }

    public void callHide()
    {
        hidden = false;
        index = 0;
        isRendered = false;
        Debug.Log("called");
    }

    public void callShow()
    {
        index = 0;
        hidden = false;
        isRendered = true;
        Debug.Log("Show");
    }

    public void LookLogic()
    {
        if (index < maxLength)
        {
            var localPlayerPosition = localPlayer.GetPosition();
            var dist = Vector3.Distance(localPlayerPosition, PathwayObjects[index].transform.position);
            if (dist > distance)
            {
                PathwayObjects[index].SetActive(false);
            }
            else
            {
                PathwayObjects[index].SetActive(true);
                PathwayObjects[index].transform.localScale = new Vector3(UIScript.IconSize, UIScript.IconSize, UIScript.IconSize) * dist;
                PathwayObjects[index].transform.LookAt(localPlayer.GetPosition());
            }
            if (index + 1 < maxLength)
            {
                index = index + 1;
            }
            else
            {
                index = 0;
            }
        }
    }

    public void HideLogic()
    {
        PathwayObjects[index].SetActive(false);
        index = index + 1;
        hidden = !(index < maxLength);
    }
}

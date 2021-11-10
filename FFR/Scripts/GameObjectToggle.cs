
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class GameObjectToggle : UdonSharpBehaviour
{
    public GameObject[] ObjectsSetOff;
    public GameObject[] ObjectsSetOn;
    public GameObject[] ObjectsToggle;

    public void Interact()
    {
        Activate();
    }
    
    public void Activate()
    {
        {
            foreach (var t in ObjectsSetOff)
            {
                t.gameObject.SetActive(false);
            }

            foreach (var t in ObjectsSetOn)
            {
                t.gameObject.SetActive(true);
            }

            foreach (var t in ObjectsToggle)
            {
                t.gameObject.SetActive(!t.gameObject.activeSelf);
            }
        }
    }
}

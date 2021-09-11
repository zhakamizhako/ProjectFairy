
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class SceneSelector : UdonSharpBehaviour
{
    public Image targetImageCanvas;
    public Text targetTitleText;
    public Text targetDescriptionText;

    public SceneAdaptor[] Scenes;
    public int currentlySelected = 0;
    public bool QueueAsPilot;
    public bool QueueAsCoPilot;
    void Start()
    {
        processInfo();
    }

    public void queueAsPilot()
    {
        Scenes[currentlySelected].asPilot = true;
    }

    public void beginScene(){
        Scenes[currentlySelected].asPilot=true;
        Scenes[currentlySelected].startScene();
    }

    public void queueAsPassenger()
    {

    }
    public void nextScene()
    {
        if (currentlySelected + 1 < Scenes.Length)
        {
            currentlySelected = currentlySelected + 1;
        }
        else
        {
            currentlySelected = 0;
        }
        processInfo();
    }

    void processInfo(){
        Scenes[currentlySelected].asPilot = false;
        targetDescriptionText.text = Scenes[currentlySelected].description;
        targetTitleText.text = Scenes[currentlySelected].ChapterTitle;
        targetImageCanvas.material = Scenes[currentlySelected].SceneImage.material;
    }

    public void prevScene()
    {
        if (currentlySelected - 1 > -1)
        {
            currentlySelected = currentlySelected - 1;
        }
        else
        {
            currentlySelected = 0;
        }
        processInfo();
    }
}

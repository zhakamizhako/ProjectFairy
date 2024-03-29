﻿using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class ButtonNavigator : UdonSharpBehaviour
{
    public ButtonListenerUI selectedButton;
    public float triggerThreshold = 0.75f;
    public float triggeranimate = 0.15f;
    private float LTrigger = 0f;
    private float LstickH = 0f;
    private float LstickV = 0f;
    private Vector2 Rstick = Vector2.zero;
    private bool keyboardTrigger = false;
    private bool LastFrameTrigger = false;
    private bool LStickLastFrame = false;
    public bool SetMode = false;
    public ButtonListenerUI North;
    public ButtonListenerUI South;
    public ButtonListenerUI East;
    public ButtonListenerUI West;
    public Text NorthText;
    public Text SouthText;
    public Text EastText;
    public Text WestText;
    public EngineController EngineControl;
    public GameObject QueryObject;
    //Circle Menu Animator
    public Animator BUIAnimator;
    public Text DebugText;
    public float standbyTime = 0f;
    public bool RSelected = false;
    public bool isDebug = false;
    public CanvasHud CVHVR;
    public Animator MenuAnimator;
    public Animator[] MenuAnimators;

    public ButtonListenerUI[] Categories;

    void Start()
    {
        if (QueryObject != null)
        {
            QueryObject.SetActive(false);
        }

        if (selectedButton != null && MenuAnimator != null)
        {
            MenuAnimator.SetTrigger(selectedButton.AnimatorTrigger);
            MenuAnimator.SetTrigger(selectedButton.AnimatorCategory);
        }
        if (selectedButton != null && MenuAnimators != null && MenuAnimators.Length >0)
        {
            foreach (Animator x in MenuAnimators)
            {
                x.SetTrigger(selectedButton.AnimatorTrigger);
                x.SetTrigger(selectedButton.AnimatorCategory);
            }
        }
    }

    public void buttonSelectCallback()
    {
        if (MenuAnimator != null)
        {
            MenuAnimator.SetTrigger(selectedButton.AnimatorTrigger);
            MenuAnimator.SetTrigger(selectedButton.AnimatorCategory);
        }
        if (MenuAnimators != null && MenuAnimators.Length > 0)
        {
            foreach (var x in MenuAnimators)
            {
                x.SetTrigger(selectedButton.AnimatorTrigger);
                x.SetTrigger(selectedButton.AnimatorCategory);
            }
        }
        if (selectedButton.audioSFX != null && selectedButton.select != null)
        {
            selectedButton.audioSFX.PlayOneShot(selectedButton.select);
        }
    }

    public void handleCategory(string dir, ButtonListenerUI btton)
    {
        if (Categories != null && Categories.Length > 0)
        {
            Categories[btton.categoryId] = btton;
            if (dir == "up")
            {
                if (btton.categoryId - 1 < 0)
                {
                    selectedButton = Categories[Categories.Length - 1];
                }
                else
                {
                    selectedButton = Categories[btton.categoryId - 1];
                }
            }
            if (dir == "down")
            {
                if (btton.categoryId + 1 == Categories.Length)
                {
                    selectedButton = Categories[0];
                }
                else
                {
                    selectedButton = Categories[btton.categoryId + 1];
                }
            }
            if (selectedButton.gameObject.GetComponent<Button>() != null)
            {
                selectedButton.gameObject.GetComponent<Button>().Select();
            }
            else if (selectedButton.gameObject.GetComponent<Slider>() != null)
            {
                selectedButton.gameObject.GetComponent<Slider>().Select();
            }
            buttonSelectCallback();
        }
        else
        {
            btton.navigate(dir, true);
        }
    }

    void Update()
    {
        if (EngineControl == null || !(EngineControl.Piloting || EngineControl.Passenger))
        {
            return;
        }
        if (standbyTime < 1)
        {
            standbyTime = standbyTime + Time.deltaTime;
        }

        LstickH = Input.GetAxisRaw("Oculus_CrossPlatform_PrimaryThumbstickHorizontal");
        LstickV = Input.GetAxisRaw("Oculus_CrossPlatform_PrimaryThumbstickVertical");
        LTrigger = Input.GetAxisRaw("Oculus_CrossPlatform_PrimaryIndexTrigger");

        keyboardTrigger = Input.GetKey(KeyCode.O);

        Rstick.x = Input.GetAxisRaw("Oculus_CrossPlatform_SecondaryThumbstickHorizontal");
        Rstick.y = Input.GetAxisRaw("Oculus_CrossPlatform_SecondaryThumbstickVertical");
        if (isDebug)
        {
            Rstick.x = Input.GetKey(KeyCode.Keypad6) ? 0.75f : (Input.GetKey(KeyCode.Keypad4) ? -0.75f : 0);
            Rstick.y = Input.GetKey(KeyCode.Keypad8) ? 0.75f : (Input.GetKey(KeyCode.Keypad2) ? -0.75f : 0);
        }

        if ((LstickH > 0.75f || Input.GetKeyDown(KeyCode.L)) && !LStickLastFrame)
        {
            LStickLastFrame = true;
            selectedButton.right();
        }
        else if ((LstickH < -0.75f || Input.GetKeyDown(KeyCode.J)) && !LStickLastFrame)
        {
            LStickLastFrame = true;
            selectedButton.left();
        }
        else if ((LstickV > 0.75f || Input.GetKeyDown(KeyCode.I)) && !LStickLastFrame)
        {
            LStickLastFrame = true;
            selectedButton.up();
        }
        else if ((LstickV < -0.75f || Input.GetKeyDown(KeyCode.K)) && !LStickLastFrame)
        {
            LStickLastFrame = true;
            selectedButton.down();
        }
        else if (LstickV < 0.75f && LstickV > -0.75f && LstickH < 0.75f && LstickH > -0.75f)
        {
            LStickLastFrame = false;
        }

        if (((keyboardTrigger || LTrigger > 0.75) && !LastFrameTrigger))
        {
            LastFrameTrigger = true;
            // Debug.Log(selectedButton.colors);
            selectedButton.Trigger();
            Debug.Log("Trigger");
        }
        else if ((LTrigger < 0.75 && !keyboardTrigger) && LastFrameTrigger)
        {
            if (selectedButton.isTrigger)
            {
                Debug.Log("Released Trigger");
                selectedButton.ReleaseTrigger();
            }
            LastFrameTrigger = false;
        }

        if (Input.GetButtonDown("Oculus_CrossPlatform_PrimaryThumbstick") && !SetMode && selectedButton != null)
        {
            SetMode = true;
        }
        else if (Input.GetButtonDown("Oculus_CrossPlatform_PrimaryThumbstick") && SetMode && selectedButton != null)
        {
            SetMode = false;
        }
        if (CVHVR != null && CVHVR.gameObject.activeSelf)
        {
            if (BUIAnimator != null)
            {
                BUIAnimator.SetFloat("H", Rstick.x + .5f);
                BUIAnimator.SetFloat("V", Rstick.y + .5f);
                BUIAnimator.SetFloat("U", Rstick.y > 0.01f ? Rstick.y : 0);
                BUIAnimator.SetFloat("L", Rstick.x < -0.01f ? -(Rstick.x) : 0);
                BUIAnimator.SetFloat("D", Rstick.y < -0.01f ? -(Rstick.y) : 0);
                BUIAnimator.SetFloat("R", Rstick.x > 0.01f ? Rstick.x : 0);
                BUIAnimator.SetFloat("standby", standbyTime);
            }

        }

        if (DebugText != null)
        {
            DebugText.text = "X:" + Rstick.x + "\nY:" + Rstick.y;
        }

        if (NorthText != null)
        {
            NorthText.text = North != null ? North.gameObject.name : "NONE";
        }
        if (WestText != null)
        {
            WestText.text = West != null ? West.gameObject.name : "NONE";
        }
        if (EastText != null)
        {
            EastText.text = East != null ? East.gameObject.name : "NONE";
        }
        if (SouthText != null)
        {
            SouthText.text = South != null ? South.gameObject.name : "NONE";
        }

        if (SetMode)
        {
            if (QueryObject != null)
            {
                QueryObject.SetActive(true);
            }
            if (Rstick.magnitude > 0.75f)
            {
                if (Rstick.y > 0.75f)
                {//North
                    North = selectedButton;
                }
                if (Rstick.y < -0.75f)
                {//South
                    South = selectedButton;
                }
                if (Rstick.x > 0.75f)
                {//East
                    East = selectedButton;
                }
                if (Rstick.x < -0.75f)
                {//West
                    West = selectedButton;
                }
                RSelected = true;
                SetMode = false;
            }
        }
        else
        {
            if (QueryObject != null)
            {
                QueryObject.SetActive(false);
            }

            if (Rstick.magnitude > 0.75f && RSelected == false)
            {
                if (Rstick.y > 0.75f && North != null)
                {//North
                    North.Trigger();
                }
                if (Rstick.y < -0.75f && South != null)
                {//South
                    South.Trigger();
                }
                if (Rstick.x > 0.75f && East != null)
                {//East
                    East.Trigger();
                }
                if (Rstick.x < -0.75f && West != null)
                {//West
                    West.Trigger();
                }
                RSelected = true;
            }
            else if (Rstick.magnitude < 0.75f && RSelected == true)
            {
                if (North != null && North.isTrigger)
                {
                    North.ReleaseTrigger();
                }
                if (South != null && South.isTrigger)
                {
                    South.ReleaseTrigger();
                }
                if (East != null && East.isTrigger)
                {
                    East.ReleaseTrigger();
                }
                if (West != null && West.isTrigger)
                {
                    West.ReleaseTrigger();
                }
                RSelected = false;
            }
        }
    }
}
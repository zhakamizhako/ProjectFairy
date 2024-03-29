﻿using UdonSharp;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class ButtonListenerUI : UdonSharpBehaviour
{
    public ButtonNavigator buttonNavigator;
    public ButtonListenerUI buttonUp;
    public ButtonListenerUI buttonDown;
    public ButtonListenerUI buttonLeft;
    public ButtonListenerUI buttonRight;
    public UdonBehaviour UB;
    public Slider ReferenceSlider;
    public bool isBinary = false;
    public string customEvent;
    public string customEventRelease;
    public bool setVariableValue = false;
    public string variableValue;
    public float valuebeforeTrigger = 0.75f;
    public bool isSelected = false;
    public bool isTrigger = false;
    private bool isButton;
    private bool isSlider;
    private bool isSliderAdjusting = false;
    public bool isTriggering = false;
    public string AnimatorTrigger = "";
    public string AnimatorCategory = "";

    public AudioSource audioSFX;
    public AudioClip confirm;
    public AudioClip deselect;
    public AudioClip select;

    public int categoryId;
    public bool hasCategory = false;

    void Start()
    {
        if (gameObject.GetComponent<Button>() != null)
        {
            isButton = true;
        }
        if (gameObject.GetComponent<Slider>() != null)
        {
            isSlider = true;
            if (ReferenceSlider != null)
            {
                gameObject.GetComponent<Slider>().minValue = ReferenceSlider.minValue;
                gameObject.GetComponent<Slider>().maxValue = ReferenceSlider.maxValue;
                gameObject.GetComponent<Slider>().value = ReferenceSlider.value;
            }
        }
    }
    public void up()
    {
        navigate("up", false);
    }

    public void down()
    {
        navigate("down", false);
    }

    public void left()
    {
        navigate("left", false);
    }

    public void right()
    {
        navigate("right", false);
    }
    public void navigate(string direction, bool ignoreCat)
    {
        if (!isSliderAdjusting)
        {
            if (buttonUp != null && direction == "up")
            {
                if (hasCategory || !ignoreCat)
                {
                    buttonNavigator.handleCategory("up", this);
                }
                else
                {
                    buttonNavigator.selectedButton = buttonUp;
                    if (buttonUp.gameObject.GetComponent<Button>() != null)
                    {
                        buttonUp.gameObject.GetComponent<Button>().Select();
                    }
                    else if (buttonUp.gameObject.GetComponent<Slider>() != null)
                    {
                        buttonUp.gameObject.GetComponent<Slider>().Select();
                    }
                    buttonNavigator.buttonSelectCallback();
                }

            }
            if (buttonDown != null && direction == "down")
            {
                if (hasCategory || !ignoreCat)
                {
                    buttonNavigator.handleCategory("down", this);
                }
                else
                {
                    buttonNavigator.selectedButton = buttonDown;
                    if (buttonDown.gameObject.GetComponent<Button>() != null)
                    {
                        buttonDown.gameObject.GetComponent<Button>().Select();
                    }
                    else if (buttonDown.gameObject.GetComponent<Slider>() != null)
                    {
                        buttonDown.gameObject.GetComponent<Slider>().Select();
                    }
                    buttonNavigator.buttonSelectCallback();
                }

            }
            if (buttonLeft != null && direction == "left")
            {
                buttonNavigator.selectedButton = buttonLeft;
                if (buttonLeft.gameObject.GetComponent<Button>() != null)
                {
                    buttonLeft.gameObject.GetComponent<Button>().Select();
                }
                else if (buttonLeft.gameObject.GetComponent<Slider>() != null)
                {
                    buttonLeft.gameObject.GetComponent<Slider>().Select();
                }
                buttonNavigator.buttonSelectCallback();
            }
            if (buttonRight != null && direction == "right")
            {
                buttonNavigator.selectedButton = buttonRight;
                if (buttonRight.gameObject.GetComponent<Button>() != null)
                {
                    buttonRight.gameObject.GetComponent<Button>().Select();
                }
                else if (buttonRight.gameObject.GetComponent<Slider>() != null)
                {
                    buttonRight.gameObject.GetComponent<Slider>().Select();
                }
                buttonNavigator.buttonSelectCallback();
            }
        }

        if (isSliderAdjusting)
        {
            if (direction == "left" && isSlider)
            {
                var sliderObject = gameObject.GetComponent<Slider>();
                if (ReferenceSlider != null)
                {
                    if (ReferenceSlider.value > ReferenceSlider.minValue)
                    {
                        ReferenceSlider.value = ReferenceSlider.value + (((ReferenceSlider.minValue / ReferenceSlider.maxValue) / 100) * 10);
                        sliderObject.value = ReferenceSlider.value;
                    }
                }
                else
                {
                    if (sliderObject.value > sliderObject.minValue)
                        sliderObject.value = sliderObject.value - (sliderObject.minValue / sliderObject.maxValue) / 100;
                }
            }
            if (direction == "right" && isSlider)
            {
                var sliderObject = gameObject.GetComponent<Slider>();
                if (ReferenceSlider != null)
                {
                    if (ReferenceSlider.value < ReferenceSlider.maxValue)
                    {
                        ReferenceSlider.value = ReferenceSlider.value - (((ReferenceSlider.minValue / ReferenceSlider.maxValue) / 100) * 10);
                        sliderObject.value = ReferenceSlider.value;
                    }
                }
                else
                {
                    if (sliderObject.value < sliderObject.maxValue)
                        sliderObject.value = sliderObject.value + (sliderObject.minValue / sliderObject.maxValue) / 100;
                }
            }
        }


    }
    public void Trigger()
    {
        if (isSlider)
        {
            isSliderAdjusting = !isSliderAdjusting;
            if (confirm != null && deselect != null)
                if (isSliderAdjusting)
                {
                    audioSFX.PlayOneShot(confirm);
                }
                else
                {
                    audioSFX.PlayOneShot(deselect);
                }
        }
        else
        {
            if (UB != null)
            {
                UB.SendCustomEvent(customEvent);
            }
            if (audioSFX != null && confirm != null)
            {
                audioSFX.PlayOneShot(confirm);
            }
        }
    }

    public void ReleaseTrigger()
    {
        UB.SendCustomEvent(customEventRelease);
    }
}
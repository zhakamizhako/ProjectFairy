using UdonSharp;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class ButtonListenerUI : UdonSharpBehaviour {
    public ButtonNavigator buttonNavigator;
    public ButtonListenerUI buttonUp;
    public ButtonListenerUI buttonDown;
    public ButtonListenerUI buttonLeft;
    public ButtonListenerUI buttonRight;
    public UdonBehaviour UB;
    public Slider ReferenceSlider;
    public string customEvent;
    public string customEventRelease;
    public bool isSelected = false;
    public bool isTrigger = false;
    private bool isButton;
    private bool isSlider;
    public bool isTriggering = false;

    void Start () {
        if (gameObject.GetComponent<Button> () != null) {
            isButton = true;
        }
        if (gameObject.GetComponent<Slider> () != null) {
            isSlider = true;
            if (ReferenceSlider != null) {
                gameObject.GetComponent<Slider> ().minValue = ReferenceSlider.minValue;
                gameObject.GetComponent<Slider> ().maxValue = ReferenceSlider.maxValue;
                gameObject.GetComponent<Slider> ().value = ReferenceSlider.value;
            }
        }
    }
    public void up () {
        navigate ("up");
    }

    public void down () {
        navigate ("down");
    }

    public void left () {
        navigate ("left");
    }

    public void right () {
        navigate ("right");
    }
    public void navigate (string direction) {
        if (buttonUp != null && direction == "up") {
            buttonNavigator.selectedButton = buttonUp;
            if (buttonUp.gameObject.GetComponent<Button> () != null) {
                buttonUp.gameObject.GetComponent<Button> ().Select ();
            } else if (buttonUp.gameObject.GetComponent<Slider> () != null) {
                buttonUp.gameObject.GetComponent<Slider> ().Select ();
            }
        }
        if (buttonDown != null && direction == "down") {
            buttonNavigator.selectedButton = buttonDown;
            if (buttonDown.gameObject.GetComponent<Button> () != null) {
                buttonDown.gameObject.GetComponent<Button> ().Select ();
            } else if (buttonDown.gameObject.GetComponent<Slider> () != null) {
                buttonDown.gameObject.GetComponent<Slider> ().Select ();
            }
        }
        if (buttonLeft != null && direction == "left" && isButton) {
            buttonNavigator.selectedButton = buttonLeft;
            if (buttonLeft.gameObject.GetComponent<Button> () != null) {
                buttonLeft.gameObject.GetComponent<Button> ().Select ();
            } else if (buttonLeft.gameObject.GetComponent<Slider> () != null) {
                buttonLeft.gameObject.GetComponent<Slider> ().Select ();
            }
        }
        if (buttonRight != null && direction == "right" && isButton) {
            buttonNavigator.selectedButton = buttonRight;
            if (buttonRight.gameObject.GetComponent<Button> () != null) {
                buttonRight.gameObject.GetComponent<Button> ().Select ();
            } else if (buttonRight.gameObject.GetComponent<Slider> () != null) {
                buttonRight.gameObject.GetComponent<Slider> ().Select ();
            }
        }

        if (direction == "left" && isSlider) {
            var sliderObject = gameObject.GetComponent<Slider> ();
            if (ReferenceSlider != null) {
                if (ReferenceSlider.value > ReferenceSlider.minValue) {
                    ReferenceSlider.value = ReferenceSlider.value + ((ReferenceSlider.minValue / ReferenceSlider.maxValue) / 100);
                    sliderObject.value = ReferenceSlider.value;
                }
            } else {
                if(sliderObject.value > sliderObject.minValue )
                sliderObject.value = sliderObject.value - (sliderObject.minValue / sliderObject.maxValue) / 100;
            }
        }
        if (direction == "right" && isSlider) {
            var sliderObject = gameObject.GetComponent<Slider> ();
            if (ReferenceSlider != null) {
                if (ReferenceSlider.value < ReferenceSlider.maxValue) {
                    ReferenceSlider.value = ReferenceSlider.value - ((ReferenceSlider.minValue / ReferenceSlider.maxValue) / 100);
                    sliderObject.value = ReferenceSlider.value;
                }
            } else {
                if(sliderObject.value < sliderObject.maxValue )
                sliderObject.value = sliderObject.value + (sliderObject.minValue / sliderObject.maxValue) / 100;
            }
        }
    }
    public void Trigger () {
        if ( UB != null) {
            UB.SendCustomEvent (customEvent);
        }
    }

    public void ReleaseTrigger(){
        UB.SendCustomEvent(customEventRelease);
    }
}
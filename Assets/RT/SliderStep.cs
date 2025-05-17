using UnityEngine;
using UnityEngine.UI;

//Modified from https://forum.unity.com/threads/slider-bar-stepping.267467/
//To use this, drag onto a slider GUI object, and add an OnValueChanged callback to UpdateStep()

public class SliderStep : MonoBehaviour
{
    public float stepAmount = 0.5f;
    private Slider mySlider;
    private int numberOfSteps;

    // Start is called before the first frame update
    private void Awake()
    {
        mySlider = gameObject.GetComponent<Slider>();
    }

    private void Start()
    {
        mySlider = GetComponent<Slider>();
        numberOfSteps = Mathf.CeilToInt(mySlider.maxValue / stepAmount);
    }

    public void UpdateStep()
    {
        var range = mySlider.value / mySlider.maxValue * numberOfSteps;
        var ceil = Mathf.CeilToInt(range);
        mySlider.value = ceil * stepAmount;
    }
}
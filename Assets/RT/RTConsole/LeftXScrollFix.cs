using UnityEngine;

//I don't know why I need this .. (crawls into a ball and cries)

public class LeftXScrollFix : MonoBehaviour
{
    private Vector3 _originalLocalPos;

    // Use this for initialization
    private void Start()
    {
        _originalLocalPos = transform.localPosition;
        _originalLocalPos.x += 5; //the evil tweak so the letters on the left aren't cutoff
    }

    // Update is called once per frame
    private void Update()
    {
        var vTemp = transform.localPosition;
        vTemp.x = _originalLocalPos.x;


        transform.localPosition = vTemp;
    }
}
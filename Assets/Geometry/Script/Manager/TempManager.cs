using UnityEngine;

public class TempManager : MonoBehaviour
{
    public enum Straight
    {
        X,
        Y,
        Z
    }

    public static TempManager instance;

    public Straight ModeStraight;

    private void Start()
    {
        instance = this;
    }
}
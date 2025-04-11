using System;
using UnityEngine;
 
public class TempManager : MonoBehaviour
{

    public static TempManager instance;

    public enum Straight
    {
        X, Y, Z
    }

    public Straight ModeStraight;
    
    private void Start()
    {
        instance = this;
    }
}
using System;
using UnityEngine;

[Serializable]
public class CellStyle
{
    public int Number;
    public Color32 CellColor;
    public Color32 TextColor;
}

public class CellStyleHolder : MonoBehaviour
{
    public static CellStyleHolder Instance;
    public CellStyle[] CellStyles;

    public void Awake()
    {
        Instance = this;
    }
}
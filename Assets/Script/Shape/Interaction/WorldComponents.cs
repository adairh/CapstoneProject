﻿using System;
using UnityEngine;

public class WorldComponents : MonoBehaviour
{
    private void Start()
    {
        if (gameObject.GetComponent<ShapeClickHandler>() != null)
            Destroy(gameObject.GetComponent<ShapeClickHandler>());
        if (gameObject.GetComponent<HoverableShape>() != null)
            Destroy(gameObject.GetComponent<HoverableShape>());
    }
}
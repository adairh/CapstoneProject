using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasOrderInitializer : MonoBehaviour
{
    public Canvas canvasPlaygame;

    void Start()
    {
        if (CanvasSortOrderManager.setPlayGameCanvasOnTop)
        {
            if (canvasPlaygame != null)
            {
                canvasPlaygame.sortingOrder = 100; // or any number that makes it on top
                Debug.Log("canvasPlaygame sortingOrder set to 100");
            }

            CanvasSortOrderManager.setPlayGameCanvasOnTop = false; // Reset
        }
    }
}

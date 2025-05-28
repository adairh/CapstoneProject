using UnityEngine;



public static class CanvasSortOrderManager
{
    
    /// Set this to true before loading the "MAIN" scene from PlayGame.
    /// The receiving scene can check this flag and set canvasPlaygame's sort order accordingly.
    
    public static bool setPlayGameCanvasOnTop = false;

    
    /// Optionally store a custom sort order value if needed later.
    
    public static int desiredSortOrder = 100;

    
    /// Reset all flags after use to avoid affecting future scene loads.
    
    public static void Reset()
    {
        setPlayGameCanvasOnTop = false;
        desiredSortOrder = 100;
    }
}


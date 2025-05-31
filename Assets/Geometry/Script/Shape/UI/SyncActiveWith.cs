using UnityEngine;

public class SyncActiveWith : MonoBehaviour
{
    [Tooltip("The GameObject to activate/deactivate in sync with this object.")]
    public GameObject target; // Drag B here in Inspector

    private void OnEnable()
    {
        if (target != null)
            target.SetActive(true);
    }

    private void OnDisable()
    {
        if (target != null)
            target.SetActive(false);
    }
}
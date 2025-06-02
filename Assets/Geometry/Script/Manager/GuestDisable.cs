using UnityEngine;
using Unity.Netcode;

namespace Manipulator
{
    // Attach this to any GameObject that should be hidden from clients (guest users)
    public class GuestDisable : MonoBehaviour
    {
        [Header("Target Component to Disable (optional)")]
        public Behaviour componentToDisable; // Drag a component (e.g., Button, Collider) here if needed

        private void Start()
        {
            // Only disable on client (not host/server)
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost)
            {
                if (componentToDisable != null)
                {
                    componentToDisable.enabled = false;
                }
                else
                {
                    gameObject.SetActive(false);
                }
            }
        }
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SlimUI.ModernMenu
{
    public class ResetDemo : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKeyDown("r")) SceneManager.LoadScene(0);
        }
    }
}
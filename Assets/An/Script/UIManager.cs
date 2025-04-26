using UnityEngine;

namespace An_An
{
    public class UIManager : MonoBehaviour
    {
        public GameObject canvasOnboarding;
        public GameObject canvasHome;

        public void ShowHome()
        {
            canvasOnboarding.SetActive(false);
            canvasHome.SetActive(true);
        }

        void Start()
        {
            canvasOnboarding.SetActive(true);
            canvasHome.SetActive(false); // An home khi bat dau
        }
    }
}
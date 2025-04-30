using UnityEngine;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace An_An
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        /*private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // Makes the UIManager persist across scenes
            }
            else
            {
                Destroy(gameObject); // Ensures only one instance exists
                return;
            }
        }*/
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
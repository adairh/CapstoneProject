using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Manipulator
{
    [RequireComponent(typeof(Button))]
    public class BaseButton : MonoBehaviour
    {
        protected Button button;
        protected Image buttonImage; // The background image, probably transparent
        protected Image iconImage;   // First child Image (your icon)
        protected GameObject highlightObj; // Highlight child object

        [Header("Toggle")]
        public bool IsToggleButton = false;
        public bool IsToggled = false;
        public Color iconNormal = Color.white;
        public Color iconToggled = new Color(1f, 0.78f, 0f, 1f);  

        [Header("Click Effect")]
        public float clickScale = 0.92f;
        public float clickDuration = 0.06f;

        private Vector3 originalScale;
        private Coroutine effectCoroutine;

        protected virtual void Awake()
        {
            button = GetComponent<Button>();
            buttonImage = GetComponent<Image>();

            // Find icon: first child with Image, not self
            iconImage = GetComponentInChildren<Image>(true);
            if (iconImage == buttonImage) // If self, find next
            {
                var images = GetComponentsInChildren<Image>(true);
                foreach (var img in images)
                {
                    if (img != buttonImage) { iconImage = img; break; }
                }
            }
            
            originalScale = transform.localScale;
            button.onClick.AddListener(OnButtonClick);
            
        }

        protected virtual void Start()
        {
            if (ButtonController.Instance == null)
            {
                Debug.LogError("[BaseButton] ButtonController instance is null!");
                return;
            }

            ButtonController.Instance.RegisterButton(this);
            UpdateVisual();
        }

        protected virtual void OnButtonClick()
        {
            if (IsToggleButton)
            {
                IsToggled = !IsToggled;
                UpdateVisual();
            }
            else
            {
                // Only pop effect for non-toggle
                if (effectCoroutine != null) StopCoroutine(effectCoroutine);
                effectCoroutine = StartCoroutine(ClickEffectCoroutine());
            }

            if (ButtonController.Instance != null)
                ButtonController.Instance.OnButtonClicked(this);
        }

        public virtual void UpdateVisual()
        {
            // Toggle effect: show highlight and change icon color
            if (IsToggleButton)
            {
                if (highlightObj != null)
                    highlightObj.SetActive(IsToggled);
                if (iconImage != null)
                    iconImage.color = IsToggled ? iconToggled : iconNormal;
            }
            else
            {
                if (highlightObj != null)
                    highlightObj.SetActive(false);
                if (iconImage != null)
                    iconImage.color = iconNormal;
            }
        }

        public void SetToggled(bool toggled)
        {
            if (IsToggleButton)
            {
                IsToggled = toggled;
                UpdateVisual();
            }
        }

        // This is what your manager should call to reset button (toggle off)
        public void ResetButton()
        {
            if (IsToggleButton)
            {
                IsToggled = false;
                UpdateVisual();
            }
        }

        protected virtual IEnumerator ClickEffectCoroutine()
        {
            float t = 0f;
            while (t < clickDuration)
            {
                t += Time.unscaledDeltaTime;
                float lerp = t / clickDuration;
                transform.localScale = Vector3.Lerp(originalScale, originalScale * clickScale, lerp);
                yield return null;
            }
            t = 0f;
            while (t < clickDuration)
            {
                t += Time.unscaledDeltaTime;
                float lerp = t / clickDuration;
                transform.localScale = Vector3.Lerp(originalScale * clickScale, originalScale, lerp);
                yield return null;
            }
            transform.localScale = originalScale;
        }
    }
}

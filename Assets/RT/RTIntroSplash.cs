using UnityEngine;

public class RTIntroSplash : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
    }

    public void OnCloseButtonClicked()
    {
        // Debug.Log("Close button clicked");
        Destroy(gameObject);
    }

    public void OnLogoClicked()
    {
        //Debug.Log("Clicked logo, opening website");
        RTUtil.PopupUnblockOpenURL("https://www.rtsoft.com");
    }
}
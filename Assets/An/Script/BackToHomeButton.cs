using Geometry;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToHomeButton : MonoBehaviour
{
    [Header("Main Canvases")] public GameObject canvasHome;

    public GameObject canvasPlaygame;
    public GameObject canvasOnboarding;
    public GameObject canvas;

    [Header("Popups")] public GameObject popupJoinRoom;

    public GameObject popupCreateRoom;
    public GameObject darkOverlay;

    [Header("Join Room Inputs")] public TMP_InputField joinRoomIDInput;

    public TMP_InputField joinPasswordInput;

    [Header("Create Room Inputs")] public TMP_InputField createRoomIDInput;

    public TMP_InputField createPasswordInput;
    /*private void Start()
    {
        // Không đụng 
        //mac dinh hien canvasHome, an canvasPlaygame
        //overPlayCanvas.SetActive(false);
        //ConfigureUIBasedOnScene();
        //canvasHome.SetActive(true);
        canvasPlaygame.SetActive(false);
        canvasOnboarding.SetActive(false); // Hide onboarding screen if it's active
        canvas.SetActive(false); // an canvas chinh khi bat dau

        // dam bao cac popup + overlay duoc an ngay khi lo bat trong editor
        if (popupJoinRoom != null) popupJoinRoom.SetActive(false);
        if (popupCreateRoom != null) popupCreateRoom.SetActive(false);
        if (darkOverlay != null) darkOverlay.SetActive(false);
        // Configure UI based on the current scene
    }*/
    public void OnBackToHome()
    {
        // Load MAIN scene fresh
        canvasHome.SetActive(true);
        canvasOnboarding.SetActive(false);
        canvasPlaygame.SetActive(false);
    }
    public void OnBackToHomeFrom_DrawingScene()
    {
        // Load MAIN scene fresh
        SceneManager.LoadScene("MAIN");
        /*canvasHome.SetActive(true);
        canvasOnboarding.SetActive(false);
        canvasPlaygame.SetActive(false);*/
    }
    public void OnBackFromAI_to_Home()
    {
        SceneManager.LoadScene("MAIN");
        /*canvasHome.SetActive(true);
        canvasOnboarding.SetActive(false);
        canvasPlaygame.SetActive(false);*/
    }
    public void OnBackToHomeFrom_PlayGame()
    {
        CanvasSortOrderManager.setPlayGameCanvasOnTop = true;
        SceneManager.LoadScene("MAIN");
    }

    public void OnLoadAITutor()
    {
        SceneManager.LoadScene("AI_Tutor");
    }

    public void OnPlayGamesButton()
    {
        canvasHome.SetActive(false);
        canvasPlaygame.SetActive(true);
    }

    public void OnPlayGame1()
    {
        SceneManager.LoadScene("Menu");
    }

    public void OnPlayGame2()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void OnJoinRoomButton()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void SubmitJoinRoom()
    {
        Debug.Log("Join Room: " + joinRoomIDInput.text + " | Password: " + joinPasswordInput.text);
        popupJoinRoom.SetActive(false);
        darkOverlay.SetActive(false);
    }


    public void OnCreateRoomButton()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void OnCreateRandomRoomButton()
    {
        Debug.LogWarning("OnCreateRandomRoomButton 1:" + SceneFlag.IsRandom);

        SceneFlag.IsRandom = true;
        Debug.LogWarning("OnCreateRandomRoomButton 2:" + SceneFlag.IsRandom);
        SceneManager.LoadScene("SampleScene");
        Debug.LogWarning("OnCreateRandomRoomButton 3:" + SceneFlag.IsRandom);
    }

    public void SubmitCreateRoom()
    {
        Debug.Log("Create Room: " + createRoomIDInput.text + " | Password: " + createPasswordInput.text);
        popupCreateRoom.SetActive(false);
        darkOverlay.SetActive(false);
    }

    public void CloseAllPopups()
    {
        popupJoinRoom.SetActive(false);
        popupCreateRoom.SetActive(false);
        darkOverlay.SetActive(false);
        canvasOnboarding.SetActive(false);
        canvasHome.SetActive(true);
    }
}
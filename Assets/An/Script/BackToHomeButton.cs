using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class BackToHomeButton : MonoBehaviour
{
    [Header("Main Canvases")]
    public GameObject canvasHome;
    public GameObject canvasPlaygame;
    public GameObject canvasOnboarding;
    public GameObject canvas;

    [Header("Popups")]
    public GameObject popupJoinRoom;
    public GameObject popupCreateRoom;
    public GameObject darkOverlay;

    [Header("Join Room Inputs")]
    public TMP_InputField joinRoomIDInput;
    public TMP_InputField joinPasswordInput;

    [Header("Create Room Inputs")]
    public TMP_InputField createRoomIDInput;
    public TMP_InputField createPasswordInput;

    public void OnBackToHome()
    {
        // Load MAIN scene fresh
        canvasHome.SetActive(true);
        canvasOnboarding.SetActive(false);
        canvasPlaygame.SetActive(false);
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
        SceneManager.LoadScene("Copy_SampleScene");
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
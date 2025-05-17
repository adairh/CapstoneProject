using TMPro;
using UnityEngine;

public class HomeScreenController2 : MonoBehaviour
{
    [Header("Main Canvases")] public GameObject canvasHome;

    /*public GameObject canvasPlaygame;*/
    public GameObject canvasOnboarding; // Onboarding screen with ShowHome button
    public GameObject canvas; // Main Canvas transform for UI instantiation

    [Header("Popups")] public GameObject popupJoinRoom;

    public GameObject popupCreateRoom;
    public GameObject darkOverlay;

    [Header("Join Room Inputs")] public TMP_InputField lobbyNameInputField;

    public TMP_InputField passwordInputField;

    [Header("Create Room Inputs")] public TMP_InputField createRoomIDInput;

    public TMP_InputField createPasswordInput;

    private void Start()
    {
        // canvasPlaygame.SetActive(false);
        canvasOnboarding.SetActive(false);
        canvasHome.SetActive(true);
        darkOverlay.SetActive(false);
        popupJoinRoom.SetActive(false);
        popupCreateRoom.SetActive(false);
    }

    public void OnPlayGamesButton()
    {
        canvasOnboarding.SetActive(false);
        canvasHome.SetActive(true);
    }

    /*public void OnCreateRoomButton()
    {
        SceneManager.LoadScene("SampleScene");

        popupJoinRoom.SetActive(false);
        popupCreateRoom.SetActive(true);
        darkOverlay.SetActive(true);
        canvasHome.SetActive(false);
        canvasPlaygame.SetActive(false);


        */
    /* StartCoroutine(CheckLobbyCreation());*/ /*

    }
    public void OnJoinRoomButton()
    {
        SceneManager.LoadScene("SampleScene");

        popupCreateRoom.SetActive(false);
        popupJoinRoom.SetActive(true);
        darkOverlay.SetActive(true);
        canvasHome.SetActive(false);
        canvasPlaygame.SetActive(false);
    }*/
}
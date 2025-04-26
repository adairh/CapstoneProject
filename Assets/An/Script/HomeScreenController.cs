//using UnityEngine;
//using UnityEngine.SceneManagement;

//public class HomeScreenController : MonoBehaviour
//{
//    [Header("Canvas Panels")]
//    public GameObject canvasHome;
//    public GameObject canvasPlaygame;

//    void Start()
//    {
//        canvasPlaygame.SetActive(false); // An playgame khi bat dau
//    }

//    // Button Play Game (trong panel_home)
//    public void OnPlayGamesButton()
//    {
//        canvasHome.SetActive(false);
//        canvasPlaygame.SetActive(true);
//    }

//    // Button Back (trong canvas_playgame)
//    public void OnBackToHome()
//    {
//        canvasPlaygame.SetActive(false);
//        canvasHome.SetActive(true);
//    }

//    // Button de choi game 1
//    public void OnPlayGame1()
//    {
//        SceneManager.LoadScene("Game1");
//    }

//    // Button de choi game 2
//    public void OnPlayGame2()
//    {
//        SceneManager.LoadScene("Game2");
//    }
//}

using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class HomeScreenController : MonoBehaviour
{
    [Header("Main Canvases")]
    public GameObject canvasHome;
    public GameObject canvasPlaygame;

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

    //void Start()
    //{
    //    // mac dinh hien thi canvas home
    //    canvasHome.SetActive(true);
    //    canvasPlaygame.SetActive(false);

    //    // an popup, overlay luc dau
    //    popupJoinRoom.SetActive(false);
    //    popupCreateRoom.SetActive(false);
    //    darkOverlay.SetActive(false);
    //}
    void Start()
    {
        //mac dinh hien canvasHome, an canvasPlaygame
        canvasHome.SetActive(true);
        canvasPlaygame.SetActive(false);

        // dam bao cac popup + overlay duoc an ngay khi lo bat trong editor
        if (popupJoinRoom != null) popupJoinRoom.SetActive(false);
        if (popupCreateRoom != null) popupCreateRoom.SetActive(false);
        if (darkOverlay != null) darkOverlay.SetActive(false);
    }
    // ===================== PLAY GAME =====================

    public void OnPlayGamesButton()
    {
        canvasHome.SetActive(false);
        canvasPlaygame.SetActive(true);
    }

    public void OnBackToHome()
    {
        canvasPlaygame.SetActive(false);
        canvasHome.SetActive(true);
    }

    public void OnPlayGame1()
    {
        SceneManager.LoadScene("Game1");
    }

    public void OnPlayGame2()
    {
        SceneManager.LoadScene("Game2");
    }

    // ===================== JOIN ROOM =====================

    public void OnJoinRoomButton()
    {
        popupJoinRoom.SetActive(true);
        popupCreateRoom.SetActive(false);
        darkOverlay.SetActive(true);
    }

    public void SubmitJoinRoom()
    {
        Debug.Log("Join Room: " + joinRoomIDInput.text + " | Password: " + joinPasswordInput.text);
        popupJoinRoom.SetActive(false);
        darkOverlay.SetActive(false);
    }

    // ===================== CREATE ROOM =====================

    public void OnCreateRoomButton()
    {
        popupCreateRoom.SetActive(true);
        popupJoinRoom.SetActive(false);
        darkOverlay.SetActive(true);
    }

    public void SubmitCreateRoom()
    {
        Debug.Log("Create Room: " + createRoomIDInput.text + " | Password: " + createPasswordInput.text);
        popupCreateRoom.SetActive(false);
        darkOverlay.SetActive(false);
    }

    // ===================== QUIT POPUP =====================

    public void CloseAllPopups()
    {
        popupJoinRoom.SetActive(false);
        popupCreateRoom.SetActive(false);
        darkOverlay.SetActive(false);
        canvasHome.SetActive(true);
    }
}

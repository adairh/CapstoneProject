
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
// Khoa đã ở đây
public class HomeScreenController : MonoBehaviour
{
    public static HomeScreenController Instance;

    [Header("Main Canvases")]
    public GameObject canvasHome;
    public GameObject canvasPlaygame;
    public GameObject canvasOnboarding; // Onboarding screen with ShowHome button
    public GameObject canvas; // Main Canvas transform for UI instantiation
    public GameObject overPlayCanvas;

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

    private bool isCreateRoomButtonClicked = false;

    private void Awake()
    {
        // Singleton pattern to prevent duplicates
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Make this object persistent
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
    }

    // ===================== KHONG BO =====================
    void Start()
    {
        // Không đụng 
        //mac dinh hien canvasHome, an canvasPlaygame
        overPlayCanvas.SetActive(false);
        ConfigureUIBasedOnScene();
        canvasHome.SetActive(true);
        canvasPlaygame.SetActive(false);
        canvasOnboarding.SetActive(false); // Hide onboarding screen if it's active
        canvas.SetActive(false); // an canvas chinh khi bat dau

        // dam bao cac popup + overlay duoc an ngay khi lo bat trong editor
        if (popupJoinRoom != null) popupJoinRoom.SetActive(false);
        if (popupCreateRoom != null) popupCreateRoom.SetActive(false);
        if (darkOverlay != null) darkOverlay.SetActive(false);
        // Configure UI based on the current scene

    }
    // ===================== kHONG BO =====================
    //void Start()
    //{
    //    ConfigureUIBasedOnScene();

    //    // An popup vA overlay
    //    if (popupJoinRoom != null) popupJoinRoom.SetActive(false);
    //    if (popupCreateRoom != null) popupCreateRoom.SetActive(false);
    //    if (darkOverlay != null) darkOverlay.SetActive(false);

    //    // Xu ly giao dien khoi đau theo tinh huong
    //    if (returnFromGame)
    //    {
    //        canvasHome.SetActive(false);
    //        canvasPlaygame.SetActive(true);
    //        returnFromGame = false;
    //    }
    //    else
    //    {
    //        canvasHome.SetActive(true);         // truong hop mac dinh
    //        canvasPlaygame.SetActive(false);
    //        canvasOnboarding.SetActive(false); // Hide onboarding screen if it's active
    //        canvas.SetActive(false); // an canvas chinh khi bat dau
    //    }
    //}

    private void ConfigureUIBasedOnScene()
    {
        // Get the active scene
        Scene currentScene = SceneManager.GetActiveScene();

        // Check the scene name or build index
        if (currentScene.name == "SampleScene")
        {
            if (isCreateRoomButtonClicked)
            {
                popupCreateRoom.SetActive(true);
                popupJoinRoom.SetActive(false);

                darkOverlay.SetActive(true);

                isCreateRoomButtonClicked = false;
            }

        }
        
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
        SceneManager.LoadScene("Menu");
    }

    public void OnPlayGame2()
    {
        SceneManager.LoadScene("GameScene");
    }

    // ===================== JOIN ROOM =====================

    public void OnJoinRoomButton()
    {
        // Không đụng - KHoa
        SceneManager.LoadScene("SampleScene");
        //canvasHome.SetActive(false);
        
        
        //popupJoinRoom.SetActive(false);
        // popupCreateRoom.SetActive(true);

        //darkOverlay.SetActive(true);

        /*canvasPlaygame.SetActive(false);
        canvasOnboarding.SetActive(false);*/ // Hide onboarding screen if it's active
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
        isCreateRoomButtonClicked = true;
        // Không đụng - Khoa
        SceneManager.LoadScene("SampleScene");
        /*popupCreateRoom.SetActive(true);
        popupJoinRoom.SetActive(false);*/
        
      /*  canvasHome.SetActive(true);
        darkOverlay.SetActive(true);
        */
        /*canvasPlaygame.SetActive(false);
        canvasOnboarding.SetActive(false); */

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
        //SceneManager.LoadScene("MAIN");
        popupJoinRoom.SetActive(false);
        popupCreateRoom.SetActive(false);
        darkOverlay.SetActive(false);
        canvasOnboarding.SetActive(false);
        canvasHome.SetActive(true);
    }
}

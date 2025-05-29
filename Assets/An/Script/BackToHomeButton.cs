using System;
using Geometry;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    // --- Handle scene change and hide onboarding ---
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Find the "Onboarding" object by tag in the new scene and hide it
        GameObject onboardingObj = GameObject.FindGameObjectWithTag("Onboarding");
        if (onboardingObj != null)
        {
            Debug.Log("Found Onboarding object in scene '" + scene.name + "': " + onboardingObj.name);
            onboardingObj.SetActive(false);
        }
        else
        {
            Debug.LogWarning("No object with tag 'Onboarding' found in scene '" + scene.name + "'.");
        }
    }

    // --- UI Logic ---
    public void OnBackToHome()
    {
        canvasHome.SetActive(true);
        canvasOnboarding.SetActive(false);
        canvasPlaygame.SetActive(false);
    }
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public void OnBackToHomeFrom_DrawingScene()
    {
        SceneManager.LoadScene("MAIN");
    }

    public void OnBackFromAI_to_Home()
    {
        // Optionally: can still check or do logic here before loading scene
        SceneManager.LoadScene("MAIN");
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

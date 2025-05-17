/*

Source code by Seth A. Robinson

 */

//#define RT_NOAUDIO

using DG.Tweening;
using UnityEngine;

//using UnityEngine.Networking;

public class GameLogic : MonoBehaviour
{
    private static GameLogic _this;
    public GameObject m_notepadTemplatePrefab;

    private void Awake()
    {
        //float targetFrameRate = Screen.currentResolution.refreshRateRatio * 60f;
        //Application.targetFrameRate = (int)targetFrameRate;
        QualitySettings.vSyncCount = 1;
        //QualitySettings.antiAliasing = 4;
    }

    // Use this for initialization
    private void Start()
    {
        DOTween.Init(true, true, LogBehaviour.Verbose).SetCapacity(200, 20);
        // RTAudioManager.Get().SetDefaultMusicVol(0.4f);
        _this = this;

#if RT_NOAUDIO
		AudioListener.pause = true;
#endif


        RTConsole.Get().SetShowUnityDebugLogInConsole(true);

        //RTEventManager.Get().Schedule(RTAudioManager.GetName(), "PlayMusic", 1, "intro");
        var version = "Unity V " + Application.unityVersion + " :";

#if NET_2_0
        version += " Net 2.0 API";
#endif
#if NET_2_0_SUBSET
        version += " Net 2.0 Subset API";
#endif

#if NET_4_6
            version += " .Net 4.6 API";
#endif

#if RT_BETA
        print ("Beta build detected!");
#endif


        RTConsole.Get().SetMirrorToDebugLog(true);
    }

    // Update is called once per frame
    private void Update()
    {
    }


    private void OnDestroy()
    {
        print("Game logic destroyed");
    }

    private void OnApplicationQuit()
    {
        // Make sure prefs are saved before quitting.
        //PlayerPrefs.Save();
        RTConsole.Log("Application quitting normally");

//        NetworkTransport.Shutdown();
        print("QUITTING!");
    }

    public static string GetName()
    {
        return Get().name;
    }

    public static GameLogic Get()
    {
        return _this;
    }

    public void OnConfigButton()
    {
        var notepadScript = RTNotepad.OpenFile(Config.Get().GetConfigText(), m_notepadTemplatePrefab);
        notepadScript.m_onClickedSavedCallback += OnConfigSaved;
        notepadScript.m_onClickedCancelCallback += OnConfigCanceled;
    }

    private void OnConfigSaved(string text)
    {
        //Config.Get().ProcessConfigString(text);
        //Config.Get().SaveConfigToFile(); //it might have changed.

        //Debug.Log("They clicked save.  Text entered: " + text);

        Config.Get().LoadConfigFile(text);
    }

    private void OnConfigCanceled(string text)
    {
        Debug.Log("Clicked cancel.");
    }
}
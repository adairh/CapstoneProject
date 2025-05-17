using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/*

    RTConsole by Seth A. Robinson

    To use this, drag the RTConsole prefab so its a child of a Canvas. 

    You should see a console that allows input when you play.

    To see Unity debug messages, run this from a script on startup somewhere:
    
    RTConsole.Get().SetShowUnityDebugLogInConsole(true);

    //To add your own debug messages, do this:

    RTConsole.Log("Hello! `4This is the color red``. Cool, right?!");

 */

public class RTConsole : MonoBehaviour
{
    private static RTConsole _this;
    public TextMeshProUGUI _consoleText;
    public InputField _inputField;

    private bool _isDisplayingUnityDebugLog;
    private bool _isHeadlessMode;
    private bool _isSendingToUnityDebugLog;
    private Queue<string> _lines;

    private string
        _logPrependString =
            "RTLOG"; //only applies to the Unity internal debug logs, this helps me filter for it when watching Android stuff with logcat

    private int _maxConsoleLines = 500;

    private bool _requiresRefresh;
    private ScrollRect _scrollRect;

    private void Awake()
    {
        _lines = new Queue<string>();

        _this = GetComponent<RTConsole>();

        if (!_this)
        {
            print("Error findingRTConsole");
            return;
        }

        _scrollRect = GetComponent<ScrollRect>();
    }

    private void Start()
    {
        if (_consoleText != null)
            _consoleText.text = "";
    }

    // Update is called once per frame
    private void Update()
    {
        if (_requiresRefresh) TrimAndUpdateWidget();
    }

    public event Action<string> OnGotConsoleInputEvent;

    // Use this for initialization
    public static RTConsole Get()
    {
        if (!_this)
        {
            var us = RTUtil.FindIncludingInactive("RTConsole");
            if (us)
            {
                if (us.activeSelf == false) us.SetActive(true);
            }
            else
            {
                //Actually, let's just create it
                //_this = new GameObject("RTConsole").AddComponent<RTConsole>();
                return null;
            }
        }

        return _this;
    }

    public void SetLogPrependString(string s)
    {
        _logPrependString = s;
    }

    public void CopyToClipboard()
    {
        //put all the lines of text into a string, then copy that to the system clipboard
        var s = "";
        foreach (var sLine in _lines) s += sLine + "\n";
        GUIUtility.systemCopyBuffer = s;
    }

    public void SetMaxLines(int count)
    {
        if (_maxConsoleLines == count) return;

        _maxConsoleLines = count;
        _requiresRefresh = true;
    }

    public void SetHeadlessMode(bool bNew)
    {
        _isHeadlessMode = bNew;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        var color = "";
        var colorEnd = "";

        switch (type)
        {
            case LogType.Error:
                color = "`4";
                colorEnd = "``";
                break;
            case LogType.Warning:
                color = "`$";
                colorEnd = "``";
                break;
            case LogType.Exception:
                color = "`#";
                colorEnd = "``";
                logString += stackTrace;
                break;
            case LogType.Assert:
                color = "`@";
                logString += stackTrace;
                colorEnd = "``";
                break;
        }

        Log(color + logString + colorEnd);
    }

    public void SetShowUnityDebugLogInConsole(bool bNew)
    {
        if (bNew == _isDisplayingUnityDebugLog) return;

        _isDisplayingUnityDebugLog = bNew;

        if (bNew)
            Application.logMessageReceived += HandleLog;
        else
            Application.logMessageReceived -= HandleLog;
    }

    public void SetMirrorToDebugLog(bool bNew)
    {
        _isSendingToUnityDebugLog = bNew;
    }

    public static void Log(string text)
    {
        if (Get() == null) return;

        //I tend to send huge texts with \r\n for the lines, so I'm going to be slow and split them
        var strings = text.Split('\n');
        foreach (var s in strings) _this.Add(s + "\n");
    }

    //just adds red. 
    public static void LogError(string text)
    {
        if (Get() == null) return;

        //I tend to send huge texts with \r\n for the lines, so I'm going to be slow and split them
        var strings = text.Split('\n');
        foreach (var s in strings) _this.Add("`4" + s + "``\n");
    }

    public static void LogRaw(string text)
    {
        _this.Add(text);
    }

    public string GetCurrentText()
    {
        return _inputField.text;
    }

    public void SetFocusOnInput(string text)
    {
        _inputField.text = text;
        _inputField.ActivateInputField(); //returns focus to field after pressing enter.  Possibly not wanted/needed on mobiles
        StartCoroutine(MoveTextEnd_NextFrame());
        //trick to de-highlight text:  https://answers.unity.com/questions/1103287/how-to-deselect-text-in-an-inputfield.html
    }

    private IEnumerator MoveTextEnd_NextFrame()
    {
        yield return 0; // Skip the first frame in which this is called.
        _inputField.MoveTextEnd(false); // Do this during the next frame.
    }

    public void OnEndEdit(string text)
    {
        //if (!Input.GetKeyDown(KeyCode.Return)) return; //probably not wanted/needed on touch screens...
        if (!Keyboard.current.enterKey.isPressed) return; //new input system

        SetFocusOnInput("");
        if (text.Length == 0) return;
        // print(text);
        _inputField.text = "";
        if (OnGotConsoleInputEvent == null)
            print(
                "RTConsole::OnEndEdit:  User typed something in, but you didn't add a handler to OnGotConsoleInputEvent");
        else
            OnGotConsoleInputEvent(text);
    }

    private void Add(string text)
    {
        if (_isSendingToUnityDebugLog)
        {
            if (_isDisplayingUnityDebugLog)
            {
                //yes, this is bad, but it's only used for debug stuff
                SetShowUnityDebugLogInConsole(false);
                Debug.unityLogger.Log(_logPrependString, text);
                SetShowUnityDebugLogInConsole(true);
            }
            else
            {
                Debug.Log(text);
            }
        }

        if (!_isHeadlessMode && _consoleText)
        {
            //add the line
            _lines.Enqueue(RTUtil.ConvertSansiToUnityColors(text));
            _requiresRefresh = true;
        }
    }

    private void TrimAndUpdateWidget()
    {
        _requiresRefresh = false;
        //if we have too many lines, kill the oldest one.  Unity's Text widget sucks btw, it can't show that many lines
        while (_lines.Count > _maxConsoleLines) _lines.Dequeue();

        //copy them to the text object

        _consoleText.text = string.Concat(_lines.ToArray());

        // _consoleText.text = _consoleText.text + RTUtil.ConvertSansiToUnityColors(text);
        Canvas.ForceUpdateCanvases();
        _scrollRect.verticalNormalizedPosition = 0f;
    }
}
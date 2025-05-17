using System;
using System.Collections;
using System.IO;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Networking;

public class OpenAISpeechToTextManager : MonoBehaviour
{
    public void Start()
    {
        // ExampleOfUse();
    }

    //*  EXAMPLE START (this could be moved to your own code) */

    private void ExampleOfUse()
    {
        var speechToTextScript = gameObject.GetComponent<OpenAISpeechToTextManager>();

        var fileName = "output.wav";
        var fileBytes = File.ReadAllBytes(fileName);
        var openAI_APIKey = "put it here";
        var prompt = "";

        var db = new RTDB();
        speechToTextScript.SpawnSpeechToTextRequest(prompt, OnSpeechToTextCompletedCallback, db, openAI_APIKey,
            fileBytes);
    }

    private void OnSpeechToTextCompletedCallback(RTDB db, JSONObject jsonNode)
    {
        if (jsonNode == null)
        {
            //must have been an error
            Debug.Log("Got callback! Data: " + db);
            RTQuickMessageManager.Get().ShowMessage(db.GetString("msg"));
            return;
        }


        foreach (var kvp in jsonNode) Debug.Log("Key: " + kvp.Key + " Val: " + kvp.Value);

        string reply = jsonNode["text"];
        // RTQuickMessageManager.Get().ShowMessage(reply);
    }

    //*  EXAMPLE END */
    public bool SpawnSpeechToTextRequest(string prompt, Action<RTDB, JSONObject> myCallback, RTDB db,
        string openAI_APIKey, byte[] wavData)
    {
        StartCoroutine(GetRequest(prompt, myCallback, db, openAI_APIKey, wavData));
        return true;
    }

    private IEnumerator GetRequest(string prompt, Action<RTDB, JSONObject> myCallback, RTDB db, string openAI_APIKey,
        byte[] wavData)
    {
        string url;
        url = "https://api.openai.com/v1/audio/transcriptions";
        var model = "whisper-1";

        var formData = new WWWForm();
        formData.AddField("model", model);

        if (prompt != "") formData.AddField("prompt", model);

        formData.AddBinaryData("file", wavData, "openai.wav", "audio/wav");

        using (var postRequest = UnityWebRequest.Post(url, formData))
        {
            postRequest.SetRequestHeader("Authorization", "Bearer " + openAI_APIKey);
            postRequest.downloadHandler = new DownloadHandlerBuffer();
            yield return postRequest.SendWebRequest();

            if (postRequest.result != UnityWebRequest.Result.Success)
            {
                var msg = postRequest.error;
                Debug.Log(msg);
                //#if UNITY_STANDALONE && !RT_RELEASE
                File.WriteAllText("last_error_returned.json", postRequest.downloadHandler.text);
                //#endif
                db.Set("status", "failed");
                db.Set("msg", msg);
                myCallback.Invoke(db, null);
            }
            else
            {
#if UNITY_STANDALONE && !RT_RELEASE
                //Debug.Log("Form upload complete! Downloaded " + postRequest.downloadedBytes);
                File.WriteAllText("textgen_json_received.json", postRequest.downloadHandler.text);
#endif

                var rootNode = JSON.Parse(postRequest.downloadHandler.text);
                yield return null; //wait a frame to lesson the jerkiness
                Debug.Assert(rootNode.Tag == JSONNodeType.Object);
                db.Set("status", "success");
                myCallback.Invoke(db, (JSONObject)rootNode);
            }
        }
    }
}
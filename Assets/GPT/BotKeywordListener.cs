using UnityEngine;
using UnityEngine.SceneManagement;

public class BotKeywordListener : MonoBehaviour
{
    public string keyword;

    // Start is called before the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        if (GetComponent<OpenAiTextChat>().enabled)
            if (OpenAiTextChat.latestResponseMessage.Contains(keyword))
            {
                //Do things here 
                Debug.Log("cheesebuger");
                SceneManager.LoadScene(1);
            }
    }
}
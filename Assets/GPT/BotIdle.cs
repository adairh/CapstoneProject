using TMPro;
using UnityEngine;

public class BotIdle : MonoBehaviour
{
    public float talkRange;
    public int conversationMode; //Indicates which mode the player will be in for conversation. 0: Text chat, 1: TBD
    private GameObject player;
    private bool playerInRange;
    private GameObject talkHint;


    // Start is called before the first frame update
    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        talkHint = GameObject.FindGameObjectWithTag("TalkHint");
    }

    // Update is called once per frame
    private void Update()
    {
        RaycastHit hit;
        var playerInRangeNow =
            Physics.Raycast(transform.position, player.transform.position - transform.position, out hit, talkRange) &&
            hit.transform.position == player.transform.position; //Checks if player is in the talkrange of bot

        if (playerInRange && !playerInRangeNow)
            talkHint.GetComponent<TMP_Text>().enabled =
                false; //If statement checks when the player immedietly leaves the talk range. This will disable the talkhint

        playerInRange = playerInRangeNow; //Updates playerInRange bool to current status of the range of the player 
        //*All this was done to avoid an else statement. This was required since multiple BotIdle scripts will be present in the scene. Ask Amir if you have any questions*


        if (playerInRange)
        {
            talkHint.GetComponent<TMP_Text>().enabled = true; //Enables the talkhint
            if (conversationMode == 0 && Input.GetKeyUp(KeyCode.F)) StartConversation();
        }
    }


    private void StartConversation()
    {
        if (conversationMode == 0)
            gameObject.GetComponent<OpenAiTextChat>().enabled = true;
        else Debug.LogError("Conversation mode is invalid!");
        enabled = false;
    }
}
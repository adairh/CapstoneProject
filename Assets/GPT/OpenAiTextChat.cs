using System;
using System.Collections.Generic;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OpenAiTextChat : MonoBehaviour
{
    public static string latestResponseMessage = "";
    public GameObject conversationCamera;
    public AudioClip startSound;
    public AudioClip stopSound;
    public AudioClip sendSound;
    public AudioClip receiveSound;

    public string botName = "Bot";

    [TextArea(5, 20)] //minlines, maxlines
    public string initialRole = "You are a kind villager in the town of Vibeland";

    public string startString = "Approaching the villager..";

    private OpenAIAPI api;
    private AudioSource audioSource;
    private GameObject chatUI;
    private TMP_InputField inputField;
    private List<ChatMessage> messages;
    private Button okButton;
    private TMP_Text outputField;
    private GameObject player;

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape)) StopConversation();
        if (Input.GetKeyUp(KeyCode.Return)) okButton.onClick.Invoke();
    }

    private void OnEnable()
    {
        //Sound setup
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = startSound;
        audioSource.Play();

        //Player setup
        player = GameObject.FindGameObjectWithTag("Player");
        player.GetComponent<PlayerFunctions>().DisablePlayer();

        //Camera setup
        conversationCamera.SetActive(true);

        //UI Setup
        chatUI = GameObject.FindGameObjectWithTag("ChatUI");
        chatUI.GetComponent<Canvas>().enabled = true;
        inputField = chatUI.transform.GetComponentInChildren<TMP_InputField>();
        okButton = chatUI.transform.GetComponentInChildren<Button>();
        outputField = chatUI.transform.GetComponentInChildren<TMP_Text>();

        api = new OpenAIAPI(); //Gets api key from default location. In your username's folder on Windows.
        okButton.onClick.AddListener(() => GetResponse());


        messages = new List<ChatMessage>
        {
            new(ChatMessageRole.System, initialRole)
        };

        inputField.text = "";
        outputField.text = "";

        Debug.Log(startString);
        outputField.text = startString;
    }

    private void StopConversation()
    {
        audioSource.clip = stopSound;
        audioSource.Play();

        chatUI.GetComponent<Canvas>().enabled = false;
        conversationCamera.SetActive(false);

        player.GetComponent<PlayerFunctions>().EnablePlayer();

        gameObject.GetComponent<BotIdle>().enabled = true;
        enabled = false;
    }

    private async void GetResponse()
    {
        if (enabled == false) return;
        if (inputField.text.Length < 1) return;

        audioSource.clip = sendSound;
        audioSource.Play();

        okButton.enabled = false;

        var userMessage = new ChatMessage();
        userMessage.Role = ChatMessageRole.User; //Sets role to user

        userMessage.Content = inputField.text; //Sets content to what is in the text field

        if (userMessage.Content.Length > 100)
            userMessage.Content = userMessage.Content.Substring(0, 100); //Shortens for safety

        Debug.Log(string.Format("{0}: {1}", userMessage.rawRole, userMessage.Content)); //Logs user input
        outputField.text = string.Format("You: {0}", userMessage.Content);

        messages.Add(userMessage);
        inputField.text = "";

        try
        {
            var chatResult = await api.Chat.CreateChatCompletionAsync(new ChatRequest
            {
                Model = Model.ChatGPTTurbo,
                Temperature = 0.1,
                MaxTokens = 90,
                Messages = messages
            });

            var responseMessage = new ChatMessage();
            responseMessage.Role = chatResult.Choices[0].Message.Role; //Get role first choice from list of responses
            responseMessage.Content =
                chatResult.Choices[0].Message.Content; //Get content first choice from list of responses
            latestResponseMessage = responseMessage.Content;

            Debug.Log(string.Format("{0}: {1}", responseMessage.rawRole,
                responseMessage.Content)); //Logs response output
            outputField.text = string.Format("You: {0}\n\n{1}: {2}", userMessage.Content, botName,
                responseMessage.Content);

            messages.Add(responseMessage);
        }
        catch (Exception e)
        {
            outputField.text = "Sorry, something went wrong: " + e;
        }
        //Try and catch used to display errors in game


        audioSource.clip = receiveSound;
        audioSource.Play();

        okButton.enabled = true;
    }
}
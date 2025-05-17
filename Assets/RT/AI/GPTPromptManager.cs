using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using static OpenAITextCompletionManager;

public class GPTPromptManager : MonoBehaviour
{
    private string _baseSystemPrompt = "";

    private readonly Queue<GPTInteractions> _interactions = new();
    private readonly int _interactionsToKeepWhenBuildingJournal = 8;
    private string _journalSystemPrompt = "";
    private readonly int _maxTokensBeforeJournaling = 1024 * 6;
    private readonly int _maxWordsForJournal = 1000;

    private readonly float _tokensPerWordMult = 1.25f;

    public void Reset()
    {
        _baseSystemPrompt = "";
        _journalSystemPrompt = "";
        _interactions.Clear();
    }

    // Start is called before the first frame update
    private void Start()
    {
    }

    private void Update()
    {
    }

    public void AddInteraction(string role, string content)
    {
        /*
         //actually, I don't think is so important.  You can invent roles if you want.
        if (role != "assistant" && role != "system" && role != "user")
        {
            Debug.LogError("Invalid role: " + role);
            Debug.Assert(false);
            return;
        }
        */
        _interactions.Enqueue(new GPTInteractions(role, content));
    }

    public void SetBaseSystemPrompt(string prompt)
    {
        _baseSystemPrompt = prompt;
    }

    public void SetJournalSystemPrompt(string prompt)
    {
        _journalSystemPrompt = prompt;
    }

    public bool IsTooBig()
    {
        var size = _baseSystemPrompt.Length * _tokensPerWordMult;

        size = _journalSystemPrompt.Length * _tokensPerWordMult;

        foreach (var interaction in _interactions) size += interaction._content.Length * _tokensPerWordMult;

        if (size > _maxTokensBeforeJournaling) return true;

        return false;
    }

    //a function that removes all but the last N lines from our interaction queue
    public void TrimInteractionsToLastNLines(int linesToKeepAtTheEnd)
    {
        while (_interactions.Count > linesToKeepAtTheEnd) _interactions.Dequeue();
    }


    public void SummarizeHistoryIntoJournal(string openAI_APIKey, Action<RTDB, JSONObject> myCallback)
    {
        var lines = BuildPrompt(_interactionsToKeepWhenBuildingJournal);

        var basePrompt =
            $@"Summarize the entire conversation of you playing this game thus far into {_maxWordsForJournal} words or less.";

        //add a line with role system using the base prompt
        lines.Enqueue(new GTPChatLine("user", basePrompt));


        var textCompletionScript = gameObject.GetComponent<OpenAITextCompletionManager>();

        var json = textCompletionScript.BuildChatCompleteJSON(lines, 1500, 0.2f, "gpt-4");
        var db = new RTDB();


        TrimInteractionsToLastNLines(_interactionsToKeepWhenBuildingJournal);
        textCompletionScript.SpawnChatCompleteRequest(json, myCallback, db, openAI_APIKey);
    }

    public Queue<GTPChatLine> BuildPrompt(int linesToIgnoreAtTheEnd = 0)
    {
        var lines = new Queue<GTPChatLine>();

        //add a line with role system using the base prompt
        lines.Enqueue(new GTPChatLine("system", _baseSystemPrompt));
        lines.Enqueue(new GTPChatLine("system", _journalSystemPrompt));

        //add the last few interactions, but ignore the last linesToIgnoreAtTheEnd lines
        //add the last few interactions, but ignore the last linesToIgnoreAtTheEnd lines
        var count = _interactions.Count - linesToIgnoreAtTheEnd;
        if (count < 0) count = 0;
        foreach (var interaction in _interactions)
        {
            if (count <= 0) break;
            count--;
            lines.Enqueue(new GTPChatLine(interaction._role, interaction._content));
        }


        return lines;
    }

    private class GPTInteractions
    {
        public readonly string _content;

        public readonly string _role;

        //build constructer that takes both parms
        public GPTInteractions(string role, string content)
        {
            _role = role;
            _content = content;
        }
    }
}
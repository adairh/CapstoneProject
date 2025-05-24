using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using _QuestionAnswersModule.Scripts.SimpleRealization;

public class WrongQuestionsManager : MonoBehaviour
{
    private static WrongQuestionsManager instance;
    public static WrongQuestionsManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("WrongQuestionsManager");
                instance = go.AddComponent<WrongQuestionsManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private string wrongQuestionsFilePath;
    private List<WrongQuestionData> wrongQuestions = new List<WrongQuestionData>();

    [Serializable]
    public class WrongQuestionData
    {
        public string questionText;
        public string correctAnswer;
        public string userAnswer;
        public string levelName;
        public DateTime timestamp;
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeWrongQuestionsFile();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeWrongQuestionsFile()
    {
        wrongQuestionsFilePath = Path.Combine(Application.persistentDataPath, "wrong_questions.txt");
        Debug.Log($"Wrong questions will be saved to: {wrongQuestionsFilePath}");
        LoadWrongQuestions();
    }

    public void AddWrongQuestion(string questionText, string correctAnswer, string userAnswer, string levelName)
    {
        var wrongQuestion = new WrongQuestionData
        {
            questionText = questionText,
            correctAnswer = correctAnswer,
            userAnswer = userAnswer,
            levelName = levelName,
            timestamp = DateTime.Now
        };

        wrongQuestions.Add(wrongQuestion);
        SaveWrongQuestions();
    }

    private void SaveWrongQuestions()
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(wrongQuestionsFilePath, true))
            {
                var latestQuestion = wrongQuestions[wrongQuestions.Count - 1];
                string entry = $"[{latestQuestion.timestamp}] Level: {latestQuestion.levelName}\n" +
                             $"Question: {latestQuestion.questionText}\n" +
                             $"Correct Answer: {latestQuestion.correctAnswer}\n" +
                             $"Your Answer: {latestQuestion.userAnswer}\n" +
                             $"----------------------------------------\n";
                writer.Write(entry);
            }
            Debug.Log($"Wrong question saved to {wrongQuestionsFilePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving wrong question: {e.Message}");
        }
    }

    private void LoadWrongQuestions()
    {
        if (File.Exists(wrongQuestionsFilePath))
        {
            try
            {
                string[] lines = File.ReadAllLines(wrongQuestionsFilePath);
                // We don't need to load the questions into memory since we're just appending
                Debug.Log($"Loaded wrong questions file from {wrongQuestionsFilePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading wrong questions: {e.Message}");
            }
        }
    }

    public void ClearWrongQuestions()
    {
        try
        {
            if (File.Exists(wrongQuestionsFilePath))
            {
                File.Delete(wrongQuestionsFilePath);
                wrongQuestions.Clear();
                Debug.Log("Wrong questions file cleared");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error clearing wrong questions: {e.Message}");
        }
    }
} 
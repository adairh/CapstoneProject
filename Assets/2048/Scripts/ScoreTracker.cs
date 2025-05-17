using UnityEngine;
using UnityEngine.UI;

public class ScoreTracker : MonoBehaviour
{
    public static ScoreTracker Instance;
    public Text ScoreText;
    public Text HighScoreText;

    private int score;

    public int Score
    {
        get => score;
        set
        {
            score = value;
            ScoreText.text = score.ToString();

            if (PlayerPrefs.GetInt("HighScore") < score)
            {
                PlayerPrefs.SetInt("HighScore", score);
                HighScoreText.text = score.ToString();
            }
        }
    }

    private void Awake()
    {
        Instance = this;

        if (!PlayerPrefs.HasKey("HighScore"))
            PlayerPrefs.SetInt("HighScore", 0);

        ScoreText.text = "0";
        HighScoreText.text = PlayerPrefs.GetInt("HighScore").ToString();
    }
}
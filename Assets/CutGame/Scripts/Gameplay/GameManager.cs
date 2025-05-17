using System;
using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static int timeLeft;

    public static GameManager instance;

    public GameUI gameUI;

    [HideInInspector] public int rating;

    [SerializeField] private LevelData levelData;

    [HideInInspector] public bool failed;

    [HideInInspector] public bool hasEnded;

    public int countdown = 3;

    public Canvas Quiz;

    private int alliesCount,
        alliesDestroyed,
        enemiesCount,
        enemiesDestroyed;

    private Level level;

    private Virus virus;

    private void Start()
    {
        instance = this;
        InitGame();
        //Debug.Log($"Game has started with {enemiesCount} enemies and {alliesCount} allies.");
    }

    private void OnDestroy()
    {
        LeanTween.cancelAll();
    }

    public event Action OnCountdown;
    public event Action OnStart;

    public event Action OnTick;
    public event Action OnAllyKilled;

    public event Action OnTimesUp;
    public event Action OnComplete;

    public void SetVirus(Virus virus)
    {
        this.virus = virus;
    }

    public void DeleteVirus()
    {
        if (virus != null)
        {
            virus.Destroy();
            virus = null;
        }
    }

    public Virus GetVirus()
    {
        return virus;
    }

    // Initializes the game (selected level) in Game scene
    private void InitGame()
    {
        var go = Instantiate(LevelLoader.levelToLoad.levelPrefab);

        level = go.GetComponent<Level>();
        timeLeft = LevelLoader.levelToLoad.timeLimit;

        level.OnAllyDestroy += OnAllyDestroy;
        level.OnEnemyDestroy += OnEnemyDestroy;

        alliesCount = level.bloodCells.Count;
        enemiesCount = level.viruses.Count;

        level.enabled = true;

        gameUI.Init(timeLeft);

        //OnTick += () => { };
        //OnTimesUp += () => { };
        //OnComplete += () => { };

        StartCoroutine(Timer());
    }

    // Executes when BloodCell is destoyed
    private void OnAllyDestroy()
    {
        alliesDestroyed++;
        //Debug.Log($"Destroyed allies {alliesDestroyed} of {alliesCount}");

        UpdateRating();
        OnAllyKilled();

        if (alliesDestroyed > 3)
            failed = true;
    }

    // Executes when Virus is destoyed
    private void OnEnemyDestroy()
    {
        enemiesDestroyed++;
        //Debug.Log($"Destroyed enemies {enemiesDestroyed} of {enemiesCount}");

        if (enemiesDestroyed == enemiesCount || failed)
        {
            if (failed)
            {
                OnTimesUp();
                hasEnded = true;
                gameObject.SetActive(false);
            }
            else
            {
                Complete();
                OnComplete();
                hasEnded = true;
                gameObject.SetActive(false);
            }
        }
    }

    // Handles clock and timings in game
    private IEnumerator Timer()
    {
        yield return new WaitForSecondsRealtime(0.1f);

        var delay = new WaitForSecondsRealtime(0.33f);

        for (; countdown >= 0; countdown--)
        {
            OnCountdown();

            if (countdown > 0)
                yield return delay;
        }

        yield return new WaitForSecondsRealtime(0.1f);

        OnStart();

        delay = new WaitForSecondsRealtime(1.0f);

        for (; timeLeft > 0; timeLeft--)
        {
            OnTick();
            yield return delay;
        }

        OnTimesUp();
        hasEnded = true;
        gameObject.SetActive(false);
        Quiz.gameObject.SetActive(false);
    }

    private void UpdateRating()
    {
        rating = 3 - alliesDestroyed;
    }

    // Executes when game is in "complete" state
    public void Complete()
    {
        var index = LevelLoader.currentLevelIndex;
        var level = levelData.levels[index];

        var progress = false;

        //Debug.Log("Allies destroyed " + alliesDestroyed);
        UpdateRating();

        if (level.rating < rating)
        {
            levelData.SetRating(index, rating);
            progress = true;
        }

        index++;

        if (index < levelData.levels.Count
            && !levelData.levels[index].unlocked)
        {
            levelData.Unlock(index);
            progress = true;
        }

        if (progress)
            levelData.Save();
    }
}